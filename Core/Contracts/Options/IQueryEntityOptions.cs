namespace Core.Contracts.Options;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

public interface IQueryEntityOptions<TEntity>
{
    IReadOnlyCollection<Expression<Func<TEntity, bool>>> AdditionalFilters { get; }
}