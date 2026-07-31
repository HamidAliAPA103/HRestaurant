using AutoMapper;
using HRestaurant.Data;
using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.Responses;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class MenuCategoryService : IMenuCategoryService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public MenuCategoryService(AppDbContext db, IMapper mapper,
        ICurrentUserContext currentUser, TimeProvider timeProvider)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<Guid>> CreateAsync(MenuCategoryCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        EnsureRestaurantAccess(dto.ResdaranId);
        await EnsureRestaurantExistsAsync(dto.ResdaranId, cancellationToken);
        var name = dto.Name.Trim();
        var normalized = Normalize(name);
        await EnsureUniqueNameAsync(dto.ResdaranId, normalized, null, cancellationToken);
        var entity = _mapper.Map<MenuCategory>(dto);
        entity.Name = name;
        entity.NormalizedName = normalized;
        entity.Description = NormalizeOptional(dto.Description);
        entity.ImageUrl = NormalizeOptional(dto.ImageUrl);
        entity.IsActive = true;
        entity.CreatAt = UtcNow;
        _db.MenuCategories.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Created(entity.ID, "Category created successfully.");
    }

    public async Task<PagedResponse<MenuCategoryGetDTO>> GetAllAsync(
        MenuCategoryListRequest request, CancellationToken cancellationToken = default)
    {
        var restaurantId = request.RestaurantId;
        if (!_currentUser.IsSuperAdmin) restaurantId = _currentUser.RestaurantId;
        if (request.RestaurantId.HasValue) EnsureRestaurantAccess(request.RestaurantId.Value);
        var query = _db.MenuCategories.AsNoTracking().Where(x => !x.IsDeleted);
        if (restaurantId.HasValue) query = query.Where(x => x.ResdaranId == restaurantId);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return PagedResponse<MenuCategoryGetDTO>.Create(
            _mapper.Map<List<MenuCategoryGetDTO>>(items), request.PageNumber,
            request.PageSize, total, "Categories retrieved successfully.");
    }

    public async Task<ApiResponse<MenuCategoryGetDTO>> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.MenuCategories.AsNoTracking().FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Category", id);
        EnsureRestaurantAccess(entity.ResdaranId);
        return ApiResponse.Ok(_mapper.Map<MenuCategoryGetDTO>(entity));
    }

    public async Task<ApiResponse<object?>> UpdateAsync(Guid id,
        MenuCategoryUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        if (dto.Name is not null)
        {
            var name = dto.Name.Trim();
            var normalized = Normalize(name);
            await EnsureUniqueNameAsync(entity.ResdaranId, normalized, entity.ID, cancellationToken);
            entity.Name = name;
            entity.NormalizedName = normalized;
        }
        _mapper.Map(dto, entity);
        if (dto.Description is not null) entity.Description = NormalizeOptional(dto.Description);
        if (dto.ImageUrl is not null) entity.ImageUrl = NormalizeOptional(dto.ImageUrl);
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Category updated successfully.");
    }

    public async Task<ApiResponse<object?>> SoftDeleteAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        var hasActiveItems = await _db.Menus.AsNoTracking().AnyAsync(x =>
            x.CategoryId == id && !x.IsDeleted && x.IsAvailable, cancellationToken);
        if (hasActiveItems)
            throw new ConflictException("A category with active menu items cannot be deleted.");
        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.DeletedAt = UtcNow;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.NoContent("Category deleted successfully.");
    }

    public Task<ApiResponse<object?>> ActivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        SetActiveAsync(id, true, cancellationToken);
    public Task<ApiResponse<object?>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        SetActiveAsync(id, false, cancellationToken);

    public async Task<ApiResponse<object?>> UpdateDisplayOrderAsync(Guid id,
        MenuCategoryDisplayOrderDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        entity.DisplayOrder = dto.DisplayOrder;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Category display order updated successfully.");
    }

    private async Task<ApiResponse<object?>> SetActiveAsync(Guid id, bool active,
        CancellationToken cancellationToken)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        entity.IsActive = active;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success(active ? "Category activated successfully." : "Category deactivated successfully.");
    }

    private async Task<MenuCategory> GetForMutationAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.MenuCategories.FirstOrDefaultAsync(x => x.ID == id && !x.IsDeleted,
            cancellationToken) ?? throw new NotFoundException("Category", id);
        EnsureRestaurantAccess(entity.ResdaranId);
        return entity;
    }

    private void EnsureRestaurantAccess(Guid restaurantId)
    {
        if (!_currentUser.IsSuperAdmin && _currentUser.RestaurantId != restaurantId)
            throw new ForbiddenException("Another restaurant's categories cannot be managed.");
    }

    private async Task EnsureRestaurantExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await _db.Restaurants.AsNoTracking().AnyAsync(x => x.ID == id && !x.IsDeleted,
                cancellationToken)) throw new NotFoundException("Restaurant", id);
    }

    private async Task EnsureUniqueNameAsync(Guid restaurantId, string normalized,
        Guid? excluded, CancellationToken cancellationToken)
    {
        if (await _db.MenuCategories.AsNoTracking().AnyAsync(x => x.ResdaranId == restaurantId
                && x.NormalizedName == normalized && !x.IsDeleted
                && (!excluded.HasValue || x.ID != excluded), cancellationToken))
            throw new ConflictException("A category with the same name already exists in this restaurant.");
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
