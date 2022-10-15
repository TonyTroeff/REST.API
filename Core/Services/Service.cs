namespace Core.Services;

using System.Linq.Expressions;
using Core.Contracts.Options;
using Core.Contracts.Services;
using Core.Options;
using FluentValidation;
using Data.Contracts;
using Utilities;

public class Service<TEntity> : IService<TEntity>
    where TEntity : class, IEntity
{
    private readonly IRepository<TEntity> _repository;
    private readonly IValidator<TEntity> _validator;

    public Service(IRepository<TEntity> repository, IValidator<TEntity> validator = null)
    {
        this._repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this._validator = validator;
    }

    public async Task<OperationResult<bool>> AnyAsync(CancellationToken cancellationToken, IQueryEntityOptions<TEntity> options = null)
    {
        var operationResult = new OperationResult<bool>();

        var allFilters = (options?.AdditionalFilters).OrEmptyIfNull().IgnoreNullValues().ToArray();
        var anyEntityMatches = await this._repository.AnyAsync(allFilters, cancellationToken);
        if (!anyEntityMatches.IsSuccessful) return operationResult.AppendErrors(anyEntityMatches);

        return operationResult.WithData(anyEntityMatches.Data);
    }

    public async Task<OperationResult<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult<bool>();
        if (!operationResult.ValidateNotDefault(id)) return operationResult;

        var idFilter = ConstructIdFilter(id);
        var queryOptions = new QueryEntityOptions<TEntity>();
        queryOptions.AddFilter(idFilter);

        var anyAsync = await this.AnyAsync(cancellationToken, queryOptions);
        if (!anyAsync.IsSuccessful) return operationResult.AppendErrors(anyAsync);
        return operationResult.WithData(anyAsync.Data);
    }

    public async Task<OperationResult<TEntity>> GetAsync(Guid id, CancellationToken cancellationToken, IQueryEntityOptions<TEntity> options = null)
    {
        var operationResult = new OperationResult<TEntity>();
        if (!operationResult.ValidateNotDefault(id)) return operationResult;

        var idFilter = ConstructIdFilter(id);
        var getResult = await this._repository.GetAsync((options?.AdditionalFilters).OrEmptyIfNull().ConcatenateWith(idFilter), options?.Transforms, cancellationToken);
        if (!getResult.IsSuccessful) return operationResult.AppendErrors(getResult);

        return operationResult.WithData(getResult.Data);
    }

    public async Task<OperationResult<IEnumerable<TEntity>>> GetManyAsync(CancellationToken cancellationToken, IQueryEntityOptions<TEntity> options = null)
    {
        var operationResult = new OperationResult<IEnumerable<TEntity>>();

        var getResult = await this._repository.GetManyAsync(options?.AdditionalFilters, options?.Transforms, cancellationToken);
        if (!getResult.IsSuccessful) return operationResult.AppendErrors(getResult);

        return operationResult.WithData(getResult.Data);
    }

    public async Task<OperationResult<TEntity>> CreateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult<TEntity>();
        if (!operationResult.ValidateNotNull(entity)) return operationResult;

        var validateEntity = await this.ValidateAsync(entity, cancellationToken);
        if (!validateEntity.IsSuccessful) return operationResult.AppendErrors(validateEntity);

        entity.Created = entity.LastModified = GetCurrentTime();
        var createEntity = await this._repository.CreateAsync(entity, cancellationToken);
        if (!createEntity.IsSuccessful) return operationResult.AppendErrors(createEntity);

        return operationResult.WithData(entity);
    }

    public async Task<OperationResult<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult<TEntity>();
        if (!operationResult.ValidateNotNull(entity)) return operationResult;

        var validateEntity = await this.ValidateAsync(entity, cancellationToken);
        if (!validateEntity.IsSuccessful) return operationResult.AppendErrors(validateEntity);

        entity.LastModified = GetCurrentTime();
        var updateEntity = await this._repository.UpdateAsync(entity, cancellationToken);
        if (!updateEntity.IsSuccessful) return operationResult.AppendErrors(updateEntity);

        return operationResult.WithData(entity);
    }

    public async Task<OperationResult> DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult();
        if (!operationResult.ValidateNotNull(entity)) return operationResult;

        var deleteEntity = await this._repository.DeleteAsync(entity, cancellationToken);
        if (!deleteEntity.IsSuccessful) return operationResult.AppendErrors(deleteEntity);
        
        return operationResult;
    }

    private static Expression<Func<TEntity, bool>> ConstructIdFilter(Guid id) => x => x.Id == id;

    private static long GetCurrentTime() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private async Task<OperationResult> ValidateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult();
        if (!operationResult.ValidateNotNull(entity)) return operationResult;

        // If no explicit validator is registered in DI.
        if (this._validator is null) return operationResult;

        var validateEntity = await this._validator.ValidateAsync(entity, cancellationToken);
        if (validateEntity.IsValid) return operationResult;

        foreach (var error in validateEntity.Errors.Select(validationFailure => new Error { Message = validationFailure.ErrorMessage })) operationResult.AddError(error);
        return operationResult;
    }
}