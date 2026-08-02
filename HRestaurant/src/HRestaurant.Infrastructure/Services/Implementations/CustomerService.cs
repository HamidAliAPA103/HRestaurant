using AutoMapper;
using HRestaurant.Data;
using HRestaurant.DTOS.Customer;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class CustomerService : ICustomerService
{
    private const string CustomerRole = "Customer";
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public CustomerService(AppDbContext db, IMapper mapper,
        ICurrentUserContext currentUser, TimeProvider timeProvider)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<Guid>> CreateAsync(
        CustomerCreateDTO dto, CancellationToken cancellationToken = default)
    {
        var restaurantId = ResolveRestaurant(dto.RestaurantId);
        var branchId = await ResolveBranchAsync(
            dto.BranchId, restaurantId, cancellationToken);
        var phone = PublicInputSanitizer.NormalizePhone(dto.Phone);
        var email = NormalizeEmail(dto.Email);
        await EnsureUniqueAsync(restaurantId, phone, email, null, cancellationToken);

        var customer = new User
        {
            RestaurantId = restaurantId,
            BranchId = branchId,
            Name = dto.FullName.Trim(),
            Phone = phone,
            NormalizedPhone = phone,
            Email = email ?? string.Empty,
            NormalizedEmail = email?.ToUpperInvariant() ?? string.Empty,
            Role = CustomerRole,
            Birthday = dto.Birthday,
            Notes = NormalizeOptional(dto.Notes),
            IsActive = true,
            CreatAt = UtcNow,
            LoyaltyAccount = new LoyaltyAccount
            {
                CreatAt = UtcNow
            }
        };

        _db.BusinessUsers.Add(customer);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Created(customer.ID, "Customer created successfully.");
    }

    public async Task<ApiResponse<CustomerDetailDTO>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await CustomerQuery().FirstOrDefaultAsync(
            x => x.ID == id, cancellationToken)
            ?? throw new NotFoundException("Customer", id);
        await EnsureAccessAsync(customer, cancellationToken);
        var dto = _mapper.Map<CustomerDetailDTO>(customer);
        dto.FavoriteMenuItems = (await FavoriteQuery(id)
                .OrderByDescending(x => x.TotalSpent)
                .ThenByDescending(x => x.Quantity)
                .Take(10)
                .ToListAsync(cancellationToken))
            .Select(x => new FavoriteMenuItemDTO
            {
                MenuItemId = x.MenuItemId,
                Name = x.Name,
                Quantity = x.Quantity,
                TotalSpent = x.TotalSpent
            }).ToList();
        return ApiResponse.Ok(dto);
    }

    public async Task<PagedResponse<CustomerGetDTO>> GetAllAsync(
        CustomerListRequest request, CancellationToken cancellationToken = default)
    {
        var query = ApplyScope(CustomerQuery());
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            var normalized = search.ToUpperInvariant();
            query = query.Where(x => x.Name.Contains(search)
                || x.NormalizedEmail.Contains(normalized)
                || (x.NormalizedPhone != null && x.NormalizedPhone.Contains(search)));
        }
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var phone = PublicInputSanitizer.NormalizePhone(request.Phone);
            query = query.Where(x => x.NormalizedPhone == phone);
        }
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim().ToUpperInvariant();
            query = query.Where(x => x.NormalizedEmail == email);
        }

        var total = await query.CountAsync(cancellationToken);
        var entities = await query.OrderBy(x => x.Name).ThenBy(x => x.ID)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize).ToListAsync(cancellationToken);
        return PagedResponse<CustomerGetDTO>.Create(
            _mapper.Map<List<CustomerGetDTO>>(entities), request.PageNumber,
            request.PageSize, total, "Customers retrieved successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateAsync(
        Guid id, CustomerUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var customer = await GetForMutationAsync(id, cancellationToken);
        var phone = PublicInputSanitizer.NormalizePhone(dto.Phone);
        var email = NormalizeEmail(dto.Email);
        await EnsureUniqueAsync(customer.RestaurantId!.Value, phone, email,
            customer.ID, cancellationToken);
        customer.Name = dto.FullName.Trim();
        customer.Phone = phone;
        customer.NormalizedPhone = phone;
        customer.Email = email ?? string.Empty;
        customer.NormalizedEmail = email?.ToUpperInvariant() ?? string.Empty;
        customer.Birthday = dto.Birthday;
        customer.Notes = NormalizeOptional(dto.Notes);
        customer.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Customer updated successfully.");
    }

    public async Task<ApiResponse<object?>> SoftDeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await GetForMutationAsync(id, cancellationToken);
        var hasActiveOrders = await _db.Orders.AsNoTracking().AnyAsync(x =>
            x.CustomerId == id && !x.IsDeleted
            && x.Status != OrderStatus.Completed && x.Status != OrderStatus.Cancelled,
            cancellationToken);
        if (hasActiveOrders)
            throw new ConflictException("A customer with active orders cannot be deleted.");
        customer.IsDeleted = true;
        customer.IsActive = false;
        customer.DeletedAt = UtcNow;
        customer.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.NoContent("Customer deleted successfully.");
    }

    public async Task<PagedResponse<CustomerOrderHistoryDTO>> GetOrderHistoryAsync(
        Guid id, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        await GetAccessibleAsync(id, cancellationToken);
        var query = _db.Orders.AsNoTracking().Where(x =>
            x.CustomerId == id && !x.IsDeleted);
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => x.Branch.ManagerId == userId);
        }
        var total = await query.CountAsync(cancellationToken);
        var data = await query.OrderByDescending(x => x.CreatAt)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new CustomerOrderHistoryDTO
            {
                OrderId = x.ID,
                OrderNumber = x.OrderNumber,
                BranchName = x.Branch.Name,
                TotalAmount = x.TotalAmount,
                CreatedAt = x.CreatAt,
                Status = x.Status.ToString()
            }).ToListAsync(cancellationToken);
        return PagedResponse<CustomerOrderHistoryDTO>.Create(
            data, pageNumber, pageSize, total, "Customer orders retrieved successfully.");
    }

    public async Task<PagedResponse<CustomerReservationHistoryDTO>> GetReservationHistoryAsync(
        Guid id, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        await GetAccessibleAsync(id, cancellationToken);
        var query = _db.Reservations.AsNoTracking().Where(x =>
            x.CustomerId == id && !x.IsDeleted);
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => x.Branch.ManagerId == userId);
        }
        var total = await query.CountAsync(cancellationToken);
        var data = await query.OrderByDescending(x => x.ReservationTime)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new CustomerReservationHistoryDTO
            {
                ReservationId = x.ID,
                BranchName = x.Branch.Name,
                TableNumber = x.Table.TableNumber,
                ReservationTime = x.ReservationTime,
                GuestCount = x.GuestCount,
                Status = x.Status.ToString()
            }).ToListAsync(cancellationToken);
        return PagedResponse<CustomerReservationHistoryDTO>.Create(
            data, pageNumber, pageSize, total,
            "Customer reservations retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<FavoriteMenuItemDTO>>> GetFavoritesAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        await GetAccessibleAsync(id, cancellationToken);
        var data = await FavoriteQuery(id).OrderByDescending(x => x.Quantity)
            .Take(20).Select(x => new FavoriteMenuItemDTO
            {
                MenuItemId = x.MenuItemId,
                Name = x.Name,
                Quantity = x.Quantity,
                TotalSpent = x.TotalSpent
            }).ToListAsync(cancellationToken);
        return ApiResponse.Ok<IReadOnlyCollection<FavoriteMenuItemDTO>>(data);
    }

    private IQueryable<User> CustomerQuery() => _db.BusinessUsers.AsNoTracking()
        .Where(x => !x.IsDeleted && x.IsActive && x.Role == CustomerRole
            && x.RestaurantId.HasValue);

    private IQueryable<FavoriteAggregate> FavoriteQuery(Guid customerId) =>
        _db.OrderItems.AsNoTracking().Where(x => !x.IsDeleted
                && x.Order.CustomerId == customerId
                && !x.Order.IsDeleted && x.Order.Status != OrderStatus.Cancelled)
            .GroupBy(x => new { x.MenuItemId, x.MenuItemName })
            .Select(group => new FavoriteAggregate(
                group.Key.MenuItemId, group.Key.MenuItemName,
                group.Sum(x => x.Quantity), group.Sum(x => x.TotalPrice)));

    private IQueryable<User> ApplyScope(IQueryable<User> query)
    {
        if (_currentUser.IsSuperAdmin) return query;
        var restaurantId = _currentUser.RestaurantId;
        query = query.Where(x => x.RestaurantId == restaurantId);
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => x.BranchId.HasValue
                && x.Branch!.ManagerId == userId);
        }
        return query;
    }

    private async Task<User> GetAccessibleAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await CustomerQuery().FirstOrDefaultAsync(
            x => x.ID == id, cancellationToken)
            ?? throw new NotFoundException("Customer", id);
        await EnsureAccessAsync(customer, cancellationToken);
        return customer;
    }

    private async Task<User> GetForMutationAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _db.BusinessUsers.FirstOrDefaultAsync(x =>
            x.ID == id && !x.IsDeleted && x.Role == CustomerRole,
            cancellationToken) ?? throw new NotFoundException("Customer", id);
        await EnsureAccessAsync(customer, cancellationToken);
        return customer;
    }

    private async Task EnsureAccessAsync(User customer, CancellationToken cancellationToken)
    {
        if (_currentUser.IsSuperAdmin) return;
        if (customer.RestaurantId != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant's customer cannot be accessed.");
        if (_currentUser.IsManager)
        {
            var allowed = customer.BranchId.HasValue && await _db.Branches.AsNoTracking()
                .AnyAsync(x => x.ID == customer.BranchId && x.ManagerId == _currentUser.UserId,
                    cancellationToken);
            if (!allowed)
                throw new ForbiddenException("Managers can access only their own branch customers.");
        }
    }

    private async Task<Guid?> ResolveBranchAsync(Guid? branchId, Guid restaurantId,
        CancellationToken cancellationToken)
    {
        if (_currentUser.IsManager)
        {
            var managed = await _db.Branches.AsNoTracking().Where(x =>
                    x.RestaurantId == restaurantId && x.ManagerId == _currentUser.UserId
                    && !x.IsDeleted && x.IsActive)
                .Select(x => x.ID).ToListAsync(cancellationToken);
            if (managed.Count == 0 || (branchId.HasValue && !managed.Contains(branchId.Value)))
                throw new ForbiddenException("Managers can create customers only for their own branch.");
            if (!branchId.HasValue && managed.Count != 1)
                throw new ConflictException("BranchId is required when the manager owns multiple branches.");
            return branchId ?? managed[0];
        }
        if (!branchId.HasValue) return null;
        if (!await _db.Branches.AsNoTracking().AnyAsync(x => x.ID == branchId
                && x.RestaurantId == restaurantId && !x.IsDeleted, cancellationToken))
            throw new NotFoundException("Branch", branchId.Value);
        return branchId;
    }

    private Guid ResolveRestaurant(Guid restaurantId)
    {
        if (_currentUser.IsSuperAdmin) return restaurantId;
        if (_currentUser.RestaurantId != restaurantId)
            throw new ForbiddenException("Another restaurant cannot be managed.");
        return restaurantId;
    }

    private async Task EnsureUniqueAsync(Guid restaurantId, string phone,
        string? email, Guid? excludedId, CancellationToken cancellationToken)
    {
        var duplicatePhone = await _db.BusinessUsers.AsNoTracking().AnyAsync(x =>
            x.RestaurantId == restaurantId && x.Role == CustomerRole
            && x.NormalizedPhone == phone && !x.IsDeleted
            && (!excludedId.HasValue || x.ID != excludedId), cancellationToken);
        if (duplicatePhone)
            throw new ConflictException("A customer with this phone already exists.");
        if (email is null) return;
        var normalized = email.ToUpperInvariant();
        var duplicateEmail = await _db.BusinessUsers.AsNoTracking().AnyAsync(x =>
            x.RestaurantId == restaurantId && x.Role == CustomerRole
            && x.NormalizedEmail == normalized && !x.IsDeleted
            && (!excludedId.HasValue || x.ID != excludedId), cancellationToken);
        if (duplicateEmail)
            throw new ConflictException("A customer with this email already exists.");
    }

    private static string? NormalizeEmail(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;

    private sealed record FavoriteAggregate(
        Guid MenuItemId, string Name, int Quantity, decimal TotalSpent);
}
