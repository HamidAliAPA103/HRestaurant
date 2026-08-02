using AutoMapper;
using HRestaurant.Data;
using HRestaurant.DTOS.Inventory;
using HRestaurant.DTOS.Responses;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class InventoryNotificationService : IInventoryNotificationService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public InventoryNotificationService(AppDbContext db, IMapper mapper,
        ICurrentUserContext currentUser, TimeProvider timeProvider)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public Task<PagedResponse<InventoryNotificationGetDTO>> GetAllAsync(
        InventoryNotificationListRequest request,
        CancellationToken cancellationToken = default) =>
        GetListAsync(request, cancellationToken);

    public Task<PagedResponse<InventoryNotificationGetDTO>> GetUnreadAsync(
        InventoryNotificationListRequest request,
        CancellationToken cancellationToken = default)
    {
        request.IsRead = false;
        request.IsResolved ??= false;
        return GetListAsync(request, cancellationToken);
    }

    public async Task<ApiResponse<InventoryNotificationGetDTO>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var query = ApplyAccess(_db.InventoryNotifications.AsNoTracking()
                .Include(x => x.InventoryItem).ThenInclude(x => x!.Ingredient)
                .Where(x => x.ID == id && !x.IsDeleted),
            null, null, cancellationToken, out var accessTask);
        await accessTask;
        var entity = await query.FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Notification", id);
        return ApiResponse.Ok(_mapper.Map<InventoryNotificationGetDTO>(entity));
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(
        Guid? branchId, CancellationToken cancellationToken = default)
    {
        var query = ApplyAccess(_db.InventoryNotifications.AsNoTracking()
            .Where(x => !x.IsDeleted && !x.IsRead && !x.IsResolved), null, branchId,
            cancellationToken, out var accessTask);
        await accessTask;
        return ApiResponse.Ok(await query.CountAsync(cancellationToken),
            "Unread notification count retrieved successfully.");
    }

    public async Task<ApiResponse<object?>> MarkAsReadAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        if (!entity.IsRead)
        {
            entity.IsRead = true;
            entity.ReadAtUtc = UtcNow;
            entity.UpdateAt = UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
        return ApiResponse.Success("Notification marked as read.");
    }

    public async Task<ApiResponse<object?>> MarkAllAsReadAsync(
        Guid? branchId, CancellationToken cancellationToken = default)
    {
        var query = ApplyAccess(_db.InventoryNotifications.Where(x =>
                !x.IsDeleted && !x.IsRead && !x.IsResolved), null, branchId,
            cancellationToken, out var accessTask);
        await accessTask;
        var entities = await query.ToListAsync(cancellationToken);
        var now = UtcNow;
        foreach (var entity in entities)
        {
            entity.IsRead = true;
            entity.ReadAtUtc = now;
            entity.UpdateAt = now;
        }
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("All notifications marked as read.");
    }

    public async Task<ApiResponse<object?>> ResolveAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        entity.IsResolved = true;
        entity.ResolvedAtUtc = UtcNow;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Inventory alert resolved successfully.");
    }

    private async Task<PagedResponse<InventoryNotificationGetDTO>> GetListAsync(
        InventoryNotificationListRequest request, CancellationToken cancellationToken)
    {
        var query = _db.InventoryNotifications.AsNoTracking()
            .Include(x => x.InventoryItem).ThenInclude(x => x!.Ingredient)
            .Where(x => !x.IsDeleted);
        query = ApplyAccess(query, request.RestaurantId, request.BranchId,
            cancellationToken, out var accessTask);
        await accessTask;
        if (request.Type.HasValue) query = query.Where(x => x.Type == request.Type.Value);
        if (request.IsRead.HasValue) query = query.Where(x => x.IsRead == request.IsRead.Value);
        if (request.IsResolved.HasValue) query = query.Where(x => x.IsResolved == request.IsResolved.Value);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatAt).ThenBy(x => x.ID)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return PagedResponse<InventoryNotificationGetDTO>.Create(
            _mapper.Map<List<InventoryNotificationGetDTO>>(items), request.PageNumber,
            request.PageSize, total, "Inventory notifications retrieved successfully.");
    }

    private IQueryable<InventoryNotification> ApplyAccess(
        IQueryable<InventoryNotification> query, Guid? restaurantId, Guid? branchId,
        CancellationToken cancellationToken, out Task accessTask)
    {
        accessTask = Task.CompletedTask;
        if (!_currentUser.IsSuperAdmin)
        {
            if (restaurantId.HasValue && restaurantId.Value != _currentUser.RestaurantId)
                throw new ForbiddenException("Another restaurant's notifications cannot be accessed.");
            query = query.Where(x => x.RestaurantId == _currentUser.RestaurantId);
        }
        else if (restaurantId.HasValue)
            query = query.Where(x => x.RestaurantId == restaurantId.Value);
        if (branchId.HasValue)
        {
            accessTask = EnsureBranchAccessAsync(branchId.Value, cancellationToken);
            query = query.Where(x => x.BranchId == branchId.Value);
        }
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => _db.Branches.Any(branch =>
                branch.ID == x.BranchId && branch.ManagerId == userId && !branch.IsDeleted));
        }
        return query;
    }

    private async Task<InventoryNotification> GetForMutationAsync(
        Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.InventoryNotifications.FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Inventory notification", id);
        await EnsureBranchAccessAsync(entity.BranchId, cancellationToken);
        return entity;
    }

    private async Task EnsureBranchAccessAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(
            x => x.ID == branchId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Branch", branchId);
        if (!_currentUser.IsSuperAdmin && branch.RestaurantId != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant's notifications cannot be accessed.");
        if (_currentUser.IsManager && branch.ManagerId != _currentUser.UserId)
            throw new ForbiddenException("Managers can access only their own branch notifications.");
    }

    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
