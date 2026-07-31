using AutoMapper;
using HRestaurant.Data;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Table;
using HRestaurant.Enum;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class TableService : ITableService
{
    private static readonly OrderStatus[] ActiveOrderStatuses =
        [OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Preparing, OrderStatus.Ready];
    private static readonly ReservationStatus[] ActiveReservationStatuses =
        [ReservationStatus.Pending, ReservationStatus.Confirmed, ReservationStatus.Seated];

    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public TableService(AppDbContext db, IMapper mapper,
        ICurrentUserContext currentUser, TimeProvider timeProvider)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<Guid>> CreateAsync(
        TableCreateDTO dto, CancellationToken cancellationToken = default)
    {
        await EnsureBranchAccessAsync(dto.BranchId, dto.RestaurantId, cancellationToken);
        var tableNumber = dto.TableNumber.Trim();
        await EnsureNumberUniqueAsync(dto.BranchId, tableNumber, null, cancellationToken);
        var entity = _mapper.Map<Table>(dto);
        entity.TableNumber = tableNumber;
        if (!entity.IsActive || entity.Status == TableStatus.Disabled)
        {
            entity.IsActive = false;
            entity.Status = TableStatus.Disabled;
        }
        entity.CreatAt = UtcNow;
        _db.Tables.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Created(entity.ID, "Table created successfully.");
    }

    public async Task<PagedResponse<TableGetDTO>> GetAllAsync(
        TableListRequest request, CancellationToken cancellationToken = default)
    {
        var restaurantId = request.RestaurantId;
        if (!_currentUser.IsSuperAdmin)
        {
            if (restaurantId.HasValue && restaurantId.Value != _currentUser.RestaurantId)
                throw new ForbiddenException("Another restaurant's tables cannot be accessed.");
            restaurantId = _currentUser.RestaurantId;
        }
        if (request.BranchId.HasValue)
        {
            var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(x =>
                x.ID == request.BranchId.Value && !x.IsDeleted, cancellationToken)
                ?? throw new NotFoundException("Branch", request.BranchId.Value);
            await EnsureBranchAccessAsync(branch.ID, branch.RestaurantId, cancellationToken);
        }
        var query = _db.Tables.AsNoTracking().Include(x => x.Branch)
            .Where(x => !x.IsDeleted && x.BranchId != null);
        if (restaurantId.HasValue) query = query.Where(x => x.RestaurantID == restaurantId.Value);
        if (request.BranchId.HasValue) query = query.Where(x => x.BranchId == request.BranchId.Value);
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => x.Branch!.ManagerId == userId);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.TableNumber.Contains(search));
        }
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.TableNumber).ThenBy(x => x.ID)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return PagedResponse<TableGetDTO>.Create(_mapper.Map<List<TableGetDTO>>(items),
            request.PageNumber, request.PageSize, total, "Tables retrieved successfully.");
    }

    public async Task<ApiResponse<TableGetDTO>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Tables.AsNoTracking().Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.ID == id && !x.IsDeleted && x.BranchId != null,
                cancellationToken)
            ?? throw new NotFoundException("Table", id);
        await EnsureBranchAccessAsync(entity.BranchId!.Value, entity.RestaurantID, cancellationToken);
        return ApiResponse.Ok(_mapper.Map<TableGetDTO>(entity));
    }

    public async Task<ApiResponse<object?>> UpdateAsync(
        Guid id, TableUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        var number = dto.TableNumber.Trim();
        await EnsureNumberUniqueAsync(entity.BranchId!.Value, number, entity.ID, cancellationToken);
        _mapper.Map(dto, entity);
        entity.TableNumber = number;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Table updated successfully.");
    }

    public async Task<ApiResponse<object?>> SoftDeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        var activeOrder = await _db.Orders.AsNoTracking().AnyAsync(x =>
            x.TableID == id && !x.IsDeleted && ActiveOrderStatuses.Contains(x.Status),
            cancellationToken);
        var activeReservation = await _db.Reservations.AsNoTracking().AnyAsync(x =>
            x.TableId == id && !x.IsDeleted && ActiveReservationStatuses.Contains(x.Status),
            cancellationToken);
        if (activeOrder || activeReservation)
            throw new ConflictException("A table with an active order or reservation cannot be deleted.");
        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.Status = TableStatus.Disabled;
        entity.DeletedAt = UtcNow;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.NoContent("Table deleted successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateStatusAsync(
        Guid id, TableStatusUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        if (!entity.IsActive && dto.Status != TableStatus.Disabled)
            throw new ConflictException("Activate the table before changing it to an operational status.");
        entity.Status = dto.Status;
        if (dto.Status == TableStatus.Disabled)
            entity.IsActive = false;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Table status updated successfully.");
    }

    public Task<ApiResponse<object?>> UpdatePositionAsync(
        Guid id, TablePositionUpdateDTO dto, CancellationToken cancellationToken = default) =>
        UpdateGeometryAsync(id, table =>
        {
            table.PositionX = dto.PositionX;
            table.PositionY = dto.PositionY;
            table.PositionZ = dto.PositionZ;
        }, "Table position updated successfully.", cancellationToken);

    public Task<ApiResponse<object?>> UpdateRotationAsync(
        Guid id, TableRotationUpdateDTO dto, CancellationToken cancellationToken = default) =>
        UpdateGeometryAsync(id, table =>
        {
            table.RotationX = dto.RotationX;
            table.RotationY = dto.RotationY;
            table.RotationZ = dto.RotationZ;
        }, "Table rotation updated successfully.", cancellationToken);

    public Task<ApiResponse<object?>> UpdateSizeAsync(
        Guid id, TableSizeUpdateDTO dto, CancellationToken cancellationToken = default) =>
        UpdateGeometryAsync(id, table =>
        {
            table.Width = dto.Width;
            table.Length = dto.Length;
            table.Height = dto.Height;
        }, "Table size updated successfully.", cancellationToken);

    public async Task<ApiResponse<object?>> SaveLayoutAsync(
        TableLayoutBulkUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(x =>
            x.ID == dto.BranchId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Branch", dto.BranchId);
        await EnsureBranchAccessAsync(branch.ID, branch.RestaurantId, cancellationToken);
        var ids = dto.Tables.Select(x => x.TableId).ToArray();
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var entities = await _db.Tables.Where(x => ids.Contains(x.ID) && !x.IsDeleted)
            .ToListAsync(cancellationToken);
        if (entities.Count != ids.Length || entities.Any(x => x.BranchId != dto.BranchId))
            throw new ForbiddenException("Every table must belong to the selected branch.");
        var layouts = dto.Tables.ToDictionary(x => x.TableId);
        var now = UtcNow;
        foreach (var entity in entities)
        {
            var layout = layouts[entity.ID];
            entity.PositionX = layout.PositionX;
            entity.PositionY = layout.PositionY;
            entity.PositionZ = layout.PositionZ;
            entity.RotationX = layout.RotationX;
            entity.RotationY = layout.RotationY;
            entity.RotationZ = layout.RotationZ;
            entity.Width = layout.Width;
            entity.Length = layout.Length;
            entity.UpdateAt = now;
        }
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Success("Table layout saved successfully.");
    }

    public Task<ApiResponse<object?>> ActivateAsync(
        Guid id, CancellationToken cancellationToken = default) =>
        SetActiveAsync(id, true, cancellationToken);

    public Task<ApiResponse<object?>> DeactivateAsync(
        Guid id, CancellationToken cancellationToken = default) =>
        SetActiveAsync(id, false, cancellationToken);

    public async Task<ApiResponse<IReadOnlyCollection<PublicTableLayoutDTO>>> GetPublicLayoutAsync(
        Guid branchId, CancellationToken cancellationToken = default)
    {
        if (!await _db.Branches.AsNoTracking().AnyAsync(x =>
                x.ID == branchId && x.IsActive && !x.IsDeleted, cancellationToken))
            throw new NotFoundException("Branch", branchId);
        var rows = await _db.Tables.AsNoTracking().Where(x =>
                x.BranchId == branchId && !x.IsDeleted)
            .OrderBy(x => x.TableNumber).Select(x => new
            {
                x.ID,
                x.TableNumber,
                x.Tutum,
                x.Shape,
                x.Status,
                x.IsActive,
                x.PositionX,
                x.PositionY,
                x.PositionZ,
                x.RotationX,
                x.RotationY,
                x.RotationZ,
                x.Width,
                x.Length,
                x.Height
            }).ToListAsync(cancellationToken);
        var tables = rows.Select(x => new PublicTableLayoutDTO
            {
                Id = x.ID,
                TableNumber = x.TableNumber,
                Capacity = x.Tutum,
                Shape = x.Shape.ToString(),
                Position = new TableVectorDTO
                {
                    X = x.PositionX ?? 0, Y = x.PositionY ?? 0, Z = x.PositionZ ?? 0
                },
                Rotation = new TableVectorDTO
                {
                    X = x.RotationX ?? 0, Y = x.RotationY ?? 0, Z = x.RotationZ ?? 0
                },
                Dimensions = new TableDimensionsDTO
                {
                    Width = x.Width, Length = x.Length, Height = x.Height
                },
                PublicStatus = !x.IsActive
                    ? TableStatus.Disabled.ToString()
                    : x.Status == TableStatus.Available
                        ? nameof(TableStatus.Available)
                        : x.Status.ToString()
            }).ToList();
        return ApiResponse.Ok<IReadOnlyCollection<PublicTableLayoutDTO>>(
            tables, "Public table layout retrieved successfully.");
    }

    private async Task<Table> GetForMutationAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.Tables.FirstOrDefaultAsync(x =>
            x.ID == id && !x.IsDeleted && x.BranchId != null, cancellationToken)
            ?? throw new NotFoundException("Table", id);
        await EnsureBranchAccessAsync(entity.BranchId!.Value, entity.RestaurantID, cancellationToken);
        return entity;
    }

    private async Task<ApiResponse<object?>> UpdateGeometryAsync(
        Guid id, Action<Table> update, string message, CancellationToken cancellationToken)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        update(entity);
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success(message);
    }

    private async Task<ApiResponse<object?>> SetActiveAsync(
        Guid id, bool active, CancellationToken cancellationToken)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        entity.IsActive = active;
        if (!active) entity.Status = TableStatus.Disabled;
        else if (entity.Status == TableStatus.Disabled) entity.Status = TableStatus.Available;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success(active ? "Table activated successfully." : "Table deactivated successfully.");
    }

    private async Task EnsureNumberUniqueAsync(
        Guid branchId, string number, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (await _db.Tables.AsNoTracking().AnyAsync(x => x.BranchId == branchId
                && x.TableNumber == number && !x.IsDeleted
                && (!excludedId.HasValue || x.ID != excludedId.Value), cancellationToken))
            throw new ConflictException("A table with the same number already exists in this branch.");
    }

    private async Task EnsureBranchAccessAsync(
        Guid branchId, Guid restaurantId, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsSuperAdmin && restaurantId != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant's tables cannot be accessed or modified.");
        var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(x =>
            x.ID == branchId && x.RestaurantId == restaurantId && !x.IsDeleted,
            cancellationToken) ?? throw new NotFoundException("Branch", branchId);
        if (_currentUser.IsManager && branch.ManagerId != _currentUser.UserId)
            throw new ForbiddenException("Managers can manage only their own branch tables.");
    }

    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
