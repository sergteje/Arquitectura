using Arquitectura.Application.Interfaces;
using Arquitectura.Infrastructure.Auth;
using Arquitectura.Infrastructure.Behaviors;
using Arquitectura.Infrastructure.Cache;
using Arquitectura.Infrastructure.HealthChecks;
using Arquitectura.Infrastructure.Jobs;
using Arquitectura.Infrastructure.Middleware;
using Arquitectura.Infrastructure.Resilience;
using Arquitectura.Persistence.DependencyInjection;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Connection strings from docker-compose (same as SkeletonApp)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var redisConnection = builder.Configuration["Redis:Connection"] ?? "redis:6379";

// Add Persistence (Dapper + Stored Procs)
builder.Services.AddPersistence(builder.Configuration);

// Memory Cache for behaviors
builder.Services.AddMemoryCache();

// Redis / Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
});
builder.Services.AddScoped<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddScoped<IDistributedCacheService, RedisCacheService>();

// JWT Authentication (same as SkeletonApp)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "Arquitectura-SuperSecretKey-2026-MustBeLongEnough!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Arquitectura";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ArquitecturaClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS - Allow all for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc6585#section-4",
            title = "Too Many Requests",
            status = 429,
            detail = "Rate limit exceeded. Try again in 60 seconds.",
            retryAfter = 60
        }, cancellationToken);
    };

    options.AddFixedWindowLimiter("fixed", config =>
    {
        config.PermitLimit = 100;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueLimit = 10;
    });
});

builder.Services.AddResponseCaching();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// MediatR + Behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Arquitectura.Application.Features.WeatherForecast.Queries.GetWeatherForecastQuery).Assembly);
});

builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ResilienceBehavior<,>));

// Register validators from Application assembly
builder.Services.AddValidatorsFromAssemblyContaining<Arquitectura.Application.Features.WeatherForecast.Commands.CreateWeatherForecastCommand>();

// Auth - User Repository and JWT Service
// IUserRepository ya se registro en AddPersistence con IDbConnectionFactory
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Hangfire with SQL Server storage - only if DB is available
var hangfireEnabled = !string.IsNullOrEmpty(connectionString) && connectionString.Contains("Server=");
if (hangfireEnabled)
{
    try
    {
        builder.Services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new Hangfire.SqlServer.SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        builder.Services.AddHangfireServer();
        builder.Services.AddScoped<IJobScheduler, HangfireJobScheduler>();
    }
    catch
    {
        // DB not available - skip Hangfire
    }
}

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<SqlServerHealthCheck>("sqlserver", tags: new[] { "ready" })
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" })
    .AddCheck<HangfireHealthCheck>("hangfire", tags: new[] { "ready" })
    .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready" });

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Schedule recurring jobs (only in development, and only if DB is available)
try
{
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var scheduler = scope.ServiceProvider.GetService<IJobScheduler>();
        if (scheduler != null)
        {
            scheduler.ScheduleCleanupOldForecasts();
            scheduler.ScheduleDailyReport();
            scheduler.ScheduleWeatherCheck();
            scheduler.ScheduleExternalSync();

            // Hangfire Dashboard
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
            });
        }
    }
}
catch
{
    // Skip jobs if DB not available
    app.Logger.LogWarning("Database not available - skipping scheduled jobs");
}

app.UseCors("AllowAll");
app.UseSecurityHeaders();
app.UseRateLimiter();
app.UseApiKey();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arquitectura API v1");
    c.RoutePrefix = "swagger"; // Opens at /swagger
});

app.UseAuthentication();
app.UseAuthorization();

app.UseCorrelationId();
app.UseAuditTrail();
app.UseRequestResponseLogging();
app.UseExceptionHandler();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();