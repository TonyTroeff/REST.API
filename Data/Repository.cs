namespace Data;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using Data.Extensions;
using Utilities;

public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbContext _dbContext;

    public Repository(DbContext schedulerDbContext)
    {
        this._dbContext = schedulerDbContext ?? throw new ArgumentNullException(nameof(schedulerDbContext));
    }

    public async Task<OperationResult> CreateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult();
        if (operationResult.ValidateNotNull(entity) == false) return operationResult;

        try
        {
            await this._dbContext.AddAsync(entity, cancellationToken);
            await this._dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            operationResult.AddException(e);
        }

        return operationResult;
    }

    public async Task<OperationResult<bool>> AnyAsync(IEnumerable<Expression<Func<TEntity, bool>>> filters, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult<bool>();

        try
        {
            var result = await this._dbContext.Set<TEntity>().Filter(filters).AnyAsync(cancellationToken);
            operationResult.Data = result;
        }
        catch (Exception e)
        {
            operationResult.AddException(e);
        }

        return operationResult;
    }

    public async Task<OperationResult<TEntity>> GetAsync(IEnumerable<Expression<Func<TEntity, bool>>> filters, IEnumerable<Func<IQueryable<TEntity>, IQueryable<TEntity>>> transforms, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult<TEntity>();

        try
        {
            var result = await this._dbContext.Set<TEntity>().Filter(filters).Transform(transforms).FirstOrDefaultAsync(cancellationToken);
            operationResult.Data = result;
        }
        catch (Exception e)
        {
            operationResult.AddException(e);
        }

        return operationResult;
    }

    public async Task<OperationResult<TLayout>> GetAsync<TLayout>(IEnumerable<Expression<Func<TEntity, bool>>> filters, Expression<Func<TEntity, TLayout>> projection, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult<TLayout>();

        try
        {
            var result = await this._dbContext.Set<TEntity>().Filter(filters).Select(projection).FirstOrDefaultAsync(cancellationToken);
            operationResult.Data = result;
        }

        catch (Exception e)
        {
            operationResult.AddException(e);
        }

        return operationResult;
    }

    public async Task<OperationResult<IEnumerable<TEntity>>> GetManyAsync(IEnumerable<Expression<Func<TEntity, bool>>> filters, IEnumerable<Func<IQueryable<TEntity>, IQueryable<TEntity>>> transforms, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult<IEnumerable<TEntity>>();

        try
        {
            var result = await this._dbContext.Set<TEntity>().Filter(filters).Transform(transforms).ToListAsync(cancellationToken);
            operationResult.Data = result;
        }
        catch (Exception e)
        {
            operationResult.AddException(e);
        }

        return operationResult;
    }

    public async Task<OperationResult<IEnumerable<TLayout>>> GetManyAsync<TLayout>(IEnumerable<Expression<Func<TEntity, bool>>> filters, Expression<Func<TEntity, TLayout>> projection, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult<IEnumerable<TLayout>>();

        try
        {
            var result = await this._dbContext.Set<TEntity>().Filter(filters).Select(projection).ToListAsync(cancellationToken);
            operationResult.Data = result;
        }

        catch (Exception e)
        {
            operationResult.AddException(e);
        }

        return operationResult;
    }

    public async Task<OperationResult> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult();
        if (operationResult.ValidateNotNull(entity) == false) return operationResult;

        try
        {
            var trackedEntity = this._dbContext.Set<TEntity>().Local.FirstOrDefault(x => x.Id == entity.Id);
            if (trackedEntity != null) this._dbContext.Entry(trackedEntity).State = EntityState.Detached;
            this._dbContext.Entry(entity).State = EntityState.Modified;

            this._dbContext.Update(entity);
            await this._dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            operationResult.AddException(e);
        }

        return operationResult;
    }

    public async Task<OperationResult> DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult();
        if (operationResult.ValidateNotNull(entity) == false) return operationResult;

        try
        {
            this._dbContext.Remove(entity);
            await this._dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            operationResult.AddException(e);
        }

        return operationResult;
    }
}