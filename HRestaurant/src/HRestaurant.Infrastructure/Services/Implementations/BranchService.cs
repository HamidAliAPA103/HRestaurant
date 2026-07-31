using AutoMapper;
using HRestaurant.Data;
using HRestaurant.DTOS.Branch;
using HRestaurant.DTOS.Responses;
using HRestaurant.Exceptions;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class BranchService : IBranchService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public BranchService(
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
        BranchCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await EnsureRestaurantExistsAndAccessibleAsync(
            dto.RestaurantId,
            cancellationToken);

        var name = NormalizeRequiredText(dto.Name, 100);
        var normalizedName = NormalizeName(name);

        await EnsureNameIsUniqueAsync(
            dto.RestaurantId,
            normalizedName,
            excludedBranchId: null,
            cancellationToken);

        var branch = _mapper.Map<Branch>(dto);
        branch.Name = name;
        branch.NormalizedName = normalizedName;
        branch.Address = NormalizeRequiredText(dto.Address, 250);
        branch.Phone = NormalizePhone(dto.Phone);
        branch.Email = NormalizeEmail(dto.Email);
        branch.TimeZoneId = dto.TimeZoneId.Trim();
        branch.Slug = await CreateUniqueSlugAsync(
            dto.RestaurantId,
            dto.Slug ?? name,
            excludedBranchId: null,
            cancellationToken);
        branch.CreatAt = UtcNow;
        branch.IsActive = true;

        if (branch.WorkingHours.Count == 0)
        {
            branch.WorkingHours = CreateDefaultWorkingHours();
        }
        else
        {
            foreach (var workingHour in branch.WorkingHours)
            {
                workingHour.CreatAt = UtcNow;
            }
        }

        await _dbContext.Branches.AddAsync(branch, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse.Created(
            branch.ID,
            "Branch created successfully.");
    }

    public Task<PagedResponse<BranchGetDTO>> GetAllAsync(
        BranchListRequest request,
        CancellationToken cancellationToken = default)
    {
        return GetListAsync(
            request,
            fixedRestaurantId: null,
            cancellationToken);
    }

    public async Task<PagedResponse<BranchGetDTO>> GetByRestaurantAsync(
        Guid restaurantId,
        BranchListRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureRestaurantExistsAndAccessibleAsync(
            restaurantId,
            cancellationToken);

        return await GetListAsync(
            request,
            restaurantId,
            cancellationToken);
    }

    public async Task<ApiResponse<BranchGetDTO>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var branch = await BranchQuery()
            .FirstOrDefaultAsync(
                entity =>
                    entity.ID == id
                    && !entity.IsDeleted
                    && !entity.Restaurant.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException("Branch", id);

        EnsureCanAccess(branch.RestaurantId);

        var dto = _mapper.Map<BranchGetDTO>(branch);
        SortWorkingHours(dto);
        await PopulateManagerDetailsAsync([dto], cancellationToken);

        return ApiResponse.Ok(
            dto,
            "Branch retrieved successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateAsync(
        Guid id,
        BranchUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var branch = await GetForMutationAsync(id, cancellationToken);
        var name = NormalizeRequiredText(dto.Name, 100);
        var normalizedName = NormalizeName(name);

        await EnsureNameIsUniqueAsync(
            branch.RestaurantId,
            normalizedName,
            branch.ID,
            cancellationToken);

        _mapper.Map(dto, branch);
        branch.Name = name;
        branch.NormalizedName = normalizedName;
        branch.Address = NormalizeRequiredText(dto.Address, 250);
        branch.Phone = NormalizePhone(dto.Phone);
        branch.Email = NormalizeEmail(dto.Email);
        branch.TimeZoneId = dto.TimeZoneId.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Slug))
        {
            branch.Slug = await CreateUniqueSlugAsync(
                branch.RestaurantId,
                dto.Slug,
                branch.ID,
                cancellationToken);
        }

        branch.UpdateAt = UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success("Branch updated successfully.");
    }

    public async Task<ApiResponse<object?>> SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var branch = await GetForMutationAsync(id, cancellationToken);

        branch.IsDeleted = true;
        branch.IsActive = false;
        branch.ManagerId = null;
        branch.DeletedAt = UtcNow;
        branch.UpdateAt = UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse.NoContent("Branch deleted successfully.");
    }

    public Task<ApiResponse<object?>> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return SetActiveStateAsync(id, isActive: true, cancellationToken);
    }

    public Task<ApiResponse<object?>> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return SetActiveStateAsync(id, isActive: false, cancellationToken);
    }

    public async Task<ApiResponse<object?>> AssignManagerAsync(
        Guid id,
        BranchManagerAssignmentDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var branch = await GetForMutationAsync(id, cancellationToken);
        var normalizedManagerRole = AppRoles.Manager.ToUpperInvariant();

        var isEligibleManager = await (
                from user in _dbContext.Users.AsNoTracking()
                join userRole in _dbContext.UserRoles.AsNoTracking()
                    on user.Id equals userRole.UserId
                join role in _dbContext.Roles.AsNoTracking()
                    on userRole.RoleId equals role.Id
                where user.Id == dto.ManagerId
                      && user.RestaurantId == branch.RestaurantId
                      && role.NormalizedName == normalizedManagerRole
                select user.Id)
            .AnyAsync(cancellationToken);

        if (!isEligibleManager)
        {
            throw new ConflictException(
                "The selected user must be a Manager belonging to the branch restaurant.");
        }

        branch.ManagerId = dto.ManagerId;
        branch.UpdateAt = UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(
            "Manager assigned to branch successfully.");
    }

    public async Task<ApiResponse<object?>> RemoveManagerAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var branch = await GetForMutationAsync(id, cancellationToken);

        if (branch.ManagerId.HasValue)
        {
            branch.ManagerId = null;
            branch.UpdateAt = UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse.Success(
            "Manager removed from branch successfully.");
    }

    public async Task<
        ApiResponse<IReadOnlyCollection<BranchWorkingHourDTO>>>
        GetWorkingHoursAsync(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        var branch = await BranchQuery()
            .FirstOrDefaultAsync(
                entity =>
                    entity.ID == id
                    && !entity.IsDeleted
                    && !entity.Restaurant.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException("Branch", id);

        EnsureCanAccess(branch.RestaurantId);

        var workingHours = branch.WorkingHours
            .OrderBy(entry => entry.DayOfWeek)
            .Select(entry => _mapper.Map<BranchWorkingHourDTO>(entry))
            .ToArray();

        return ApiResponse.Ok<IReadOnlyCollection<BranchWorkingHourDTO>>(
            workingHours,
            "Branch working hours retrieved successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateWorkingHoursAsync(
        Guid id,
        BranchWorkingHoursUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var branch = await GetForMutationAsync(
            id,
            cancellationToken,
            includeWorkingHours: true);

        _dbContext.BranchWorkingHours.RemoveRange(branch.WorkingHours);
        branch.WorkingHours.Clear();

        foreach (var workingHourDto in dto.WorkingHours)
        {
            var workingHour =
                _mapper.Map<BranchWorkingHour>(workingHourDto);
            workingHour.CreatAt = UtcNow;
            branch.WorkingHours.Add(workingHour);
        }

        branch.UpdateAt = UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(
            "Branch working hours updated successfully.");
    }

    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;

    private IQueryable<Branch> BranchQuery()
    {
        return _dbContext.Branches
            .AsNoTracking()
            .Include(branch => branch.Restaurant)
            .Include(branch => branch.WorkingHours)
            .AsSplitQuery();
    }

    private async Task<PagedResponse<BranchGetDTO>> GetListAsync(
        BranchListRequest request,
        Guid? fixedRestaurantId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.PageNumber < 1
            || request.PageSize is < 1 or > PaginationRequest.MaxPageSize)
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    [nameof(request.PageNumber)] =
                    [
                        "PageNumber must be at least 1."
                    ],
                    [nameof(request.PageSize)] =
                    [
                        $"PageSize must be between 1 and {PaginationRequest.MaxPageSize}."
                    ]
                });
        }

        var restaurantId = fixedRestaurantId ?? request.RestaurantId;

        if (!_currentUser.IsSuperAdmin)
        {
            if (restaurantId.HasValue
                && restaurantId.Value != _currentUser.RestaurantId)
            {
                throw new ForbiddenException(
                    "Branches from another restaurant cannot be accessed.");
            }

            restaurantId = _currentUser.RestaurantId;
        }
        else if (restaurantId.HasValue && !fixedRestaurantId.HasValue)
        {
            await EnsureRestaurantExistsAndAccessibleAsync(
                restaurantId.Value,
                cancellationToken);
        }

        var query = BranchQuery()
            .Where(branch =>
                !branch.IsDeleted
                && !branch.Restaurant.IsDeleted);

        if (restaurantId.HasValue)
        {
            query = query.Where(branch =>
                branch.RestaurantId == restaurantId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            var normalizedSearch = NormalizeName(search);
            query = query.Where(branch =>
                branch.NormalizedName.Contains(normalizedSearch)
                || branch.Address.Contains(search));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(branch =>
                branch.IsActive == request.IsActive.Value);
        }

        if (request.ManagerId.HasValue)
        {
            query = query.Where(branch =>
                branch.ManagerId == request.ManagerId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var skip = (long)(request.PageNumber - 1) * request.PageSize;
        List<Branch> branches;

        if (skip >= totalCount)
        {
            branches = [];
        }
        else
        {
            branches = await query
                .OrderBy(branch => branch.Name)
                .ThenBy(branch => branch.ID)
                .Skip((int)skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
        }

        var data = _mapper.Map<List<BranchGetDTO>>(branches);

        foreach (var dto in data)
        {
            SortWorkingHours(dto);
        }

        await PopulateManagerDetailsAsync(data, cancellationToken);

        return PagedResponse<BranchGetDTO>.Create(
            data,
            request.PageNumber,
            request.PageSize,
            totalCount,
            "Branches retrieved successfully.");
    }

    private async Task<Branch> GetForMutationAsync(
        Guid id,
        CancellationToken cancellationToken,
        bool includeWorkingHours = false)
    {
        IQueryable<Branch> query = _dbContext.Branches
            .Include(branch => branch.Restaurant);

        if (includeWorkingHours)
        {
            query = query.Include(branch => branch.WorkingHours);
        }

        var branch = await query.FirstOrDefaultAsync(
                entity =>
                    entity.ID == id
                    && !entity.IsDeleted
                    && !entity.Restaurant.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException("Branch", id);

        EnsureCanAccess(branch.RestaurantId);
        return branch;
    }

    private async Task<ApiResponse<object?>> SetActiveStateAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var branch = await GetForMutationAsync(id, cancellationToken);

        if (isActive)
        {
            var restaurantIsActive = await _dbContext.Restaurants
                .AsNoTracking()
                .AnyAsync(
                    restaurant =>
                        restaurant.ID == branch.RestaurantId
                        && !restaurant.IsDeleted
                        && restaurant.IsActive,
                    cancellationToken);

            if (!restaurantIsActive)
            {
                throw new ConflictException(
                    "A branch cannot be activated while its restaurant is inactive.");
            }
        }

        if (branch.IsActive != isActive)
        {
            branch.IsActive = isActive;
            branch.UpdateAt = UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse.Success(
            isActive
                ? "Branch activated successfully."
                : "Branch deactivated successfully.");
    }

    private async Task EnsureRestaurantExistsAndAccessibleAsync(
        Guid restaurantId,
        CancellationToken cancellationToken)
    {
        EnsureCanAccess(restaurantId);

        var exists = await _dbContext.Restaurants
            .AsNoTracking()
            .AnyAsync(
                restaurant =>
                    restaurant.ID == restaurantId
                    && !restaurant.IsDeleted,
                cancellationToken);

        if (!exists)
        {
            throw new NotFoundException("Restaurant", restaurantId);
        }
    }

    private void EnsureCanAccess(Guid restaurantId)
    {
        if (!_currentUser.IsSuperAdmin
            && _currentUser.RestaurantId != restaurantId)
        {
            throw new ForbiddenException(
                "Branches from another restaurant cannot be accessed or modified.");
        }
    }

    private Task EnsureNameIsUniqueAsync(
        Guid restaurantId,
        string normalizedName,
        Guid? excludedBranchId,
        CancellationToken cancellationToken)
    {
        return EnsureDoesNotExistAsync(
            _dbContext.Branches.AsNoTracking().AnyAsync(
                branch =>
                    branch.RestaurantId == restaurantId
                    && branch.NormalizedName == normalizedName
                    && !branch.IsDeleted
                    && (!excludedBranchId.HasValue
                        || branch.ID != excludedBranchId.Value),
                cancellationToken),
            "A branch with the same name already exists in this restaurant.");
    }

    private async Task<string> CreateUniqueSlugAsync(
        Guid restaurantId,
        string value,
        Guid? excludedBranchId,
        CancellationToken cancellationToken)
    {
        var baseSlug = SlugUtility.Create(value, "branch", 100);
        var slug = baseSlug;

        for (var attempt = 0; attempt < 20; attempt++)
        {
            var exists = await _dbContext.Branches
                .AsNoTracking()
                .AnyAsync(
                    branch =>
                        branch.RestaurantId == restaurantId
                        && branch.Slug == slug
                        && !branch.IsDeleted
                        && (!excludedBranchId.HasValue
                            || branch.ID != excludedBranchId.Value),
                    cancellationToken);

            if (!exists)
            {
                return slug;
            }

            slug = $"{baseSlug}-{Guid.NewGuid():N}"[..Math.Min(
                baseSlug.Length + 7,
                120)];
        }

        throw new ConflictException(
            "A unique branch slug could not be generated.");
    }

    private async Task PopulateManagerDetailsAsync(
        IReadOnlyCollection<BranchGetDTO> branches,
        CancellationToken cancellationToken)
    {
        var managerIds = branches
            .Where(branch => branch.ManagerId.HasValue)
            .Select(branch => branch.ManagerId!.Value)
            .Distinct()
            .ToArray();

        if (managerIds.Length == 0)
        {
            return;
        }

        var managers = await _dbContext.Users
            .AsNoTracking()
            .Where(user => managerIds.Contains(user.Id))
            .Select(user => new ManagerSummary(
                user.Id,
                user.FullName,
                user.Email))
            .ToDictionaryAsync(
                manager => manager.Id,
                cancellationToken);

        foreach (var branch in branches)
        {
            if (branch.ManagerId.HasValue
                && managers.TryGetValue(
                    branch.ManagerId.Value,
                    out var manager))
            {
                branch.ManagerName = manager.FullName;
                branch.ManagerEmail = manager.Email;
            }
        }
    }

    private List<BranchWorkingHour> CreateDefaultWorkingHours()
    {
        return System.Enum.GetValues<DayOfWeek>()
            .Select(day => new BranchWorkingHour
            {
                DayOfWeek = day,
                IsClosed = true,
                CreatAt = UtcNow
            })
            .ToList();
    }

    private static void SortWorkingHours(BranchGetDTO dto)
    {
        dto.WorkingHours = dto.WorkingHours
            .OrderBy(entry => entry.DayOfWeek)
            .ToList();
    }

    private static async Task EnsureDoesNotExistAsync(
        Task<bool> existsTask,
        string message)
    {
        if (await existsTask)
        {
            throw new ConflictException(message);
        }
    }

    private static string NormalizeRequiredText(
        string value,
        int maximumLength)
    {
        return PublicInputSanitizer.SanitizeRequired(value, maximumLength);
    }

    private static string NormalizeName(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string? NormalizePhone(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : PublicInputSanitizer.NormalizePhone(value);
    }

    private static string? NormalizeEmail(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToLowerInvariant();
    }

    private sealed record ManagerSummary(
        Guid Id,
        string FullName,
        string? Email);
}
