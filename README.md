# SubastaYa

Proyecto de Software — UNAJ (2º cuatrimestre 2026)

Plataforma de subastas en tiempo real con billetera (escrow), anti-sniping, worker de cierre y auditoría.

## Stack

- **Backend:** C# / ASP.NET Core 9 + Entity Framework Core (Code-First)
- **Frontend:** (pendiente)
- **Arquitectura:** Domain → Application → Infrastructure → API

## Estructura

```
backend/
  SubastaYa.Domain/          # Entidades
  SubastaYa.Application/     # Servicios e interfaces
  SubastaYa.Infrastructure/  # EF Core, repositorios, seed
  SubastaYa.API/             # Controllers, Swagger, Program.cs
frontend/
```

## Cómo correr (backend)

```bash
dotnet tool restore
cd backend/SubastaYa.API
dotnet run
```

En Development la API aplica las migraciones pendientes y carga los datos de prueba sola al
arrancar, así que no hace falta correr `dotnet ef database update` a mano.

Swagger: `http://localhost:5240` redirige a `/swagger`.

## Usuarios de prueba

Todos comparten la contraseña **`Subasta2026!`**.

| Email | Total | Retenido | Disponible | Para qué sirve |
|---|---|---|---|---|
| `vendedor@test.com` | $0 | $0 | $0 | Publica las subastas |
| `comprador1@test.com` | $150.000 | $45.000 | $105.000 | Lidera la subasta activa |
| `comprador2@test.com` | $200.000 | $0 | $200.000 | Puede pujar sin restricciones |
| `sinfondos@test.com` | $500 | $500 | $0 | Probar el rechazo por fondos insuficientes |

Para obtener un token: `POST /api/auth/login` con el email y la contraseña, y después pegar
el token en el botón **Authorize** de Swagger.

## API lista para el frontend

| Uso | Endpoint |
|---|---|
| Sesión | `POST /api/auth/login`, `GET /api/auth/me` |
| Catálogo | `GET /api/auctions`, `GET /api/auctions/{id}`, `GET /api/categories` |
| Historial | `GET /api/auctions/{id}/bids` |
| Publicar / pujar | `POST /api/auctions`, `POST /api/auctions/{id}/bids` |
| Billetera | `GET /api/wallet/balance`, `POST /api/wallet/deposit`, `GET /api/wallet/transactions` |
| Mi cuenta | `GET /api/me/auctions`, `GET /api/me/purchases`, `GET /api/me/bids` |

Tiempo real: hub SignalR en `/hubs/auctions`. El cliente llama `JoinAuction(auctionId)` y
escucha el evento `auctionEvent` (`bidPlaced`, `auctionExtended`, `auctionActivated`,
`auctionClosed`). Si el WebSocket no está, los GET de detalle e historial sirven para
polling. En Development la API acepta CORS desde cualquier `localhost`.

## Prueba de concurrencia (201 / 409)

Con la API corriendo (`dotnet run` en `backend/SubastaYa.API`), desde la raíz del repo:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\concurrency-bid-test.ps1
```

El script:

1. Hace login como `comprador2@test.com`
2. Toma una subasta activa del catálogo
3. Dispara **dos** `POST /api/auctions/{id}/bids` idénticos en paralelo
4. Espera ver **un 201** y **un 409** (conflicto de `RowVersion`)

Ejemplo de salida esperada:

```text
Resultados: 201, 409
OK: una puja entro (201) y la otra fue rechazada por concurrencia (409).
```

Si ambas dan 201 o ambas fallan, reiniciá la API (seed fresco) y volvé a correr el script
sin otras pujas manuales en el medio.

## Equipo

-Zeballos Dayana usuario de github (Dayana-Zeballos)
-Zeballos Roxana  usuario de github (RoxanaZeballos)
