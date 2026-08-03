using AutoMapper;
using HRestaurant.Data;
using HRestaurant.DTOS.Public;
using HRestaurant.DTOS.Responses;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class PublicRestaurantService
    : IPublicRestaurantService
{
    private const string DefaultTimeZoneId = "Asia/Baku";

    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public PublicRestaurantService(
        AppDbContext dbContext,
        IMapper mapper,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dbContext = dbContext;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<IReadOnlyCollection<PublicRestaurantDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var restaurants = await _dbContext.Restaurants
            .AsNoTracking()
            .AsSplitQuery()
            .Include(restaurant => restaurant.WorkingHours)
            .Include(restaurant => restaurant.Branches)
                .ThenInclude(branch => branch.WorkingHours)
            .Where(restaurant => restaurant.IsActive && !restaurant.IsDeleted)
            .OrderBy(restaurant => restaurant.Name)
            .ToArrayAsync(cancellationToken);

        var result = restaurants.Select(MapRestaurant).ToArray();
        return ApiResponse.Ok<IReadOnlyCollection<PublicRestaurantDto>>(
            result,
            "Public restaurants retrieved successfully.");
    }

    public async Task<ApiResponse<PublicRestaurantDto>> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await GetRestaurantAsync(
            slug,
            cancellationToken);
        var dto = MapRestaurant(restaurant);

        return ApiResponse.Ok(
            dto,
            "Public restaurant information retrieved successfully.");
    }

    public async Task<
        ApiResponse<IReadOnlyCollection<PublicBranchDto>>>
        GetBranchesAsync(
            string restaurantSlug,
            CancellationToken cancellationToken = default)
    {
        var restaurant = await GetRestaurantAsync(
            restaurantSlug,
            cancellationToken);

        return ApiResponse.Ok<IReadOnlyCollection<PublicBranchDto>>(
            MapBranches(restaurant.Branches),
            "Public branches retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<PublicMenuCategoryDto>>> GetMenuAsync(
        string restaurantSlug,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await GetRestaurantAsync(restaurantSlug, cancellationToken);
        var categories = await _dbContext.MenuCategories
            .AsNoTracking()
            .Where(category =>
                category.ResdaranId == restaurant.ID
                && category.IsActive
                && !category.IsDeleted)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .Select(category => new PublicMenuCategoryDto
            {
                Id = category.ID,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                Items = category.Menus
                    .Where(item => !item.IsDeleted)
                    .OrderByDescending(item => item.IsPopular)
                    .ThenBy(item => item.Name)
                    .Select(item => new PublicMenuItemDto
                    {
                        Id = item.ID,
                        CategoryId = item.CategoryId,
                        Name = item.Name,
                        Description = item.Desc,
                        Nutrition = item.Nutrition,
                        ImageUrl = item.ImageURL,
                        Price = item.Price,
                        DiscountPercentage = item.DiscountPercentage,
                        FinalPrice = item.FinalPrice,
                        PreparationTimeMinutes = item.PreparationTimeMinutes,
                        IsAvailable = item.IsAvailable,
                        IsPopular = item.IsPopular,
                        Is3DEnabled = item.Is3DEnabled,
                        ModelPosterUrl = item.ModelPosterUrl
                    })
                    .ToArray()
            })
            .ToArrayAsync(cancellationToken);

        return ApiResponse.Ok<IReadOnlyCollection<PublicMenuCategoryDto>>(
            categories,
            "Public menu retrieved successfully.");
    }

    public async Task<ApiResponse<PublicMenuItem3DDto>> GetMenuItem3DAsync(
        Guid menuItemId,
        CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.Menus
            .AsNoTracking()
            .Where(menu =>
                menu.ID == menuItemId
                && !menu.IsDeleted
                && !menu.Category.IsDeleted
                && menu.Category.IsActive
                && !menu.Restaurant.IsDeleted
                && menu.Restaurant.IsActive)
            .Select(menu => new PublicMenuItem3DDto
            {
                Id = menu.ID,
                RestaurantSlug = menu.Restaurant.Slug,
                RestaurantName = menu.Restaurant.Name,
                CategoryName = menu.Category.Name,
                Name = menu.Name,
                Description = menu.Desc,
                Nutrition = menu.Nutrition,
                ImageUrl = menu.ImageURL,
                Price = menu.Price,
                DiscountPercentage = menu.DiscountPercentage,
                FinalPrice = menu.FinalPrice,
                PreparationTimeMinutes = menu.PreparationTimeMinutes,
                IsAvailable = menu.IsAvailable,
                IsPopular = menu.IsPopular,
                Model3DUrl = menu.Model3DUrl,
                ModelPosterUrl = menu.ModelPosterUrl,
                ModelScale = menu.ModelScale,
                ModelRotationX = menu.ModelRotationX,
                ModelRotationY = menu.ModelRotationY,
                ModelRotationZ = menu.ModelRotationZ,
                Is3DEnabled = menu.Is3DEnabled
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Menu item", menuItemId);

        return ApiResponse.Ok(item, "Public 3D menu item retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<PublicIngredient3DDto>>>
        GetMenuItemIngredients3DAsync(
            Guid menuItemId,
            CancellationToken cancellationToken = default)
    {
        var menuItemExists = await _dbContext.Menus.AsNoTracking().AnyAsync(menu =>
            menu.ID == menuItemId
            && !menu.IsDeleted
            && !menu.Category.IsDeleted
            && menu.Category.IsActive
            && !menu.Restaurant.IsDeleted
            && menu.Restaurant.IsActive,
            cancellationToken);
        if (!menuItemExists)
            throw new NotFoundException("Menu item", menuItemId);

        var rows = await _dbContext.MenuItemIngredients
            .AsNoTracking()
            .Where(link =>
                link.MenuItemId == menuItemId
                && !link.MenuItem.IsDeleted
                && !link.MenuItem.Category.IsDeleted
                && link.MenuItem.Category.IsActive
                && !link.MenuItem.Restaurant.IsDeleted
                && link.MenuItem.Restaurant.IsActive
                && link.Ingredient.RestaurantId == link.MenuItem.RestaurantId
                && !link.Ingredient.IsDeleted
                && link.Ingredient.IsActive)
            .OrderBy(link => link.DisplayOrder)
            .ThenBy(link => link.Ingredient.Name)
            .Select(link => new
            {
                link.Ingredient.ID,
                link.Ingredient.Name,
                link.Ingredient.Unit,
                link.RequiredQuantity,
                link.Ingredient.Model3DUrl,
                link.Ingredient.ImageUrl,
                link.Ingredient.Description,
                link.Ingredient.Calories,
                link.Ingredient.Protein,
                link.Ingredient.Carbohydrates,
                link.Ingredient.Fat,
                link.Ingredient.Origin,
                link.Ingredient.AllergenInformation,
                link.ExplodedPositionX,
                link.ExplodedPositionY,
                link.ExplodedPositionZ,
                link.ExplodedRotationX,
                link.ExplodedRotationY,
                link.ExplodedRotationZ,
                link.DisplayOrder,
                link.IsVisibleIn3D
            })
            .ToArrayAsync(cancellationToken);

        var result = rows.Select(row => new PublicIngredient3DDto
        {
            Id = row.ID,
            Name = row.Name,
            Unit = row.Unit.ToString(),
            RequiredQuantity = row.RequiredQuantity,
            Model3DUrl = row.Model3DUrl,
            ImageUrl = row.ImageUrl,
            Description = row.Description,
            Calories = row.Calories,
            Protein = row.Protein,
            Carbohydrates = row.Carbohydrates,
            Fat = row.Fat,
            Origin = row.Origin,
            AllergenInformation = row.AllergenInformation,
            ExplodedPositionX = row.ExplodedPositionX,
            ExplodedPositionY = row.ExplodedPositionY,
            ExplodedPositionZ = row.ExplodedPositionZ,
            ExplodedRotationX = row.ExplodedRotationX,
            ExplodedRotationY = row.ExplodedRotationY,
            ExplodedRotationZ = row.ExplodedRotationZ,
            DisplayOrder = row.DisplayOrder,
            IsVisibleIn3D = row.IsVisibleIn3D,
            FallbackKind = GetFallbackKind(row.Name)
        }).ToArray();

        return ApiResponse.Ok<IReadOnlyCollection<PublicIngredient3DDto>>(
            result,
            "Public 3D ingredient data retrieved successfully.");
    }

    private async Task<Restaurant> GetRestaurantAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new NotFoundException(
                "The requested restaurant was not found.");
        }

        var normalizedSlug = slug.Trim().ToLowerInvariant();

        return await _dbContext.Restaurants
                .AsNoTracking()
                .AsSplitQuery()
                .Include(restaurant => restaurant.WorkingHours)
                .Include(restaurant => restaurant.Branches)
                    .ThenInclude(branch => branch.WorkingHours)
                .FirstOrDefaultAsync(
                    restaurant =>
                        restaurant.Slug == normalizedSlug
                        && restaurant.IsActive
                        && !restaurant.IsDeleted,
                    cancellationToken)
            ?? throw new NotFoundException(
                "The requested restaurant was not found.");
    }

    private IReadOnlyCollection<PublicBranchDto> MapBranches(
        IEnumerable<Branch> branches)
    {
        return branches
            .Where(branch =>
                branch.IsActive
                && !branch.IsDeleted)
            .OrderBy(branch => branch.Name)
            .Select(branch =>
            {
                var dto = _mapper.Map<PublicBranchDto>(branch);
                dto.WorkingHours = branch.WorkingHours
                    .Where(entry => !entry.IsDeleted)
                    .OrderBy(entry => entry.DayOfWeek)
                    .Select(entry =>
                        _mapper.Map<PublicWorkingHourDto>(entry))
                    .ToArray();
                dto.IsOpenNow = ReservationSchedule.IsOpenNow(
                    branch.WorkingHours,
                    _timeProvider.GetUtcNow().UtcDateTime,
                    branch.TimeZoneId);

                return dto;
            })
            .ToArray();
    }

    private PublicRestaurantDto MapRestaurant(Restaurant restaurant)
    {
        var dto = _mapper.Map<PublicRestaurantDto>(restaurant);
        var branches = MapBranches(restaurant.Branches);
        dto.WorkingHours = restaurant.WorkingHours
            .Where(entry => !entry.IsDeleted)
            .OrderBy(entry => entry.DayOfWeek)
            .Select(entry => _mapper.Map<PublicWorkingHourDto>(entry))
            .ToArray();
        dto.Branches = branches;
        dto.IsOpenNow = branches.Any(branch => branch.IsOpenNow)
            || (branches.Count == 0
                && ReservationSchedule.IsOpenNow(
                    restaurant.WorkingHours,
                    _timeProvider.GetUtcNow().UtcDateTime,
                    DefaultTimeZoneId));
        return dto;
    }

    private static string GetFallbackKind(string ingredientName)
    {
        var name = ingredientName.Trim().ToLowerInvariant();
        if (ContainsAny(name, "pomidor", "tomato")) return "tomato";
        if (ContainsAny(name, "xiyar", "cucumber")) return "cucumber";
        if (ContainsAny(name, "pendir", "cheese")) return "cheese";
        if (ContainsAny(name, "sous", "sauce")) return "sauce";
        if (ContainsAny(name, "göyərti", "goyerti", "herb", "cəfəri", "ceferi", "şüyüd", "suyud"))
            return "herb";
        return "generic";
    }

    private static bool ContainsAny(string value, params string[] terms) =>
        terms.Any(value.Contains);
}
