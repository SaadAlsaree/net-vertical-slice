using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
#if CACHE_OUTPUT
using Microsoft.AspNetCore.OutputCaching;
#endif
using Serilog;
using VsaTemplate.Data;
using VsaTemplate.Extensions;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

#if APIDOCS_SCALAR
using Scalar.AspNetCore;
#endif
#if AUTH_KEYCLOAK
using VsaTemplate.Models;
#endif

var builder = WebApplication.CreateBuilder(args);

// 1. Logging
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

// 2. Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<AuditSaveChangesInterceptor>();

builder.Services.AddDbContextPool<AppDbContext>((sp, options) =>
{
#if DB_SQLSERVER
    options.UseSqlServer(connectionString);
#elif DB_SQLITE
    options.UseSqlite(connectionString);
#else
    options.UseNpgsql(connectionString);
#endif
    options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
});

// 3. Wolverine
builder.Host.UseWolverine(options =>
{
    options.UseFluentValidation();
    options.Policies.AutoApplyTransactions();
});

// 4. Wolverine HTTP
builder.Services.AddWolverineHttp();

#if AUTH_KEYCLOAK
var keycloakSettings = builder.Configuration
    .GetSection("Keycloak")
    .Get<KeycloakSettings>() ?? throw new InvalidOperationException("Keycloak configuration section is missing or invalid.");
#endif

// 5. API Documentation
#if APIDOCS_SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
#if AUTH_KEYCLOAK
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
builder.Services.AddOpenApi(options =>
{
#if AUTH_KEYCLOAK
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title += " - Keycloak";
        document.Components ??= new Microsoft.OpenApi.OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme."
        };

        document.Security ??= [];
        document.Security.Add(new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            { new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
        });

        return Task.CompletedTask;
    });
#endif
});
#endif

// 6. Mapster
builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// 7. FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 8. CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (corsOrigins.Length == 0 && builder.Environment.IsProduction())
{
    throw new InvalidOperationException("Cors:AllowedOrigins must be configured in Production. Add it to appsettings.Production.json or via environment variables.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        if (corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Development fallback: permissive
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

// 9. JSON options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// 10. Exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

#if AUTH_KEYCLOAK
// 11. Authentication
builder.Services.AddKeycloakAuthentication(keycloakSettings, builder.Environment);
#endif

#if CACHE_OUTPUT
// 12. Output caching
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("ApiGet", policy =>
        policy
            .Expire(TimeSpan.FromSeconds(60))
            .SetVaryByQuery("*"));
});
#endif

var app = builder.Build();

// Middleware pipeline
app.UseExceptionHandler();
app.UseCors("DefaultPolicy");
#if AUTH_KEYCLOAK
app.UseAuthentication();
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

app.MapWolverineEndpoints();

app.Run();
