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
        var direction = string.Equals(orderBy, "desc", StringComparison.OrdinalIgnoreCase)
            ? "descending"
            : "ascending";

        var property = ResolveProperty(typeof(T), sortBy) ?? ResolveProperty(typeof(T), defaultSortBy);
        if (property is null)
        {
            return query;
        }

        return query.OrderBy($"{property} {direction}");
    }

    private static string? ResolveProperty(Type type, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return type.GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)?.Name;
    }
}
