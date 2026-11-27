using System.Text;
using Arquitectura.Host.Identity;
using Arquitectura.Host.Web.HealthChecks;
using Arquitectura.Host.Web.Middleware;
using BuildingBlocks.Abstractions.Identity;
using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Infrastructure.HealthChecks;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Modules.Auth.Infrastructure.DependencyInjection;
using Modules.DataTable.Infrastructure.DependencyInjection;
using Modules.Weather.Infrastructure.DependencyInjection;
using Modules.Weather.Infrastructure.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Shared technical infrastructure: mediator, Dapper, unit of work and Redis.
builder.Services.AddBuildingBlocksInfrastructure(builder.Configuration);

// The host resolves the authenticated caller from the transport.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Compose the modules. Each module owns its own registrations.
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddWeatherModule(builder.Configuration);
builder.Services.AddDataTableModule(builder.Configuration);

// JWT bearer validation. There is no hardcoded fallback secret.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "The 'Jwt:Key' setting is required. Provide it through configuration or the 'Jwt__Key' environment variable.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Arquitectura";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ArquitecturaClients";

builder.Services
    .AddAuthentication(options =>
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
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

// Hangfire with SQL Server storage, only when a connection string is configured.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var hangfireEnabled = !string.IsNullOrWhiteSpace(connectionString);

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
        builder.Services.AddWeatherModuleJobs();
    }
    catch (Exception exception)
    {
        hangfireEnabled = false;
        Console.Error.WriteLine($"Hangfire could not be configured and will be disabled: {exception.Message}");
    }
}

builder.Services.AddHealthChecks()
    .AddCheck<SqlServerHealthCheck>("sqlserver", tags: ["ready"])
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"])
    .AddCheck<HangfireHealthCheck>("hangfire", tags: ["ready"])
    .AddCheck<RedisHealthCheck>("redis", tags: ["ready"]);

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

if (hangfireEnabled && app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var scheduler = scope.ServiceProvider.GetService<IJobScheduler>();
        scheduler?.ScheduleCleanupOldForecasts();
        scheduler?.ScheduleDailyReport();
        scheduler?.ScheduleWeatherCheck();
        scheduler?.ScheduleExternalSync();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new HangfireDashboardAuthorizationFilter(app.Environment)]
        });
    }
    catch (Exception exception)
    {
        app.Logger.LogWarning(exception, "Database not available - skipping scheduled jobs");
    }
}

app.UseCors("AllowAll");
app.UseSecurityHeaders();
app.UseRateLimiter();
app.UseApiKey();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Arquitectura API v1");
    options.RoutePrefix = "swagger";
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

/// <summary>Exposes the entry point to integration tests.</summary>
public partial class Program;
