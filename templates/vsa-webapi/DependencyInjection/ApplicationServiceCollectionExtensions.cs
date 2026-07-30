using FluentValidation;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Model;
using Mapster;
using MapsterMapper;
#if AUTH_IDENTITY
using Microsoft.AspNetCore.Identity;
#endif
using VsaTemplate.Data;
#if AUTH_IDENTITY
using VsaTemplate.Data.Entities;
#endif
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

namespace VsaTemplate.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IHostBuilder AddApplicationMessaging(
        this IHostBuilder host,
        IHostEnvironment environment)
    {
        host.UseWolverine(options =>
        {
            var canCompileAtRuntime = File.Exists(Path.Combine(
                AppContext.BaseDirectory,
                "Wolverine.RuntimeCompilation.dll"));

            options.CodeGeneration.TypeLoadMode =
                environment.IsDevelopment() && canCompileAtRuntime
                    ? TypeLoadMode.Auto
                    : TypeLoadMode.Static;

            options.ServiceLocationPolicy = ServiceLocationPolicy.NotAllowed;
            options.CodeGeneration.AlwaysUseServiceLocationFor<AppDbContext>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<IMapper>();
            AllowFluentValidationProblemDetailSources(options);
#if AUTH_IDENTITY
            options.CodeGeneration.AlwaysUseServiceLocationFor<UserManager<ApplicationUser>>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<SignInManager<ApplicationUser>>();
#endif

            options.UseFluentValidation();
            options.Policies.AutoApplyTransactions();
        });

        return host;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.CritterStackDefaults(options =>
        {
            options.Production.GeneratedCodeMode = TypeLoadMode.Static;
            options.Production.AssertAllPreGeneratedTypesExist = true;
        });

        services.AddWolverineHttp();

        var mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(typeof(Program).Assembly);
        services.AddSingleton(mapsterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    private static void AllowFluentValidationProblemDetailSources(WolverineOptions options)
    {
        var requestTypes = typeof(Program).Assembly
            .GetTypes()
            .SelectMany(type => type.GetInterfaces())
            .Where(type =>
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IValidator<>))
            .Select(type => type.GetGenericArguments()[0])
            .Distinct();

        foreach (var requestType in requestTypes)
        {
            var problemDetailSourceType =
                typeof(IProblemDetailSource<>).MakeGenericType(requestType);

            options.CodeGeneration.AlwaysUseServiceLocationFor(problemDetailSourceType);
        }
    }
}
