using Serilog;

namespace VsaTemplate.DependencyInjection;

public static class LoggingHostBuilderExtensions
{
    public static IHostBuilder AddApplicationLogging(this IHostBuilder host)
    {
#if OTEL_ENABLED
        // writeToProviders forwards Serilog events to the OpenTelemetry logger
        // provider, so logs reach the OTLP exporter alongside traces and metrics.
        // Default providers are cleared first: Serilog already writes to the console,
        // and the built-in console provider would print every event twice.
        host.ConfigureLogging(logging => logging.ClearProviders());
        host.UseSerilog(
            (context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration),
            writeToProviders: true);
#else
        host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));
#endif

        return host;
    }
}
