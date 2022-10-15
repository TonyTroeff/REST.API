namespace Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Utilities;

public static class QueryableExtensions
{
    public static IQueryable<T> Filter<T>(this IQueryable<T> queryable, IEnumerable<Expression<Func<T, bool>>> filters)
    {
        if (queryable is null) throw new ArgumentNullException(nameof(queryable));

        foreach (var filter in filters.OrEmptyIfNull().IgnoreNullValues())
            queryable = queryable.Where(filter);

        return queryable;
    }

    public static IQueryable<T> Transform<T>(this IQueryable<T> queryable, IEnumerable<Func<IQueryable<T>, IQueryable<T>>> transforms)
        where T : class
    {
        if (queryable is null) throw new ArgumentNullException(nameof(queryable));

        foreach (var transform in transforms.OrEmptyIfNull().IgnoreNullValues()) queryable = transform(queryable);
        return queryable;
    }
}