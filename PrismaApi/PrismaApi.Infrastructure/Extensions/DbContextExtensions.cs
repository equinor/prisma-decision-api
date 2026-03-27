using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace PrismaApi.Infrastructure.Extensions;

public static class DbContextExtensions
{
    public static IQueryable<T> ConditionalInclude<T, TProperty>(
        this IQueryable<T> queryable,
        bool include,
        Expression<Func<T, TProperty>> navigationProperty
    ) where T : class =>
        include ? queryable.Include(navigationProperty) : queryable;

    public static IQueryable<T> ConditionalInclude<T, TProperty, TProperty2>(
        this IQueryable<T> queryable,
        bool include,
        Expression<Func<T, IEnumerable<TProperty>>> navigationProperty,
        Expression<Func<TProperty, TProperty2>> thenIncludeProperty
    ) where T : class =>
        include
            ? queryable.Include(navigationProperty).ThenInclude(thenIncludeProperty)
            : queryable;

    public static IQueryable<T> ConditionalWhere<T>(
        this IQueryable<T> queryable,
        bool applyFilter,
        Expression<Func<T, bool>> predicate
    ) where T : class =>
        applyFilter ? queryable.Where(predicate) : queryable;

    public static IQueryable<T> OptionalWhere<T>(
        this IQueryable<T> queryable,
        Expression<Func<T, bool>>? predicate
    ) where T : class =>
        predicate != null ? queryable.Where(predicate) : queryable;
}

