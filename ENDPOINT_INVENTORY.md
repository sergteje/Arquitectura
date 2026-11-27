# Endpoint Inventory

The HTTP contract is frozen. Method, route, auth requirement, request shape, success shape and success
status must not change without updating this document and the frontend.

| Method | Route | Auth | Request | Success |
|--------|-------|------|---------|---------|
| POST | `/api/auth/login` | anonymous | `LoginRequest` | 200 `AuthResponse` |
| POST | `/api/auth/register` | anonymous | `RegisterRequest` | 201 `AuthResponse` |
| POST | `/api/auth/refresh` | bearer | `RefreshTokenRequest` | 200 `RefreshTokenResponse` |
| POST | `/api/auth/logout` | bearer | — | 204 |
| GET | `/api/weatherforecast` | bearer | `?page&pageSize` | 200 `PaginatedResponse<WeatherForecastDto>` |
| GET | `/api/weatherforecast/{id:int}` | bearer | — | 200 `WeatherForecastDto` |
| POST | `/api/weatherforecast` | bearer | `CreateWeatherForecastRequest` | 201 `WeatherForecastDto` |
| PUT | `/api/weatherforecast/{id:int}` | bearer | `CreateWeatherForecastRequest` | 200 `WeatherForecastDto` |
| DELETE | `/api/weatherforecast/{id:int}` | bearer | — | 204 |
| GET | `/api/datatable/{tableKey}` | bearer | — | 200 `DataTableConfigResponseDto` |
| POST | `/api/datatable/{tableKey}/config` | bearer | `SaveDataTableConfigRequestDto` | 204 |
| DELETE | `/api/datatable/{tableKey}/config` | bearer | — | 204 |
| GET | `/api/datatable/{tableKey}/data` | bearer | `?page&pageSize&sort&direction&filters` | 200 `DataResponseDto<Dictionary<string,object>>` |
| POST | `/api/datatable/{tableKey}/export` | bearer | `DataTableExportRequestDto` | 202 `DataTableExportResponseDto` |
| GET | `/health` | anonymous | — | 200 (Development only) |
| GET | `/alive` | anonymous | — | 200 (Development only) |

Notes:

- ASP.NET routing is case-insensitive; the canonical Swagger case is `api/Auth`, `api/WeatherForecast`,
  `api/DataTable`. The lowercase forms above are what the frontend uses.
- `GET /health` includes SQL Server, database, Hangfire and Redis checks. `GET /alive` only runs the
  `live`-tagged self check.
- Errors keep the body `{ "code": string, "message": string, "details": object|null }`. Status mapping:
  400 validation/failure, 401 unauthorized, 403 forbidden, 404 not found, 409 conflict, 429 rate limited,
  503 service unavailable (`RETRY_EXHAUSTED`), 504 timeout.
- Rate limiting uses the fixed window policy `fixed` (100 requests/minute) on all `/api` controllers.
- A 429 response uses the historical ProblemDetails-like body with `retryAfter`.

Frontend coupling: `Arquitectura.Client/src/lib/constants.ts` sets `API_BASE`; the data table client
reads `error.message` from the error body. Both are satisfied by this contract.
