using System.Data;
using System.Security.Cryptography;
using AutoMapper;
using HRestaurant.Configuration;
using HRestaurant.Data;
using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.OrderItem;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Exceptions;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace HRestaurant.Services.Implementations;

public sealed class OrderService : IOrderService
{
    private static readonly OrderStatus[] KitchenStatuses =
        [OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Preparing, OrderStatus.Ready];
    private static readonly OrderStatus[] TableBlockingStatuses =
        [OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Preparing,
            OrderStatus.Ready, OrderStatus.Served];

    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;
    private readonly IInventoryAlertService _alerts;
    private readonly IKitchenNotifier _notifier;
    private readonly OrderWorkflowSettings _settings;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        AppDbContext db,
        IMapper mapper,
        ICurrentUserContext currentUser,
        IInventoryAlertService alerts,
        IKitchenNotifier notifier,
        OrderWorkflowSettings settings,
        TimeProvider timeProvider,
        ILogger<OrderService> logger)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
        _alerts = alerts;
        _notifier = notifier;
        _settings = settings;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ApiResponse<Guid>> CreateAsync(
        OrderCreatDTO dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var branch = await GetAccessibleBranchAsync(
            dto.BranchId, dto.RestaurantId, cancellationToken);
        if (dto.TableId.HasValue)
            await AcquireOrderTableLockAsync(dto.TableId.Value, cancellationToken);
        var table = await ValidateTableAsync(
            dto.TableId, dto.OrderType, branch, null, cancellationToken);
        var waiter = await ResolveWaiterAsync(
            dto.WaiterId, branch.ID, branch.RestaurantId, cancellationToken);
        var customer = await ValidateCustomerAsync(
            dto.CustomerId, branch.RestaurantId, cancellationToken);
        var menuItems = await LoadMenuItemsAsync(
            dto.Items.Select(x => x.MenuItemId), branch.RestaurantId, cancellationToken);

        var order = _mapper.Map<Order>(dto);
        order.RestaurantId = branch.RestaurantId;
        order.BranchId = branch.ID;
        order.TableId = table?.ID;
        order.WaiterId = waiter?.ID;
        order.CustomerId = customer?.ID;
        order.OrderNumber = await CreateOrderNumberAsync(
            branch.RestaurantId, cancellationToken);
        order.Notes = NormalizeOptional(dto.Notes);
        order.CreatAt = UtcNow;
        order.Items = dto.Items.Select(item => CreateOrderItem(
            item, menuItems[item.MenuItemId], order.CreatAt)).ToList();
        Recalculate(order, branch.Restaurant.TaxRate);

        if (table is not null)
            table.Status = TableStatus.Occupied;

        _db.Orders.Add(order);
        await SaveWithConcurrencyAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await NotifyAsync(order.ID, "OrderCreated", cancellationToken);
        return ApiResponse.Created(order.ID, "Order created successfully.");
    }

    public async Task<ApiResponse<OrderGetDTO>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var order = await DetailQuery().FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Order", id);
        await EnsureOrderAccessAsync(order, cancellationToken);
        return ApiResponse.Ok(_mapper.Map<OrderGetDTO>(order));
    }

    public Task<PagedResponse<OrderGetDTO>> GetAllAsync(
        OrderListRequest request, CancellationToken cancellationToken = default) =>
        GetListAsync(request, null, null, cancellationToken);

    public Task<PagedResponse<OrderGetDTO>> GetByBranchAsync(
        Guid branchId, OrderListRequest request,
        CancellationToken cancellationToken = default) =>
        GetListAsync(request, branchId, null, cancellationToken);

    public Task<PagedResponse<OrderGetDTO>> GetByWaiterAsync(
        Guid waiterId, OrderListRequest request,
        CancellationToken cancellationToken = default) =>
        GetListAsync(request, null, waiterId, cancellationToken);

    public async Task<ApiResponse<object?>> UpdateAsync(
        Guid id, OrderUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderForMutationAsync(id, cancellationToken);
        EnsureNotFinal(order);
        ApplyExpectedVersion(order, dto.RowVersion);
        order.Notes = NormalizeOptional(dto.Notes);
        order.IsPriority = dto.IsPriority;
        order.UpdateAt = UtcNow;
        await SaveWithConcurrencyAsync(cancellationToken);
        await NotifyAsync(order.ID, "OrderUpdated", cancellationToken);
        return ApiResponse.Success("Order updated successfully.");
    }

    public async Task<ApiResponse<object?>> AddItemAsync(
        Guid orderId, OrderItemAddDTO dto,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var order = await LoadOrderForMutationAsync(orderId, cancellationToken, true);
        EnsureEditable(order);
        ApplyExpectedVersion(order, dto.RowVersion);
        var menu = (await LoadMenuItemsAsync(
            [dto.MenuItemId], order.RestaurantId, cancellationToken))[dto.MenuItemId];
        var existing = order.Items.FirstOrDefault(x =>
            !x.IsDeleted && x.MenuItemId == dto.MenuItemId
            && string.Equals(x.KitchenNote, NormalizeOptional(dto.KitchenNote),
                StringComparison.Ordinal));
        if (existing is null)
        {
            order.Items.Add(CreateOrderItem(new OrderItemCreatDTO
            {
                MenuItemId = dto.MenuItemId,
                Quantity = dto.Quantity,
                KitchenNote = dto.KitchenNote
            }, menu, UtcNow));
        }
        else
        {
            SetItemQuantity(existing, existing.Quantity + dto.Quantity);
        }
        Recalculate(order, order.Restaurant.TaxRate);
        order.UpdateAt = UtcNow;
        await SaveWithConcurrencyAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await NotifyAsync(order.ID, "OrderUpdated", cancellationToken);
        return ApiResponse.Success("Order item added successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateItemQuantityAsync(
        Guid orderId, Guid itemId, OrderItemUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderForMutationAsync(orderId, cancellationToken, true);
        EnsureEditable(order);
        ApplyExpectedVersion(order, dto.RowVersion);
        var item = GetActiveItem(order, itemId);
        SetItemQuantity(item, dto.Quantity);
        Recalculate(order, order.Restaurant.TaxRate);
        order.UpdateAt = UtcNow;
        await SaveWithConcurrencyAsync(cancellationToken);
        await NotifyAsync(order.ID, "OrderUpdated", cancellationToken);
        return ApiResponse.Success("Order item quantity updated successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateItemKitchenNoteAsync(
        Guid orderId, Guid itemId, OrderItemKitchenNoteDTO dto,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderForMutationAsync(orderId, cancellationToken);
        EnsureEditable(order);
        ApplyExpectedVersion(order, dto.RowVersion);
        var item = GetActiveItem(order, itemId);
        item.KitchenNote = NormalizeOptional(dto.KitchenNote);
        item.UpdateAt = UtcNow;
        order.UpdateAt = UtcNow;
        await SaveWithConcurrencyAsync(cancellationToken);
        await NotifyAsync(order.ID, "OrderUpdated", cancellationToken);
        return ApiResponse.Success("Kitchen note updated successfully.");
    }

    public async Task<ApiResponse<object?>> RemoveItemAsync(
        Guid orderId, Guid itemId, byte[] rowVersion,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderForMutationAsync(orderId, cancellationToken, true);
        EnsureEditable(order);
        ApplyExpectedVersion(order, rowVersion);
        var item = GetActiveItem(order, itemId);
        if (order.Items.Count(x => !x.IsDeleted) <= 1)
            throw new ConflictException("An order must contain at least one item.");
        item.IsDeleted = true;
        item.DeletedAt = UtcNow;
        item.Status = OrderItemStatus.Cancelled;
        Recalculate(order, order.Restaurant.TaxRate);
        order.UpdateAt = UtcNow;
        await SaveWithConcurrencyAsync(cancellationToken);
        await NotifyAsync(order.ID, "OrderUpdated", cancellationToken);
        return ApiResponse.NoContent("Order item removed successfully.");
    }

    public Task<ApiResponse<object?>> UpdateStatusAsync(
        Guid id, OrderStatusUpdateDTO dto,
        CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, dto.Status, dto.RowVersion, kitchenOnly: false,
            cancellationToken);

    public Task<ApiResponse<object?>> UpdateKitchenStatusAsync(
        Guid id, KitchenOrderStatusUpdateDTO dto,
        CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, dto.Status, dto.RowVersion, kitchenOnly: true,
            cancellationToken);

    public async Task<ApiResponse<object?>> CancelAsync(
        Guid id, OrderCancelDTO dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var order = await LoadOrderForMutationAsync(id, cancellationToken, true, true);
        EnsureNotFinal(order);
        ApplyExpectedVersion(order, dto.RowVersion);
        if (order.IsPaid && !dto.RequestRefund)
            throw new ConflictException("A paid order requires a refund request before cancellation.");

        if (order.InventoryConsumedAt.HasValue
            && !order.InventoryReturnedAt.HasValue
            && order.Status == OrderStatus.Preparing)
        {
            await ReturnConsumedInventoryAsync(order, cancellationToken);
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = UtcNow;
        order.CancelReason = dto.Reason.Trim();
        order.RefundRequired = order.IsPaid;
        order.UpdateAt = UtcNow;
        foreach (var item in order.Items.Where(x => !x.IsDeleted))
            item.Status = OrderItemStatus.Cancelled;
        await ReleaseTableIfUnusedAsync(order, cancellationToken);
        await SaveWithConcurrencyAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await NotifyAsync(order.ID, "OrderCancelled", cancellationToken);
        return ApiResponse.Success("Order cancelled successfully.");
    }

    public async Task<ApiResponse<object?>> ChangeTableAsync(
        Guid id, OrderTableUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var order = await LoadOrderForMutationAsync(id, cancellationToken);
        EnsureNotFinal(order);
        if (order.OrderType != OrderType.DineIn)
            throw new ConflictException("Only dine-in orders can be assigned to a table.");
        ApplyExpectedVersion(order, dto.RowVersion);
        var branch = await _db.Branches.Include(x => x.Restaurant).FirstAsync(
            x => x.ID == order.BranchId, cancellationToken);
        var oldTableId = order.TableId;
        await AcquireOrderTableLockAsync(dto.TableId, cancellationToken);
        var table = await ValidateTableAsync(
            dto.TableId, OrderType.DineIn, branch, order.ID, cancellationToken);
        order.TableId = table!.ID;
        table.Status = TableStatus.Occupied;
        order.UpdateAt = UtcNow;
        if (oldTableId.HasValue && oldTableId != table.ID)
            await ReleaseSpecificTableIfUnusedAsync(oldTableId.Value, order.ID, cancellationToken);
        await SaveWithConcurrencyAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await NotifyAsync(order.ID, "OrderUpdated", cancellationToken);
        return ApiResponse.Success("Order table changed successfully.");
    }

    public async Task<ApiResponse<object?>> ApplyDiscountAsync(
        Guid id, OrderDiscountDTO dto, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderForMutationAsync(id, cancellationToken, true);
        EnsureEditable(order);
        ApplyExpectedVersion(order, dto.RowVersion);
        order.OrderDiscountPercentage = dto.DiscountPercentage;
        Recalculate(order, order.Restaurant.TaxRate);
        order.UpdateAt = UtcNow;
        await SaveWithConcurrencyAsync(cancellationToken);
        await NotifyAsync(order.ID, "OrderUpdated", cancellationToken);
        return ApiResponse.Success("Order discount applied successfully.");
    }

    public async Task<ApiResponse<object?>> MergeAsync(
        Guid targetOrderId, OrderMergeDTO dto,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var ids = dto.SourceOrderIds.Append(targetOrderId).Distinct().ToArray();
        var orders = await MutationQuery(includeMenuIngredients: false)
            .Where(x => ids.Contains(x.ID) && !x.IsDeleted)
            .ToListAsync(cancellationToken);
        if (orders.Count != ids.Length)
            throw new NotFoundException("One or more merge orders were not found.");
        var target = orders.Single(x => x.ID == targetOrderId);
        await EnsureOrderAccessAsync(target, cancellationToken);
        ApplyExpectedVersion(target, dto.RowVersion);
        foreach (var order in orders)
        {
            await EnsureOrderAccessAsync(order, cancellationToken);
            EnsureEditable(order);
            if (order.RestaurantId != target.RestaurantId || order.BranchId != target.BranchId)
                throw new ConflictException("Merged orders must belong to the same branch.");
            if (order.IsPaid)
                throw new ConflictException("Paid orders cannot be merged.");
        }

        foreach (var source in orders.Where(x => x.ID != targetOrderId))
        {
            foreach (var item in source.Items.Where(x => !x.IsDeleted))
            {
                var targetItem = target.Items.FirstOrDefault(x => !x.IsDeleted
                    && x.MenuItemId == item.MenuItemId
                    && x.KitchenNote == item.KitchenNote);
                if (targetItem is null)
                {
                    target.Items.Add(CloneItem(item, item.Quantity));
                }
                else
                {
                    SetItemQuantity(targetItem, targetItem.Quantity + item.Quantity);
                }
                item.IsDeleted = true;
                item.DeletedAt = UtcNow;
            }
            source.Status = OrderStatus.Cancelled;
            source.CancelledAt = UtcNow;
            source.CancelReason = $"Merged into {target.OrderNumber}.";
            source.UpdateAt = UtcNow;
            await ReleaseTableIfUnusedAsync(source, cancellationToken);
        }
        Recalculate(target, target.Restaurant.TaxRate);
        target.UpdateAt = UtcNow;
        await SaveWithConcurrencyAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await NotifyAsync(target.ID, "OrderUpdated", cancellationToken);
        return ApiResponse.Success("Orders merged successfully.");
    }

    public async Task<ApiResponse<Guid>> SplitAsync(
        Guid orderId, OrderSplitDTO dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var source = await LoadOrderForMutationAsync(orderId, cancellationToken, true);
        EnsureEditable(source);
        if (source.IsPaid) throw new ConflictException("Paid orders cannot be split.");
        ApplyExpectedVersion(source, dto.RowVersion);
        var selections = dto.Items.ToDictionary(x => x.OrderItemId);
        var selectedItems = source.Items.Where(x =>
            !x.IsDeleted && selections.ContainsKey(x.ID)).ToList();
        if (selectedItems.Count != selections.Count)
            throw new ConflictException("One or more split items do not belong to the order.");
        foreach (var item in selectedItems)
        {
            if (selections[item.ID].Quantity > item.Quantity)
                throw new ConflictException("Split quantity cannot exceed the order item quantity.");
        }
        if (source.Items.Where(x => !x.IsDeleted).Sum(x => x.Quantity)
            <= dto.Items.Sum(x => x.Quantity))
            throw new ConflictException("The source order must retain at least one item.");

        Table? splitTable = null;
        if (source.OrderType == OrderType.DineIn
            && dto.TableId.HasValue && dto.TableId != source.TableId)
        {
            await AcquireOrderTableLockAsync(dto.TableId.Value, cancellationToken);
            splitTable = await ValidateTableAsync(
                dto.TableId, OrderType.DineIn, source.Branch, null, cancellationToken);
        }

        var newOrder = new Order
        {
            RestaurantId = source.RestaurantId,
            BranchId = source.BranchId,
            TableId = splitTable?.ID ?? source.TableId,
            WaiterId = source.WaiterId,
            CustomerId = source.CustomerId,
            OrderNumber = await CreateOrderNumberAsync(source.RestaurantId, cancellationToken),
            OrderType = source.OrderType,
            Status = source.Status,
            Notes = source.Notes,
            IsPriority = source.IsPriority,
            OrderDiscountPercentage = source.OrderDiscountPercentage,
            CreatAt = UtcNow
        };
        foreach (var item in selectedItems)
        {
            var quantity = selections[item.ID].Quantity;
            newOrder.Items.Add(CloneItem(item, quantity));
            if (quantity == item.Quantity)
            {
                item.IsDeleted = true;
                item.DeletedAt = UtcNow;
            }
            else
            {
                SetItemQuantity(item, item.Quantity - quantity);
            }
        }
        Recalculate(source, source.Restaurant.TaxRate);
        Recalculate(newOrder, source.Restaurant.TaxRate);
        source.UpdateAt = UtcNow;
        _db.Orders.Add(newOrder);
        if (splitTable is not null)
            splitTable.Status = TableStatus.Occupied;
        await SaveWithConcurrencyAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await NotifyAsync(source.ID, "OrderUpdated", cancellationToken);
        await NotifyAsync(newOrder.ID, "OrderCreated", cancellationToken);
        return ApiResponse.Created(newOrder.ID, "Order split successfully.");
    }

    public async Task<ApiResponse<KitchenDashboardDTO>> GetKitchenDashboardAsync(
        Guid? branchId, CancellationToken cancellationToken = default)
    {
        if (branchId.HasValue)
            await GetAccessibleBranchAsync(branchId.Value,
                _currentUser.IsSuperAdmin ? null : _currentUser.RestaurantId,
                cancellationToken);
        var query = _db.Orders.AsNoTracking()
            .Include(x => x.Table).Include(x => x.Waiter)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .Where(x => !x.IsDeleted && KitchenStatuses.Contains(x.Status));
        query = await ApplyListAccessAsync(query, branchId, cancellationToken);
        var orders = await query.OrderByDescending(x => x.IsPriority)
            .ThenBy(x => x.CreatAt).ToListAsync(cancellationToken);
        var kitchenOrders = orders.Select(MapKitchenOrder).ToArray();
        var completedDurations = orders.Where(x => x.PreparingAt.HasValue && x.ReadyAt.HasValue)
            .Select(x => (x.ReadyAt!.Value - x.PreparingAt!.Value).TotalMinutes).ToArray();
        return ApiResponse.Ok(new KitchenDashboardDTO
        {
            PendingCount = orders.Count(x => x.Status is OrderStatus.Pending or OrderStatus.Confirmed),
            PreparingCount = orders.Count(x => x.Status == OrderStatus.Preparing),
            ReadyCount = orders.Count(x => x.Status == OrderStatus.Ready),
            AveragePreparationMinutes = completedDurations.Length == 0
                ? 0 : Math.Round(completedDurations.Average(), 1),
            Orders = kitchenOrders
        }, "Kitchen dashboard retrieved successfully.");
    }

    public async Task<ApiResponse<object?>> ProcessPaymentAsync(
        Guid id, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderForMutationAsync(id, cancellationToken);
        EnsureNotFinal(order);
        if (order.IsPaid) throw new ConflictException("Order is already paid.");
        ApplyExpectedVersion(order, rowVersion);
        order.IsPaid = true;
        if (order.Status == OrderStatus.Pending) order.Status = OrderStatus.Confirmed;
        order.UpdateAt = UtcNow;
        await SaveWithConcurrencyAsync(cancellationToken);
        await NotifyAsync(order.ID, "OrderStatusChanged", cancellationToken);
        return ApiResponse.Success("Payment processed successfully.");
    }

    private async Task<ApiResponse<object?>> ChangeStatusAsync(
        Guid id, OrderStatus next, byte[] rowVersion, bool kitchenOnly,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var order = await LoadOrderForMutationAsync(id, cancellationToken, true, true);
        EnsureNotFinal(order);
        if (kitchenOnly && next is not (
                OrderStatus.Confirmed or OrderStatus.Preparing or OrderStatus.Ready))
            throw new ConflictException(
                "Kitchen can set only Confirmed, Preparing or Ready status.");
        var expected = order.Status switch
        {
            OrderStatus.Pending => OrderStatus.Confirmed,
            OrderStatus.Confirmed => OrderStatus.Preparing,
            OrderStatus.Preparing => OrderStatus.Ready,
            OrderStatus.Ready => OrderStatus.Served,
            OrderStatus.Served => OrderStatus.Completed,
            _ => throw new ConflictException("The order status cannot be changed.")
        };
        if (next != expected)
            throw new ConflictException($"Invalid order transition: {order.Status} -> {next}.");
        ApplyExpectedVersion(order, rowVersion);

        if (next == OrderStatus.Preparing)
        {
            await ConsumeInventoryAsync(order, cancellationToken);
            order.PreparingAt = UtcNow;
            foreach (var item in order.Items.Where(x => !x.IsDeleted))
                item.Status = OrderItemStatus.Preparing;
        }
        else if (next == OrderStatus.Ready)
        {
            order.ReadyAt = UtcNow;
            foreach (var item in order.Items.Where(x => !x.IsDeleted))
                item.Status = OrderItemStatus.Ready;
        }
        else if (next == OrderStatus.Served)
        {
            foreach (var item in order.Items.Where(x => !x.IsDeleted))
                item.Status = OrderItemStatus.Served;
        }
        else if (next == OrderStatus.Completed)
        {
            order.CompletedAt = UtcNow;
            await ReleaseTableIfUnusedAsync(order, cancellationToken);
        }
        order.Status = next;
        order.UpdateAt = UtcNow;
        await SaveWithConcurrencyAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await NotifyAsync(order.ID,
            next == OrderStatus.Ready ? "OrderReady" : "OrderStatusChanged",
            cancellationToken);
        return ApiResponse.Success("Order status updated successfully.");
    }

    private async Task ConsumeInventoryAsync(Order order, CancellationToken cancellationToken)
    {
        if (order.InventoryConsumedAt.HasValue)
            throw new ConflictException("Inventory was already consumed for this order.");
        var requirements = order.Items.Where(x => !x.IsDeleted)
            .SelectMany(x => x.MenuItem.Ingredients.Select(ingredient => new
            {
                ingredient.IngredientId,
                Quantity = ingredient.RequiredQuantity * x.Quantity
            }))
            .GroupBy(x => x.IngredientId)
            .ToDictionary(x => x.Key, x => x.Sum(v => v.Quantity));
        var today = DateOnly.FromDateTime(UtcNow);
        var changedIds = new HashSet<Guid>();
        foreach (var requirement in requirements)
        {
            var batches = await _db.InventoryItems.Where(x =>
                    x.BranchId == order.BranchId
                    && x.IngredientId == requirement.Key
                    && x.IsActive && !x.IsDeleted
                    && (!x.ExpirationDate.HasValue || x.ExpirationDate >= today))
                .OrderBy(x => x.ExpirationDate ?? DateOnly.MaxValue)
                .ThenBy(x => x.CreatAt).ToListAsync(cancellationToken);
            if (batches.Sum(x => x.CurrentQuantity) < requirement.Value)
                throw new ConflictException(
                    "Insufficient inventory for one or more order ingredients.");
            var remaining = requirement.Value;
            foreach (var batch in batches.Where(x => x.CurrentQuantity > 0))
            {
                var quantity = Math.Min(batch.CurrentQuantity, remaining);
                if (quantity <= 0) continue;
                var previous = batch.CurrentQuantity;
                batch.CurrentQuantity -= quantity;
                batch.UpdateAt = UtcNow;
                _db.StockTransactions.Add(CreateStockTransaction(
                    batch, StockTransactionType.OrderConsumption, quantity,
                    previous, batch.CurrentQuantity, order.OrderNumber,
                    $"Consumed by order {order.OrderNumber}."));
                changedIds.Add(batch.ID);
                remaining -= quantity;
                if (remaining <= 0) break;
            }
        }
        order.InventoryConsumedAt = UtcNow;
        await SaveWithConcurrencyAsync(cancellationToken);
        foreach (var inventoryItemId in changedIds)
            await _alerts.EvaluateItemAsync(inventoryItemId, cancellationToken);
    }

    private async Task ReturnConsumedInventoryAsync(
        Order order, CancellationToken cancellationToken)
    {
        var consumptions = await _db.StockTransactions.Where(x =>
                x.ReferenceNumber == order.OrderNumber
                && x.TransactionType == StockTransactionType.OrderConsumption
                && !x.IsDeleted)
            .ToListAsync(cancellationToken);
        var inventoryIds = consumptions.Select(x => x.InventoryItemId).Distinct().ToArray();
        var inventory = await _db.InventoryItems.Where(x => inventoryIds.Contains(x.ID))
            .ToDictionaryAsync(x => x.ID, cancellationToken);
        foreach (var consumption in consumptions)
        {
            var item = inventory[consumption.InventoryItemId];
            var previous = item.CurrentQuantity;
            item.CurrentQuantity += consumption.Quantity;
            item.UpdateAt = UtcNow;
            _db.StockTransactions.Add(CreateStockTransaction(
                item, StockTransactionType.Return, consumption.Quantity,
                previous, item.CurrentQuantity, order.OrderNumber,
                $"Returned after cancelling order {order.OrderNumber}."));
        }
        order.InventoryReturnedAt = UtcNow;
        await SaveWithConcurrencyAsync(cancellationToken);
        foreach (var inventoryItemId in inventoryIds)
            await _alerts.EvaluateItemAsync(inventoryItemId, cancellationToken);
    }

    private StockTransaction CreateStockTransaction(
        InventoryItem item, StockTransactionType type, decimal quantity,
        decimal previous, decimal next, string reference, string reason) => new()
    {
        InventoryItemId = item.ID,
        TransactionType = type,
        Quantity = quantity,
        PreviousQuantity = previous,
        NewQuantity = next,
        UnitPrice = item.PurchasePrice,
        Reason = reason,
        ReferenceNumber = reference,
        CreatedByUserId = _currentUser.UserId,
        CreatAt = UtcNow
    };

    private async Task<PagedResponse<OrderGetDTO>> GetListAsync(
        OrderListRequest request, Guid? fixedBranchId, Guid? fixedWaiterId,
        CancellationToken cancellationToken)
    {
        var branchId = fixedBranchId ?? request.BranchId;
        var waiterId = fixedWaiterId ?? request.WaiterId;
        var query = DetailQuery().Where(x => !x.IsDeleted);
        query = await ApplyListAccessAsync(query, branchId, cancellationToken);
        if (_currentUser.IsSuperAdmin && request.RestaurantId.HasValue)
            query = query.Where(x => x.RestaurantId == request.RestaurantId.Value);
        if (branchId.HasValue) query = query.Where(x => x.BranchId == branchId.Value);
        if (waiterId.HasValue) query = query.Where(x => x.WaiterId == waiterId.Value);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.OrderType.HasValue) query = query.Where(x => x.OrderType == request.OrderType.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.OrderNumber.Contains(search)
                || (x.Table != null && x.Table.TableNumber.Contains(search))
                || (x.Customer != null && x.Customer.Name.Contains(search)));
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatAt).ThenBy(x => x.ID)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return PagedResponse<OrderGetDTO>.Create(
            _mapper.Map<List<OrderGetDTO>>(items), request.PageNumber,
            request.PageSize, total, "Orders retrieved successfully.");
    }

    private async Task<IQueryable<Order>> ApplyListAccessAsync(
        IQueryable<Order> query, Guid? requestedBranchId,
        CancellationToken cancellationToken)
    {
        if (_currentUser.IsSuperAdmin) return query;
        query = query.Where(x => x.RestaurantId == _currentUser.RestaurantId);
        if (_currentUser.IsRestaurantOwner) return query;
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            return query.Where(x => x.Branch.ManagerId == userId);
        }
        var employeeBranchId = await GetCurrentEmployeeBranchIdAsync(cancellationToken);
        if (requestedBranchId.HasValue && requestedBranchId != employeeBranchId)
            throw new ForbiddenException("Orders from another branch cannot be accessed.");
        return query.Where(x => x.BranchId == employeeBranchId);
    }

    private IQueryable<Order> DetailQuery() => _db.Orders.AsNoTracking()
        .AsSplitQuery()
        .Include(x => x.Branch).Include(x => x.Table)
        .Include(x => x.Waiter).Include(x => x.Customer)
        .Include(x => x.Items.Where(item => !item.IsDeleted));

    private IQueryable<Order> MutationQuery(bool includeMenuIngredients)
    {
        IQueryable<Order> query = _db.Orders
            .Include(x => x.Restaurant).Include(x => x.Branch)
            .Include(x => x.Table).Include(x => x.Waiter).Include(x => x.Customer)
            .Include(x => x.Items);
        if (includeMenuIngredients)
            query = query.Include(x => x.Items).ThenInclude(x => x.MenuItem)
                .ThenInclude(x => x.Ingredients);
        return query.AsSplitQuery();
    }

    private async Task<Order> LoadOrderForMutationAsync(
        Guid id, CancellationToken cancellationToken,
        bool includeRestaurant = false, bool includeMenuIngredients = false)
    {
        var order = await MutationQuery(includeMenuIngredients)
            .FirstOrDefaultAsync(x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Order", id);
        await EnsureOrderAccessAsync(order, cancellationToken);
        return order;
    }

    private async Task EnsureOrderAccessAsync(Order order, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsSuperAdmin && order.RestaurantId != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant's order cannot be accessed.");
        if (_currentUser.IsSuperAdmin || _currentUser.IsRestaurantOwner) return;
        if (_currentUser.IsManager)
        {
            var managesBranch = order.Branch.ManagerId == _currentUser.UserId
                || await _db.Branches.AsNoTracking().AnyAsync(x =>
                    x.ID == order.BranchId && x.ManagerId == _currentUser.UserId,
                    cancellationToken);
            if (!managesBranch)
                throw new ForbiddenException("Managers can access only their own branch orders.");
            return;
        }
        if (order.BranchId != await GetCurrentEmployeeBranchIdAsync(cancellationToken))
            throw new ForbiddenException("Orders from another branch cannot be accessed.");
    }

    private async Task<Branch> GetAccessibleBranchAsync(
        Guid branchId, Guid? restaurantId, CancellationToken cancellationToken)
    {
        var branch = await _db.Branches.Include(x => x.Restaurant)
            .FirstOrDefaultAsync(x => x.ID == branchId && x.IsActive && !x.IsDeleted
                && x.Restaurant.IsActive && !x.Restaurant.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Branch", branchId);
        if (restaurantId.HasValue && branch.RestaurantId != restaurantId.Value)
            throw new ConflictException("The branch does not belong to the selected restaurant.");
        if (!_currentUser.IsSuperAdmin && branch.RestaurantId != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant's branch cannot be accessed.");
        if (_currentUser.IsSuperAdmin || _currentUser.IsRestaurantOwner) return branch;
        if (_currentUser.IsManager)
        {
            if (branch.ManagerId != _currentUser.UserId)
                throw new ForbiddenException("Managers can access only their own branch.");
            return branch;
        }
        if (branch.ID != await GetCurrentEmployeeBranchIdAsync(cancellationToken))
            throw new ForbiddenException("Employees can access only their own branch.");
        return branch;
    }

    private async Task<Guid> GetCurrentEmployeeBranchIdAsync(CancellationToken cancellationToken)
    {
        var branchId = await _db.BusinessUsers.AsNoTracking()
            .Where(x => x.AppUserId == _currentUser.UserId && x.IsActive && !x.IsDeleted)
            .Select(x => x.BranchId).FirstOrDefaultAsync(cancellationToken);
        return branchId ?? throw new ForbiddenException(
            "The authenticated account is not linked to an active branch employee.");
    }

    private async Task<Table?> ValidateTableAsync(
        Guid? tableId, OrderType orderType, Branch branch, Guid? excludedOrderId,
        CancellationToken cancellationToken)
    {
        if (orderType != OrderType.DineIn) return null;
        if (!tableId.HasValue) throw new ConflictException("A dine-in order requires a table.");
        var table = await _db.Tables.FirstOrDefaultAsync(x =>
            x.ID == tableId.Value && x.BranchId == branch.ID
            && x.RestaurantID == branch.RestaurantId && x.IsActive && !x.IsDeleted,
            cancellationToken) ?? throw new NotFoundException("Table", tableId.Value);
        if (table.Status != TableStatus.Available)
            throw new ConflictException("The selected table is not available.");
        var hasActiveOrder = await _db.Orders.AsNoTracking().AnyAsync(x =>
            x.TableId == table.ID && !x.IsDeleted
            && (!excludedOrderId.HasValue || x.ID != excludedOrderId.Value)
            && TableBlockingStatuses.Contains(x.Status), cancellationToken);
        if (hasActiveOrder) throw new ConflictException("The selected table already has an active order.");
        return table;
    }

    private async Task<User?> ResolveWaiterAsync(
        Guid? waiterId, Guid branchId, Guid restaurantId,
        CancellationToken cancellationToken)
    {
        if (!waiterId.HasValue && _currentUser.IsInRole(AppRoles.Waiter))
            return await _db.BusinessUsers.FirstOrDefaultAsync(x =>
                x.AppUserId == _currentUser.UserId && x.BranchId == branchId
                && x.IsActive && !x.IsDeleted, cancellationToken)
                ?? throw new ForbiddenException("The waiter account is not linked to this branch.");
        if (!waiterId.HasValue) return null;
        return await _db.BusinessUsers.FirstOrDefaultAsync(x =>
            x.ID == waiterId.Value && x.RestaurantId == restaurantId
            && x.BranchId == branchId && x.Role == AppRoles.Waiter
            && x.IsActive && !x.IsDeleted, cancellationToken)
            ?? throw new ConflictException("The waiter must be active and belong to the order branch.");
    }

    private async Task<User?> ValidateCustomerAsync(
        Guid? customerId, Guid restaurantId, CancellationToken cancellationToken)
    {
        if (!customerId.HasValue) return null;
        return await _db.BusinessUsers.FirstOrDefaultAsync(x =>
            x.ID == customerId.Value && x.RestaurantId == restaurantId
            && x.Role == "Customer" && x.IsActive && !x.IsDeleted, cancellationToken)
            ?? throw new ConflictException("The customer must belong to the order restaurant.");
    }

    private async Task<Dictionary<Guid, Menu>> LoadMenuItemsAsync(
        IEnumerable<Guid> ids, Guid restaurantId, CancellationToken cancellationToken)
    {
        var requested = ids.Distinct().ToArray();
        var items = await _db.Menus.AsNoTracking().Where(x =>
            requested.Contains(x.ID) && x.RestaurantId == restaurantId
            && x.IsAvailable && !x.IsDeleted).ToDictionaryAsync(x => x.ID, cancellationToken);
        if (items.Count != requested.Length)
            throw new ConflictException(
                "Every menu item must be available and belong to the order restaurant.");
        return items;
    }

    private static OrderItem CreateOrderItem(
        OrderItemCreatDTO dto, Menu menu, DateTime now)
    {
        var unitPrice = RoundMoney(menu.Price);
        var finalPrice = RoundMoney(menu.FinalPrice);
        return new OrderItem
        {
            MenuItemId = menu.ID,
            MenuItemName = menu.Name,
            UnitPrice = unitPrice,
            Quantity = dto.Quantity,
            DiscountAmount = RoundMoney((unitPrice - finalPrice) * dto.Quantity),
            TotalPrice = RoundMoney(finalPrice * dto.Quantity),
            KitchenNote = NormalizeOptional(dto.KitchenNote),
            Status = OrderItemStatus.Pending,
            CreatAt = now
        };
    }

    private static OrderItem CloneItem(OrderItem item, int quantity)
    {
        var unitDiscount = item.Quantity == 0 ? 0 : item.DiscountAmount / item.Quantity;
        return new OrderItem
        {
            MenuItemId = item.MenuItemId,
            MenuItemName = item.MenuItemName,
            UnitPrice = item.UnitPrice,
            Quantity = quantity,
            DiscountAmount = RoundMoney(unitDiscount * quantity),
            TotalPrice = RoundMoney((item.UnitPrice - unitDiscount) * quantity),
            KitchenNote = item.KitchenNote,
            Status = item.Status,
            CreatAt = DateTime.UtcNow
        };
    }

    private static void SetItemQuantity(OrderItem item, int quantity)
    {
        var unitDiscount = item.Quantity == 0 ? 0 : item.DiscountAmount / item.Quantity;
        item.Quantity = quantity;
        item.DiscountAmount = RoundMoney(unitDiscount * quantity);
        item.TotalPrice = RoundMoney((item.UnitPrice - unitDiscount) * quantity);
        item.UpdateAt = DateTime.UtcNow;
    }

    private static void Recalculate(Order order, decimal taxRate)
    {
        var items = order.Items.Where(x => !x.IsDeleted).ToArray();
        order.Subtotal = RoundMoney(items.Sum(x => x.UnitPrice * x.Quantity));
        var itemDiscount = RoundMoney(items.Sum(x => x.DiscountAmount));
        var afterItemDiscount = Math.Max(0, order.Subtotal - itemDiscount);
        var orderDiscount = RoundMoney(afterItemDiscount
            * order.OrderDiscountPercentage / 100m);
        order.DiscountAmount = RoundMoney(itemDiscount + orderDiscount);
        var taxable = Math.Max(0, order.Subtotal - order.DiscountAmount);
        order.TaxAmount = RoundMoney(taxable * taxRate / 100m);
        order.TotalAmount = RoundMoney(taxable + order.TaxAmount);
    }

    private static OrderItem GetActiveItem(Order order, Guid itemId) =>
        order.Items.FirstOrDefault(x => x.ID == itemId && !x.IsDeleted)
        ?? throw new NotFoundException("Order item", itemId);

    private static void EnsureEditable(Order order)
    {
        if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            throw new ConflictException(
                "Order items can be changed only before preparation starts.");
    }

    private static void EnsureNotFinal(Order order)
    {
        if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled)
            throw new ConflictException("Completed or cancelled orders cannot be modified.");
    }

    private void ApplyExpectedVersion(Order order, byte[] rowVersion) =>
        _db.Entry(order).Property(x => x.RowVersion).OriginalValue = rowVersion;

    private async Task SaveWithConcurrencyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The order or inventory changed concurrently. Reload and retry.");
        }
    }

    private async Task ReleaseTableIfUnusedAsync(
        Order order, CancellationToken cancellationToken)
    {
        if (order.TableId.HasValue)
            await ReleaseSpecificTableIfUnusedAsync(
                order.TableId.Value, order.ID, cancellationToken);
    }

    private async Task ReleaseSpecificTableIfUnusedAsync(
        Guid tableId, Guid excludedOrderId, CancellationToken cancellationToken)
    {
        var stillUsed = await _db.Orders.AsNoTracking().AnyAsync(x =>
            x.TableId == tableId && x.ID != excludedOrderId && !x.IsDeleted
            && TableBlockingStatuses.Contains(x.Status), cancellationToken);
        if (!stillUsed)
        {
            var table = await _db.Tables.FirstOrDefaultAsync(x =>
                x.ID == tableId && !x.IsDeleted, cancellationToken);
            if (table is not null && table.IsActive)
                table.Status = TableStatus.Available;
        }
    }

    private async Task<string> CreateOrderNumberAsync(
        Guid restaurantId, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var number = $"ORD-{UtcNow:yyMMdd}-{RandomNumberGenerator.GetHexString(3)}";
            if (!await _db.Orders.AsNoTracking().AnyAsync(x =>
                    x.RestaurantId == restaurantId && x.OrderNumber == number,
                    cancellationToken))
                return number;
        }
        throw new InvalidOperationException("A unique order number could not be generated.");
    }

    private async Task AcquireOrderTableLockAsync(
        Guid tableId, CancellationToken cancellationToken)
    {
        if (!_db.Database.IsSqlServer()) return;
        var resource = $"order-table:{tableId:N}";
        try
        {
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 DECLARE @lockResult int;
                 EXEC @lockResult = sys.sp_getapplock
                     @Resource = {resource},
                     @LockMode = 'Exclusive',
                     @LockOwner = 'Transaction',
                     @LockTimeout = 10000;
                 IF @lockResult < 0
                     THROW 51000, 'Order table lock acquisition failed.', 1;
                 """,
                cancellationToken);
        }
        catch (SqlException exception) when (exception.Number is 1205 or 1222 or 51000)
        {
            throw new ConflictException(
                "The selected table changed concurrently. Reload and retry.");
        }
    }

    private KitchenOrderDTO MapKitchenOrder(Order order)
    {
        var start = order.PreparingAt ?? order.CreatAt;
        var end = order.ReadyAt ?? UtcNow;
        var duration = Math.Max(0, (end - start).TotalMinutes);
        return new KitchenOrderDTO
        {
            Id = order.ID,
            RestaurantId = order.RestaurantId,
            BranchId = order.BranchId,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            TableNumber = order.Table?.TableNumber,
            WaiterName = order.Waiter?.Name,
            KitchenNotes = order.Items.Where(x => !x.IsDeleted
                    && !string.IsNullOrWhiteSpace(x.KitchenNote))
                .Select(x => x.KitchenNote!).Distinct().ToArray(),
            Items = _mapper.Map<List<OrderItemGetDTO>>(
                order.Items.Where(x => !x.IsDeleted)),
            PreparationDurationMinutes = Math.Round(duration, 1),
            IsDelayed = order.Status != OrderStatus.Ready
                && duration >= _settings.DelayedAfterMinutes,
            IsPriority = order.IsPriority,
            CreatedAt = order.CreatAt,
            RowVersion = order.RowVersion
        };
    }

    private async Task NotifyAsync(
        Guid orderId, string eventName, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _db.Orders.AsNoTracking()
                .Include(x => x.Table).Include(x => x.Waiter)
                .Include(x => x.Items.Where(i => !i.IsDeleted))
                .FirstAsync(x => x.ID == orderId, cancellationToken);
            var notification = new KitchenOrderEventDTO(
                eventName, MapKitchenOrder(order), UtcNow,
                eventName == "OrderCreated" ? "new-order"
                    : eventName == "OrderReady" ? "order-ready" : null);
            switch (eventName)
            {
                case "OrderCreated":
                    await _notifier.OrderCreatedAsync(notification, cancellationToken);
                    break;
                case "OrderCancelled":
                    await _notifier.OrderCancelledAsync(notification, cancellationToken);
                    break;
                case "OrderReady":
                    await _notifier.OrderReadyAsync(notification,
                        order.Waiter?.AppUserId, cancellationToken);
                    break;
                case "OrderStatusChanged":
                    await _notifier.OrderStatusChangedAsync(notification, cancellationToken);
                    break;
                default:
                    await _notifier.OrderUpdatedAsync(notification, cancellationToken);
                    break;
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception,
                "Order {OrderId} was saved but realtime notification {EventName} failed.",
                orderId, eventName);
        }
    }

    private static decimal RoundMoney(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
