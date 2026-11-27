# Arquitectura

> **Work in progress.** A production-minded reference application that demonstrates a **modular monolith** on **.NET 11**, with a React 19 frontend.

## Tech Stack

### Backend
- .NET 11 (ASP.NET Core Controllers) + Aspire for local orchestration
- Modular monolith: `Auth`, `Weather`, `DataTable` modules over shared `BuildingBlocks`
- First-party lightweight mediator (commands, queries, pipeline behaviors)
- [ResulTus 1.3.0](https://www.nuget.org/packages/ResulTus) result pattern
- Dapper + SQL Server (stored procedures)
- JWT bearer authentication with PBKDF2 password hashing
- Redis distributed cache
- Polly resilience, FluentValidation, Mapster
- Hangfire background jobs
- OpenTelemetry, health checks, Serilog
- OpenAPI + Scalar API reference

### Frontend
- React 19, Vite, TypeScript
- Zustand, TanStack Query
- shadcn/ui

### Infrastructure
- Docker + Docker Compose
- SQL Server, Redis
- Jaeger (tracing), Seq (logs), Prometheus + Grafana

## Getting Started

### Prerequisites

- [.NET 11 SDK](https://dotnet.microsoft.com/download/dotnet/11.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Quick Start (Docker)

```bash
cp .env.example .env
docker compose up -d
```

| Service | URL |
|---------|-----|
| API | http://localhost:8080 |
| Scalar UI | http://localhost:8080/scalar |
| OpenAPI document | http://localhost:8080/openapi/v1.json |
| Frontend | http://localhost:5173 |
| Seq | http://localhost:5341 |
| Jaeger | http://localhost:16686 |
| Grafana | http://localhost:3000 (admin/admin123) |

### Local Development (no Docker)

```bash
# Backend (CLI solution filter excludes the SQL project and the frontend esproj)
dotnet build Arquitectura.Cli.slnf
dotnet run --project Arquitectura.Host
# API: https://localhost:44358 (Scalar at /scalar)

# Frontend
cd Arquitectura.Client
npm install
npm run dev
```

### Configuration / User Secrets

Secrets are never hardcoded. Configure them through `appsettings`, environment variables or user secrets:

```bash
cd Arquitectura.Host
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=Arquitectura;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "your-super-secret-key-min-32-chars-long!"
dotnet user-secrets set "Jwt:Issuer" "Arquitectura"
dotnet user-secrets set "Jwt:Audience" "ArquitecturaClients"
```

`Jwt:Key` is mandatory: the host refuses to start without it. `ApiKey:Key` is only required in Production.

## Project Structure

```
Arquitectura/
├── Arquitectura.Host/                 # Single ASP.NET Core entry point (composition root)
├── BuildingBlocks/
│   ├── BuildingBlocks.Abstractions/   # Shared technical contracts (mediator, persistence, caching, identity)
│   └── BuildingBlocks.Infrastructure/ # Mediator, behaviors, Dapper/Redis primitives, health checks
├── Modules/
│   ├── Auth/                          # Modules.Auth + Modules.Auth.Infrastructure
│   ├── Weather/                       # Modules.Weather + Modules.Weather.Infrastructure
│   └── DataTable/                     # Modules.DataTable + Modules.DataTable.Infrastructure
├── Arquitectura.ServiceDefaults/      # Aspire defaults, OpenTelemetry, Serilog, health
├── Arquitectura.AppHost/              # Aspire orchestration → Arquitectura.Host
├── Arquitectura.Database/             # SQL Server project (tables, stored procedures)
├── Arquitectura.Client/               # React frontend
├── Arquitectura.Tests/                # Unit, integration, architecture and endpoint tests
└── Arquitectura.Cli.slnf              # CLI solution filter (excludes SQL project + esproj)
```

See [ARCHITECTURE.md](ARCHITECTURE.md), [MODULE_BOUNDARIES.md](MODULE_BOUNDARIES.md) and [ADR/](ADR/) for the design rationale.

## Architecture Notes

- **Modular monolith**: one deployable, modules with hard boundaries and their own persistence.
- **CQRS via a first-party mediator**: commands and queries, pipeline behaviors for logging, validation, resilience and cache invalidation.
- **Persistence**: Dapper over stored procedures; modules own their SQL.
- **Results**: `ResulTus` `Result`/`Result<T>` with a typed `Error`; HTTP mapping preserves the `{ code, message, details }` body.
- **Caching**: explicit read-through (`QueryCachingExtensions.GetOrSetAsync`) instead of a behavior, because `Result<T>` is not serializable.
- **API docs**: native OpenAPI + Scalar (`/openapi/v1.json`, `/scalar`).

## API Endpoints

See [ENDPOINT_INVENTORY.md](ENDPOINT_INVENTORY.md) for the full, authoritative list.

## Testing

```bash
dotnet test Arquitectura.Tests/Arquitectura.Tests.csproj
```

Covers unit tests (handlers, validators, hashing, caching, result mapping), architecture tests (module isolation) and endpoint smoke tests.

## Deployment

See [DEPLOYMENT.md](DEPLOYMENT.md). Known technical debt is tracked in [TECHNICAL_DEBT.md](TECHNICAL_DEBT.md).

## License

MIT
