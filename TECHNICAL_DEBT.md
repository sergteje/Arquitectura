# Technical Debt

Tracked known issues, ordered by impact. Each item names the affected area and the intended fix.

## Open

### 1. Legacy password hashes are upgraded lazily
- **Where**: `Modules/Auth/Modules.Auth/Features/Auth/PasswordHasher.cs`,
  `Features/Auth/Handlers/AuthHandlers.cs`.
- **Context**: new hashes use PBKDF2-HMAC-SHA256 with a random salt. Existing unsalted SHA-256 hashes
  still verify and are rehashed automatically on the next successful login.
- **Impact**: accounts that never log in again keep the legacy hash indefinitely.
- **Fix**: optionally add a one-off background migration that rehashes known credentials, or force a
  password reset for dormant accounts.

### 2. `RedisCacheService` uses Newtonsoft.Json
- **Where**: `BuildingBlocks/BuildingBlocks.Infrastructure/Caching/RedisCacheService.cs`.
- **Impact**: an extra serializer dependency alongside `System.Text.Json`.
- **Fix**: migrate to `System.Text.Json` and drop the Newtonsoft reference once no other code needs it.

### 3. `DatabaseHealthCheck` hardcodes the Weather table
- **Where**: `Arquitectura.Host/Web/HealthChecks/DatabaseHealthCheck.cs`.
- **Impact**: a database check coupled to one module's table.
- **Fix**: replace with a generic `SELECT 1` connectivity check (the existing `SqlServerHealthCheck`
  already covers this), or move the table-specific check into the Weather module.

### 4. Health endpoints are public
- **Where**: `Arquitectura.ServiceDefaults/Extensions.cs`.
- **Impact**: `/health` and `/alive` are anonymous in every environment. They expose only the aggregate
  status, never check details, but are reachable.
- **Fix**: restrict at the network/gateway layer if the deployment requires it.

### 5. Database error details are exposed to clients
- **Where**: `CommonErrors.Database(string detail)` and the repositories that pass `exception.Message`.
- **Impact**: problem details can leak SQL Server messages (table names, constraints).
- **Fix**: return a generic `DATABASE_ERROR` message and log the exception server-side.

## Resolved

- **Missing `sp_DeleteUserTableConfig`** — added to `Arquitectura.Database/dbo/Stored Procedures`
  and wired into the SQL project. `DELETE /api/datatable/{tableKey}/config` now works and records an
  audit row with `ChangeType = 'DELETE'`.
- **Unsalted SHA-256 password hashing** — replaced by salted PBKDF2-HMAC-SHA256 (210k iterations) with
  transparent rehash on successful login; legacy hashes still verify.
- **Duplicate Redis connections** — the distributed cache now runs on the single shared
  `IConnectionMultiplexer`; `Microsoft.Extensions.Caching.StackExchangeRedis` was removed.
- **Health endpoints Development-only** — `/health` and `/alive` are mapped in every environment.
- **`MessagePack` advisory (NU1902)** — upgraded to 3.1.12.
- **Legacy projects on disk** — `Arquitectura.Api`, `Application`, `Domain`, `Infrastructure` and
  `Persistence` were deleted; the solution only contains the modular monolith.
- **`.sqlproj` not buildable from the dotnet CLI** — `Arquitectura.Cli.slnf` excludes the SQL project
  and the frontend esproj, so `dotnet build Arquitectura.Cli.slnf` works; use Visual Studio / SSDT or
  `sqlpackage` to build and deploy `Arquitectura.Database`.
- **Hand-rolled result pattern** — replaced by ResulTus 1.3.0.
- **MediatR** — replaced by a first-party mediator.
- **Caching behavior that still ran the handler on a hit** — replaced by explicit read-through caching.
- **Cache invalidation on every non-query** — scoped to the Weather write commands.
- **Retries on non-idempotent commands** — limited to `IResilientRequest`.
- **DataTable user id read from `userId`** — now read from the `sub` claim.
- **Hardcoded JWT fallback secret** — removed; `Jwt:Key` is mandatory.
- **`Application → Persistence` and `Domain → ASP.NET Core` dependencies** — eliminated.
