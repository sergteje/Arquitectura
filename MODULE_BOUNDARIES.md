# Module Boundaries

These rules are enforced by the compiler (project references) and by
`Arquitectura.Tests/Architecture/ModuleBoundaryTests.cs`. A change that violates them must be
rejected.

## Allowed dependencies

```
Arquitectura.Host
  → Modules.*.Contracts.*            (DTOs, commands, queries)
  → Modules.*.Infrastructure         (only the AddXModule DI extension)
  → BuildingBlocks.*
  → Arquitectura.ServiceDefaults

Modules.X
  → BuildingBlocks.Abstractions

Modules.X.Infrastructure
  → Modules.X
  → BuildingBlocks.Abstractions
  → BuildingBlocks.Infrastructure

BuildingBlocks.Infrastructure
  → BuildingBlocks.Abstractions

BuildingBlocks.Abstractions
  → (nothing but ResulTus)
```

## Forbidden dependencies

- A module **must not** reference another `Modules.Y` assembly (directly or transitively through
  another module).
- A module **must not** reference `Arquitectura.Host`, `Arquitectura.Api`, or any legacy
  `Arquitectura.*` project.
- `BuildingBlocks.*` **must not** reference a module or a legacy project.
- `Modules.X.Domain` and `Modules.X.Features` are internal to the module. Only `Modules.X.Contracts`
  may be used by other modules or the Host.
- No shared domain entities. If two modules need the same concept, each owns its own type and the
  contract exposes its own DTO.
- No circular project references.

## Cross-module communication

Only two mechanisms are allowed:

1. **Contracts**: `Modules.A.Contracts` types consumed by `Modules.B` or the Host.
2. **BuildingBlocks abstractions**: `ICurrentUser`, `IDistributedCacheService`, `IDbConnectionFactory`,
   `IUnitOfWork`, the mediator contracts.

A module never calls another module's handler, repository or service directly.

## Composition

The Host is the only composition root. It calls the module DI extensions:

```csharp
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddWeatherModule(builder.Configuration);
builder.Services.AddDataTableModule(builder.Configuration);
```

A module's `AddXModule` is the only infrastructure type the Host is allowed to name.

## Enforcement

The architecture test verifies, at the assembly level:

- `Modules.Auth` references no `Modules.Weather` / `Modules.DataTable`;
- `Modules.Weather` references no `Modules.Auth` / `Modules.DataTable`;
- `Modules.DataTable` references no `Modules.Auth` / `Modules.Weather`;
- `BuildingBlocks.Abstractions` and `BuildingBlocks.Infrastructure` reference no module and no legacy
  project.

When a new module is added, extend `ModuleBoundaryTests` with the corresponding assertions.
