namespace Core.Contracts.Services;

using Core.Contracts.Options;
using Data.Contracts;
using Utilities;

public interface IService<TEntity>
    where TEntity : class, IEntity
{
    Task<OperationResult<bool>> AnyAsync(CancellationToken cancellationToken, IQueryEntityOptions<TEntity> options = null);
    Task<OperationResult<TEntity>> GetAsync(Guid id, CancellationToken cancellationToken, IQueryEntityOptions<TEntity> options = null);
    Task<OperationResult<IEnumerable<TEntity>>> GetManyAsync(CancellationToken cancellationToken, IQueryEntityOptions<TEntity> options = null);
    Task<OperationResult<TEntity>> CreateAsync(TEntity entity, CancellationToken cancellationToken);
    Task<OperationResult<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken);
    Task<OperationResult> DeleteAsync(TEntity entity, CancellationToken cancellationToken);
}