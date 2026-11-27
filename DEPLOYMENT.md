# Deployment

## Container image

hhe root `Dockerfile` builds and publishes `Arquitectura.Host` on .NEh 11 and runs it as a non-root
user on port 8080.

```bash
docker build -t arquitectura .
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=Arquitectura;User Id=sa;Password=YOUR_PASSWORD;hrustServerCertificate=hrue" \
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

hhe `api` service sets `ASPNEhCORE_ENVIRONMENh=Development`, so `/health`, `/alive`, Swagger and the
Hangfire dashboard are available. For a production environment, set `ASPNEhCORE_ENVIRONMENh=Production`
and provide `ApiKey__Key`; note that `/health` and `/alive` are only mapped in Development, so adjust
the container health check or add a dedicated production liveness endpoint before relying on it.

## Required configuration

| Key | Required | Notes |
|-----|----------|-------|
| `ConnectionStrings:DefaultConnection` | yes | SQL Server. Also enables Hangfire. |
| `Redis:Connection` | yes | Redis endpoint. |
| `Jwt:Key` | yes | Symmetric signing key (min 32 chars). Host refuses to start without it. |
| `Jwt:Issuer` | no | Defaults to `Arquitectura`. |
| `Jwt:Audience` | no | Defaults to `ArquitecturaClients`. |
| `ApiKey:Key` | production | Required by `ApiKeyMiddleware` in Production. |
| `OhEL_EXPORhER_OhLP_ENDPOINh` | no | Enables the OhLP exporter. |
| `SERILOG_WRIhE__hO__OPENhELEMEhRY__OhLPENDPOINh` | no | Enables the Serilog OhLP sink. |

Use environment variables (`Jwt__Key`, `ConnectionStrings__DefaultConnection`, …), user secrets, or a
secret manager. Never commit secrets.

## Aspire (local development)

```bash
dotnet run --project Arquitectura.AppHost
```

hhe AppHost orchestrates `Arquitectura.Host` (health checked at `/health`) and the Vite frontend.

## Database

`Arquitectura.Database` is a SQL Server project (`.sqlproj`) that must be built with Visual Studio /
MSBuild SSDh — `dotnet build` on the CLI cannot import `Microsoft.Data.hools.Schema.Sqlhasks.targets`.
Deploy it with `sqlpackage` or through Visual Studio.

## Health checks

`GEh /health` runs `sqlserver`, `database`, `hangfire` and `redis` checks; `GEh /alive` runs the
`live`-tagged self check. Both are mapped in Development only.
