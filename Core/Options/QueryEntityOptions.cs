namespace Core.Options;

using System.Linq.Expressions;
using Core.Contracts.Options;

public class QueryEntityOptions<TEntity> : IQueryEntityOptions<TEntity>
{
    private readonly List<Expression<Func<TEntity, bool>>> _additionalFilters = new();
    private readonly List<Func<IQueryable<TEntity>, IQueryable<TEntity>>> _transforms = new();

    public IReadOnlyCollection<Expression<Func<TEntity, bool>>> AdditionalFilters => this._additionalFilters.AsReadOnly();
    public IReadOnlyCollection<Func<IQueryable<TEntity>, IQueryable<TEntity>>> Transforms => this._transforms.AsReadOnly();

    public bool AddFilter(Expression<Func<TEntity, bool>> filter)
    {
        if (filter is null) return false;

        this._additionalFilters.Add(filter);
        return true;
    }
    
    public bool AddTransform(Func<IQueryable<TEntity>, IQueryable<TEntity>> transform)
    {
        if (transform is null) return false;

        this._transforms.Add(transform);
        return true;
    }
}