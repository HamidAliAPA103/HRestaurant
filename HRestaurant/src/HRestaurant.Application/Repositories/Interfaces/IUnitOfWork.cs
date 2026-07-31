using HRestaurant.Models;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Repositories.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IGenericRepository<Restaurant> Restaurants { get; }
    IGenericRepository<MenuCategory> Categories { get; }
    IGenericRepository<Menu> MenuItems { get; }
    IGenericRepository<Order> Orders { get; }
    IGenericRepository<Table> Tables { get; }
    IGenericRepository<Reservation> Reservations { get; }
    IGenericRepository<User> Employees { get; }
    IGenericRepository<OrderItem> OrderItems { get; }
    IGenericRepository<Review> Reviews { get; }
    IGenericRepository<User> Users { get; }
    IGenericRepository<Shift> Shifts { get; }
    IGenericRepository<EmployeeShift> EmployeeShifts { get; }
    IGenericRepository<Ingredient> Ingredients { get; }

    IGenericRepository<TEntity> Repository<TEntity>()
        where TEntity : BaseEntity;

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(
        CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(
        CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default);
}
