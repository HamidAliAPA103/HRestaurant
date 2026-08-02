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
                        IsPopular = item.IsPopular
                    })
                    .ToArray()
            })
            .ToArrayAsync(cancellationToken);

        return ApiResponse.Ok<IReadOnlyCollection<PublicMenuCategoryDto>>(
            categories,
            "Public menu retrieved successfully.");
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
}
