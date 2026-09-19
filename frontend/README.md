# Frontend SubastaYa

React + Vite. Habla con la API por el proxy de Development (`/api` y `/hubs` → `http://localhost:5240`).

## Cómo correr

Con la API levantada (`dotnet run` en `backend/SubastaYa.API`):

```bash
cd frontend
npm install
npm run dev
```

Queda en `http://localhost:5173`.

Usuarios de prueba (contraseña `Subasta2026!`): `vendedor@test.com`, `comprador1@test.com`, `comprador2@test.com`.

## Qué hay armado

- Router y layout (catálogo, publicar, sala, billetera, actividad).
- Login / registro y sesión con JWT (`localStorage` + `GET /api/auth/me`).
- Cliente HTTP que manda el Bearer y lee `ProblemDetails`.
- Helper de SignalR en `src/realtime/auctionHub.js` (`JoinAuction` + `auctionEvent`).

Las pantallas de negocio están como placeholders para repartir después.
