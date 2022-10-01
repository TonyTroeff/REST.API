namespace Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
}