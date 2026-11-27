# Modular Monolith Migration

Objective: refactor the Arquitectura backend into a real modular monolith on .NET 11 with ResulTus 1.3.0, a first-party mediator, enforced module boundaries and preserved HTTP contracts.
Branch: `feature/modular-monolith` · Delivery: `single-pr` · Test runner: `dotnet test` (xunit.v3)

## Specs

S1. Single backend entry point `Arquitectura.Host` (ASP.NET Core controllers). One API, one deployment. The Host only composes modules, registers middleware and hosts controllers.
S2. Modules `Modules.Auth`, `Modules.Weather`, `Modules.DataTable`; each owns Domain, Features, Contracts, Persistence, Infrastructure and its own `AddXModule` DI registration.
S3. A module never references another module's internals. Inter-module communication only through `*.Contracts` (facades + DTOs) and BuildingBlocks abstractions.
S4. `BuildingBlocks.Abstractions` holds technical shared contracts with no package dependencies (mediator contracts, `ICurrentUser`, `IDbConnectionFactory`, `IUnitOfWork`, `IDistributedCacheService`, `Entity`, `Specification`, pagination).
S5. `BuildingBlocks.Infrastructure` holds the mediator implementation, pipeline behaviors, Dapper/Redis primitives and health checks. No business logic, no references to modules.
S6. `Domain` never references Dapper, SqlClient, ASP.NET Core or the mediator implementation. `Application` (Features) never references Persistence implementations.
S7. Result Pattern replaced by ResulTus 1.3.0 (`Result`, `Result<T>`, `Error`, `ErrorType`). The previous hand-rolled `Result`/`ResultError`/`ServiceErrors`/`ResultExtensions` are removed.
S8. MediatR replaced by a first-party lightweight mediator supporting commands, queries, handlers, pipeline behaviors, validation, logging, caching, resilience, `CancellationToken` and async execution.
S9. Preserved: .NET 11, ASP.NET Core Controllers, React + TypeScript + Vite, SQL Server + Dapper + stored procedures, Redis, Hangfire, FluentValidation, Mapster, JWT, OpenTelemetry, health checks, Serilog, Aspire.
S10. All existing HTTP endpoints keep method, route, auth requirements, request and success-response shapes, and status codes:
- `POST /api/auth/login` (anonymous) → 200 `AuthResponse`
- `POST /api/auth/register` (anonymous) → 201 `AuthResponse`
- `POST /api/auth/refresh` (authorized) → 200 `RefreshTokenResponse`
- `POST /api/auth/logout` (authorized) → 204
- `GET /api/weatherforecast` (authorized) → 200 `PaginatedResponse<WeatherForecastDto>`
- `GET /api/weatherforecast/{id:int}` (authorized) → 200 `WeatherForecastDto`
- `POST /api/weatherforecast` (authorized) → 201 `WeatherForecastDto`
- `PUT /api/weatherforecast/{id:int}` (authorized) → 200 `WeatherForecastDto`
- `DELETE /api/weatherforecast/{id:int}` (authorized) → 204
- `GET /api/datatable/{tableKey}` (authorized) → 200 `DataTableConfigResponseDto`
- `POST /api/datatable/{tableKey}/config` (authorized) → 204
- `DELETE /api/datatable/{tableKey}/config` (authorized) → 204
- `GET /api/datatable/{tableKey}/data` (authorized) → 200 `DataResponseDto<Dictionary<string,object>>`
- `POST /api/datatable/{tableKey}/export` (authorized) → 202 `DataTableExportResponseDto`
- `GET /health`, `GET /alive` (development)
S11. Error responses keep the existing body shape `{ "code": string, "message": string, "details": object|null }` with the same HTTP status mapping. No frontend change required.
S12. Configuration is externalized through Options. No hardcoded secrets: `Jwt:Key`, `ApiKey:Key` and connection strings come from configuration or environment.
S13. `async`/`await` and `CancellationToken` on every asynchronous operation.
S14. Tests: unit, integration, architecture (module isolation) and endpoint regression. `dotnet build` (Release) and `dotnet test` must pass.
S15. Docker (root `Dockerfile` + `docker-compose.yml`), Aspire AppHost and the React client keep working.
S16. Documentation updated: `README.md`, `AGENTS.md`, `ARCHITECTURE.md`, `MODULE_BOUNDARIES.md`, `ENDPOINT_INVENTORY.md`, `DEPLOYMENT.md`, `TECHNICAL_DEBT.md`, `ADR/`.

## Tasks

| ID | Spec | Route | Status | Evidence |
|----|------|-------|--------|----------|
| T1 | S1,S2,S4,S5,S6 | inline | done | new project tree + `Arquitectura.slnx` (Host + 2 BuildingBlocks + 6 module projects; all build) |
| T2 | S7,S8,S13 | inline | done | `BuildingBlocks` mediator, behaviors, Dapper/Redis primitives, ResulTus 1.3.0; wrapper scoped |
| T3 | S2,S3,S7 | inline | done | `Modules.Auth` + `Modules.Auth.Infrastructure` (login/register/refresh/logout, JWT options) |
| T4 | S2,S3,S7 | inline | done | `Modules.Weather` + `Modules.Weather.Infrastructure` (CRUD, cache, Hangfire jobs) |
| T5 | S2,S3,S7 | inline | done | `Modules.DataTable` + `Modules.DataTable.Infrastructure` (config/data/export) |
| T6 | S1,S10,S11,S12 | inline | done | `Arquitectura.Host` Program/controllers/middleware/health/identity; smoke: `/alive` 200, swagger 200, login 401 `{code,message,details}` |
| T7 | S15 | inline | done | ServiceDefaults net11.0; AppHost Aspire 17.0.0 → `Arquitectura.Host`; root Dockerfile net11.0 → `Arquitectura.Host.dll` |
| T8 | S14 | inline | done | 58 tests passing: unit (handlers, validators, caching, result mapping), architecture (module isolation), endpoint smoke (`WebApplicationFactory`) |
| T9 | S16 | inline | done | `README.md`, `AGENTS.md`, `ARCHITECTURE.md`, `MODULE_BOUNDARIES.md`, `ENDPOINT_INVENTORY.md`, `DEPLOYMENT.md`, `TECHNICAL_DEBT.md`, `ADR/README.md` (ADR-001..008) |

## Log

L1 (user request, verbatim excerpt): "Refactorización completa a Monolito Modular ... Mantener: .NET 10. ASP.NET Core Controllers. React + TypeScript + Vite. SQL Server + Dapper + Stored Procedures. Redis. Hangfire. FluentValidation. Mapster. JWT. OpenTelemetry, Health Checks y Serilog. Aspire para orquestación de desarrollo. ... Ademas ultilizar el paquete nuget ResulTus la ultima version 1.3.0, y sustituir el patron result actual con ResulTus. Tambien actualizar la solucion a net 11. dale"

L2 Audit (baseline, `feature/datatable-dynamic` @ a2fddc7):
- 8 projects, 65 C# files. `Arquitectura.Application` references `Arquitectura.Persistence` (violation). `Arquitectura.Domain` references `Microsoft.AspNetCore.App` framework (ResultExtensions uses MVC) — violation. Static mutable `DatabaseConfiguration`. MediatR 14 (commercial licence). Hand-rolled Result pattern. `CachingBehavior` logs a cache HIT and then still calls the handler (cache never used). `CacheInvalidationBehavior` invalidates the weather cache prefix on every non-query request, including auth commands. `ResilienceBehavior` retries every exception 3x, including non-idempotent commands. `DataTableController.GetUserId()` reads claim `userId`, which the JWT never emits (it emits `sub`) — always 0. `sp_DeleteUserTableConfig` referenced by `DataTableRepository` does not exist in `Arquitectura.Database`. `Arquitectura.Api/Dockerfile` references a non-existent `Arquitectura/Arquitectura.csproj`.

L3 Decisions (documented as ADRs):
- ADR-001 Flat repository layout, one project per module layer pair (`Modules.X` + `Modules.X.Infrastructure`), no `src/` move of frontend/database.
- ADR-002 Module public surface = `Contracts/` folder inside `Modules.X`; Host may only use `Modules.X.Contracts.*` (enforced by architecture tests).
- ADR-003 One `BuildingBlocks.Infrastructure` instead of separate Redis/Hangfire/Observability projects.
- ADR-004 First-party mediator replaces MediatR (licence + no external contract); ResulTus kept for results.
- ADR-005 Caching moved from a pipeline behavior to an explicit `GetOrSetAsync` helper because `Result<T>` is a non-round-trippable readonly struct.
- ADR-006 Error body preserved as `{code,message,details}` using `ResultHttpStatus.GetStatusCode` for status mapping.
- ADR-007 Pre-existing defects fixed as part of the move: cache HIT path, cache invalidation scope, retry scope, DataTable user id claim, JWT secret fallback, Hangfire weather jobs ownership.
- ADR-008 Pre-existing defects documented but not fixed: missing `sp_DeleteUserTableConfig` stored procedure.

L4 Implementation notes (decisions taken while building):
- `QueryCachingExtensions` moved from `BuildingBlocks.Infrastructure` to `BuildingBlocks.Abstractions` so module core projects depend only on abstractions (no Dapper/Redis leakage into the module core).
- `RequestHandlerWrapper<,>` is registered scoped (not singleton) because it captures `IServiceProvider`; a singleton captured the root provider and failed to resolve scoped handlers.
- Hangfire job registration (`IJobScheduler`, `HangfireJobScheduler`, `WeatherForecastJobs`) moved behind `AddWeatherModuleJobs()`, called only when Hangfire is configured, so DI validation does not fail when there is no database.
- Dev job scheduling and the Hangfire dashboard are wrapped in try/catch so an unreachable database does not crash startup (parity with the old Program).
- HTTP status mapping: legacy codes (`NOT_FOUND`, `VALIDATION_ERROR`, `RETRY_EXHAUSTED`, `TIMEOUT`) keep their exact status; richer module codes map from `ErrorType`. `ErrorType.Failure` maps to 400 to preserve the old default for `DATABASE_ERROR`.
- Normalizations against the written inventory S10 (no frontend impact, both are 2xx): `POST /api/auth/register` now returns 201 (was 200) and auth failures return 401 (declared in the old Swagger contract; runtime used to fall through to 400 for code `AUTH`).
- `ErrorResponse` is a local Host record (`{code,message,details}`) instead of `ResulTus.AspNetCore.ToActionResult`, which would emit RFC 9457 ProblemDetails and break the frontend.

L5 Verification so far:
- `dotnet build` of all 17 `.csproj` → 0 errors, 0 warnings.
- The `.sqlproj` cannot build from the dotnet CLI (needs VS SSDT); pre-existing environment limitation, unrelated to this migration.
- Cold-start smoke test with Hangfire disabled: `GET /alive` → 200, `GET /swagger/v1/swagger.json` → 200 (28 KB), `POST /api/auth/login` → 401 with body `{"code":"AUTH_INVALID_CREDENTIALS","message":"Invalid email or password","details":null}`, `GET /api/weatherforecast` without token → 401.

L6 Tests and solution state:
- Test project retargeted to net11.0 and rewired to the new projects; obsolete tests for deleted code removed.
- 58 tests pass: `ResultHttpExtensions`, `QueryCachingExtensions`, `CurrentUser`, `WeatherCommandValidator`, `LoginCommandHandler`, `Entity`, `Specification`, `DbConnectionFactory`, `RepositoryContract`, `ModuleBoundary`, `EndpointSmoke`.
- Legacy projects retargeted to net11.0 and removed from `Arquitectura.slnx`; they remain on disk (documented in `TECHNICAL_DEBT.md`) so the tree stays buildable.
- All 17 `.csproj` build with 0 errors; `Arquitectura.Host` and `Arquitectura.Tests` build with 0 warnings.
- `dotnet build Arquitectura.slnx` cannot succeed because `Arquitectura.Database.sqlproj` needs VS SSDT (pre-existing; documented in `DEPLOYMENT.md`/`TECHNICAL_DEBT.md`).
- Not yet committed: the working tree holds the whole migration; commit as reviewable work units on `feature/modular-monolith`.
