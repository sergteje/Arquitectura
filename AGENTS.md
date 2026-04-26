# AGENTS.md - Arquitectura Project

## Solution Structure

```
Arquitectura.slnx
├── Backend (.NET 10)
│   ├── Arquitectura.Api           # Web API (entrypoint)
│   ├── Arquitectura.Application   # MediatR handlers, commands, queries
│   ├── Arquitectura.Domain         # Entities, Specifications, Result<T>
│   ├── Arquitectura.Infrastructure# Auth (JWT), Cache, Behaviors, Middleware
│   ├── Arquitectura.Persistence   # Dapper repos, UnitOfWork, SQL Server
│   ├── Arquitectura.ServiceDefaults
│   ├── Arquitectura.Tests          # xUnit + Moq + FluentAssertions
│   └── Arquitectura.Database      # EF Core for migrations (if any)
├── Frontend (React 19)
│   └── Arquitectura.Client         # Vite + React 19 + Zustand + TanStack Query
└── Arquitectura.AppHost            # Aspire hosting
```

## Developer Commands

### Backend
```powershell
# From root: dotnet build
# From Arquitectura.Api: dotnet run (ports 8080, 8081)
```

### Frontend
```powershell
cd Arquitectura.Client
npm run dev      # Vite dev server, port 5173
npm run build    # TypeScript build + Vite production
npm run lint     # ESLint
```

### Testing
```powershell
# API tests (assumes HTTPS localhost:44358 running)
./test-api.ps1
./test-auth.ps1

# Unit tests
dotnet test
```

### Docker
```powershell
docker compose up -d  # API + SQL Server + Redis + Observability stack
```

## Architecture Notes

- **MediatR Pipeline**: ValidationBehavior → LoggingBehavior → CachingBehavior → CacheInvalidationBehavior → ResilienceBehavior
- **Persistence**: Dapper with stored procedures (not EF Core for queries)
- **Authentication**: JWT Bearer, keys in docker-compose environment
- **Caching**: StackExchange Redis + MemoryCache for behaviors
- **Background Jobs**: Hangfire with SQL Server storage (dashboard at `/hangfire`)
- **Frontend State**: Zustand stores, TanStack Query for server state

## Infrastructure

| Service | Port | Notes |
|---------|------|-------|
| API | 8080/8081 | HTTP/HTTPS |
| Frontend | 5173 | Vite dev |
| SQL Server | 1433 | Docker (see .env) |
| Redis | 6379 | Docker |
| Seq (logs) | 5341 | Docker |
| Jaeger | 16686 | Docker |
| Grafana | 3000 | Docker (admin/admin123) |

## Testing

- Tests use `xunit.v3`, `Moq`, `FluentAssertions`
- Integration tests in `Arquitectura.Tests/Persistence/` require running SQL Server
- Test scripts assume HTTPS on localhost:44358

## Frontend Components

### Shared Components
```
src/components/
├── Background.tsx      # Animated gradient background (dark/light adaptive)
├── ThemeToggle.tsx       # Sun/Moon toggle with system theme detection
├── Header.tsx          # Glassmorphism header (user, logout, theme toggle)
└── ui/                # shadcn/ui components
```

### Pages
```
src/pages/
├── Login.tsx           # Landing with login/register + header + theme toggle
├── WeatherList.tsx      # Dashboard con stats + tabla
└── WeatherForm.tsx     # Create/edit modal
```

### Lib
```
src/lib/
└── constants.ts       # API_BASE, APP_NAME, WEATHER_ICONS, COLORS
```

## Theme System

- **Detection**: `prefers-color-scheme: dark` + localStorage override
- **Components**: Own theme detection (no next-themes dependency for theme state)
- **Toggle**: Dispatches `theme-change` event to sync components
- **Styles**: Check `isDark` from hook, apply adaptive classes

## Next Session (To-Do)

### 🧪 Testing [PRIORIDAD 1]
- Tests de frontend (Vitest + React Testing Library)
- Tests E2E (Playwright/Cypress)

### 🎨 UI/UX
- Agregar más páginas (Profile, Settings, etc.)
- Animaciones adicionales (framer-motion)
- Skeleton loaders más elaborados

### 🔧 Features
- Refresh token automático
- Protected routes con redirect
- Notificaciones push
- Persist auth (remember me)

### 📦 Calidad
- Agregar Zustand para estado global (auth, theme)
- Code splitting lazy routes
- PWA con service workers