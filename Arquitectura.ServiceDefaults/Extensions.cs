using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using ResulTus.Diagnostics;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace Microsoft.Extensions.Hosting;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(ResultMetrics.MeterName)
                    .AddPrometheusExporter();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddSource(ResultDiagnostics.SourceName)
                    // The data providers do not publish spans on their own. These two adapters turn
                    // their diagnostics into activities, so a slow request shows the query behind it.
                    .AddSqlClientInstrumentation()
                    .AddRedisInstrumentation()
                    .AddAspNetCoreInstrumentation(tracing =>
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                            && !context.Request.Path.StartsWithSegments("/metrics")
                    )
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        var seqUrl = builder.Configuration["Seq:Url"];
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            if (builder.Environment.IsDevelopment())
            {
                // Surfaces a failing log sink on stderr instead of dropping events silently.
                Serilog.Debugging.SelfLog.Enable(Console.Error);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                // Serilog ignores Logging:LogLevel from appsettings, so ASP.NET Core's per-request
                // chatter (request starting/finished, endpoint executed, status set) is silenced here.
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
                // Seq's own sink speaks CLEF over HTTP. Its OTLP ingest aborted the OTLP sink's
                // batches, so traces and metrics travel over OTLP while logs use this transport.
                .WriteTo.Seq(seqUrl)
                .CreateLogger();

            builder.Services.AddSerilog(Log.Logger);
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Health endpoints are mapped in every environment so container orchestrators can probe the
        // service in production as well. They expose only the aggregate status, never check details.
        app.MapHealthChecks(HealthEndpointPath);

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return app;
    }
}
