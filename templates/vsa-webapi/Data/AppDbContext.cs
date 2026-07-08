using Microsoft.EntityFrameworkCore;
#if AUTH_IDENTITY
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
#endif
using VsaTemplate.Data.Common;
using VsaTemplate.Data.Entities;

namespace VsaTemplate.Data;

#if AUTH_IDENTITY
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
#else
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
#endif
{
    public DbSet<Sample> Samples => Set<Sample>();

#if AUTH_IDENTITY
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
#endif

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
#if AUTH_IDENTITY
        base.OnModelCreating(modelBuilder);
#endif

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(
                        GenerateSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    private static System.Linq.Expressions.LambdaExpression GenerateSoftDeleteFilter(Type entityType)
    {
        var param = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var prop = System.Linq.Expressions.Expression.Property(param, nameof(BaseEntity.IsDeleted));
        var filter = System.Linq.Expressions.Expression.Not(prop);
        return System.Linq.Expressions.Expression.Lambda(filter, param);
    }
}
