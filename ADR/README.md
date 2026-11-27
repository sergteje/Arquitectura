# Architecture Decision Records

Records of the decisions taken during the modular-monolith migration. Format: **Context → Decision →
Consequences**.

| ID | Title | Status |
|----|-------|--------|
| ADR-001 | Flat repository layout, one project per module layer pair | Accepted |
| ADR-002 | Module public surface is `Contracts/` | Accepted |
| ADR-003 | One `BuildingBlocks.Infrastructure` | Accepted |
| ADR-004 | First-party mediator replaces MediatR | Accepted |
| ADR-005 | Explicit read-through caching | Accepted |
| ADR-006 | Preserve error body and status mapping | Superseded by ADR-010 |
| ADR-007 | Fix pre-existing defects as part of the move | Accepted |
| ADR-008 | Missing `sp_DeleteUserTableConfig` documented, later added | Resolved |
| ADR-009 | Minimal API endpoints owned by each module | Accepted |
| ADR-010 | Dogfood the full ResulTus suite | Accepted |

---

## ADR-001 — Flat repository layout, one project per module layer pair

**Context.** The previous layout was a classic Clean Architecture with one project per layer
(`Api`, `Application`, `Domain`, `Infrastructure`, `Persistence`). That layout encouraged cross-feature
coupling and made a module boundary impossible to enforce.

**Decision.** Use a flat repository layout with one project per module layer pair:

```
Modules/<Name>/Modules.<Name>                (Domain + Features + Contracts + Persistence ports)
Modules/<Name>/Modules.<Name>.Infrastructure  (Dapper repositories + services + DI)
BuildingBlocks/BuildingBlocks.Abstractions
BuildingBlocks/BuildingBlocks.Infrastructure
```

The frontend and the database project keep their existing locations.

**Consequences.** The compiler enforces boundaries. The repository grows project-by-project rather
than layer-by-layer. A module can be read as a unit.

---

## ADR-002 — Module public surface is `Contracts/`

**Context.** Modules must be independently changeable, so their internals must not leak.

**Decision.** The only public surface of a module is its `Contracts/` folder: DTOs plus the
commands/queries other code may send. The Host may reference `Modules.X.Contracts.*` and the
`AddXModule` DI extension, nothing else from the module.

**Consequences.** Cross-module and Host code compiles against stable DTOs. `Domain`, `Features` and
`Persistence` can be refactored freely. The rule is documented in `MODULE_BOUNDARIES.md` and checked
by architecture tests (assembly level).

---

## ADR-003 — One `BuildingBlocks.Infrastructure`

**Context.** A tempting design is one infrastructure project per concern (caching, jobs,
observability). That multiplies projects without adding boundaries.

**Decision.** Keep a single `BuildingBlocks.Infrastructure` for the mediator implementation, pipeline
behaviors, Dapper/Redis primitives and health checks. It never references a module and contains no
business logic.

**Consequences.** Fewer projects to maintain; the seam that matters (modules vs. shared tech) is still
explicit.

---

## ADR-004 — First-party mediator replaces MediatR

**Context.** MediatR 14 introduced a commercial licence. The project only needs commands, queries,
handlers, pipeline behaviors, validation, logging, caching, resilience, `CancellationToken` and async
execution.

**Decision.** Implement a small first-party mediator in `BuildingBlocks`:
`IRequest<T>`, `ICommand<T>`, `IQuery<T>`, `IRequestHandler<TRequest,TResponse>`,
`IPipelineBehavior<TRequest,TResponse>` and `ISender`. The wrapper is registered **scoped** because it
captures `IServiceProvider` and must resolve scoped handlers.

**Consequences.** No licence dependency and full control of the pipeline. The behavior order is
explicit: Logging → Validation → Resilience → CacheInvalidation. Reintroducing MediatR would require
updating handlers to its interfaces.

---

## ADR-005 — Explicit read-through caching

**Context.** The old `CachingBehavior` attempted to cache `Result<T>`. `Result<T>` is a readonly struct
without public setters and does not round-trip through a serializer, so either the cache never hit or
the handler ran anyway.

**Decision.** Remove the caching behavior. Cache only successful payloads through an explicit helper,
`QueryCachingExtensions.GetOrSetAsync`, used by query handlers. Invalidate through
`ICacheInvalidatingRequest` on the write commands that own the affected keys.

**Consequences.** Correct and predictable caching. Queries opt in; writes declare exactly what they
evict. `Result<T>` stays un-serialized.

---

## ADR-006 — Preserve error body and status mapping

**Status: superseded by ADR-010.** The original decision kept a host-local `{ code, message, details }`
body. ADR-010 replaces it with RFC 9457 ProblemDetails from `ResulTus.AspNetCore`.

**Context.** The frontend reads `error.message`; the previous API returned
`{ code, message, details }`. `ResulTus.AspNetCore.ToActionResult` emits RFC 9457 ProblemDetails, which
would break the frontend.

**Decision (historical).** Keep a Host-local mapper, `ResultHttpExtensions`, that emits `ErrorResponse`
(`{ code, message, details }`). Legacy codes keep their historical status; other errors map from
`ErrorType`. `ErrorType.Failure` maps to 400 to preserve the old default for `DATABASE_ERROR`.

**Consequences.** No frontend change at the time. The HTTP contract was documented in
`ENDPOINT_INVENTORY.md`. This decision was later reversed in ADR-010.

---

## ADR-007 — Fix pre-existing defects as part of the move

**Context.** The audit found defects that would otherwise be copied into the new structure.

**Decision.** Fix them while moving the code:

- cache HIT path no longer runs the handler;
- cache invalidation scoped to the Weather write commands;
- resilience retries only idempotent requests (`IResilientRequest`);
- DataTable user id read from `sub` (the JWT never emitted `userId`);
- no hardcoded JWT fallback secret — `Jwt:Key` is mandatory;
- Hangfire Weather jobs owned by the Weather module;
- `Application → Persistence` and `Domain → ASP.NET Core` dependencies removed.

**Consequences.** The migrated code is correct on day one. Each fix is covered by the test suite where
deterministic.

---

## ADR-008 — Missing `sp_DeleteUserTableConfig` documented, later added

**Context.** `DataTableRepository.ResetUserConfigAsync` calls `sp_DeleteUserTableConfig`, which did not
exist in `Arquitectura.Database`. The SQL project requires SSDT/Visual Studio to build and deploy.

**Decision (initial).** Document the gap in `TECHNICAL_DEBT.md` rather than fabricate the procedure
during a structural migration. The repository maps the failure to a `Result` failure instead of crashing.

**Resolution.** `sp_DeleteUserTableConfig.sql` was added to `dbo/Stored Procedures`, registered in the
SQL project, and follows the same audit pattern as `sp_SaveUserTableConfig`: it records the removed
values in `UserTablePreferencesHistory` with `ChangeType = 'DELETE'` before deleting the row.

**Consequences.** `DELETE /api/datatable/{tableKey}/config` works once the database is deployed. The
procedure still requires a SQL deployment (Visual Studio / `sqlpackage`); it is excluded from the CLI
solution filter.

---

## ADR-009 — Minimal API endpoints owned by each module

**Context.** The first iteration hosted MVC controllers in `Arquitectura.Host`. That kept the modules
framework-free, but it concentrated all routing in the Host and required `IActionResult` mapping that
only the Host could use. The team wants Minimal API and wants each module to own its HTTP surface.

**Decision.** Add a `Modules.X.Api` project per module. It references its own module,
`BuildingBlocks.Abstractions` and a new `BuildingBlocks.AspNetCore` project that maps
`Result`/`Error` to `IResult` while preserving `{ code, message, details }`. Each `Api` project exposes
`MapXEndpoints(this IEndpointRouteBuilder)`; the Host only calls the three maps. Controllers and the
Host-local mapper were removed.

**Consequences.**

- The HTTP surface lives next to the module that owns it; the Host stays a thin composition root.
- Module boundary tests now also assert that `Modules.*.Api` assemblies never reference another module.
- Minimal API binding is explicit (`[FromQuery]`, nullable query parameters with defaults); there are no
  MVC conventions to rely on.
- OpenAPI/Scalar continue to work through `AddOpenApi()`/`MapOpenApi()`, with `.Produces<T>()` metadata.
- The error body and status mapping moved to `BuildingBlocks.AspNetCore`; ADR-010 later switched the
  body to RFC 9457 ProblemDetails.

---

## ADR-010 — Dogfood the full ResulTus suite

**Context.** The author of ResulTus owns this repository and uses it as a real test bed for the
package family. The goal is to exercise the packages in production-shaped code, not only the core
`Result` type.

**Decision.** Adopt the whole suite:

| Package | Use |
|---|---|
| `ResulTus` | Core result types (already in use) |
| `ResulTus.Extensions` | `Ensure`/`Map`/`Bind` in handlers (`LoginCommandHandler`) |
| `ResulTus.AspNetCore` | `ToHttpResult`/problem results; RFC 9457 ProblemDetails |
| `ResulTus.Validation` | `ValidationErrors`/`ValidationIssue` for structured failures |
| `ResulTus.Validation.FluentValidation` | `ValidateAsync` inside the mediator validation behavior |
| `ResulTus.SourceGenerators` | `[ResultErrors]`/`[ErrorFactory]` error factories per module |
| `ResulTus.Logging` | `LogAll` on the login result (structured auth audit) |
| `ResulTus.Testing` | `ShouldSucceed`/`ShouldFail` in handler tests |
| `ResulTus.Analyzers` | `RESULT001..008` as build-time warnings on every project |

**Consequences.**

- The error body is RFC 9457 ProblemDetails: `{ type, title, status, detail, code }` and validation
  failures carry `errors` per field. The frontend was updated to read `detail`/`errors`
  (`src/lib/problem.ts`).
- `ResultStatus.Map` keeps the legacy status codes (including `Failure` → 400) and delegates the rest to
  `ResultHttpStatus.GetStatusCode`.
- Validation runs before logging and throws `RequestValidationException` carrying `ValidationErrors`; the
  host renders it through `ResulTus.AspNetCore`'s validation conversion.
- Error codes and messages live in generated factories (`CommonErrors`, `AuthErrors`, `WeatherErrors`,
  `DataTableErrors`) instead of inline `Error.X("CODE", ...)` calls.
- `ResulTus.Logging` cannot be used inside the generic `LoggingBehavior` because `Result<T>` has no
  non-generic abstraction; it is used where the concrete result type is known (login audit).
