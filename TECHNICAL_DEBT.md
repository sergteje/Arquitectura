# Technical Debt

Tracked known issues, ordered by impact. Each item names the affected area and the intended fix.

## Open

### 1. Missing `sp_DeleteUserTableConfig` stored procedure
- **Where**: `Modules/DataTable/Modules.DataTable.Infrastructure/Persistence/DataTableRepository.cs`
  (`ResetUserConfigAsync`), `Arquitectura.Database`.
- **Impact**: `DELETE /api/datatable/{tableKey}/config` calls a stored procedure that does not exist;
  the repository catches the failure and the endpoint returns an error.
- **Fix**: add `sp_DeleteUserTableConfig` to the SQL project and deploy it. Documented, intentionally
  not fixed during the structural migration (ADR-008).

### 2. Password hashing uses unsalted SHA-256
- **Where**: `Modules/Auth/Modules.Auth/Features/Auth/PasswordHasher.cs`.
- **Impact**: weak against rainbow tables; no per-user salt.
- **Fix**: migrate to a salted KDF (PBKDF2/Argon2) with a rehash-on-login strategy. Requires a data
  migration path for existing hashes.

### 3. Duplicate Redis connections
- **Where**: `BuildingBlocks/.../DependencyInjection/BuildingBlocksServiceCollectionExtensions.cs`.
- **Impact**: `AddStackExchangeRedisCache` and the separately registered `IConnectionMultiplexer`
  open two connection pools.
- **Fix**: build the distributed cache on top of the registered multiplexer, or drop the extra
  multiplexer if nothing else uses it.

### 4. Health endpoints are Development-only
- **Where**: `Arquitectura.ServiceDefaults/Extensions.cs` (`MapDefaultEndpoints`).
- **Impact**: the production container health check (`/health`) is not mapped in Production.
- **Fix**: map a minimal, unauthenticated liveness endpoint in all environments (or make the
  container health check environment-aware).

### 5. Pre-existing `MessagePack` advisory
- **Where**: `Arquitectura.AppHost/Arquitectura.AppHost.csproj` (MessagePack 3.1.7).
- **Impact**: `NU1902` moderate-severity advisory during restore.
- **Fix**: upgrade MessagePack or remove the direct reference if Aspire no longer needs it.

### 6. Legacy projects still on disk
- **Where**: `Arquitectura.Api`, `Arquitectura.Application`, `Arquitectura.Domain`,
  `Arquitectura.Infrastructure`, `Arquitectura.Persistence`.
- **Impact**: dead code; only excluded from the solution.
- **Fix**: delete after the migration is signed off.

### 7. `.sqlproj` not buildable from the dotnet CLI
- **Where**: `Arquitectura.Database/Arquitectura.Database.sqlproj`.
- **Impact**: `dotnet build Arquitectura.slnx` fails for the SQL project on machines without SSDT.
- **Fix**: build the SQL project with MSBuild/`sqlpackage`, or add a solution filter that excludes it
  from CLI builds.

## Resolved during the migration

- Hand-rolled `Result`/`ResultError`/`ServiceErrors`/`ResultExtensions` replaced by ResulTus.
- MediatR replaced by a first-party mediator (licensing + control).
- Caching behavior that warmed the cache and still called the handler replaced by explicit
  read-through caching.
- Cache invalidation scoped to the Weather write commands instead of every non-query request.
- Resilience retries limited to idempotent requests (`IResilientRequest`).
- DataTable user id read from the `sub` claim (previously `userId`, which the JWT never emitted).
- JWT signing key no longer falls back to a hardcoded secret.
- `Application → Persistence` dependency removed.
- `Domain` no longer references ASP.NET Core.
- Weather Hangfire jobs owned by the Weather module.
