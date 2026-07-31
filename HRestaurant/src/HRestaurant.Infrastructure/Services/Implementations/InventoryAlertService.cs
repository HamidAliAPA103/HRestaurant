using HRestaurant.Configuration;
using HRestaurant.Data;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace HRestaurant.Services.Implementations;

public sealed class InventoryAlertService : IInventoryAlertService
{
    private readonly AppDbContext _db;
    private readonly InventoryAlertSettings _settings;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<InventoryAlertService> _logger;

    public InventoryAlertService(
        AppDbContext db,
        InventoryAlertSettings settings,
        TimeProvider timeProvider,
        ILogger<InventoryAlertService> logger)
    {
        _db = db;
        _settings = settings;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task EvaluateItemAsync(
        Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        await using var ownedTransaction = _db.Database.CurrentTransaction is null
            ? await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken)
            : null;
        var item = await _db.InventoryItems.Include(x => x.Ingredient)
            .FirstOrDefaultAsync(x => x.ID == inventoryItemId && !x.IsDeleted,
                cancellationToken);
        if (item is null) return;

        var today = DateOnly.FromDateTime(UtcNow);
        var expiringLimit = today.AddDays(_settings.ExpiringSoonDays);
        var desired = new Dictionary<InventoryAlertType, bool>
        {
            [InventoryAlertType.LowStock] = item.IsActive
                && item.CurrentQuantity < item.MinimumQuantity,
            [InventoryAlertType.OutOfStock] = item.IsActive
                && item.CurrentQuantity == 0,
            [InventoryAlertType.ExpiringSoon] = item.IsActive
                && item.ExpirationDate.HasValue
                && item.ExpirationDate.Value >= today
                && item.ExpirationDate.Value <= expiringLimit,
            [InventoryAlertType.Expired] = item.IsActive
                && item.ExpirationDate.HasValue
                && item.ExpirationDate.Value < today
        };

        var existing = await _db.InventoryNotifications.Where(x =>
            x.InventoryItemId == item.ID && !x.IsDeleted && !x.IsResolved)
            .ToListAsync(cancellationToken);
        var now = UtcNow;
        foreach (var pair in desired)
        {
            if (pair.Value)
            {
                if (!existing.Any(x => x.Type == pair.Key && !x.IsRead))
                    _db.InventoryNotifications.Add(CreateNotification(item, pair.Key, now));
            }
            else
            {
                foreach (var alert in existing.Where(x => x.Type == pair.Key))
                {
                    alert.IsResolved = true;
                    alert.ResolvedAtUtc = now;
                    alert.UpdateAt = now;
                }
            }
        }
        await _db.SaveChangesAsync(cancellationToken);
        if (ownedTransaction is not null)
            await ownedTransaction.CommitAsync(cancellationToken);
    }

    public async Task<int> ScanAsync(CancellationToken cancellationToken = default)
    {
        var ids = await _db.InventoryItems.AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive)
            .Select(x => x.ID).ToListAsync(cancellationToken);
        var processed = 0;
        foreach (var id in ids)
        {
            try
            {
                await EvaluateItemAsync(id, cancellationToken);
                processed++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Inventory alert evaluation failed for item {InventoryItemId}.", id);
                _db.ChangeTracker.Clear();
            }
        }
        return processed;
    }

    private static InventoryNotification CreateNotification(
        InventoryItem item, InventoryAlertType type, DateTime now)
    {
        var (title, message) = type switch
        {
            InventoryAlertType.LowStock => ("Low stock",
                $"{item.Ingredient.Name} stock is below the minimum quantity."),
            InventoryAlertType.OutOfStock => ("Out of stock",
                $"{item.Ingredient.Name} is out of stock."),
            InventoryAlertType.ExpiringSoon => ("Expiring soon",
                $"{item.Ingredient.Name} expires on {item.ExpirationDate:yyyy-MM-dd}."),
            _ => ("Expired",
                $"{item.Ingredient.Name} expired on {item.ExpirationDate:yyyy-MM-dd}.")
        };
        return new InventoryNotification
        {
            RestaurantId = item.RestaurantId,
            BranchId = item.BranchId,
            InventoryItemId = item.ID,
            Type = type,
            Title = title,
            Message = message,
            CreatAt = now
        };
    }

    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
