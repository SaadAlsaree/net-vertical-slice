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

var builder = WebApplication.CreateBuilder(args);

// 1. Logging
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

// 2. Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContextPool<AppDbContext>(options =>
{
#if DB_SQLSERVER
    options.UseSqlServer(connectionString);
#elif DB_SQLITE
    options.UseSqlite(connectionString);
#else
    options.UseNpgsql(connectionString);
#endif
});

// 3. Wolverine
builder.Host.UseWolverine(options =>
{
    options.UseFluentValidation();
    options.Policies.AutoApplyTransactions();
});

// 4. Wolverine HTTP
builder.Services.AddWolverineHttp();

// 5. API Documentation
#if APIDOCS_SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#elif APIDOCS_SCALAR
builder.Services.AddOpenApi();
#endif

// 6. Mapster
builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// 7. FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 8. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

#if CACHE_OUTPUT
// 11. Output caching
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
