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
cd backend/SubastaYa.API
dotnet restore
dotnet ef database update
dotnet run
```

Swagger: `https://localhost:7xxx/swagger` (ver puerto en la consola).

## Equipo

-Zeballos Dayana Dayana-Zeballos
-Zeballos Roxana 
