using System.Linq.Expressions;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Repositories.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    IQueryable<T> GetQueryable(bool asNoTracking = true);

    Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default);

    void Update(T entity);

    void Delete(T entity);

    Task<bool> AnyAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}
