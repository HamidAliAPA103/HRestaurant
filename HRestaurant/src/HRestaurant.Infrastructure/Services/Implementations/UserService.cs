using AutoMapper;
using HRestaurant.Data;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.User;
using HRestaurant.Exceptions;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Identity;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class UserService : IUserService
{
    private static readonly string[] EmployeeRoles =
        [AppRoles.Manager, AppRoles.Cashier, AppRoles.Waiter, AppRoles.Chef];

    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;
    private readonly UserManager<AppUser> _userManager;
    private readonly TimeProvider _timeProvider;

    public UserService(
        AppDbContext db,
        IMapper mapper,
        ICurrentUserContext currentUser,
        UserManager<AppUser> userManager,
        TimeProvider timeProvider)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
        _userManager = userManager;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<Guid>> CreateAsync(
        UserCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var restaurantId = ResolveRestaurantId(dto.RestaurantId);
        var role = CanonicalRole(dto.Role);
        await EnsureBranchAccessAsync(dto.BranchId, restaurantId, cancellationToken);

        if (_currentUser.IsManager && role == AppRoles.Manager)
        {
            throw new ForbiddenException("A manager cannot create another manager.");
        }

        var email = dto.Email.Trim().ToLowerInvariant();
        var phone = NormalizePhone(dto.Phone);
        await EnsureUniqueAsync(email, phone, null, cancellationToken);

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        await using var transaction = await _db.Database
            .BeginTransactionAsync(cancellationToken);

        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            PhoneNumber = phone,
            FullName = dto.Name.Trim(),
            RestaurantId = restaurantId,
            LockoutEnabled = true,
            CreatedAtUtc = UtcNow
        };

        EnsureIdentitySucceeded(
            await _userManager.CreateAsync(appUser, dto.Password),
            "Employee account could not be created.");
        EnsureIdentitySucceeded(
            await _userManager.AddToRoleAsync(appUser, role),
            "Employee role could not be assigned.");

        var employee = _mapper.Map<User>(dto);
        employee.RestaurantId = restaurantId;
        employee.BranchId = dto.BranchId;
        employee.AppUserId = appUser.Id;
        employee.Name = dto.Name.Trim();
        employee.Email = email;
        employee.NormalizedEmail = email.ToUpperInvariant();
        employee.Phone = phone;
        employee.NormalizedPhone = phone;
        employee.Role = role;
        employee.AvatarUrl = NormalizeOptional(dto.AvatarUrl);
        employee.EmergencyContact = dto.EmergencyContact.Trim();
        employee.IsActive = true;
        employee.CreatAt = UtcNow;

        _db.BusinessUsers.Add(employee);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ApiResponse.Created(employee.ID, "Employee created successfully.");
    }

    public Task<PagedResponse<UserGetDTO>> GetAllAsync(
        EmployeeListRequest request,
        CancellationToken cancellationToken = default) =>
        GetListAsync(request, null, null, cancellationToken);

    public Task<PagedResponse<UserGetDTO>> GetByRestaurantAsync(
        Guid restaurantId,
        EmployeeListRequest request,
        CancellationToken cancellationToken = default) =>
        GetListAsync(request, restaurantId, null, cancellationToken);

    public Task<PagedResponse<UserGetDTO>> GetByBranchAsync(
        Guid branchId,
        EmployeeListRequest request,
        CancellationToken cancellationToken = default) =>
        GetListAsync(request, null, branchId, cancellationToken);

    public async Task<ApiResponse<UserGetDTO>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var employee = await EmployeeQuery().FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted,
            cancellationToken) ?? throw new NotFoundException("Employee", id);
        await EnsureEmployeeAccessAsync(employee, cancellationToken);
        return ApiResponse.Ok(_mapper.Map<UserGetDTO>(employee));
    }

    public async Task<ApiResponse<object?>> UpdateAsync(
        Guid id,
        UserUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var employee = await GetForMutationAsync(id, cancellationToken);
        EnsureManagerCanManageRole(employee.Role);

        var email = dto.Email is null
            ? employee.Email
            : dto.Email.Trim().ToLowerInvariant();
        var phone = dto.Phone is null
            ? employee.NormalizedPhone
            : NormalizePhone(dto.Phone);
        await EnsureUniqueAsync(email, phone, employee.ID, cancellationToken);

        await using var transaction = await _db.Database
            .BeginTransactionAsync(cancellationToken);
        var appUser = await GetAppUserAsync(employee, cancellationToken);

        if (dto.Email is not null && appUser is not null)
        {
            appUser.Email = email;
            appUser.UserName = email;
            appUser.NormalizedEmail = _userManager.NormalizeEmail(email);
            appUser.NormalizedUserName = _userManager.NormalizeName(email);
        }

        if (dto.Name is not null && appUser is not null)
            appUser.FullName = dto.Name.Trim();
        if (dto.Phone is not null && appUser is not null)
            appUser.PhoneNumber = phone;

        _mapper.Map(dto, employee);
        employee.Email = email;
        employee.NormalizedEmail = email.ToUpperInvariant();
        employee.Phone = phone;
        employee.NormalizedPhone = phone;
        if (dto.Name is not null) employee.Name = dto.Name.Trim();
        if (dto.AvatarUrl is not null) employee.AvatarUrl = NormalizeOptional(dto.AvatarUrl);
        if (dto.EmergencyContact is not null)
            employee.EmergencyContact = dto.EmergencyContact.Trim();

        if (dto.Role is not null)
            await ChangeRoleInternalAsync(employee, appUser, dto.Role);

        if (appUser is not null)
            EnsureIdentitySucceeded(await _userManager.UpdateAsync(appUser), "Employee account could not be updated.");

        employee.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Success("Employee updated successfully.");
    }

    public async Task<ApiResponse<object?>> SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var employee = await GetForMutationAsync(id, cancellationToken);
        EnsureManagerCanManageRole(employee.Role);
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        employee.IsDeleted = true;
        employee.IsActive = false;
        employee.DeletedAt = UtcNow;
        employee.UpdateAt = UtcNow;
        await SetAccountEnabledAsync(employee, false, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.NoContent("Employee deleted successfully.");
    }

    public Task<ApiResponse<object?>> ActivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        SetActiveAsync(id, true, cancellationToken);

    public Task<ApiResponse<object?>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        SetActiveAsync(id, false, cancellationToken);

    public async Task<ApiResponse<object?>> AssignBranchAsync(
        Guid id,
        EmployeeBranchAssignmentDTO dto,
        CancellationToken cancellationToken = default)
    {
        var employee = await GetForMutationAsync(id, cancellationToken);
        EnsureManagerCanManageRole(employee.Role);
        await EnsureBranchAccessAsync(dto.BranchId, employee.RestaurantId!.Value, cancellationToken);
        employee.BranchId = dto.BranchId;
        employee.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Employee branch changed successfully.");
    }

    public async Task<ApiResponse<object?>> AssignRoleAsync(
        Guid id,
        EmployeeRoleAssignmentDTO dto,
        CancellationToken cancellationToken = default)
    {
        var employee = await GetForMutationAsync(id, cancellationToken);
        EnsureManagerCanManageRole(employee.Role);
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var appUser = await GetAppUserAsync(employee, cancellationToken);
        await ChangeRoleInternalAsync(employee, appUser, dto.Role);
        employee.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Success("Employee role changed successfully.");
    }

    private async Task<PagedResponse<UserGetDTO>> GetListAsync(
        EmployeeListRequest request,
        Guid? fixedRestaurantId,
        Guid? fixedBranchId,
        CancellationToken cancellationToken)
    {
        var restaurantId = fixedRestaurantId ?? request.RestaurantId;
        if (!_currentUser.IsSuperAdmin)
        {
            if (restaurantId.HasValue && restaurantId != _currentUser.RestaurantId)
                throw new ForbiddenException("Another restaurant's employees cannot be accessed.");
            restaurantId = _currentUser.RestaurantId;
        }

        var branchId = fixedBranchId ?? request.BranchId;
        var query = EmployeeQuery().Where(x => !x.IsDeleted && x.RestaurantId != null);
        if (restaurantId.HasValue) query = query.Where(x => x.RestaurantId == restaurantId);
        if (branchId.HasValue) query = query.Where(x => x.BranchId == branchId);

        if (_currentUser.IsManager)
        {
            var managedBranchIds = await GetManagedBranchIdsAsync(cancellationToken);
            if (branchId.HasValue && !managedBranchIds.Contains(branchId.Value))
                throw new ForbiddenException("Managers can access only employees in their own branch.");
            query = query.Where(x => x.BranchId.HasValue && managedBranchIds.Contains(x.BranchId.Value));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            var normalized = search.ToUpperInvariant();
            query = query.Where(x => x.Name.Contains(search)
                || x.NormalizedEmail.Contains(normalized)
                || (x.NormalizedPhone != null && x.NormalizedPhone.Contains(search)));
        }
        if (!string.IsNullOrWhiteSpace(request.Role))
            query = query.Where(x => x.Role == CanonicalRole(request.Role));
        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive);

        var total = await query.CountAsync(cancellationToken);
        var asc = request.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase);
        var byHire = request.SortBy.Equals("hireDate", StringComparison.OrdinalIgnoreCase);
        IOrderedQueryable<User> ordered = (byHire, asc) switch
        {
            (true, true) => query.OrderBy(x => x.HireDate),
            (true, false) => query.OrderByDescending(x => x.HireDate),
            (false, false) => query.OrderByDescending(x => x.Name),
            _ => query.OrderBy(x => x.Name)
        };
        var data = total == 0 ? [] : await ordered.ThenBy(x => x.ID)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize).ToListAsync(cancellationToken);
        return PagedResponse<UserGetDTO>.Create(
            _mapper.Map<List<UserGetDTO>>(data), request.PageNumber, request.PageSize, total,
            "Employees retrieved successfully.");
    }

    private IQueryable<User> EmployeeQuery() => _db.BusinessUsers.AsNoTracking()
        .Include(x => x.Branch);

    private async Task<User> GetForMutationAsync(Guid id, CancellationToken cancellationToken)
    {
        var employee = await _db.BusinessUsers.FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted && x.RestaurantId != null,
            cancellationToken) ?? throw new NotFoundException("Employee", id);
        await EnsureEmployeeAccessAsync(employee, cancellationToken);
        return employee;
    }

    private async Task EnsureEmployeeAccessAsync(User employee, CancellationToken cancellationToken)
    {
        if (_currentUser.IsSuperAdmin) return;
        if (employee.RestaurantId != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant's employee cannot be accessed.");
        if (_currentUser.IsManager)
        {
            var ids = await GetManagedBranchIdsAsync(cancellationToken);
            if (!employee.BranchId.HasValue || !ids.Contains(employee.BranchId.Value))
                throw new ForbiddenException("Managers can manage only employees in their own branch.");
        }
    }

    private async Task EnsureBranchAccessAsync(Guid branchId, Guid restaurantId, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsSuperAdmin && restaurantId != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant cannot be managed.");
        var exists = await _db.Branches.AsNoTracking().AnyAsync(x =>
            x.ID == branchId && x.RestaurantId == restaurantId && !x.IsDeleted,
            cancellationToken);
        if (!exists) throw new NotFoundException("Branch", branchId);
        if (_currentUser.IsManager)
        {
            var ids = await GetManagedBranchIdsAsync(cancellationToken);
            if (!ids.Contains(branchId))
                throw new ForbiddenException("Managers can manage only their own branch.");
        }
    }

    private Task<List<Guid>> GetManagedBranchIdsAsync(CancellationToken cancellationToken) =>
        _db.Branches.AsNoTracking().Where(x => x.ManagerId == _currentUser.UserId && !x.IsDeleted)
            .Select(x => x.ID).ToListAsync(cancellationToken);

    private Guid ResolveRestaurantId(Guid requested)
    {
        if (_currentUser.IsSuperAdmin) return requested;
        if (requested != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant cannot be managed.");
        return _currentUser.RestaurantId;
    }

    private void EnsureManagerCanManageRole(string role)
    {
        if (_currentUser.IsManager && (role.Equals(AppRoles.Manager, StringComparison.OrdinalIgnoreCase)
            || role.Equals(AppRoles.RestaurantOwner, StringComparison.OrdinalIgnoreCase)))
            throw new ForbiddenException("A manager cannot manage Manager or RestaurantOwner accounts.");
    }

    private async Task ChangeRoleInternalAsync(User employee, AppUser? appUser, string role)
    {
        var canonical = CanonicalRole(role);
        if (_currentUser.IsManager && canonical == AppRoles.Manager)
            throw new ForbiddenException("A manager cannot assign the Manager role.");
        employee.Role = canonical;
        if (appUser is null) return;
        var currentRoles = await _userManager.GetRolesAsync(appUser);
        if (currentRoles.Count > 0)
            EnsureIdentitySucceeded(await _userManager.RemoveFromRolesAsync(appUser, currentRoles), "Existing role could not be removed.");
        EnsureIdentitySucceeded(await _userManager.AddToRoleAsync(appUser, canonical), "Employee role could not be assigned.");
    }

    private async Task SetAccountEnabledAsync(User employee, bool enabled, CancellationToken cancellationToken)
    {
        var appUser = await GetAppUserAsync(employee, cancellationToken);
        if (appUser is null) return;
        appUser.LockoutEnabled = true;
        appUser.LockoutEnd = enabled ? null : DateTimeOffset.MaxValue;
        if (enabled) appUser.AccessFailedCount = 0;
        EnsureIdentitySucceeded(await _userManager.UpdateAsync(appUser), "Employee login status could not be updated.");
        if (!enabled)
        {
            var tokens = await _db.RefreshTokens.Where(x => x.UserId == appUser.Id
                && x.RevokedAtUtc == null).ToListAsync(cancellationToken);
            foreach (var token in tokens)
            {
                token.RevokedAtUtc = UtcNow;
                token.RevocationReason = "Employee disabled.";
            }
        }
    }

    private async Task<ApiResponse<object?>> SetActiveAsync(Guid id, bool active, CancellationToken cancellationToken)
    {
        var employee = await GetForMutationAsync(id, cancellationToken);
        EnsureManagerCanManageRole(employee.Role);
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        employee.IsActive = active;
        employee.UpdateAt = UtcNow;
        await SetAccountEnabledAsync(employee, active, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Success(active ? "Employee activated successfully." : "Employee deactivated successfully.");
    }

    private Task<AppUser?> GetAppUserAsync(User employee, CancellationToken cancellationToken) =>
        employee.AppUserId.HasValue
            ? _db.Users.FirstOrDefaultAsync(x => x.Id == employee.AppUserId.Value, cancellationToken)
            : Task.FromResult<AppUser?>(null);

    private async Task EnsureUniqueAsync(string email, string? phone, Guid? excludedId, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var duplicateEmail = await _db.BusinessUsers.AsNoTracking().AnyAsync(x =>
            x.RestaurantId != null && x.NormalizedEmail == normalizedEmail
            && (!excludedId.HasValue || x.ID != excludedId), cancellationToken);
        if (duplicateEmail) throw new ConflictException("An employee with this email already exists.");
        if (phone is null) return;
        var duplicatePhone = await _db.BusinessUsers.AsNoTracking().AnyAsync(x =>
            x.RestaurantId != null && x.NormalizedPhone == phone
            && (!excludedId.HasValue || x.ID != excludedId), cancellationToken);
        if (duplicatePhone) throw new ConflictException("An employee with this phone already exists.");
    }

    private static string CanonicalRole(string role) => EmployeeRoles.FirstOrDefault(x =>
        x.Equals(role.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? throw new ConflictException("Employee role is invalid.");

    private static string NormalizePhone(string value) => PublicInputSanitizer.NormalizePhone(value);
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;

    private static void EnsureIdentitySucceeded(IdentityResult result, string message)
    {
        if (result.Succeeded) return;
        throw new ConflictException($"{message} {string.Join("; ", result.Errors.Select(x => x.Description))}");
    }
}
