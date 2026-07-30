using System.Text.Json;
using System.Text.Json.Serialization;
#if CACHE_OUTPUT
using Microsoft.AspNetCore.OutputCaching;
#endif
#if APIDOCS_SCALAR
using Microsoft.OpenApi;
#endif
using VsaTemplate.Extensions;

namespace VsaTemplate.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public const string DefaultCorsPolicy = "DefaultPolicy";

    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
#if APIDOCS_SWAGGER
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
#if AUTH_ENABLED
            options.AddSecurityDefinition(
                "Bearer",
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

            options.AddSecurityRequirement(
                new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
#endif
        });
#elif APIDOCS_SCALAR
        services.AddOpenApi(options =>
        {
#if AUTH_ENABLED
            options.AddDocumentTransformer((document, _, _) =>
            {
#if AUTH_KEYCLOAK
                document.Info.Title += " - Keycloak";
#elif AUTH_IDENTITY
                document.Info.Title += " - Identity";
#endif
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??=
                    new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] =
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        Description = "JWT Authorization header using the Bearer scheme."
                    };

                document.Security ??= [];
                document.Security.Add(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecuritySchemeReference("Bearer"),
                            []
                        }
                    });

                return Task.CompletedTask;
            });
#endif
        });
#endif

        var corsOrigins =
            configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (corsOrigins.Length == 0 && environment.IsProduction())
        {
            throw new InvalidOperationException(
                "Cors:AllowedOrigins must be configured in Production.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicy, policy =>
            {
                if (corsOrigins.Length > 0)
                {
                    policy.WithOrigins(corsOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                    return;
                }

                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DefaultIgnoreCondition =
                JsonIgnoreCondition.WhenWritingNull;
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddHealthChecks();

#if CACHE_OUTPUT
        services.AddOutputCache(options =>
        {
            options.AddPolicy(
                "ApiGet",
                policy => policy
                    .Expire(TimeSpan.FromSeconds(60))
                    .SetVaryByQuery("*"));
        });
#endif
        services.AddApplicationRateLimiting(configuration);

        return services;
    }
}
