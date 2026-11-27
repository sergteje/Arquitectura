# AGENTS.md — Arquitectura

## Solution Structure (modular monolith, .NET 11)

```
Arquitectura.slnx
├── Backend (.NET 11)
│   ├── Arquitectura.Host                  # Single entry point: controllers, middleware, DI composition
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks.Abstractions    # Mediator contracts, IDbConnectionFactory, IUnitOfWork,
│   │   │                                  # IDistributedCacheService, ICurrentUser, Entity, Specification
│   │   └── BuildingBlocks.Infrastructure  # Mediator, behaviors, Dapper/Redis primitives, health checks
│   ├── Modules/Auth/                      # Modules.Auth + Modules.Auth.Infrastructure
│   ├── Modules/Weather/                   # Modules.Weather + Modules.Weather.Infrastructure
│   ├── Modules/DataTable/                 # Modules.DataTable + Modules.DataTable.Infrastructure
│   ├── Arquitectura.ServiceDefaults       # OpenTelemetry, Serilog, health checks, service discovery
│   ├── Arquitectura.Tests                 # xUnit v3: unit, architecture, endpoint
│   └── Arquitectura.Database              # SQL Server project (tables + stored procedures)
├── Frontend (React 19)
│   └── Arquitectura.Client                # Vite + React 19 + Zustand + TanStack Query
└── Arquitectura.AppHost                   # Aspire orchestration → Arquitectura.Host
```

Legacy projects (`Arquitectura.Api`, `Arquitectura.Application`, `Arquitectura.Domain`,
`Arquitectura.Infrastructure`, `Arquitectura.Persistence`) still exist on disk but are **out of the
solution**. They are superseded by the tree above and can be deleted once the migration is signed off.

## Developer Commands

### Backend
```powershell
dotnet build Arquitectura.Host/Arquitectura.Host.csproj
dotnet run --project Arquitectura.Host          # https://localhost:44358, Scalar at /scalar
dotnet run --project Arquitectura.AppHost       # Aspire dashboard, orchestrates Host + Client
```

### Frontend
```powershell
cd Arquitectura.Client
npm run dev      # Vite dev server, port 5173
npm run build    # TypeScript build + Vite production
npm run lint     # ESLint
```

### Testing
```powershell
dotnet test Arquitectura.Tests/Arquitectura.Tests.csproj
./test-api.ps1   # assumes HTTPS localhost:44358 running
./test-auth.ps1
cd Arquitectura.Client; npm run test:e2e
```

### Docker
```powershell
docker compose up -d   # Host + SQL Server + Redis + observability
```

## Architecture Rules

1. **One entry point**: only `Arquitectura.Host` composes modules. It may use `Modules.*.Contracts.*`,
   the `AddXModule` DI extensions and the module `Infrastructure` project for composition only.
2. **Contract boundary**: a module exposes DTOs and commands/queries in `Modules.X.Contracts`. Other
   modules and the Host must not touch a module's `Domain`, `Features` or `Persistence`.
3. **No shared domain**: each module owns its entities and repositories. No cross-module entity reuse.
4. **No circular dependencies**: modules never reference each other; communication is through contracts
   or `BuildingBlocks` abstractions.
5. **BuildingBlocks is business-free**: `Abstractions` has no package dependencies beyond ResulTus;
   `Infrastructure` never references a module.
6. **Results**: use `ResulTus` (`Result`, `Result<T>`, `Error`, `ErrorType`). Do not reintroduce a
   custom result type. Error bodies stay `{ code, message, details }`.
7. **Caching**: explicit read-through via `QueryCachingExtensions.GetOrSetAsync`; never cache
   non-round-trippable results.
8. **Resilience**: only idempotent requests implement `IResilientRequest`.
9. **Configuration**: Options-based, no hardcoded secrets. `Jwt:Key` is mandatory.
10. **API docs**: OpenAPI at `/openapi/v1.json`, Scalar UI at `/scalar`. Do not reintroduce Swashbuckle.

## Persistence

- Dapper + stored procedures; each module owns its repository implementations in
  `Modules.X.Infrastructure/Persistence`.
- Stored procedures in use:
  - Auth: `sp_User_GetByEmail`, `sp_User_GetById`, `sp_User_Create`, `sp_User_Update`, `sp_User_Exists`
  - Weather: `dbo.WeatherForecast_GetAll|GetById|Create|Update|Delete`
  - DataTable: `sp_GetTableConfig`, `sp_SaveUserTableConfig`, `sp_GetTableData`
- **Known gap**: `sp_DeleteUserTableConfig` is referenced by `DataTableRepository.ResetUserConfigAsync`
  but does not exist yet (see `TECHNICAL_DEBT.md`).

## Infrastructure

| Service | Port | Notes |
|---------|------|-------|
| API (Host) | 8080/8081 | HTTP/HTTPS (container) / 44358 local HTTPS |
| Frontend | 5173 | Vite dev |
| SQL Server | 1433 | Docker (see `.env`) |
| Redis | 6379 | Docker |
| Seq (logs) | 5341 | Docker |
| Jaeger | 16686 | Docker |
| Grafana | 3000 | Docker (admin/admin123) |
| Hangfire | /hangfire | Development only |
| Scalar | /scalar | API reference UI |

## Frontend

```
src/components/  src/pages/  src/lib/constants.ts (API_BASE)
```
The frontend expects `API_BASE = http://localhost:8081` and reads `error.message` from error bodies.
HTTP contracts are frozen: see `ENDPOINT_INVENTORY.md`.

## Known Technical Debt

Tracked in `TECHNICAL_DEBT.md`: missing `sp_DeleteUserTableConfig`, SHA-256 password hashing,
duplicate Redis connections, pre-existing `MessagePack` advisory.
