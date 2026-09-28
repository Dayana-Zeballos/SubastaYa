# Frontend SubastaYa

React + Vite. En Development el proxy manda `/api` y `/hubs` a `http://localhost:5240`,
así el front no hardcodea el origen y SignalR usa el mismo host que Vite.

## Cómo correr

Con la API levantada (`dotnet run` en `backend/SubastaYa.API`):

```bash
cd frontend
npm install
npm run dev
```

Queda en `http://localhost:5173`.

Usuarios de prueba (contraseña `Subasta2026!`): `vendedor@test.com`,
`comprador1@test.com`, `comprador2@test.com`, `sinfondos@test.com`.

## Rutas

| Ruta | Quién entra | Qué hace |
|---|---|---|
| `/` | Todo el mundo | Catálogo: abiertas, próximas y cerradas |
| `/ingresar`, `/registro` | Todo el mundo | Sesión con JWT |
| `/subastas/:id` | Todo el mundo | Sala: contador, historial y ofertas |
| `/publicar` | Con token | Alta de un lote |
| `/billetera` | Con token | Saldo, cargar plata y movimientos |
| `/actividad` | Con token | Publicaciones, ofertas y compras |
| `/como-funciona` | Todo el mundo | Cómo se publica, se oferta y se retiene la plata |

## Qué hay armado

- Layout con menú, pie de página (UNAJ) y sesión (`localStorage` + `GET /api/auth/me`).
- Cliente HTTP que manda el Bearer y lee `ProblemDetails`.
- Consola de puja: saldo, rechazo al vendedor y retención al ofertar.
- SignalR en la sala (`JoinAuction` + `auctionEvent`). Si cae el WebSocket, se recarga
  con los GET de detalle e historial.
