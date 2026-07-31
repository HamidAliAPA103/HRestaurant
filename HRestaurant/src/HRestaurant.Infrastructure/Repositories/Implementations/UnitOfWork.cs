using HRestaurant.Data;
using HRestaurant.Models;
using HRestaurant.Models.BaseModels;
using HRestaurant.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace HRestaurant.Repositories.Implementations;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public IGenericRepository<Restaurant> Restaurants =>
        Repository<Restaurant>();

    public IGenericRepository<MenuCategory> Categories =>
        Repository<MenuCategory>();

    public IGenericRepository<Menu> MenuItems =>
        Repository<Menu>();

    public IGenericRepository<Order> Orders =>
        Repository<Order>();

    public IGenericRepository<Table> Tables =>
        Repository<Table>();

    public IGenericRepository<Reservation> Reservations =>
        Repository<Reservation>();

    public IGenericRepository<User> Employees =>
        Repository<User>();

    public IGenericRepository<OrderItem> OrderItems =>
        Repository<OrderItem>();

    public IGenericRepository<Review> Reviews =>
        Repository<Review>();

    public IGenericRepository<User> Users =>
        Repository<User>();

    public IGenericRepository<Shift> Shifts =>
        Repository<Shift>();

    public IGenericRepository<EmployeeShift> EmployeeShifts =>
        Repository<EmployeeShift>();

    public IGenericRepository<Ingredient> Ingredients =>
        Repository<Ingredient>();

    public IGenericRepository<TEntity> Repository<TEntity>()
        where TEntity : BaseEntity
    {
        ThrowIfDisposed();

        var entityType = typeof(TEntity);

        if (_repositories.TryGetValue(entityType, out var repository))
        {
            return (IGenericRepository<TEntity>)repository;
        }

        var newRepository = new GenericRepository<TEntity>(_context);
        _repositories.Add(entityType, newRepository);

        return newRepository;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_currentTransaction is not null)
        {
            throw new InvalidOperationException(
                "An active database transaction already exists.");
        }

        _currentTransaction = await _context.Database
            .BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var transaction = GetCurrentTransaction();

        try
        {
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception commitException)
        {
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _context.ChangeTracker.Clear();
            }
            catch (Exception rollbackException)
            {
                throw new AggregateException(
                    "The transaction commit and the subsequent rollback both failed.",
                    commitException,
                    rollbackException);
            }

            throw;
        }
        finally
        {
            await DisposeCurrentTransactionAsync(transaction);
        }
    }

    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var transaction = GetCurrentTransaction();

        try
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            _context.ChangeTracker.Clear();
            await DisposeCurrentTransactionAsync(transaction);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            if (_currentTransaction is not null)
            {
                var transaction = _currentTransaction;

                try
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                }
                finally
                {
                    _context.ChangeTracker.Clear();
                    await DisposeCurrentTransactionAsync(transaction);
                }
            }
        }
        finally
        {
            await _context.DisposeAsync();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    private IDbContextTransaction GetCurrentTransaction()
    {
        return _currentTransaction
            ?? throw new InvalidOperationException(
                "There is no active database transaction.");
    }

    private async ValueTask DisposeCurrentTransactionAsync(
        IDbContextTransaction transaction)
    {
        if (ReferenceEquals(_currentTransaction, transaction))
        {
            _currentTransaction = null;
        }

        await transaction.DisposeAsync();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
