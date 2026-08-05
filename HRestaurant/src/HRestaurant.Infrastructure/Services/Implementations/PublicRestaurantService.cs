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
                        Has3DModel = item.Is3DEnabled
                            && !string.IsNullOrWhiteSpace(item.Model3DUrl),
                        EnableIngredientAnimation = item.EnableIngredientAnimation,
                        ModelPosterUrl = item.ModelPosterUrl
                        ,VideoUrl = item.VideoUrl
                        ,VideoPosterUrl = item.VideoPosterUrl
                        ,VideoDurationSeconds = item.VideoDurationSeconds
                        ,IsVideoEnabled = item.IsVideoEnabled
                            && !string.IsNullOrWhiteSpace(item.VideoUrl)
                        ,VideoDisplayOrder = item.VideoDisplayOrder
                        ,Ingredients = item.Ingredients
                            .Where(link => !link.Ingredient.IsDeleted && link.Ingredient.IsActive)
                            .OrderBy(link => link.DisplayOrder)
                            .Select(link => link.Ingredient.Name)
                            .ToArray()
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
                Is3DEnabled = menu.Is3DEnabled,
                EnableIngredientAnimation = menu.EnableIngredientAnimation
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

    public async Task<ApiResponse<PublicRestaurantExperienceDto>> GetExperienceAsync(
        string restaurantSlug,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await GetRestaurantAsync(restaurantSlug, cancellationToken);
        var publicRestaurant = MapRestaurant(restaurant);
        var result = new PublicRestaurantExperienceDto
        {
            Restaurant = publicRestaurant,
            DefaultBranchId = publicRestaurant.Branches.FirstOrDefault()?.Id
        };

        return ApiResponse.Ok(result, "Public restaurant experience retrieved successfully.");
    }

    public async Task<ApiResponse<PublicRestaurantSceneDto>> GetSceneAsync(
        string restaurantSlug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(restaurantSlug))
            throw new NotFoundException("The requested restaurant was not found.");

        var normalizedSlug = restaurantSlug.Trim().ToLowerInvariant();
        var restaurant = await _dbContext.Restaurants
            .AsNoTracking()
            .AsSplitQuery()
            .Include(entity => entity.Branches)
                .ThenInclude(branch => branch.Tables)
            .FirstOrDefaultAsync(entity =>
                entity.Slug == normalizedSlug
                && entity.IsActive
                && !entity.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException("The requested restaurant was not found.");

        var branches = restaurant.Branches
            .Where(branch => branch.IsActive && !branch.IsDeleted)
            .OrderBy(branch => branch.Name)
            .Select(MapSceneBranch)
            .ToArray();

        var scene = new PublicRestaurantSceneDto
        {
            RestaurantId = restaurant.ID,
            RestaurantSlug = restaurant.Slug,
            RestaurantName = restaurant.Name,
            Branches = branches
        };

        return ApiResponse.Ok(scene, "Public restaurant scene retrieved successfully.");
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
                && branch.IsPubliclyVisible
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

    private static PublicBranchSceneDto MapSceneBranch(Branch branch)
    {
        var tables = branch.Tables
            .Where(table => !table.IsDeleted)
            .OrderBy(table => table.TableNumber)
            .Select(table => new PublicSceneTableDto
            {
                Id = table.ID,
                TableNumber = table.TableNumber,
                Capacity = table.Tutum,
                Shape = table.Shape.ToString(),
                Status = !table.IsActive
                    ? "Disabled"
                    : table.Status == HRestaurant.Enum.TableStatus.Available
                        ? "Available"
                        : table.Status.ToString(),
                PositionX = table.PositionX ?? 0,
                PositionY = table.PositionY ?? 0,
                PositionZ = table.PositionZ ?? 0,
                RotationX = table.RotationX ?? 0,
                RotationY = table.RotationY ?? 0,
                RotationZ = table.RotationZ ?? 0,
                Width = table.Width,
                Length = table.Length,
                Height = table.Height
            })
            .ToArray();

        var minX = tables.Length == 0
            ? -6d
            : tables.Min(table => table.PositionX - table.Width / 2);
        var maxX = tables.Length == 0
            ? 6d
            : tables.Max(table => table.PositionX + table.Width / 2);
        var minZ = tables.Length == 0
            ? -5d
            : tables.Min(table => table.PositionZ - table.Length / 2);
        var maxZ = tables.Length == 0
            ? 5d
            : tables.Max(table => table.PositionZ + table.Length / 2);
        var centerX = (minX + maxX) / 2;
        var centerZ = (minZ + maxZ) / 2;
        var width = Math.Max(12, maxX - minX + 6);
        var depth = Math.Max(10, maxZ - minZ + 6);
        var hotspots = BuildHotspots(tables, centerX, centerZ, width, depth);

        return new PublicBranchSceneDto
        {
            BranchId = branch.ID,
            BranchName = branch.Name,
            FloorWidth = width,
            FloorDepth = depth,
            WallHeight = 4,
            CenterX = centerX,
            CenterZ = centerZ,
            Tables = tables,
            Hotspots = hotspots
        };
    }

    private static IReadOnlyCollection<PublicSceneHotspotDto> BuildHotspots(
        IReadOnlyCollection<PublicSceneTableDto> tables,
        double centerX,
        double centerZ,
        double width,
        double depth)
    {
        var tableCenterX = tables.Count == 0 ? centerX : tables.Average(table => table.PositionX);
        var tableCenterZ = tables.Count == 0 ? centerZ : tables.Average(table => table.PositionZ);
        var definitions = new[]
        {
            new SceneHotspotDefinition("entrance", "Entrance", "Restoranın əsas giriş və qarşılama zonası.", centerX, centerZ + depth * 0.43, centerX, 2.4, centerZ + depth * 0.72, false),
            new SceneHotspotDefinition("main-hall", "Main Hall", "Əsas zal və restoranın mərkəzi servis sahəsi.", centerX, centerZ, centerX + 4.2, 3.1, centerZ + 5.2, true),
            new SceneHotspotDefinition("window-area", "Window Area", "Təbii işıq alan pəncərə yanı masa zonası.", centerX - width * 0.34, centerZ, centerX - width * 0.16, 2.5, centerZ + 4.2, true),
            new SceneHotspotDefinition("vip-area", "VIP Area", "Daha sakit və məxfi oturma sahəsi.", centerX + width * 0.31, centerZ - depth * 0.23, centerX + width * 0.12, 2.6, centerZ + depth * 0.08, true),
            new SceneHotspotDefinition("bar", "Bar", "İçki servisi və qısa oturuş üçün bar zonası.", centerX + width * 0.33, centerZ + depth * 0.27, centerX + width * 0.08, 2.4, centerZ + depth * 0.1, true),
            new SceneHotspotDefinition("kitchen-preview", "Kitchen Preview", "Mətbəx servis pəncərəsinə təhlükəsiz baxış nöqtəsi.", centerX - width * 0.32, centerZ - depth * 0.34, centerX - width * 0.08, 2.5, centerZ - depth * 0.05, false),
            new SceneHotspotDefinition("table-area", "Table Area", "Mövcud masa planının mərkəzi baxış nöqtəsi.", tableCenterX, tableCenterZ, tableCenterX + 3.8, 3, tableCenterZ + 4.5, true)
        };

        var assignments = definitions.ToDictionary(
            definition => definition.Key,
            _ => new List<PublicSceneTableDto>());
        var selectableZones = definitions.Where(definition => definition.AcceptsTables).ToArray();
        foreach (var table in tables)
        {
            var closest = selectableZones.MinBy(definition =>
                Math.Pow(table.PositionX - definition.X, 2)
                + Math.Pow(table.PositionZ - definition.Z, 2));
            if (closest is not null) assignments[closest.Key].Add(table);
        }

        return definitions.Select(definition => new PublicSceneHotspotDto
        {
            Key = definition.Key,
            Name = definition.Name,
            Description = definition.Description,
            PositionX = definition.X,
            PositionY = 0.15,
            PositionZ = definition.Z,
            CameraX = definition.CameraX,
            CameraY = definition.CameraY,
            CameraZ = definition.CameraZ,
            TableIds = assignments[definition.Key].Select(table => table.Id).ToArray(),
            AvailableTableCount = assignments[definition.Key].Count(table => table.Status == "Available")
        }).ToArray();
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

    private sealed record SceneHotspotDefinition(
        string Key,
        string Name,
        string Description,
        double X,
        double Z,
        double CameraX,
        double CameraY,
        double CameraZ,
        bool AcceptsTables);
}
