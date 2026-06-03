using System.Linq.Dynamic.Core;
using System.Reflection;

namespace VsaTemplate.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sortBy,
        string? orderBy,
        string defaultSortBy = "Id")
    {
        var property = sortBy ?? defaultSortBy;
        var direction = string.Equals(orderBy, "desc", StringComparison.OrdinalIgnoreCase)
            ? "descending"
            : "ascending";

        var entityProperty = typeof(T).GetProperty(
            property,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (entityProperty is null)
        {
            property = defaultSortBy;
        }

        return query.OrderBy($"{property} {direction}");
    }
}
