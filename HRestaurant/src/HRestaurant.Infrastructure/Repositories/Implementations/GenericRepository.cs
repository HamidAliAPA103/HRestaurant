using System.Linq.Expressions;
using HRestaurant.Data;
using HRestaurant.Models.BaseModels;
using HRestaurant.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Repositories.Implementations;

public sealed class GenericRepository<T> : IGenericRepository<T>
    where T : BaseEntity
{
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _dbSet = context.Set<T>();
    }

    public IQueryable<T> GetQueryable(bool asNoTracking = true)
    {
        return asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
    }

    public Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.ID == id, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Remove(entity);
    }

    public Task<bool> AnyAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? _dbSet.AnyAsync(cancellationToken)
            : _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? _dbSet.CountAsync(cancellationToken)
            : _dbSet.CountAsync(predicate, cancellationToken);
    }
}
