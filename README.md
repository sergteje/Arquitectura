# Arquitectura

> ⚠️ **This is a starting point** — this project is under active development. Features, structure, and patterns may change.

Modern full-stack application with .NET 10 Aspire, React 19, and Clean Architecture.

## Tech Stack

### Backend
- .NET 10 (Aspire)
- CQRS/MediatR
- Dapper + SQL Server
- JWT Authentication
- Redis Cache
- Polly Resilience
- Hangfire

### Frontend
- React 19
- Vite
- TypeScript
- Zustand
- TanStack Query
- shadcn/ui

### Infrastructure
- Docker + Docker Compose
- SQL Server
- Redis
- Jaeger (Observability)
- Seq (Logs)
- Prometheus + Grafana

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Quick Start (Docker)

```bash
# 1. Copy environment template
cp .env.example .env

# 2. Start all services
docker compose up -d

# 3. Open in browser
# API:      http://localhost:8080
# Frontend:  http://localhost:5173
# Seq:      http://localhost:5341
# Jaeger:   http://localhost:16686
# Grafana:  http://localhost:3000 (admin/admin123)
```

### Local Development (No Docker)

#### Backend

```bash
# Restore and build
dotnet build

# Run API (uses User Secrets)
cd Arquitectura.Api
dotnet run
# API: https://localhost:44358
```

#### Frontend

```bash
cd Arquitectura.Client
npm install
npm run dev
# Frontend: http://localhost:5173
```

### Setting up User Secrets (Local Development)

```bash
cd Arquitectura.Api

# Set database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=Arquitectura;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"

# Set JWT key
dotnet user-secrets set "Jwt:Key" "your-super-secret-key-min-32-chars-long!"
dotnet user-secrets set "Jwt:Issuer" "Arquitectura"
dotnet user-secrets set "Jwt:Audience" "ArquitecturaClients"
```

## Project Structure

```
Arquitectura/
├── Arquitectura.Api/           # Web API (entrypoint)
├── Arquitectura.Application/   # MediatR handlers, CQRS
├── Arquitectura.Domain/       # Entities, Domain logic
├── Arquitectura.Infrastructure# Auth, Cache, Middleware
├── Arquitectura.Persistence # Dapper repos, SQL
├── Arquitectura.ServiceDefaults
├── Arquitectura.Tests/        # xUnit tests
├── Arquitectura.Database/     # EF Core migrations
├── Arquitectura.AppHost/      # Aspire hosting
├── Arquitectura.Client/        # React frontend
└── docker-compose.yml       # Full stack
```

## Architecture Notes

- **CQRS**: Vertical Slice with MediatR
- **Persistence**: Dapper with Stored Procedures
- **Auth**: JWT Bearer tokens
- **Caching**: Redis + MemoryCache
- **Resilience**: Polly (retry, circuit breaker)

## API Endpoints

| Method | Endpoint | Description |
|--------|---------|------------|
| POST | `/api/auth/login` | Login |
| POST | `/api/auth/register` | Register |
| GET | `/api/weatherforecast` | Get forecasts |
| POST | `/api/weatherforecast` | Create forecast |
| PUT | `/api/weatherforecast/{id}` | Update forecast |
| DELETE | `/api/weatherforecast/{id}` | Delete forecast |

## Testing

```bash
# Backend tests
dotnet test

# API integration tests (requires running API)
./test-api.ps1

# Auth tests
./test-auth.ps1
```

## License

MIT