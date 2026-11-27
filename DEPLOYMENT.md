# Deployment

## Container image

The root `Dockerfile` builds and publishes `Arquitectura.Host` on .NET 11 and runs it as a non-root
user on port 8080.

```bash
docker build -t arquitectura .
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=Arquitectura;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True" \
  -e Redis__Connection="host.docker.internal:6379" \
  -e Jwt__Key="your-super-secret-key-min-32-chars-long!" \
  arquitectura
```

## Docker Compose

```bash
cp .env.example .env
docker compose up -d
```

Services: `api` (Host), `frontend`, `sqlserver`, `redis`, `jaeger`, `seq`, `grafana`, `prometheus`.

The `api` service sets `ASPNETCORE_ENVIRONMENT=Development`, so Scalar and the Hangfire dashboard are
available. For a production environment, set `ASPNETCORE_ENVIRONMENT=Production` and provide
`ApiKey__Key`. Health endpoints (`/health`, `/alive`) are mapped in every environment, so the container
health check keeps working in production; they expose only the aggregate status, never check details.

## Required configuration

| Key | Required | Notes |
|-----|----------|-------|
| `ConnectionStrings:DefaultConnection` | yes | SQL Server. Also enables Hangfire. |
| `Redis:Connection` | yes | Redis endpoint. |
| `Jwt:Key` | yes | Symmetric signing key (min 32 chars). Host refuses to start without it. |
| `Jwt:Issuer` | no | Defaults to `Arquitectura`. |
| `Jwt:Audience` | no | Defaults to `ArquitecturaClients`. |
| `ApiKey:Key` | production | Required by `ApiKeyMiddleware` in Production. |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | no | Enables the OTLP exporter. |
| `SERILOG_WRITE__TO__OPENTELEMETRY__OTLPENDPOINT` | no | Enables the Serilog OTLP sink. |

Use environment variables (`Jwt__Key`, `ConnectionStrings__DefaultConnection`, …), user secrets, or a
secret manager. Never commit secrets.

## Build

```bash
# CLI build: excludes the SQL project and the frontend esproj
dotnet build Arquitectura.Cli.slnf -c Release

# Tests (xunit v3 4.x runs on Microsoft.Testing.Platform via global.json)
dotnet test Arquitectura.Tests/Arquitectura.Tests.csproj -c Release
```

Package versions are centralized in `Directory.Packages.props` (Central Package Management) and the
SDK is pinned in `global.json`. Deploy **one** artifact: the `Arquitectura.Host` publish output or the
container image; the module assemblies ship inside it.

`Arquitectura.slnx` is the full Visual Studio solution and includes `Arquitectura.Database.sqlproj`,
which requires SSDT. Use `Arquitectura.Cli.slnf` on machines without Visual Studio.

## Aspire (local development)

```bash
dotnet run --project Arquitectura.AppHost
```

The AppHost orchestrates `Arquitectura.Host` (health checked at `/health`) and the Vite frontend.

## Database

`Arquitectura.Database` is a SQL Server project (`.sqlproj`) that must be built with Visual Studio /
MSBuild SSDT — `dotnet build` on the CLI cannot import `Microsoft.Data.Tools.Schema.SqlTasks.targets`.
Deploy it with `sqlpackage` or through Visual Studio. It includes the application tables, the
`sp_User_*`, `WeatherForecast_*` and DataTable stored procedures (including `sp_DeleteUserTableConfig`).

## Health checks

`GET /health` runs `sqlserver`, `database`, `hangfire` and `redis` checks; `GET /alive` runs the
`live`-tagged self check. Both are mapped in every environment and return the aggregate status only.
