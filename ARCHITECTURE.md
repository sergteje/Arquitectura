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
Arquitectura.Host                     ← composition root + transport (middleware, health, docs)
        │  (references Contracts + Api + AddXModule)
        ▼
Modules.X.Api ──► Modules.X.Contracts        (minimal API endpoint maps)
Modules.X  ──►  BuildingBlocks.Abstractions        (module core: contracts, features, domain ports)
Modules.X.Infrastructure ──► BuildingBlocks        (Dapper repositories, module services, DI)
        │
        ▼
BuildingBlocks.AspNetCore ──► BuildingBlocks.Abstractions   (Result → IResult mapping)
BuildingBlocks.Infrastructure ──► BuildingBlocks.Abstractions
```

### `Arquitectura.Host`

The single entry point. It:

- registers shared infrastructure (`AddBuildingBlocksInfrastructure`);
- calls `AddAuthModule`, `AddWeatherModule`, `AddDataTableModule`;
- maps the module endpoints (`MapAuthEndpoints`, `MapWeatherEndpoints`, `MapDataTableEndpoints`);
- configures JWT bearer, CORS, rate limiting, OpenAPI + Scalar, Hangfire, health checks;
- hosts the web middleware.

It must not contain business logic and must not touch module internals beyond contracts, the endpoint
maps and the DI extensions.

### Modules

Each module is split into three projects:

| Project | Contains | Depends on |
|---------|----------|------------|
| `Modules.X` | `Domain`, `Features` (commands, queries, handlers, validators), `Contracts`, `Persistence` ports | `BuildingBlocks.Abstractions` |
| `Modules.X.Infrastructure` | Dapper repositories, JWT/token services, jobs, `AddXModule` | `Modules.X`, `BuildingBlocks.*` |
| `Modules.X.Api` | Minimal API endpoint map (`MapXEndpoints`) | `Modules.X`, `BuildingBlocks.AspNetCore` |

`Contracts` and the `MapXEndpoints` extension are the only surfaces other code may consume.

### BuildingBlocks

- `BuildingBlocks.Abstractions` — technical contracts only: mediator interfaces, `ICurrentUser`,
  `IDbConnectionFactory`, `IUnitOfWork`, `IDistributedCacheService`, `Entity`, `Specification`,
  pagination, cache/resilience markers. No business logic.
- `BuildingBlocks.Infrastructure` — the mediator implementation, pipeline behaviors, Dapper/Redis
  primitives and health checks. It never references a module.
- `BuildingBlocks.AspNetCore` — HTTP mapping (`Result`/`Error` → `IResult`, `{code,message,details}`)
  shared by the module endpoint projects. It never references a module.

## Request flow

```
HTTP request
  → middleware pipeline (exception handling, correlation, audit, auth, rate limit)
  → minimal API endpoint (Modules.X.Api)
  → ISender.Send(command/query)
  → mediator pipeline: Logging → Validation → Resilience → CacheInvalidation
  → module handler (Modules.X.Features)
  → repository port (Modules.X.Persistence)
  → Dapper repository (Modules.X.Infrastructure.Persistence) → SQL Server stored procedure
  → Result<T> / Error
  → BuildingBlocks.AspNetCore ResultHttpExtensions → { code, message, details } + status code
```

## Result pattern

Results use **ResulTus 1.4.0**. A handler returns `Result<T>` (or `Result`). Failures carry a typed
`Error` with a stable `Code`, a `Message` and an `ErrorType`.

`BuildingBlocks.AspNetCore.ResultStatus` maps results to HTTP:

- legacy codes keep their historical status (`NOT_FOUND` → 404, `VALIDATION_ERROR` → 400,
  `RETRY_EXHAUSTED` → 503, `TIMEOUT` → 504);
- a generic `ErrorType.Failure` stays 400 (the old default for database failures);
- every other category delegates to `ResulTus.AspNetCore`'s `ResultHttpStatus.GetStatusCode`;
- the body is RFC 9457 ProblemDetails (`{ type, title, status, detail, code }`); validation failures
  include the `errors` field map.

The result pipeline dogfoods the whole ResulTus suite: `Extensions` (`Ensure`), `AspNetCore`
(ProblemDetails), `Validation` + `Validation.FluentValidation` (structured field errors),
`SourceGenerators` (`[ResultErrors]` factories), `Logging` (pipeline behavior through `IResultOutcome`), `Testing` (assertions) and
`Analyzers` (build-time checks).

Handlers stay railway-oriented: they compose `Result` with `Bind`, `Ensure`, `Map` and `MapError`,
and repositories capture infrastructure exceptions with `Result.TryAsync` instead of `try/catch`.
The only exception left is `ValidationBehavior`, which carries structured `ValidationErrors`;
`CurrentUser` exposes `Result<int> UserIdResult` for endpoint composition.

## Security

- JWT bearer authentication; the signing key is mandatory (`Jwt:Key`), with no hardcoded fallback.
- Passwords use salted **PBKDF2-HMAC-SHA256** (210k iterations). Legacy SHA-256 hashes still verify and
  are transparently rehashed on the next successful login.
- An API key middleware protects the API in Production; it is bypassed in Development.
- Security headers and non-cacheable sensitive paths are applied by middleware.

## Caching

Caching is explicit, not a pipeline behavior. `Result<T>` is a readonly struct that does not
round-trip through a serializer, so `QueryCachingExtensions.GetOrSetAsync` caches only the successful
payload of a query. Cache invalidation is opt-in through `ICacheInvalidatingRequest`, which only the
Weather write commands implement. The cache runs on the single shared Redis multiplexer.

## Background jobs

Hangfire is configured in the Host only when a connection string is present. The Weather module
contributes its recurring jobs through `AddWeatherModuleJobs()`; job registration is skipped when
Hangfire is disabled so DI validation never fails in environments without a database.

## Configuration

Everything is Options-based: `JwtOptions`, `RedisOptions`, `DatabaseOptions`, `ApiKey:Key`.
`Jwt:Key` is mandatory and validated at startup. There are no hardcoded secrets.

## API documentation

OpenAPI is generated at runtime by `Microsoft.AspNetCore.OpenApi` and served at `/openapi/v1.json`.
Scalar renders the interactive reference at `/scalar`.

## Further reading

- [MODULE_BOUNDARIES.md](MODULE_BOUNDARIES.md) — the enforceable rules.
- [ENDPOINT_INVENTORY.md](ENDPOINT_INVENTORY.md) — the frozen HTTP contract.
- [ADR/](ADR/) — the decision records.
