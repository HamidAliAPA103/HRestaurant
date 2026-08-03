using AutoMapper;
using System.Data;
using HRestaurant.Data;
using HRestaurant.DTOS.Ingredient;
using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Responses;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class IngredientService : IIngredientService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public IngredientService(
        AppDbContext db,
        IMapper mapper,
        ICurrentUserContext currentUser,
        TimeProvider timeProvider)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<Guid>> CreateAsync(
        IngredientCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        EnsureRestaurantAccess(dto.RestaurantId);
        if (!await _db.Restaurants.AsNoTracking().AnyAsync(
                x => x.ID == dto.RestaurantId && !x.IsDeleted, cancellationToken))
            throw new NotFoundException("Restaurant", dto.RestaurantId);

        var name = dto.Name.Trim();
        var normalized = Normalize(name);
        await EnsureUniqueNameAsync(dto.RestaurantId, normalized, null, cancellationToken);
        var entity = _mapper.Map<Ingredient>(dto);
        entity.Name = name;
        entity.NormalizedName = normalized;
        ApplyPublicPresentation(entity, dto.Model3DUrl, dto.ImageUrl, dto.Description,
            dto.Origin, dto.AllergenInformation, dto.Calories, dto.Protein,
            dto.Carbohydrates, dto.Fat);
        entity.CreatAt = UtcNow;
        _db.Ingredients.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Created(entity.ID, "Ingredient created successfully.");
    }

    public async Task<PagedResponse<IngredientGetDTO>> GetAllAsync(
        IngredientListRequest request,
        CancellationToken cancellationToken = default)
    {
        var restaurantId = request.RestaurantId;
        if (!_currentUser.IsSuperAdmin)
        {
            if (restaurantId.HasValue) EnsureRestaurantAccess(restaurantId.Value);
            restaurantId = _currentUser.RestaurantId;
        }

        var query = _db.Ingredients.AsNoTracking().Where(x => !x.IsDeleted);
        if (restaurantId.HasValue) query = query.Where(x => x.RestaurantId == restaurantId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = Normalize(request.Search);
            query = query.Where(x => x.NormalizedName.Contains(search));
        }
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Name).ThenBy(x => x.ID)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return PagedResponse<IngredientGetDTO>.Create(
            _mapper.Map<List<IngredientGetDTO>>(items), request.PageNumber,
            request.PageSize, total, "Ingredients retrieved successfully.");
    }

    public async Task<ApiResponse<IngredientGetDTO>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Ingredients.AsNoTracking().FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Ingredient", id);
        EnsureRestaurantAccess(entity.RestaurantId);
        return ApiResponse.Ok(_mapper.Map<IngredientGetDTO>(entity));
    }

    public async Task<ApiResponse<object?>> UpdateAsync(
        Guid id,
        IngredientUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        var name = dto.Name.Trim();
        var normalized = Normalize(name);
        await EnsureUniqueNameAsync(entity.RestaurantId, normalized, entity.ID, cancellationToken);
        entity.Name = name;
        entity.NormalizedName = normalized;
        entity.Unit = dto.Unit;
        entity.IsActive = dto.IsActive;
        ApplyPublicPresentation(entity, dto.Model3DUrl, dto.ImageUrl, dto.Description,
            dto.Origin, dto.AllergenInformation, dto.Calories, dto.Protein,
            dto.Carbohydrates, dto.Fat);
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Ingredient updated successfully.");
    }

    public async Task<ApiResponse<object?>> SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var entity = await GetForMutationAsync(id, cancellationToken);
        var isUsed = await _db.MenuItemIngredients.AsNoTracking().AnyAsync(x =>
            x.IngredientId == id && !x.MenuItem.IsDeleted, cancellationToken);
        if (isUsed)
            throw new ConflictException("An ingredient used by a menu item cannot be deleted.");
        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.DeletedAt = UtcNow;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.NoContent("Ingredient deleted successfully.");
    }

    public async Task<ApiResponse<object?>> AddToMenuItemAsync(
        Guid menuItemId,
        MenuItemIngredientDTO dto,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var menu = await GetMenuAsync(menuItemId, cancellationToken);
        var ingredient = await _db.Ingredients.AsNoTracking().FirstOrDefaultAsync(x =>
            x.ID == dto.IngredientId && !x.IsDeleted && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Ingredient", dto.IngredientId);
        if (ingredient.RestaurantId != menu.RestaurantId)
            throw new ConflictException("The ingredient and menu item must belong to the same restaurant.");
        if (await _db.MenuItemIngredients.AsNoTracking().AnyAsync(x =>
                x.MenuItemId == menuItemId && x.IngredientId == dto.IngredientId, cancellationToken))
            throw new ConflictException("The ingredient is already assigned to this menu item.");

        _db.MenuItemIngredients.Add(new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            IngredientId = dto.IngredientId,
            RequiredQuantity = dto.RequiredQuantity,
            ExplodedPositionX = dto.ExplodedPositionX,
            ExplodedPositionY = dto.ExplodedPositionY,
            ExplodedPositionZ = dto.ExplodedPositionZ,
            ExplodedRotationX = dto.ExplodedRotationX,
            ExplodedRotationY = dto.ExplodedRotationY,
            ExplodedRotationZ = dto.ExplodedRotationZ,
            DisplayOrder = dto.DisplayOrder,
            IsVisibleIn3D = dto.IsVisibleIn3D
        });
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Success("Ingredient added to menu item successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateQuantityAsync(
        Guid menuItemId,
        Guid ingredientId,
        decimal quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(quantity)] = ["Required quantity must be greater than zero."]
            });
        await GetMenuAsync(menuItemId, cancellationToken);
        var link = await _db.MenuItemIngredients.FirstOrDefaultAsync(x =>
            x.MenuItemId == menuItemId && x.IngredientId == ingredientId, cancellationToken)
            ?? throw new NotFoundException("Menu item ingredient");
        link.RequiredQuantity = quantity;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Ingredient quantity updated successfully.");
    }

    public async Task<ApiResponse<object?>> RemoveFromMenuItemAsync(
        Guid menuItemId,
        Guid ingredientId,
        CancellationToken cancellationToken = default)
    {
        await GetMenuAsync(menuItemId, cancellationToken);
        var link = await _db.MenuItemIngredients.FirstOrDefaultAsync(x =>
            x.MenuItemId == menuItemId && x.IngredientId == ingredientId, cancellationToken)
            ?? throw new NotFoundException("Menu item ingredient");
        _db.MenuItemIngredients.Remove(link);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.NoContent("Ingredient removed from menu item successfully.");
    }

    private async Task<Menu> GetMenuAsync(Guid id, CancellationToken cancellationToken)
    {
        var menu = await _db.Menus.AsNoTracking().FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Menu item", id);
        EnsureRestaurantAccess(menu.RestaurantId);
        return menu;
    }

    private async Task<Ingredient> GetForMutationAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.Ingredients.FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Ingredient", id);
        EnsureRestaurantAccess(entity.RestaurantId);
        return entity;
    }

    private async Task EnsureUniqueNameAsync(
        Guid restaurantId, string normalized, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (await _db.Ingredients.AsNoTracking().AnyAsync(x =>
                x.RestaurantId == restaurantId && x.NormalizedName == normalized && !x.IsDeleted
                && (!excludedId.HasValue || x.ID != excludedId.Value), cancellationToken))
            throw new ConflictException("An ingredient with the same name already exists in this restaurant.");
    }

    private void EnsureRestaurantAccess(Guid restaurantId)
    {
        if (!_currentUser.IsSuperAdmin && _currentUser.RestaurantId != restaurantId)
            throw new ForbiddenException("Another restaurant's ingredients cannot be accessed or modified.");
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ApplyPublicPresentation(
        Ingredient entity,
        string? model3DUrl,
        string? imageUrl,
        string? description,
        string? origin,
        string? allergenInformation,
        decimal? calories,
        decimal? protein,
        decimal? carbohydrates,
        decimal? fat)
    {
        entity.Model3DUrl = NormalizeOptional(model3DUrl);
        entity.ImageUrl = NormalizeOptional(imageUrl);
        entity.Description = NormalizeOptional(description);
        entity.Origin = NormalizeOptional(origin);
        entity.AllergenInformation = NormalizeOptional(allergenInformation);
        entity.Calories = calories;
        entity.Protein = protein;
        entity.Carbohydrates = carbohydrates;
        entity.Fat = fat;
    }

    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
