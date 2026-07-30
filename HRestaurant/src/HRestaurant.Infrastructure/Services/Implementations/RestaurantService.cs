using AutoMapper;
using System.Globalization;
using System.Text;
using HRestaurant.Data;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Enum;
using HRestaurant.Exceptions;
using HRestaurant.Extensions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class RestaurantService : IRestaurantService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public RestaurantService(
        AppDbContext dbContext,
        IMapper mapper,
        ICurrentUserContext currentUser,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dbContext = dbContext;
        _mapper = mapper;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<Guid>> CreateAsync(
        RestaurantCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (!_currentUser.IsSuperAdmin)
        {
            throw new ForbiddenException(
                "Only a SuperAdmin can create a restaurant.");
        }

        var restaurant = _mapper.Map<Restaurant>(dto);
        restaurant.Name = dto.Name.Trim();
        restaurant.Adres = dto.Adres.Trim();
        restaurant.Number = dto.Number.Trim();
        restaurant.Email = NormalizeEmail(dto.Email);
        restaurant.Description = PublicInputSanitizer.Sanitize(
            dto.Description,
            2000);
        restaurant.LogoUrl = NormalizeUrl(dto.LogoUrl);
        restaurant.CoverImageUrl = NormalizeUrl(
            dto.CoverImageUrl);
        restaurant.Currency = NormalizeCurrency(dto.Currency);
        restaurant.Slug = await CreateUniqueSlugAsync(
            dto.Slug ?? dto.Name,
            cancellationToken);
        restaurant.CreatAt = UtcNow;
        restaurant.IsActive = true;

        if (restaurant.WorkingHours.Count == 0)
        {
            restaurant.WorkingHours = CreateDefaultWorkingHours();
        }

        restaurant.Branches.Add(CreateDefaultBranch(
            restaurant,
            dto.WorkingHours));

        await _dbContext.Restaurants.AddAsync(
            restaurant,
            cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse.Created(
            restaurant.ID,
            "Restaurant created successfully.");
    }

    public Task<PagedResponse<RestaurantGetDTO>> GetAllAsync(
        ViewType type,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pagination);

        var query = RestaurantQuery();

        if (!_currentUser.IsSuperAdmin)
        {
            var restaurantId = _currentUser.RestaurantId;
            query = query.Where(restaurant =>
                restaurant.ID == restaurantId);
        }

        query = type switch
        {
            ViewType.deleted =>
                query.Where(restaurant => restaurant.IsDeleted),
            ViewType.notdeleted =>
                query.Where(restaurant => !restaurant.IsDeleted),
            _ => query
        };

        return query.ToPagedResponseAsync<Restaurant, RestaurantGetDTO>(
            _mapper,
            pagination,
            cancellationToken);
    }

    public async Task<ApiResponse<RestaurantGetDTO>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await RestaurantQuery()
            .FirstOrDefaultAsync(
                entity => entity.ID == id,
                cancellationToken)
            ?? throw new NotFoundException("Restaurant", id);

        EnsureCanAccess(restaurant.ID);

        if (restaurant.IsDeleted && !_currentUser.IsSuperAdmin)
        {
            throw new NotFoundException("Restaurant", id);
        }

        return ApiResponse.Ok(
            _mapper.Map<RestaurantGetDTO>(restaurant),
            "Restaurant retrieved successfully.");
    }

    public async Task<ApiResponse<RestaurantGetDTO>> GetCurrentAsync(
        CancellationToken cancellationToken = default)
    {
        var restaurantId = _currentUser.RestaurantId;
        var restaurant = await RestaurantQuery()
            .FirstOrDefaultAsync(
                entity =>
                    entity.ID == restaurantId
                    && !entity.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException("Restaurant", restaurantId);

        return ApiResponse.Ok(
            _mapper.Map<RestaurantGetDTO>(restaurant),
            "Current restaurant retrieved successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateAsync(
        Guid id,
        RestaurantUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var restaurant = await GetForMutationAsync(
            id,
            cancellationToken: cancellationToken);

        _mapper.Map(dto, restaurant);

        if (dto.Name is not null)
        {
            restaurant.Name = dto.Name.Trim();
        }

        if (dto.Adres is not null)
        {
            restaurant.Adres = dto.Adres.Trim();
        }

        if (dto.Number is not null)
        {
            restaurant.Number = dto.Number.Trim();
        }

        if (dto.Email is not null)
        {
            restaurant.Email = NormalizeEmail(dto.Email);
        }

        if (dto.Description is not null)
        {
            restaurant.Description = PublicInputSanitizer.Sanitize(
                dto.Description,
                2000);
        }

        if (dto.LogoUrl is not null)
        {
            restaurant.LogoUrl = NormalizeUrl(dto.LogoUrl);
        }

        if (dto.CoverImageUrl is not null)
        {
            restaurant.CoverImageUrl = NormalizeUrl(
                dto.CoverImageUrl);
        }

        restaurant.UpdateAt = UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success("Restaurant updated successfully.");
    }

    public async Task<ApiResponse<object?>> SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await GetForMutationAsync(
            id,
            allowDeleted: true,
            cancellationToken: cancellationToken);

        if (!restaurant.IsDeleted)
        {
            restaurant.IsDeleted = true;
            restaurant.IsActive = false;
            restaurant.DeletedAt = UtcNow;
            restaurant.UpdateAt = UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse.NoContent(
            "Restaurant deleted successfully.");
    }

    public Task<ApiResponse<object?>> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return SetActiveStateAsync(
            id,
            isActive: true,
            cancellationToken);
    }

    public Task<ApiResponse<object?>> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return SetActiveStateAsync(
            id,
            isActive: false,
            cancellationToken);
    }

    public async Task<
        ApiResponse<IReadOnlyCollection<RestaurantWorkingHourDTO>>>
        GetWorkingHoursAsync(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        var restaurant = await RestaurantQuery()
            .FirstOrDefaultAsync(
                entity => entity.ID == id && !entity.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException("Restaurant", id);

        EnsureCanAccess(restaurant.ID);

        var workingHours = restaurant.WorkingHours
            .OrderBy(entry => entry.DayOfWeek)
            .Select(entry =>
                _mapper.Map<RestaurantWorkingHourDTO>(entry))
            .ToArray();

        return ApiResponse.Ok<IReadOnlyCollection<
            RestaurantWorkingHourDTO>>(
            workingHours,
            "Restaurant working hours retrieved successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateWorkingHoursAsync(
        Guid id,
        RestaurantWorkingHoursUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var restaurant = await GetForMutationAsync(
            id,
            includeWorkingHours: true,
            cancellationToken: cancellationToken);

        _dbContext.RestaurantWorkingHours.RemoveRange(
            restaurant.WorkingHours);
        restaurant.WorkingHours.Clear();

        foreach (var workingHourDto in dto.WorkingHours)
        {
            var workingHour =
                _mapper.Map<RestaurantWorkingHour>(workingHourDto);
            workingHour.CreatAt = UtcNow;
            restaurant.WorkingHours.Add(workingHour);
        }

        restaurant.UpdateAt = UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(
            "Restaurant working hours updated successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateSettingsAsync(
        Guid id,
        RestaurantSettingsUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var restaurant = await GetForMutationAsync(
            id,
            cancellationToken: cancellationToken);

        restaurant.Currency = NormalizeCurrency(dto.Currency);
        restaurant.TaxRate = dto.TaxRate;
        restaurant.UpdateAt = UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(
            "Restaurant settings updated successfully.");
    }

    private DateTime UtcNow =>
        _timeProvider.GetUtcNow().UtcDateTime;

    private IQueryable<Restaurant> RestaurantQuery()
    {
        return _dbContext.Restaurants
            .AsNoTracking()
            .Include(restaurant => restaurant.WorkingHours);
    }

    private async Task<Restaurant> GetForMutationAsync(
        Guid id,
        bool includeWorkingHours = false,
        bool allowDeleted = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Restaurant> query = _dbContext.Restaurants;

        if (includeWorkingHours)
        {
            query = query.Include(restaurant =>
                restaurant.WorkingHours);
        }

        var restaurant = await query.FirstOrDefaultAsync(
                entity => entity.ID == id,
                cancellationToken)
            ?? throw new NotFoundException("Restaurant", id);

        EnsureCanAccess(restaurant.ID);

        if (restaurant.IsDeleted && !allowDeleted)
        {
            throw new ConflictException(
                "A deleted restaurant cannot be modified.");
        }

        return restaurant;
    }

    private async Task<ApiResponse<object?>> SetActiveStateAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var restaurant = await GetForMutationAsync(
            id,
            allowDeleted: true,
            cancellationToken: cancellationToken);

        if (restaurant.IsDeleted)
        {
            throw new ConflictException(
                "A deleted restaurant cannot be activated or deactivated.");
        }

        if (restaurant.IsActive != isActive)
        {
            restaurant.IsActive = isActive;
            restaurant.UpdateAt = UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse.Success(
            isActive
                ? "Restaurant activated successfully."
                : "Restaurant deactivated successfully.");
    }

    private void EnsureCanAccess(Guid restaurantId)
    {
        if (_currentUser.IsSuperAdmin)
        {
            return;
        }

        if (_currentUser.RestaurantId != restaurantId)
        {
            throw new ForbiddenException(
                "Restaurant owners can access only their own restaurant.");
        }
    }

    private List<RestaurantWorkingHour> CreateDefaultWorkingHours()
    {
        return System.Enum.GetValues<DayOfWeek>()
            .Select(day => new RestaurantWorkingHour
            {
                DayOfWeek = day,
                IsClosed = true,
                CreatAt = UtcNow
            })
            .ToList();
    }

    private Branch CreateDefaultBranch(
        Restaurant restaurant,
        IReadOnlyCollection<RestaurantWorkingHourDTO> workingHours)
    {
        var branchHours = workingHours.Count == 0
            ? System.Enum.GetValues<DayOfWeek>()
                .Select(day => new BranchWorkingHour
                {
                    DayOfWeek = day,
                    IsClosed = true,
                    CreatAt = UtcNow
                })
                .ToList()
            : workingHours
                .Select(entry => new BranchWorkingHour
                {
                    DayOfWeek = entry.DayOfWeek,
                    OpensAt = entry.OpensAt,
                    ClosesAt = entry.ClosesAt,
                    IsClosed = entry.IsClosed,
                    CreatAt = UtcNow
                })
                .ToList();

        return new Branch
        {
            Name = restaurant.Name,
            Slug = "main",
            Address = restaurant.Adres,
            Phone = restaurant.Number,
            Email = restaurant.Email,
            TimeZoneId = "Asia/Baku",
            IsActive = true,
            CreatAt = UtcNow,
            WorkingHours = branchHours
        };
    }

    private async Task<string> CreateUniqueSlugAsync(
        string value,
        CancellationToken cancellationToken)
    {
        var baseSlug = Slugify(value);
        var slug = baseSlug;

        for (var attempt = 0; attempt < 20; attempt++)
        {
            var exists = await _dbContext.Restaurants
                .AsNoTracking()
                .AnyAsync(
                    restaurant =>
                        restaurant.Slug == slug
                        && !restaurant.IsDeleted,
                    cancellationToken);

            if (!exists)
            {
                return slug;
            }

            slug =
                $"{baseSlug}-{Guid.NewGuid():N}"[..Math.Min(
                    baseSlug.Length + 7,
                    120)];
        }

        throw new ConflictException(
            "A unique restaurant slug could not be generated.");
    }

    private static string Slugify(string value)
    {
        var transliterated = value
            .Trim()
            .ToLowerInvariant()
            .Replace('ə', 'e')
            .Replace('ı', 'i')
            .Replace('ö', 'o')
            .Replace('ü', 'u')
            .Replace('ş', 's')
            .Replace('ç', 'c')
            .Replace('ğ', 'g');
        var normalized = transliterated.Normalize(
            NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var previousWasHyphen = false;

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character)
                == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasHyphen = false;
            }
            else if (!previousWasHyphen && builder.Length > 0)
            {
                builder.Append('-');
                previousWasHyphen = true;
            }
        }

        var slug = builder.ToString().Trim('-');

        if (slug.Length == 0)
        {
            slug = "restaurant";
        }

        return slug[..Math.Min(slug.Length, 100)];
    }

    private static string NormalizeCurrency(string currency)
    {
        return currency.Trim().ToUpperInvariant();
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim().ToLowerInvariant();
    }

    private static string? NormalizeUrl(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
