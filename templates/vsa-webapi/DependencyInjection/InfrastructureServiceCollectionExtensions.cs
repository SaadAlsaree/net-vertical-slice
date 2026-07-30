using Microsoft.EntityFrameworkCore;
using VsaTemplate.Data;

namespace VsaTemplate.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AuditSaveChangesInterceptor>();

        services.AddDbContextPool<AppDbContext>((serviceProvider, options) =>
        {
#if DB_SQLSERVER
            options.UseSqlServer(connectionString);
#elif DB_SQLITE
            options.UseSqlite(connectionString);
#else
            options.UseNpgsql(connectionString);
#endif
            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        return services;
    }
}
