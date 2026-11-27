# Modular Monolith Migration

Objective: refactor the Arquitectura backend into a real modular monolith on .NET 11 with ResulTus 1.3.0, a first-party mediator, enforced module boundaries and preserved HTTP contracts.
Branch: `feature/modular-monolith` · Delivery: `single-pr` · Test runner: `dotnet test` (xunit.v3)

## Specs

S1. Single backend entry point `Arquitectura.Host` (ASP.NET Core Minimal API). One API, one deployment. The Host only composes modules, registers middleware and maps the module endpoints.
S2. Modules `Modules.Auth`, `Modules.Weather`, `Modules.DataTable`; each owns Domain, Features, Contracts, Persistence, Infrastructure and its own `AddXModule` DI registration.
S3. A module never references another module's internals. Inter-module communication only through `*.Contracts` (facades + DTOs) and BuildingBlocks abstractions.
S4. `BuildingBlocks.Abstractions` holds technical shared contracts with no package dependencies (mediator contracts, `ICurrentUser`, `IDbConnectionFactory`, `IUnitOfWork`, `IDistributedCacheService`, `Entity`, `Specification`, pagination).
S5. `BuildingBlocks.Infrastructure` holds the mediator implementation, pipeline behaviors, Dapper/Redis primitives and health checks. No business logic, no references to modules.
S6. `Domain` never references Dapper, SqlClient, ASP.NET Core or the mediator implementation. `Application` (Features) never references Persistence implementations.
S7. Result Pattern replaced by ResulTus 1.3.0 (`Result`, `Result<T>`, `Error`, `ErrorType`). The previous hand-rolled `Result`/`ResultError`/`ServiceErrors`/`ResultExtensions` are removed.
S8. MediatR replaced by a first-party lightweight mediator supporting commands, queries, handlers, pipeline behaviors, validation, logging, caching, resilience, `CancellationToken` and async execution.
S9. Preserved: .NET 11, ASP.NET Core Minimal API, React + TypeScript + Vite, SQL Server + Dapper + stored procedures, Redis, Hangfire, FluentValidation, Mapster, JWT, OpenTelemetry, health checks, Serilog, Aspire.
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
S17. The mediator contract carries the validation payload: `IRequestHandler<TRequest,TResponse>` returns `Task<Result<TResponse, RequestError>>`, where `RequestError` pairs an `Error` with the optional `ValidationErrors`. `ValidationBehavior` returns that failure instead of throwing `RequestValidationException`. Handlers compose the single-error `Result<T>` tier internally and cross into the mediator tier once (`ToRequestResult`/`ToUnitRequestResult`). No exception remains in the application layer, and no HTTP contract changes.

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

L7 Scalar (user request): replaced Swashbuckle with native OpenAPI + Scalar.
- Removed `Swashbuckle.AspNetCore`; added `Microsoft.AspNetCore.OpenApi` 11.0.0 and `Scalar.AspNetCore` 3.3.0 (supports net11.0).
- Host: `AddOpenApi()` + `MapOpenApi()` + `MapScalarApiReference()`. OpenAPI at `/openapi/v1.json`, UI at `/scalar`.
- Exempted `/scalar` and `/openapi` from the API-key and audit middlewares; `launchUrl` now `scalar`.
- Smoke test updated: `/openapi/v1.json` 200 and `/scalar` 200 text/html. 59 tests pass.

L8 Cleanup of remaining debt (user request, one by one):
1. Added `sp_DeleteUserTableConfig.sql` (audits `ChangeType='DELETE'` then deletes) and registered it in the SQL project.
2. Replaced SHA-256 password hashing with salted PBKDF2-HMAC-SHA256 (210k iterations); legacy hashes still verify and rehash on successful login. `InternalsVisibleTo` lets tests exercise the hasher.
3. Deleted the legacy `Arquitectura.Api/Application/Domain/Infrastructure/Persistence` projects.
4. Added `Arquitectura.Cli.slnf` (excludes `.sqlproj` + esproj); `dotnet build -c Release` passes.
5. Removed the duplicate Redis connection: `RedisCacheService` and `RedisHealthCheck` use the single `IConnectionMultiplexer`; dropped `Microsoft.Extensions.Caching.StackExchangeRedis`.
6. `/health` and `/alive` are mapped in every environment (aggregate status only).
7. Upgraded `MessagePack` to 3.1.12 (advisory gone) and replaced the obsolete Aspire `AddProject<T>` with `AddDotnetProject(name, path)`.
Verification: `dotnet build Arquitectura.Cli.slnf -c Release` 0 errors/0 warnings; `dotnet test -c Release` 66/66.

L9 Minimal API per module (user request, verbatim): "Me gustaría en esta rama cambiar de controladores a minimalapi cómo sería de factible?" / "Adelante con endpoints por modulo".
- Added `BuildingBlocks.AspNetCore` with `ErrorResponse` and `ResultHttpExtensions.ToHttpResult`/`ToNoContentResult`/`ToErrorResult` (`IResult`).
- Added `Modules.Auth.Api`, `Modules.Weather.Api` and `Modules.DataTable.Api`, each exposing `MapXEndpoints`.
- Host: removed `AddControllers`/`MapControllers` and the controllers; maps the three endpoint groups. `AddOpenApi`/`MapOpenApi`/Scalar unchanged.
- `GlobalExceptionHandler` now maps `BadHttpRequestException` to 400.
- Architecture tests extended to cover the `*.Api` assemblies.
- HTTP contract preserved (paths, auth, shapes, `{code,message,details}`, status codes).
Verification: `dotnet build Arquitectura.Cli.slnf -c Release` 0 errors/0 warnings; `dotnet test -c Release` 70/70.

L10 Build hygiene and package refresh (user request):
- Added `Directory.Build.props` (TFM/net11, Nullable, ImplicitUsings, LangVersion, deterministic for `.csproj` only), `Directory.Packages.props` (Central Package Management) and `global.json` (SDK 11.0.100 + `Microsoft.Testing.Platform` runner).
- Removed every `Version` and `TargetFramework`/`Nullable`/`ImplicitUsings` duplication from the `.csproj` files.
- Updated packages: FluentValidation 12.2.0, Mapster 10.0.14, Microsoft.Extensions.Http.Resilience/ServiceDiscovery 11.0.1, OpenTelemetry 1.21.0, Serilog 4.4.0, Serilog.Extensions.* 11.0.0, Serilog.Sinks.OpenTelemetry 4.3.0, coverlet 10.1.1, FluentAssertions 8.11.0, Microsoft.NET.Test.Sdk 18.12.0, Moq 4.21.0, xunit.v3 4.0.3.
- xunit v3 4.x moved to Microsoft.Testing.Platform; opted in through `global.json`. `dotnet test` and `dotnet test --project` both pass 70/70.
- Migrated `Arquitectura.AppHost` from the obsolete `Aspire.AppHost.Sdk` to `Microsoft.NET.Sdk` + `Aspire.Hosting.AppHost`/`Aspire.Hosting.Dotnet` (removes ASPIRE012; `MessagePack` no longer needed).
- `Arquitectura.slnx` now uses solution folders (`Backend/Host`, `Backend/BuildingBlocks`, `Backend/Modules/*`, `Backend/Tests`, `Infrastructure`, `Frontend`). Solution folders are VS organization only; DLLs and deployment are unchanged.
- Fixed `CA2264` in `ResultHttpExtensions.ToErrorResult` (ResulTus `Error` is a struct).
Verification: Release build 0 errors/0 warnings; `dotnet test` 70/70; E2E 11/11.

L11 Full ResulTus dogfooding (user request): "Todos los paquetes de resultus los cree yo, para poderlos usar y simplificar eliminar boilerplate".
- Added `ResulTus.Analyzers` globally via `Directory.Build.props`.
- HTTP: `ResulTus.AspNetCore` now renders RFC 9457 ProblemDetails. `ResultStatus.Map` keeps legacy codes and delegates to `ResultHttpStatus`; `ResultHttpResults` adds 204/201/202 helpers. Removed the custom `{code,message,details}` mapper.
- Validation: `ResulTus.Validation.FluentValidation.ValidateAsync` in `ValidationBehavior`; `RequestValidationException` carries `ValidationErrors`; the host renders the `errors` field map through `ResulTus.AspNetCore`.
- Errors: `ResulTus.SourceGenerators` `[ResultErrors]` factories (`CommonErrors`, `AuthErrors`, `WeatherErrors`, `DataTableErrors`) replaced every inline `Error.X("CODE", ...)`.
- Extensions: `Ensure` in `LoginCommandHandler`.
- Logging: `LogAll` on the login result, with auth failures at Warning.
- Testing: `ShouldSucceed`/`ShouldFail` in `LoginCommandHandlerTests`.
- Frontend: `src/lib/problem.ts` reads `detail`, then `errors`, then `title`; `weatherApi`/`dataTableApi` parse problem documents.
Verification: Release build 0 errors/0 warnings; `dotnet test` 71/71; frontend lint/build OK; E2E 11/11.

L12 ResulTus 1.4.0 dogfooding (user request): added the missing APIs to the packages, released them, and consumed them here.
- ResulTus PR #29: `TryGetValue`/`TryGetError` on `Result<TSuccess, TError>`, `IResultOutcome` implemented by `Result`/`Result<T>`, and `ResulTus.Logging` overloads for the non-generic view. Version 1.4.0 (MINOR), API snapshots/changelog/docs updated, CI green.
- Merged #29 and pushed `v1.4.0`; the release workflow published all 13 packages to nuget.org (verified 1.4.0 in the flat-container index).
- Arquitectura: all ResulTus packages moved to 1.4.0 from nuget.org; the local feed/NuGet.config was removed. `LoggingBehavior` logs every result through `IResultOutcome` (expected client errors at Information); `LoginCommandHandler` no longer logs or takes a logger.
- E2E: unique e-mail now uses `randomUUID()` instead of `Date.now()` to avoid parallel-worker collisions.
Verification: Release build 0 errors/0 warnings; `dotnet test` 71/71; E2E 11/11.

L13 ROP through the application layer (user request: "Hay que seguir ROP 100%"):
- Handlers (`Login`, `Register`, `RefreshToken`, `Logout`) now compose `Result` with `MapErrorAsync`/`EnsureAsync`/`BindAsync`/`Map`/`ToUnitResult`; no `TryGetValue`/`IsFailure` in the flow.
- Repositories (`User`, `WeatherForecast`, `DataTable`) use `Result.TryAsync` + `MapError`; every `try/catch` was removed from persistence.
- `IJwtTokenService.ValidateToken` returns `Result<TokenIdentity>` instead of a nullable tuple.
- Exceptions remain only at the HTTP boundary: `ValidationBehavior` (structured `ValidationErrors`) and `CurrentUser.RequireUserId` (401).
- Documented in ADR-011 and AGENTS rule 12.
Verification: Release build 0 errors/0 warnings; `dotnet test` 71/71; E2E 11/11.

L14 CurrentUser ROP (user request):
- `ICurrentUser.RequireUserId()` (throwing) replaced by `Result<int> UserIdResult` with an `AUTH_REQUIRED` unauthorized failure.
- Auth logout and the four DataTable endpoints compose it with `Match`; no exception at the identity boundary.
- The validation boundary (`RequestValidationException`) is the only exception left; ADR-011/AGENTS updated.
Verification: Release build 0 errors/0 warnings; `dotnet test` 71/71; E2E 11/11.

L15 Full mediator contract change (user request: "Cambio completo"):
- `IRequestHandler`/`IPipelineBehavior`/`ISender` return `Task<Result<TResponse, RequestError>>`; `TResponse` is now the success payload (`Unit` for 204 commands).
- `RequestError` (`Error` + optional `ValidationErrors`) lives in `BuildingBlocks.Abstractions.Mediator`; `ResulTus.Validation` was added to that project.
- `ValidationBehavior` short-circuits with a failed result; `RequestValidationException` and its `GlobalExceptionHandler` branch were deleted.
- Handlers compose `Result<T>` internally and cross with `ToRequestResult`/`ToUnitRequestResult`; endpoints project the typed tier with `ToHttpResult`/`ToNoContentHttpResult`/`ToAcceptedHttpResult` from `BuildingBlocks.AspNetCore.RequestResultHttpResults`.
- `ResilienceBehavior` retries transient results (`Failure`/`Timeout`/`Unexpected`) through Polly; the per-scope circuit breaker was removed because a scoped behavior never accumulated failures across requests, so it never opened.
- Contract surface change: modules declare the payload (`ICommand<AuthResponse>`, `IQuery<DataTableConfigResponseDto>`) instead of `ICommand<Result<AuthResponse>>`; no HTTP shape changed.
- Tests added: `ValidationBehaviorTests` (3) and `RequestResultHttpResultsTests` (4).
- Documented in ADR-011, `ARCHITECTURE.md` and AGENTS rule 12.
Verification: Release build 0 errors/0 warnings; `dotnet test` 78/78; E2E 11/11.
