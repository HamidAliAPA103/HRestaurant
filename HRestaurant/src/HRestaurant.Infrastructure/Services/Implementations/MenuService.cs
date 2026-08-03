using AutoMapper;
using System.Data;
using HRestaurant.Data;
using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Responses;
using HRestaurant.Exceptions;
using HRestaurant.Extentions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class MenuService : IMenuService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _web;
    private readonly IHttpContextAccessor _accessor;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public MenuService(
        AppDbContext db,
        IMapper mapper,
        IWebHostEnvironment web,
        IHttpContextAccessor accessor,
        ICurrentUserContext currentUser,
        TimeProvider timeProvider)
    {
        _db = db;
        _mapper = mapper;
        _web = web;
        _accessor = accessor;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<Guid>> CreateAsync(
        MenuCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var category = await GetActiveCategoryAsync(dto.CategoryId, cancellationToken);
        EnsureRestaurantAccess(category.ResdaranId);

        var name = dto.Name.Trim();
        var normalizedName = Normalize(name);
        await EnsureUniqueNameAsync(category.ID, normalizedName, null, cancellationToken);

        string? createdImage = null;
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        try
        {
            await ValidateIngredientsAsync(category.ResdaranId, dto.Ingredients, cancellationToken);
            if (dto.Image is not null)
            {
                createdImage = await dto.Image.CreateFileAsync(
                    cancellationToken, _web.WebRootPath, "images", "menus");
            }

            var entity = _mapper.Map<Menu>(dto);
            entity.RestaurantId = category.ResdaranId;
            entity.CategoryId = category.ID;
            entity.Name = name;
            entity.NormalizedName = normalizedName;
            entity.Image = createdImage ?? string.Empty;
            entity.ImageURL = createdImage is not null
                ? BuildImageUrl(createdImage)
                : NormalizeOptional(dto.ImageUrl) ?? string.Empty;
            entity.Desc = dto.Desc.Trim();
            entity.Nutrition = dto.Nutrition.Trim();
            entity.Model3DUrl = NormalizeOptional(dto.Model3DUrl);
            entity.ModelPosterUrl = NormalizeOptional(dto.ModelPosterUrl);
            entity.FinalPrice = CalculateFinalPrice(dto.Price, dto.DiscountPercentage);
            entity.CreatAt = UtcNow;
            entity.Ingredients = dto.Ingredients.Select(item => new MenuItemIngredient
            {
                IngredientId = item.IngredientId,
                RequiredQuantity = item.RequiredQuantity,
                ExplodedPositionX = item.ExplodedPositionX,
                ExplodedPositionY = item.ExplodedPositionY,
                ExplodedPositionZ = item.ExplodedPositionZ,
                ExplodedRotationX = item.ExplodedRotationX,
                ExplodedRotationY = item.ExplodedRotationY,
                ExplodedRotationZ = item.ExplodedRotationZ,
                DisplayOrder = item.DisplayOrder,
                IsVisibleIn3D = item.IsVisibleIn3D
            }).ToList();

            _db.Menus.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ApiResponse.Created(entity.ID, "Menu item created successfully.");
        }
        catch
        {
            if (createdImage is not null)
                createdImage.DeleteFile(_web.WebRootPath, "images", "menus");
            throw;
        }
    }

    public Task<PagedResponse<MenuGetDTO>> GetAllAsync(
        MenuListRequest request,
        CancellationToken cancellationToken = default) =>
        GetListAsync(request, null, null, cancellationToken);

    public async Task<PagedResponse<MenuGetDTO>> GetByRestaurantAsync(
        Guid restaurantId,
        MenuListRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureRestaurantAccess(restaurantId);
        if (!await _db.Restaurants.AsNoTracking().AnyAsync(
                x => x.ID == restaurantId && !x.IsDeleted, cancellationToken))
            throw new NotFoundException("Restaurant", restaurantId);
        return await GetListAsync(request, restaurantId, null, cancellationToken);
    }

    public async Task<PagedResponse<MenuGetDTO>> GetByCategoryAsync(
        Guid categoryId,
        MenuListRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _db.MenuCategories.AsNoTracking().FirstOrDefaultAsync(
            x => x.ID == categoryId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Category", categoryId);
        EnsureRestaurantAccess(category.ResdaranId);
        return await GetListAsync(request, category.ResdaranId, categoryId, cancellationToken);
    }

    public async Task<ApiResponse<MenuGetDTO>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await DetailQuery().FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted && !x.Category.IsDeleted,
            cancellationToken) ?? throw new NotFoundException("Menu item", id);
        EnsureRestaurantAccess(entity.RestaurantId);
        return ApiResponse.Ok(_mapper.Map<MenuGetDTO>(entity), "Menu item retrieved successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateAsync(
        Guid id,
        MenuUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Menus.Include(x => x.Ingredients).FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Menu item", id);
        EnsureRestaurantAccess(entity.RestaurantId);

        var categoryId = dto.CategoryId ?? entity.CategoryId;
        var category = await GetActiveCategoryAsync(categoryId, cancellationToken);
        if (category.ResdaranId != entity.RestaurantId)
            throw new ConflictException("A category from another restaurant cannot be selected.");

        var name = dto.Name is null ? entity.Name : dto.Name.Trim();
        var normalizedName = Normalize(name);
        await EnsureUniqueNameAsync(categoryId, normalizedName, entity.ID, cancellationToken);
        string? newImage = null;
        var previousImage = entity.Image;
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        try
        {
            if (dto.Ingredients is not null)
                await ValidateIngredientsAsync(entity.RestaurantId, dto.Ingredients, cancellationToken);
            if (dto.Image is not null)
                newImage = await dto.Image.CreateFileAsync(
                    cancellationToken, _web.WebRootPath, "images", "menus");

            entity.CategoryId = categoryId;
            entity.Name = name;
            entity.NormalizedName = normalizedName;
            if (dto.Price.HasValue) entity.Price = dto.Price.Value;
            if (dto.DiscountPercentage.HasValue) entity.DiscountPercentage = dto.DiscountPercentage.Value;
            if (dto.PreparationTimeMinutes.HasValue) entity.PreparationTimeMinutes = dto.PreparationTimeMinutes.Value;
            if (dto.Desc is not null) entity.Desc = dto.Desc.Trim();
            if (dto.Nutrition is not null) entity.Nutrition = dto.Nutrition.Trim();
            if (dto.Model3DUrl is not null)
                entity.Model3DUrl = NormalizeOptional(dto.Model3DUrl);
            if (dto.ModelPosterUrl is not null)
                entity.ModelPosterUrl = NormalizeOptional(dto.ModelPosterUrl);
            if (dto.ModelScale.HasValue) entity.ModelScale = dto.ModelScale.Value;
            if (dto.ModelRotationX.HasValue) entity.ModelRotationX = dto.ModelRotationX.Value;
            if (dto.ModelRotationY.HasValue) entity.ModelRotationY = dto.ModelRotationY.Value;
            if (dto.ModelRotationZ.HasValue) entity.ModelRotationZ = dto.ModelRotationZ.Value;
            if (dto.Is3DEnabled.HasValue) entity.Is3DEnabled = dto.Is3DEnabled.Value;
            if (newImage is not null)
            {
                entity.Image = newImage;
                entity.ImageURL = BuildImageUrl(newImage);
            }
            else if (dto.ImageURL is not null)
            {
                entity.Image = string.Empty;
                entity.ImageURL = NormalizeOptional(dto.ImageURL) ?? string.Empty;
            }

            entity.FinalPrice = CalculateFinalPrice(entity.Price, entity.DiscountPercentage);
            entity.UpdateAt = UtcNow;

            if (dto.Ingredients is not null)
            {
                _db.MenuItemIngredients.RemoveRange(entity.Ingredients);
                entity.Ingredients.Clear();
                foreach (var item in dto.Ingredients)
                    entity.Ingredients.Add(new MenuItemIngredient
                    {
                        IngredientId = item.IngredientId,
                        RequiredQuantity = item.RequiredQuantity,
                        ExplodedPositionX = item.ExplodedPositionX,
                        ExplodedPositionY = item.ExplodedPositionY,
                        ExplodedPositionZ = item.ExplodedPositionZ,
                        ExplodedRotationX = item.ExplodedRotationX,
                        ExplodedRotationY = item.ExplodedRotationY,
                        ExplodedRotationZ = item.ExplodedRotationZ,
                        DisplayOrder = item.DisplayOrder,
                        IsVisibleIn3D = item.IsVisibleIn3D
                    });
            }

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            if (newImage is not null)
                newImage.DeleteFile(_web.WebRootPath, "images", "menus");
            throw;
        }

        if ((newImage is not null || dto.ImageURL is not null)
            && !string.IsNullOrWhiteSpace(previousImage))
            previousImage.DeleteFile(_web.WebRootPath, "images", "menus");
        return ApiResponse.Success("Menu item updated successfully.");
    }

    public async Task<ApiResponse<object?>> SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        entity.IsDeleted = true;
        entity.IsAvailable = false;
        entity.DeletedAt = UtcNow;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.NoContent("Menu item deleted successfully.");
    }

    public Task<ApiResponse<object?>> SetAvailabilityAsync(
        Guid id, bool isAvailable, CancellationToken cancellationToken = default) =>
        SetFlagAsync(id, isAvailable, popular: null, cancellationToken);

    public Task<ApiResponse<object?>> SetPopularAsync(
        Guid id, bool isPopular, CancellationToken cancellationToken = default) =>
        SetFlagAsync(id, available: null, isPopular, cancellationToken);

    private async Task<PagedResponse<MenuGetDTO>> GetListAsync(
        MenuListRequest request,
        Guid? fixedRestaurantId,
        Guid? fixedCategoryId,
        CancellationToken cancellationToken)
    {
        var restaurantId = fixedRestaurantId ?? request.RestaurantId;
        if (!_currentUser.IsSuperAdmin)
        {
            if (restaurantId.HasValue) EnsureRestaurantAccess(restaurantId.Value);
            restaurantId = _currentUser.RestaurantId;
        }
        else if (restaurantId.HasValue)
        {
            EnsureRestaurantAccess(restaurantId.Value);
        }

        var query = DetailQuery().Where(x => !x.IsDeleted && !x.Category.IsDeleted);
        if (restaurantId.HasValue) query = query.Where(x => x.RestaurantId == restaurantId.Value);
        var categoryId = fixedCategoryId ?? request.CategoryId;
        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = Normalize(request.Search);
            query = query.Where(x => x.NormalizedName.Contains(search));
        }
        if (request.IsAvailable.HasValue) query = query.Where(x => x.IsAvailable == request.IsAvailable.Value);
        if (request.IsPopular.HasValue) query = query.Where(x => x.IsPopular == request.IsPopular.Value);
        if (request.MinPrice.HasValue) query = query.Where(x => x.FinalPrice >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue) query = query.Where(x => x.FinalPrice <= request.MaxPrice.Value);

        query = (request.SortBy.ToLowerInvariant(), request.SortDirection.ToLowerInvariant()) switch
        {
            ("price", "desc") => query.OrderByDescending(x => x.FinalPrice).ThenBy(x => x.ID),
            ("price", _) => query.OrderBy(x => x.FinalPrice).ThenBy(x => x.ID),
            ("name", "desc") => query.OrderByDescending(x => x.Name).ThenBy(x => x.ID),
            _ => query.OrderBy(x => x.Name).ThenBy(x => x.ID)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize).ToListAsync(cancellationToken);
        return PagedResponse<MenuGetDTO>.Create(
            _mapper.Map<List<MenuGetDTO>>(items), request.PageNumber, request.PageSize,
            total, "Menu items retrieved successfully.");
    }

    private IQueryable<Menu> DetailQuery() => _db.Menus.AsNoTracking()
        .Include(x => x.Category)
        .Include(x => x.Ingredients).ThenInclude(x => x.Ingredient)
        .AsSplitQuery();

    private async Task<Menu> GetForMutationAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.Menus.FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Menu item", id);
        EnsureRestaurantAccess(entity.RestaurantId);
        return entity;
    }

    private async Task<ApiResponse<object?>> SetFlagAsync(
        Guid id, bool? available, bool? popular, CancellationToken cancellationToken)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        if (available == true && !await _db.MenuCategories.AsNoTracking().AnyAsync(
                x => x.ID == entity.CategoryId && !x.IsDeleted && x.IsActive,
                cancellationToken))
            throw new ConflictException("A menu item in a deleted or inactive category cannot be made available.");
        if (available.HasValue) entity.IsAvailable = available.Value;
        if (popular.HasValue) entity.IsPopular = popular.Value;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Menu item status updated successfully.");
    }

    private async Task<MenuCategory> GetActiveCategoryAsync(Guid id, CancellationToken cancellationToken) =>
        await _db.MenuCategories.AsNoTracking().FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted && x.IsActive, cancellationToken)
        ?? throw new ConflictException("The selected category does not exist or is inactive.");

    private async Task ValidateIngredientsAsync(
        Guid restaurantId,
        IReadOnlyCollection<MenuItemIngredientDTO> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0) return;
        var ids = items.Select(x => x.IngredientId).Distinct().ToArray();
        if (ids.Length != items.Count)
            throw new ConflictException("Duplicate ingredients are not allowed.");
        var validCount = await _db.Ingredients.AsNoTracking().CountAsync(x =>
            ids.Contains(x.ID) && x.RestaurantId == restaurantId && !x.IsDeleted && x.IsActive,
            cancellationToken);
        if (validCount != ids.Length)
            throw new ConflictException("Every ingredient must be active and belong to the menu item's restaurant.");
    }

    private async Task EnsureUniqueNameAsync(
        Guid categoryId, string normalizedName, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (await _db.Menus.AsNoTracking().AnyAsync(x => x.CategoryId == categoryId
                && x.NormalizedName == normalizedName && !x.IsDeleted
                && (!excludedId.HasValue || x.ID != excludedId.Value), cancellationToken))
            throw new ConflictException("A menu item with the same name already exists in this category.");
    }

    private void EnsureRestaurantAccess(Guid restaurantId)
    {
        if (!_currentUser.IsSuperAdmin && _currentUser.RestaurantId != restaurantId)
            throw new ForbiddenException("Another restaurant's menu items cannot be accessed or modified.");
    }

    private static decimal CalculateFinalPrice(decimal price, decimal discount) =>
        decimal.Round(price * (1 - discount / 100m), 2, MidpointRounding.AwayFromZero);

    private string BuildImageUrl(string imageName)
    {
        var request = _accessor.HttpContext?.Request
            ?? throw new InvalidOperationException("The current HTTP request is not available.");
        return $"{request.Scheme}://{request.Host}/images/menus/{imageName}";
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
