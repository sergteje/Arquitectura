# Architecture

Arquitectura is a **modular monolith**: a single deployable ASP.NET Core application whose code is
divided into strongly bounded modules. This document explains the structure, the dependency rules and
the runtime composition.

## Goals

- One API, one deployment, one database.
- Modules that can be understood, tested and changed in isolation.
- Hard boundaries enforced by the compiler and by architecture tests.
- Preserved HTTP contracts so the React client keeps working unchanged.

## Layers

```
Arquitectura.Host                     ← composition root + transport (controllers, middleware)
        │  (references Contracts + AddXModule only)
        ▼
Modules.X  ──►  BuildingBlocks.Abstractions        (module core: contracts, features, domain ports)
Modules.X.Infrastructure ──► BuildingBlocks        (Dapper repositories, module services, DI)
        │
        ▼
BuildingBlocks.Infrastructure ──► BuildingBlocks.Abstractions
```

### `Arquitectura.Host`

The single entry point. It:
- registers shared infrastructure (`AddBuildingBlocksInfrastructure`);
- calls `AddAuthModule`, `AddWeatherModule`, `AddDataTableModule`;
- configures JWT bearer, CORS, rate limiting, Swagger, Hangfire, health checks;
- hosts the controllers and the web middleware;
- maps the module `Result`/`Error` values to HTTP responses.

It must not contain business logic and must not touch module internals beyond contracts and the DI
extensions.

### Modules

Each module is split into two projects:

| Project | Contains | Depends on |
|---------|----------|------------|
| `Modules.X` | `Domain`, `Features` (commands, queries, handlers, validators), `Contracts`, `Persistence` ports | `BuildingBlocks.Abstractions` |
| `Modules.X.Infrastructure` | Dapper repositories, JWT/token services, jobs, `AddXModule` | `Modules.X`, `BuildingBlocks.*` |

`Contracts` lives inside `Modules.X` and is the only surface other code may consume.

### BuildingBlocks

- `BuildingBlocks.Abstractions` — technical contracts only: mediator interfaces, `ICurrentUser`,
  `IDbConnectionFactory`, `IUnitOfWork`, `IDistributedCacheService`, `Entity`, `Specification`,
  pagination, cache/resilience markers. No business logic.
- `BuildingBlocks.Infrastructure` — the mediator implementation, pipeline behaviors, Dapper/Redis
  primitives and health checks. It never references a module.

## Request flow

```
HTTP request
  → middleware pipeline (exception handling, correlation, audit, auth, rate limit)
  → Controller (Host)
  → ISender.Send(command/query)
  → mediator pipeline: Logging → Validation → Resilience → CacheInvalidation
  → module handler (Modules.X.Features)
  → repository port (Modules.X.Persistence)
  → Dapper repository (Modules.X.Infrastructure.Persistence) → SQL Server stored procedure
  → Result<T> / Error
  → ResultHttpExtensions → { code, message, details } + status code
```

## Result pattern

Results use **ResulTus 1.3.0**. A handler returns `Result<T>` (or `Result`). Failures carry a typed
`Error` with a stable `Code`, a `Message` and an `ErrorType`.

`Arquitectura.Host.Web.ResultHttpExtensions` maps results to HTTP:

- legacy codes keep their historical status (`NOT_FOUND` → 404, `VALIDATION_ERROR` → 400,
  `RETRY_EXHAUSTED` → 503, `TIMEOUT` → 504);
- otherwise the status is derived from `ErrorType`;
- `ErrorType.Failure` maps to 400 to preserve the legacy default for database failures;
- the body is always `{ code, message, details }` (details currently `null`).

## Caching

Caching is explicit, not a pipeline behavior. `Result<T>` is a readonly struct that does not
round-trip through a serializer, so `QueryCachingExtensions.GetOrSetAsync` caches only the successful
payload of a query. Cache invalidation is opt-in through `ICacheInvalidatingRequest`, which only the
Weather write commands implement.

## Background jobs

Hangfire is configured in the Host only when a connection string is present. The Weather module
contributes its recurring jobs through `AddWeatherModuleJobs()`; job registration is skipped when
Hangfire is disabled so DI validation never fails in environments without a database.

## Configuration

Everything is Options-based: `JwtOptions`, `RedisOptions`, `DatabaseOptions`, `ApiKey:Key`.
`Jwt:Key` is mandatory and validated at startup. There are no hardcoded secrets.

## Further reading

- [MODULE_BOUNDARIES.md](MODULE_BOUNDARIES.md) — the enforceable rules.
- [ENDPOINT_INVENTORY.md](ENDPOINT_INVENTORY.md) — the frozen HTTP contract.
- [ADR/](ADR/) — the decision records.
