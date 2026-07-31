using AutoMapper;
using System.Data;
using HRestaurant.Configuration;
using HRestaurant.Data;
using HRestaurant.DTOS.Inventory;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class InventoryService : IInventoryService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;
    private readonly IInventoryAlertService _alerts;
    private readonly InventoryAlertSettings _settings;
    private readonly TimeProvider _timeProvider;

    public InventoryService(
        AppDbContext db, IMapper mapper, ICurrentUserContext currentUser,
        IInventoryAlertService alerts, InventoryAlertSettings settings,
        TimeProvider timeProvider)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
        _alerts = alerts;
        _settings = settings;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<Guid>> CreateAsync(
        InventoryItemCreateDTO dto, CancellationToken cancellationToken = default)
    {
        await EnsureRelationsAsync(dto.RestaurantId, dto.BranchId,
            dto.IngredientId, dto.SupplierId, cancellationToken);
        if (dto.CurrentQuantity > 0 && IsExpired(dto.ExpirationDate))
            throw new ConflictException("Expired inventory cannot be stocked in.");

        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var entity = _mapper.Map<InventoryItem>(dto);
        entity.BatchNumber = NormalizeOptional(dto.BatchNumber);
        entity.CreatAt = UtcNow;
        _db.InventoryItems.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        if (entity.CurrentQuantity > 0)
            _db.StockTransactions.Add(CreateTransaction(entity,
                StockTransactionType.StockIn, entity.CurrentQuantity, 0,
                entity.CurrentQuantity, entity.PurchasePrice, "Initial stock", null));
        await _db.SaveChangesAsync(cancellationToken);
        await _alerts.EvaluateItemAsync(entity.ID, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Created(entity.ID, "Inventory item created successfully.");
    }

    public async Task<ApiResponse<InventoryItemGetDTO>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await DetailQuery().FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Inventory item", id);
        await EnsureBranchAccessAsync(entity.BranchId, entity.RestaurantId, cancellationToken);
        return ApiResponse.Ok(_mapper.Map<InventoryItemGetDTO>(entity));
    }

    public Task<PagedResponse<InventoryItemGetDTO>> GetAllAsync(
        InventoryListRequest request, CancellationToken cancellationToken = default) =>
        GetListAsync(request, null, null, InventoryListMode.All, cancellationToken);

    public Task<PagedResponse<InventoryItemGetDTO>> GetByRestaurantAsync(
        Guid restaurantId, InventoryListRequest request,
        CancellationToken cancellationToken = default) =>
        GetListAsync(request, restaurantId, null, InventoryListMode.All, cancellationToken);

    public Task<PagedResponse<InventoryItemGetDTO>> GetByBranchAsync(
        Guid branchId, InventoryListRequest request,
        CancellationToken cancellationToken = default) =>
        GetListAsync(request, null, branchId, InventoryListMode.All, cancellationToken);

    public Task<PagedResponse<InventoryItemGetDTO>> GetExpiredAsync(
        InventoryListRequest request, CancellationToken cancellationToken = default) =>
        GetListAsync(request, null, null, InventoryListMode.Expired, cancellationToken);

    public Task<PagedResponse<InventoryItemGetDTO>> GetExpiringSoonAsync(
        InventoryListRequest request, CancellationToken cancellationToken = default) =>
        GetListAsync(request, null, null, InventoryListMode.ExpiringSoon, cancellationToken);

    public Task<PagedResponse<InventoryItemGetDTO>> GetLowStockAsync(
        InventoryListRequest request, CancellationToken cancellationToken = default) =>
        GetListAsync(request, null, null, InventoryListMode.LowStock, cancellationToken);

    public async Task<ApiResponse<object?>> UpdateAsync(
        Guid id, InventoryItemUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        await EnsureSupplierAsync(entity.RestaurantId, dto.SupplierId, cancellationToken);
        ApplyExpectedVersion(entity, dto.RowVersion);
        entity.SupplierId = dto.SupplierId;
        entity.MinimumQuantity = dto.MinimumQuantity;
        entity.Unit = dto.Unit;
        entity.PurchasePrice = dto.PurchasePrice;
        entity.ExpirationDate = dto.ExpirationDate;
        entity.BatchNumber = NormalizeOptional(dto.BatchNumber);
        entity.IsActive = dto.IsActive;
        entity.UpdateAt = UtcNow;
        await SaveWithConcurrencyAsync(cancellationToken);
        await _alerts.EvaluateItemAsync(entity.ID, cancellationToken);
        return ApiResponse.Success("Inventory item updated successfully.");
    }

    public async Task<ApiResponse<object?>> SoftDeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.DeletedAt = UtcNow;
        entity.UpdateAt = UtcNow;
        var alerts = await _db.InventoryNotifications.Where(x =>
            x.InventoryItemId == id && !x.IsResolved && !x.IsDeleted)
            .ToListAsync(cancellationToken);
        foreach (var alert in alerts)
        {
            alert.IsResolved = true;
            alert.ResolvedAtUtc = UtcNow;
            alert.UpdateAt = UtcNow;
        }
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.NoContent("Inventory item deleted successfully.");
    }

    public Task<ApiResponse<InventoryItemGetDTO>> StockInAsync(
        Guid id, StockMovementDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto.TransactionType is not (StockTransactionType.StockIn or StockTransactionType.Return))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(dto.TransactionType)] = ["Stock-in supports StockIn or Return transaction types."]
            });
        return ChangeStockAsync(id, dto, increase: true, cancellationToken);
    }

    public Task<ApiResponse<InventoryItemGetDTO>> StockOutAsync(
        Guid id, StockMovementDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto.TransactionType is not (StockTransactionType.StockOut
                or StockTransactionType.Waste or StockTransactionType.OrderConsumption))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(dto.TransactionType)] = ["Stock-out supports StockOut, Waste or OrderConsumption transaction types."]
            });
        return ChangeStockAsync(id, dto, increase: false, cancellationToken);
    }

    public async Task<ApiResponse<InventoryItemGetDTO>> AdjustAsync(
        Guid id, StockAdjustmentDTO dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var entity = await GetForMutationAsync(id, cancellationToken, includeDetails: true);
        EnsureActive(entity);
        ApplyExpectedVersion(entity, dto.RowVersion);
        var previous = entity.CurrentQuantity;
        var quantity = Math.Abs(dto.NewQuantity - previous);
        if (quantity <= 0)
            throw new ConflictException("The adjusted quantity must differ from the current quantity.");
        entity.CurrentQuantity = dto.NewQuantity;
        entity.UpdateAt = UtcNow;
        _db.StockTransactions.Add(CreateTransaction(entity,
            StockTransactionType.Adjustment, quantity, previous, dto.NewQuantity,
            dto.UnitPrice, dto.Reason, dto.ReferenceNumber));
        await SaveWithConcurrencyAsync(cancellationToken);
        await _alerts.EvaluateItemAsync(entity.ID, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Ok(_mapper.Map<InventoryItemGetDTO>(entity),
            "Stock adjusted successfully.");
    }

    public async Task<PagedResponse<StockTransactionGetDTO>> GetTransactionsAsync(
        Guid inventoryItemId, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var item = await _db.InventoryItems.AsNoTracking().FirstOrDefaultAsync(
            x => x.ID == inventoryItemId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Inventory item", inventoryItemId);
        await EnsureBranchAccessAsync(item.BranchId, item.RestaurantId, cancellationToken);
        var query = _db.StockTransactions.AsNoTracking().Where(x =>
            x.InventoryItemId == inventoryItemId && !x.IsDeleted);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatAt).ThenBy(x => x.ID)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return PagedResponse<StockTransactionGetDTO>.Create(
            _mapper.Map<List<StockTransactionGetDTO>>(items), pageNumber, pageSize,
            total, "Stock transactions retrieved successfully.");
    }

    private async Task<ApiResponse<InventoryItemGetDTO>> ChangeStockAsync(
        Guid id, StockMovementDTO dto, bool increase,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var entity = await GetForMutationAsync(id, cancellationToken, includeDetails: true);
        EnsureActive(entity);
        if (increase && IsExpired(entity.ExpirationDate))
            throw new ConflictException("Stock-in is not allowed for an expired inventory batch.");
        ApplyExpectedVersion(entity, dto.RowVersion);
        var previous = entity.CurrentQuantity;
        var next = increase ? previous + dto.Quantity : previous - dto.Quantity;
        if (next < 0)
            throw new ConflictException("The stock operation would create a negative quantity.");
        entity.CurrentQuantity = next;
        if (dto.UnitPrice.HasValue && increase) entity.PurchasePrice = dto.UnitPrice.Value;
        entity.UpdateAt = UtcNow;
        _db.StockTransactions.Add(CreateTransaction(entity, dto.TransactionType,
            dto.Quantity, previous, next, dto.UnitPrice, dto.Reason, dto.ReferenceNumber));
        await SaveWithConcurrencyAsync(cancellationToken);
        await _alerts.EvaluateItemAsync(entity.ID, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Ok(_mapper.Map<InventoryItemGetDTO>(entity),
            increase ? "Stock-in completed successfully." : "Stock-out completed successfully.");
    }

    private async Task<PagedResponse<InventoryItemGetDTO>> GetListAsync(
        InventoryListRequest request, Guid? fixedRestaurantId, Guid? fixedBranchId,
        InventoryListMode mode, CancellationToken cancellationToken)
    {
        var restaurantId = fixedRestaurantId ?? request.RestaurantId;
        if (!_currentUser.IsSuperAdmin)
        {
            if (restaurantId.HasValue && restaurantId.Value != _currentUser.RestaurantId)
                throw new ForbiddenException("Another restaurant's inventory cannot be accessed.");
            restaurantId = _currentUser.RestaurantId;
        }
        var branchId = fixedBranchId ?? request.BranchId;
        if (branchId.HasValue)
        {
            var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(x =>
                x.ID == branchId.Value && !x.IsDeleted, cancellationToken)
                ?? throw new NotFoundException("Branch", branchId.Value);
            if (restaurantId.HasValue && branch.RestaurantId != restaurantId.Value)
                throw new ConflictException("The branch does not belong to the selected restaurant.");
            await EnsureBranchAccessAsync(branch.ID, branch.RestaurantId, cancellationToken);
        }
        var query = DetailQuery().Where(x => !x.IsDeleted);
        if (restaurantId.HasValue) query = query.Where(x => x.RestaurantId == restaurantId.Value);
        if (branchId.HasValue) query = query.Where(x => x.BranchId == branchId.Value);
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => x.Branch.ManagerId == userId);
        }
        if (request.IngredientId.HasValue) query = query.Where(x => x.IngredientId == request.IngredientId.Value);
        if (request.SupplierId.HasValue) query = query.Where(x => x.SupplierId == request.SupplierId.Value);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Ingredient.Name.Contains(search)
                || (x.BatchNumber != null && x.BatchNumber.Contains(search)));
        }
        var today = DateOnly.FromDateTime(UtcNow);
        query = mode switch
        {
            InventoryListMode.Expired => query.Where(x => x.ExpirationDate < today),
            InventoryListMode.ExpiringSoon => query.Where(x => x.ExpirationDate >= today
                && x.ExpirationDate <= today.AddDays(_settings.ExpiringSoonDays)),
            InventoryListMode.LowStock => query.Where(x => x.CurrentQuantity < x.MinimumQuantity),
            _ => query
        };
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Ingredient.Name).ThenBy(x => x.ExpirationDate)
            .ThenBy(x => x.ID).Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize).ToListAsync(cancellationToken);
        return PagedResponse<InventoryItemGetDTO>.Create(
            _mapper.Map<List<InventoryItemGetDTO>>(items), request.PageNumber,
            request.PageSize, total, "Inventory items retrieved successfully.");
    }

    private IQueryable<InventoryItem> DetailQuery() => _db.InventoryItems.AsNoTracking()
        .Include(x => x.Branch).Include(x => x.Ingredient).Include(x => x.Supplier);

    private async Task<InventoryItem> GetForMutationAsync(
        Guid id, CancellationToken cancellationToken, bool includeDetails = false)
    {
        IQueryable<InventoryItem> query = _db.InventoryItems;
        if (includeDetails)
            query = query.Include(x => x.Branch).Include(x => x.Ingredient).Include(x => x.Supplier);
        var entity = await query.FirstOrDefaultAsync(x => x.ID == id && !x.IsDeleted,
            cancellationToken) ?? throw new NotFoundException("Inventory item", id);
        await EnsureBranchAccessAsync(entity.BranchId, entity.RestaurantId, cancellationToken);
        return entity;
    }

    private async Task EnsureRelationsAsync(
        Guid restaurantId, Guid branchId, Guid ingredientId, Guid? supplierId,
        CancellationToken cancellationToken)
    {
        await EnsureBranchAccessAsync(branchId, restaurantId, cancellationToken);
        if (!await _db.Ingredients.AsNoTracking().AnyAsync(x =>
                x.ID == ingredientId && x.RestaurantId == restaurantId
                && !x.IsDeleted && x.IsActive, cancellationToken))
            throw new ConflictException("The ingredient must be active and belong to the inventory restaurant.");
        await EnsureSupplierAsync(restaurantId, supplierId, cancellationToken);
    }

    private async Task EnsureSupplierAsync(
        Guid restaurantId, Guid? supplierId, CancellationToken cancellationToken)
    {
        if (!supplierId.HasValue) return;
        if (!await _db.Suppliers.AsNoTracking().AnyAsync(x =>
                x.ID == supplierId.Value && x.RestaurantId == restaurantId
                && !x.IsDeleted && x.IsActive, cancellationToken))
            throw new ConflictException("The supplier must be active and belong to the inventory restaurant.");
    }

    private async Task EnsureBranchAccessAsync(
        Guid branchId, Guid restaurantId, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsSuperAdmin && restaurantId != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant's inventory cannot be accessed or modified.");
        var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(x =>
            x.ID == branchId && x.RestaurantId == restaurantId && !x.IsDeleted,
            cancellationToken) ?? throw new NotFoundException("Branch", branchId);
        if (_currentUser.IsManager && branch.ManagerId != _currentUser.UserId)
            throw new ForbiddenException("Managers can access only their own branch inventory.");
    }

    private void ApplyExpectedVersion(InventoryItem entity, byte[] rowVersion)
    {
        _db.Entry(entity).Property(x => x.RowVersion).OriginalValue = rowVersion;
    }

    private async Task SaveWithConcurrencyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "Inventory was changed by another user. Reload it and retry the operation.");
        }
    }

    private StockTransaction CreateTransaction(
        InventoryItem item, StockTransactionType type, decimal quantity,
        decimal previous, decimal next, decimal? unitPrice, string reason,
        string? referenceNumber) => new()
    {
        InventoryItemId = item.ID,
        TransactionType = type,
        Quantity = quantity,
        PreviousQuantity = previous,
        NewQuantity = next,
        UnitPrice = unitPrice,
        Reason = reason.Trim(),
        ReferenceNumber = NormalizeOptional(referenceNumber),
        CreatedByUserId = _currentUser.UserId,
        CreatAt = UtcNow
    };

    private static void EnsureActive(InventoryItem item)
    {
        if (!item.IsActive) throw new ConflictException("Inactive inventory cannot be modified.");
    }

    private bool IsExpired(DateOnly? date) => date.HasValue
        && date.Value < DateOnly.FromDateTime(UtcNow);
    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;

    private enum InventoryListMode { All, Expired, ExpiringSoon, LowStock }
}
