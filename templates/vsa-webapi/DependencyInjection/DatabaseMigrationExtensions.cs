using Microsoft.EntityFrameworkCore;
using VsaTemplate.Data;

namespace VsaTemplate.DependencyInjection;

public static class DatabaseMigrationExtensions
{
    public const string MigrateOnStartupKey = "Database:MigrateOnStartup";

    /// <summary>
    /// Applies pending EF Core migrations when <c>Database:MigrateOnStartup</c> is true
    /// (set as <c>Database__MigrateOnStartup=true</c> in .env for Docker Compose).
    /// </summary>
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>(MigrateOnStartupKey))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DatabaseMigrationExtensions));

        var pending = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();
        if (pending.Count == 0)
        {
            logger.LogInformation("Database is up to date; no migrations to apply.");
            return;
        }

        logger.LogInformation(
            "Applying {Count} migration(s): {Migrations}",
            pending.Count,
            string.Join(", ", pending));

        await dbContext.Database.MigrateAsync();
    }
}
