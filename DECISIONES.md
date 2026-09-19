# Decisiones de arquitectura

Acá registramos las decisiones técnicas del proyecto y por qué las tomamos. La idea es que
cuando volvamos sobre una parte del código en dos semanas, no tengamos que reconstruir de
memoria el razonamiento, y que cualquiera de las dos pueda explicar cualquier parte del
sistema.

**Estado de cada decisión:** `Aceptada` está implementada o en curso. `Pendiente` todavía
tenemos que definirla entre las dos.

---

## 1. Arquitectura en capas con proyectos separados

**Estado:** Aceptada

Separamos el backend en cuatro proyectos:

- `SubastaYa.Domain`: entidades y enums. No depende de nada.
- `SubastaYa.Application`: interfaces de servicios, DTOs y casos de uso.
- `SubastaYa.Infrastructure`: EF Core, `DbContext`, migraciones, repositorios y seed.
- `SubastaYa.API`: controllers, Swagger y arranque.

Las dependencias van en una sola dirección: `API → Application → Domain` e
`Infrastructure → Application → Domain`. El dominio no conoce a nadie.

Podríamos haber hecho un solo proyecto con carpetas, que es más rápido de armar. Elegimos
proyectos separados porque con carpetas nada impide que un controller termine importando el
`DbContext` y consultando la base directo: la separación queda como intención y se rompe
sola con el tiempo. Con proyectos separados es el compilador el que la hace cumplir, y no
depende de que nos acordemos.

## 2. Code-First con migraciones y la versión de la herramienta fijada

**Estado:** Aceptada

El esquema se genera exclusivamente por migraciones de EF Core. No tocamos la base a mano
nunca: si hace falta un cambio, sale de una migración.

Fijamos la versión de `dotnet-ef` en un manifiesto local (`.config/dotnet-tools.json`,
versión 9.0.8) y agregamos un `NuGet.config` en la raíz que apunta explícitamente a
nuget.org.

El manifiesto no es un detalle menor. Si cada una genera migraciones con una versión
distinta de la herramienta, el `ModelSnapshot` sale diferente y los merges se vuelven
imposibles de resolver. Con el manifiesto, un `dotnet tool restore` nos deja a las dos con
exactamente la misma herramienta.

## 3. Estados de la subasta

**Estado:** Aceptada

```
Scheduled = 0   // Próxima: publicada, la ventana de ofertas todavía no abrió
Active    = 1
Finished  = 2   // Cerró con ganador y los fondos ya se liquidaron
Deserted  = 3   // Venció sin recibir ninguna puja
Cancelled = 4
```

Los nombres van en inglés para ser consistentes con el resto del código, y cada uno está
comentado en el enum con su significado.

`Finished` y `Deserted` son dos estados distintos y no un booleano sobre `Finished`. La
razón es que el cierre llega a cada uno por un camino diferente: uno liquida fondos del
comprador al vendedor y el otro no mueve plata, y la auditoría tiene que poder
distinguirlos.

## 4. Concurrencia optimista con `RowVersion` y respuesta 409

**Estado:** Aceptada

`Auction` y `Wallet` llevan una propiedad `byte[] RowVersion` configurada con
`.IsRowVersion()`, que en SQL Server se traduce al tipo nativo `rowversion`. Lo importante
es que lo incrementa el motor de base de datos, no nuestro código.

Descartamos usar un `int Version` que incrementamos a mano en cada servicio. Funciona, pero
alcanza con olvidarse en un solo camino del código para que la protección desaparezca sin
que nadie se dé cuenta, y no hay forma de detectarlo salvo que falle en producción.

Cuando `SaveChangesAsync` lanza `DbUpdateConcurrencyException`, la API responde
**409 Conflict** con un mensaje entendible para el usuario. El mapeo vive en un único
middleware global y no repetido en cada controller, así no hay manera de que a algún
endpoint se le escape y devuelva 400 o 500.

## 5. Manejo de errores centralizado

**Estado:** Aceptada

Definimos cuatro excepciones propias en la capa Application y un middleware global que las
traduce a códigos HTTP:

| Excepción | Respuesta |
|---|---|
| `NotFoundException` | 404 |
| `BusinessRuleException` | 400 con el detalle de la regla violada |
| `ConflictException` | 409 |
| `InvalidCredentialsException` | 401 |
| `DbUpdateConcurrencyException` | 409 |
| Cualquier otra | 500 con mensaje genérico y el detalle en el log |

Los controllers no llevan `try/catch`. Todos devuelven `application/problem+json`, que es el
formato estándar de errores de ASP.NET Core, así el frontend tiene un solo formato que
parsear.

Lo resolvimos con middleware y no con `try/catch` por controller porque de esa forma solo
están cubiertos los casos que previmos al escribirlos: cualquier excepción nueva que
aparezca después cae en un 500 genérico. Con el middleware, agregar un caso es una línea y
vale para toda la API de una vez.

## 6. Autenticación con JWT, alcance mínimo

**Estado:** Aceptada

Implementamos registro y login que devuelven un JWT firmado con HMAC-SHA256. Las
contraseñas se guardan hasheadas con BCrypt, nunca en texto plano. El token lleva cuatro
claims: el id del usuario en `sub`, el email, el nombre de usuario y un `jti`. Sin roles y
sin refresh tokens, porque el sistema no distingue tipos de usuario: el mismo puede vender y
comprar.

Decidimos hacerlo al principio y no al final por tres razones.

El `DbSeeder` tiene que crear los usuarios de prueba con su `PasswordHash`, así que no se
puede escribir hasta tener definido el esquema de hasheo. Si lo cambiáramos después, habría
que regenerar el seed entero.

Agregar autenticación una vez que existen todos los controllers obliga a tocarlos todos, y
son justamente los archivos que estamos editando las dos en paralelo. Hacerlo primero evita
ese merge.

Y sin autenticación, la identidad del usuario tendría que viajar por body o por query
string, lo que significa que cualquiera puede pujar o cargarse saldo haciéndose pasar por
otro. En una plataforma donde toda puja queda respaldada por saldo congelado de verdad, eso
no se sostiene.

## 7. El id del usuario sale del token, nunca de la URL ni del body

**Estado:** Aceptada

Ningún endpoint recibe un `userId`, `walletId` o `sellerId` como parámetro. El usuario
actual se resuelve desde el token a través de la interfaz `ICurrentUserService`, definida en
`Application` con una sola propiedad `Guid UserId`. Los servicios dependen de esa interfaz;
la implementación que lee los claims vive en `API`.

Poner `[Authorize]` en un endpoint no alcanza si la ruta recibe el id del recurso: proteger
`POST /api/wallet/{walletId}/deposit` con un token cualquiera permite que un usuario
autenticado deposite en la billetera de otro. Es peor que no tener nada, porque parece
seguro. Sacando el id de la URL, el problema no puede existir.

La interfaz tiene un segundo beneficio: con el contrato definido de entrada, el servicio de
pujas se puede escribir contra `ICurrentUserService` aunque la implementación del JWT
todavía no esté mergeada, así ninguna de las dos queda esperando a la otra.

## 8. Qué endpoints son públicos y cuáles piden token

**Estado:** Aceptada

| Endpoint | Acceso |
|---|---|
| `POST /api/auth/register`, `POST /api/auth/login` | Anónimo |
| `GET /api/auctions`, `GET /api/auctions/{id}` | Anónimo |
| `GET /api/categories` | Anónimo |
| `GET /api/auctions/{id}/bids` | Anónimo, con los usuarios seudonimizados |
| `GET /api/auth/me` | Autenticado |
| `POST /api/auctions` | Autenticado |
| `POST /api/auctions/{id}/bids` | Autenticado |
| `GET /api/wallet/balance`, `POST /api/wallet/deposit`, `GET /api/wallet/transactions` | Autenticado |
| `GET /api/me/auctions`, `GET /api/me/purchases`, `GET /api/me/bids` | Autenticado |

El catálogo queda público a propósito: es la vitrina del sitio y pedir login para mirar
espantaría a cualquier visitante. El historial de ofertas también es público, pero mostrando
seudónimos en lugar de los datos reales de los postores.

Swagger se configura con el botón **Authorize** para poder pegar el token y probar los
endpoints protegidos desde la propia documentación, sin necesidad de Postman ni de curl.

`GET /api/auth/me` existe para que el frontend pueda verificar si el token que tiene guardado
sigue vigente antes de mostrar la sesión como activa.

## 9. Convenciones REST

**Estado:** Aceptada

Sustantivos en plural y en minúscula, jerarquías de recursos y cero verbos en las URLs. El
recurso anidado va bajo su padre: `POST /api/auctions/{id}/bids`.

Filtrar y ordenar son query strings sobre la colección (`GET /api/auctions?status=active`),
no rutas nuevas. Nada de `/api/auctions/active` ni `/api/auctions/filter`, porque cada filtro
nuevo agregaría un endpoint y la API se llenaría de rutas que devuelven lo mismo con otra
forma.

Cerrar una subasta tampoco es un endpoint: lo hace el worker cuando vence el plazo, y no hay
manera de forzarlo desde afuera.

Códigos de estado que usamos: 200 para lecturas, 201 con `Location` al crear, 400 para
validación, 401 sin token o con token inválido, 403 cuando hay token pero no permiso sobre
el recurso, 404 si no existe, y 409 para conflictos de concurrencia o de estado.

## 10. El escrow va en una transacción explícita

**Estado:** Aceptada

Registrar una puja implica retener el saldo del nuevo postor, liberar la retención del
anterior, crear la puja, actualizar la subasta y escribir los asientos del ledger. Todo eso
va dentro de un `BeginTransactionAsync` explícito, con commit al final y rollback ante
cualquier fallo.

Lo que hay que evitar es que retener y liberar abran cada una su propia transacción. Si eso
pasa y algo falla después de retener, el dinero del postor queda congelado sin que exista la
puja que lo justifique. No es un problema teórico: es plata bloqueada en una billetera, sin
forma de liberarla salvo a mano.

También podríamos apoyarnos en la transacción implícita que EF Core arma alrededor de un
único `SaveChangesAsync`, que técnicamente cubre el caso. Preferimos la transacción explícita
porque el flujo además escribe auditoría y porque deja la intención visible en el código
para quien lo lea después.

## 11. Anti-sniping

**Estado:** Aceptada

Si una puja válida entra cuando faltan 60 segundos o menos para el cierre, `EndsAt` se
extiende 2 minutos. Los dos umbrales van como constantes con nombre, no como números
sueltos en el medio de un `if`.

La extensión se registra en auditoría como un evento propio (`AuctionExtended`) y no como un
campo adentro del log de la puja, para poder consultar por separado cuántas veces se estiró
una subasta y quién la gatilló.

## 12. Worker de cierre

**Estado:** Aceptada

Un `BackgroundService` revisa periódicamente las subastas activas con `EndsAt` vencido. Con
ganador, la pasa a `Finished`, transfiere el saldo retenido del comprador al vendedor y
registra la venta. Sin ofertas, la pasa a `Deserted`.

El worker resuelve su trabajo llamando a los servicios de aplicación y no consultando el
`DbContext` por su cuenta. Si accediera directo a la base, la lógica de liquidación quedaría
duplicada en dos lugares y tendríamos que acordarnos de cambiar los dos cada vez.

Un cuidado que nos anotamos: el worker tiene que tolerar que su ciclo se solape con una puja
de último segundo, porque los dos van a competir por la misma fila de `Auctions`. Ahí es
donde el `RowVersion` de la decisión 4 se gana el sueldo.

## 13. Auditoría

**Estado:** Aceptada

La tabla de auditoría es append-only: se inserta y nunca se actualiza ni se borra. Los
eventos que registramos son:

- Cambios de estado de subastas, indicando si los produjo el worker o una acción de usuario.
- Extensiones por anti-sniping.
- Pujas rechazadas, tanto por conflicto de concurrencia como por validación de negocio.
- Acreditaciones de saldo en la billetera.

Auditar las pujas rechazadas es tan importante como las aceptadas: sin eso no hay forma de
demostrar que el control de concurrencia funcionó, porque el rechazo no deja rastro en
ninguna otra tabla.

## 14. Tiempo real con SignalR

**Estado:** Aceptada

La sala de subasta en vivo se sincroniza con SignalR sobre WebSockets. La alternativa es
consultar la API cada 2 o 3 segundos desde el frontend, que funciona pero genera tráfico
constante incluso cuando no pasa nada y siempre muestra información con unos segundos de
atraso.

Quedó cableado de verdad: hub en `/hubs/auctions`, `AddSignalR` y `MapHub` en `Program.cs`,
`IAuctionNotifier` inyectado y llamado al pujar, al extender por anti-sniping, al activar
y al cerrar. El cliente entra a un grupo con `JoinAuction(auctionId)` y recibe
`auctionEvent`. El catálogo y el detalle siguen sirviendo para polling: si el WebSocket
cae, el frontend puede seguir preguntando cada 2 o 3 segundos sin otro endpoint.

## 15. Datos semilla

**Estado:** Aceptada

El seeder carga los cuatro usuarios con sus billeteras y los montos definidos, las cuatro
categorías, cinco subastas que cubren los casos de prueba (activa estándar, activa por
cerrar, próxima, vencida con ganador y vencida desierta), el historial de las ofertas
previas y los asientos del ledger que respaldan los depósitos y el saldo retenido.

Dos reglas propias sobre el seeder.

Es **idempotente**: verifica si ya hay datos y no duplica. Nunca borra la base. Un seeder que
arranca limpiando todo hace que cualquiera pierda lo que haya cargado a mano para probar
algo, apenas reinicie el proyecto.

Las dos subastas vencidas se siembran con la fecha de cierre pasada pero **todavía en
estado `Active`**, no en su estado final. Es a propósito: así el worker tiene algo real que
cerrar en su primer ciclo y se puede ver que la liquidación funciona. Si las dejáramos ya
cerradas, el worker no tendría nada que hacer al levantar el proyecto y no habría forma de
demostrar que anda sin esperar a que venza una subasta de verdad.

Los saldos del seed están calculados para que cierren con las retenciones. `comprador1`
tiene $45.000 retenidos porque lidera la subasta activa, y `sinfondos` tiene sus $500
retenidos porque es el ganador pendiente de la subasta vencida, lo que además lo deja con
saldo disponible en cero y sirve para probar el rechazo de puja por fondos insuficientes.
Cada retención tiene su asiento en el ledger que la explica.

La contraseña de los usuarios de prueba se documenta en el README.

## 16. Frontend

**Estado:** Aceptada

Vamos con **React y Vite** en `frontend/`, separado de la API. La sala en vivo cambia
todo el tiempo (temporizador, historial, liderazgo) y conviene que sean componentes con
estado. El backend ya deja CORS abierto a `localhost` / `127.0.0.1` con credenciales y el
hub SignalR está en `/hubs/auctions`.

Descartamos vanilla en `wwwroot` porque la sala se vuelve un lío de DOM a mano y las dos
terminaríamos pisándonos en los mismos archivos.

## 17. Flujo de ramas

**Estado:** Aceptada

```
master          ← solo entregas, vía PR desde master_dev
  └─ master_dev ← integración
       ├─ master_roxana  → PR a master_dev
       └─ master_dayana  → PR a master_dev
```

También se puede abrir una `feature/<tema>` desde `master_dev` y mandar el PR a
`master_dev`. Lo que no hacemos es commitear directo en `master` ni en `master_dev`.

Nunca commiteamos directo en `master`. Todo entra por Pull Request, incluso los cambios
chicos, y quien no escribió el código lo revisa antes de mergear.

Los mensajes de commit describen el porqué del cambio y no el archivo que se tocó: el
archivo ya se ve en el diff.

## 18. README con la prueba de concurrencia

**Estado:** Aceptada

El README lleva los pasos completos de build, configuración de la base, migraciones y
ejecución, más las credenciales de los usuarios de prueba y la URL de Swagger.

Incluye además el **stress test de concurrencia**: `scripts/concurrency-bid-test.ps1`
dispara dos pujas idénticas en simultáneo sobre la misma subasta. Lo esperado es un 201 y
un 409. El comando está en el README.

Sin ese script, el control de concurrencia es una afirmación sin respaldo. Con él, queda
demostrado y es reproducible por cualquiera que clone el repositorio.

## 19. Alta de una subasta

**Estado:** Aceptada

`POST /api/auctions` pide token. El vendedor no viaja en el body: sale de
`ICurrentUserService`. Así nadie puede publicar a nombre de otra persona.

`StartsAt` es opcional. Si no viene, la subasta abre en el momento y nace `Active`. Si viene
a futuro, nace `Scheduled`. `EndsAt` es obligatorio y tiene que ser posterior al inicio.

Validamos en el servicio, no solo con atributos del DTO, para que el mensaje de error
explique la regla y no un código de validación genérico:

- El precio base y el incremento mínimo tienen que ser mayores a cero, con como máximo dos
  decimales, porque la base los guarda con esa precisión.
- La fecha de inicio no puede estar en el pasado, con un margen de 2 minutos para el
  desfasaje de reloj entre quien publica y el servidor.
- La duración mínima es de 5 minutos y la máxima de 30 días. Una subasta de 30 segundos no
  se puede pujar en la práctica, y una de meses deja plata retenida sin sentido.
- La categoría tiene que existir.
- Si mandan imagen, tiene que ser una URL `http` o `https`. Si no mandan, se guarda vacía.

Todas las fechas se persisten y se leen como UTC. `datetime2` no guarda zona horaria, así
que un converter de EF Core marca `Kind = Utc` al leer. Sin eso el JSON sale sin la `Z` y el
frontend toma la fecha de cierre como hora local: el contador queda corrido según el huso
de quien mira.

## 20. Estado actual

**Hecho en backend**

- Capas, Code-First, JWT, seeder, catálogo, alta, categorías.
- Billetera (saldo, depósito, movimientos), pujas con escrow, 409, anti-sniping.
- Worker de cierre y de activación, auditoría, historial de pujas seudonimizado.
- Script 201/409, Mis publicaciones / compras / pujas, SignalR cableado, CORS de localhost.

**Pendiente**

- Decidir el stack de frontend (decisión 16) y construir las pantallas.
- Consumir el hub o, si hace falta, caer a polling sobre los GET que ya existen.
