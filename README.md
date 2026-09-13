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

## Equipo

-Zeballos Dayana usuario de github (Dayana-Zeballos)
-Zeballos Roxana  usuario de github (RoxanaZeballos)
