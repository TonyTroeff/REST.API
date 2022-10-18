namespace Data.Contracts;

using System.Linq.Expressions;
using Utilities;

public interface IRepository<TEntity>
    where TEntity : class, IEntity
{
    Task<OperationResult<bool>> AnyAsync(IEnumerable<Expression<Func<TEntity, bool>>> filters, CancellationToken cancellationToken);
    Task<OperationResult<TEntity>> GetAsync(IEnumerable<Expression<Func<TEntity, bool>>> filters, IEnumerable<Func<IQueryable<TEntity>, IQueryable<TEntity>>> transforms, CancellationToken cancellationToken);
    Task<OperationResult<IEnumerable<TEntity>>> GetManyAsync(IEnumerable<Expression<Func<TEntity, bool>>> filters, IEnumerable<Func<IQueryable<TEntity>, IQueryable<TEntity>>> transforms, CancellationToken cancellationToken);
    Task<OperationResult> CreateAsync(TEntity entity, CancellationToken cancellationToken);
    Task<OperationResult> UpdateAsync(TEntity entity, CancellationToken cancellationToken);
    Task<OperationResult> DeleteAsync(TEntity entity, CancellationToken cancellationToken);
}