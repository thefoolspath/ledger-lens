using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    private const string LivenessTag = "live";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });
        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation());
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }
        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), [LivenessTag]);
        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app, string serviceName)
    {
        app.Use(async (context, next) =>
        {
            const string headerName = "X-Correlation-ID";
            var suppliedId = context.Request.Headers[headerName].FirstOrDefault();
            var correlationId = !string.IsNullOrWhiteSpace(suppliedId) && suppliedId.Length <= 128
                ? suppliedId
                : Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
            context.TraceIdentifier = correlationId;
            context.Request.Headers[headerName] = correlationId;
            context.Response.Headers[headerName] = correlationId;
            using (context.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("LedgerLens.Correlation")
                .BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            {
                await next(context);
            }
        });

        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(LivenessTag),
        });
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/internal/ping", () => new FoundationPing(
                serviceName,
                Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown"));
        }
        return app;
    }

    private sealed record FoundationPing(string Service, string Version);
}
