# AGENTS.md — Arquitectura

## Solution Structure (modular monolith, .NET 11)

```
Arquitectura.slnx
├── Backend (.NET 11)
│   ├── Arquitectura.Host                  # Single entry point: middleware, DI composition, endpoint maps
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks.Abstractions    # Mediator contracts, IDbConnectionFactory, IUnitOfWork,
│   │   │                                  # IDistributedCacheService, ICurrentUser, Entity, Specification
│   │   ├── BuildingBlocks.AspNetCore      # Result → RFC 9457 ProblemDetails mapping
│   │   └── BuildingBlocks.Infrastructure  # Mediator, behaviors, Dapper/Redis primitives, health checks
│   ├── Modules/Auth/                      # Modules.Auth + .Api + .Infrastructure
│   ├── Modules/Weather/                   # Modules.Weather + .Api + .Infrastructure
│   ├── Modules/DataTable/                 # Modules.DataTable + .Api + .Infrastructure
│   ├── Arquitectura.ServiceDefaults       # OpenTelemetry, Serilog, health checks, service discovery
│   ├── Arquitectura.Tests                 # xUnit v3: unit, architecture, endpoint
│   └── Arquitectura.Database              # SQL Server project (tables + stored procedures)
├── Frontend (React 19)
│   └── Arquitectura.Client                # Vite + React 19 + Zustand + TanStack Query
└── Arquitectura.AppHost                   # Aspire orchestration → Arquitectura.Host
```

The legacy layered projects (`Arquitectura.Api`, `Arquitectura.Application`, `Arquitectura.Domain`,
`Arquitectura.Infrastructure`, `Arquitectura.Persistence`) were removed. The solution only contains the
modular monolith above.

## Developer Commands

### Backend
```powershell
dotnet build Arquitectura.Cli.slnf                # CLI build (excludes SQL project + esproj)
dotnet build Arquitectura.Host/Arquitectura.Host.csproj
dotnet run --project Arquitectura.Host           # https://localhost:44358, Scalar at /scalar
dotnet run --project Arquitectura.AppHost        # Aspire dashboard, orchestrates Host + Client
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

## Build & Packages

- `Directory.Build.props` centralizes `TargetFramework` (net11.0), `Nullable`, `ImplicitUsings`,
  `LangVersion` and deterministic builds for every C# project.
- `Directory.Packages.props` enables **Central Package Management**. Never add a `Version` to a
  `PackageReference`; update the version in the central file instead.
- `global.json` pins SDK `11.0.100` and enables the `Microsoft.Testing.Platform` runner (xunit v3 4.x).
- `Arquitectura.slnx` groups projects into solution folders (`Backend/Host`, `Backend/BuildingBlocks`,
  `Backend/Modules/*`, `Backend/Tests`, `Infrastructure`, `Frontend`). This is Visual Studio
  organization only; it does not move DLLs or change deployment.

## Architecture Rules

1. **One entry point**: only `Arquitectura.Host` composes modules. It may use `Modules.*.Contracts.*`,
   `Modules.*.Api` (`MapXEndpoints`), the `AddXModule` DI extensions and the module `Infrastructure`
   project for composition only.
2. **Contract boundary**: a module exposes DTOs and commands/queries in `Modules.X.Contracts` and its
   Minimal API map in `Modules.X.Api`. Other modules and the Host must not touch a module's `Domain`,
   `Features` or `Persistence`.
3. **Minimal API**: endpoints live in `Modules.X.Api`, are grouped with `MapGroup`, and use
   `BuildingBlocks.AspNetCore` (`ResultStatus`, `ToHttpResult`, `ToNoContentResult`); errors are RFC 9457 ProblemDetails. Do not
   reintroduce MVC controllers.
4. **No shared domain**: each module owns its entities and repositories. No cross-module entity reuse.
5. **No circular dependencies**: modules never reference each other; communication is through contracts
   or `BuildingBlocks` abstractions.
6. **BuildingBlocks is business-free**: `Abstractions` has no package dependencies beyond ResulTus;
   `Infrastructure` never references a module.
7. **Results**: use the full ResulTus suite. Errors come from generated `[ResultErrors]` factories, not
   inline `Error.X("CODE", ...)`. HTTP bodies are RFC 9457 ProblemDetails (`detail`, `code`, validation
   `errors`). Do not reintroduce a custom result type or error body.
8. **Caching**: explicit read-through via `QueryCachingExtensions.GetOrSetAsync`; never cache
   non-round-trippable results.
9. **Resilience**: only idempotent requests implement `IResilientRequest`.
10. **Configuration**: Options-based, no hardcoded secrets. `Jwt:Key` is mandatory.
11. **API docs**: OpenAPI at `/openapi/v1.json`, Scalar UI at `/scalar`. Do not reintroduce Swashbuckle.
12. **ROP in application code**: handlers compose with `Bind`/`Ensure`/`Map`/`MapError`; repositories
    use `Result.TryAsync`, never `try/catch`. The only exception is `ValidationBehavior` (structured
    validation errors); `CurrentUser` exposes `Result<int> UserIdResult`.

## Persistence

- Dapper + stored procedures; each module owns its repository implementations in
  `Modules.X.Infrastructure/Persistence`.
- Stored procedures in use:
  - Auth: `sp_User_GetByEmail`, `sp_User_GetById`, `sp_User_Create`, `sp_User_Update`, `sp_User_Exists`
  - Weather: `dbo.WeatherForecast_GetAll|GetById|Create|Update|Delete`
  - DataTable: `sp_GetTableConfig`, `sp_SaveUserTableConfig`, `sp_GetTableData`
- **Known gap**: none. `sp_DeleteUserTableConfig` records the removed preferences in the audit history
  and deletes them (see `TECHNICAL_DEBT.md` for the remaining minor items).

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
The frontend defaults to `API_BASE = http://localhost:8080` and reads `detail`/`errors` through `src/lib/problem.ts`.
HTTP contracts are frozen: see `ENDPOINT_INVENTORY.md`.

## Known Technical Debt

Tracked in `TECHNICAL_DEBT.md`: lazy rehash of legacy password hashes, `Newtonsoft.Json` in the Redis
cache, a table-specific database health check, public health endpoints, and database error details exposed in problem responses.
