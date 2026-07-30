using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
#if APIDOCS_SCALAR
using Scalar.AspNetCore;
#endif
using Serilog;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

namespace VsaTemplate.DependencyInjection;

public static class WebApplicationExtensions
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        app.MapHealthChecks(
                "/health/live",
                new HealthCheckOptions { Predicate = _ => false })
            .AllowAnonymous()
            .DisableRateLimiting();

        app.MapHealthChecks("/health/ready")
            .AllowAnonymous()
            .DisableRateLimiting();

        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();
        app.UseCors(ApiServiceCollectionExtensions.DefaultCorsPolicy);
#if AUTH_ENABLED
        app.UseAuthentication();
#endif
        app.UseRateLimiter();
#if AUTH_ENABLED
        app.UseAuthorization();
#endif
#if CACHE_OUTPUT
        app.UseOutputCache();
#endif

        if (app.Environment.IsDevelopment())
        {
#if APIDOCS_SWAGGER
            app.UseSwagger();
            app.UseSwaggerUI();
#elif APIDOCS_SCALAR
            app.MapOpenApi();
            app.MapScalarApiReference();
#endif
        }

        app.MapWolverineEndpoints(options =>
        {
            options.WarmUpRoutes = RouteWarmup.Eager;
            options.UseFluentValidationProblemDetailMiddleware();
        });

        return app;
    }
}
