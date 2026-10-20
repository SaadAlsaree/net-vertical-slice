#if OTEL_ENABLED
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
#endif

namespace VsaTemplate.DependencyInjection;

public static class ObservabilityServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationObservability(
        this IServiceCollection services)
    {
#if OTEL_ENABLED
        var serviceName =
            typeof(Program).Assembly.GetName().Name ?? "VsaTemplate";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddSource("Wolverine"))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("Microsoft.AspNetCore.RateLimiting")
                .AddMeter("Wolverine*"))
            .WithLogging()
            .UseOtlpExporter();
#endif

        return services;
    }
}
