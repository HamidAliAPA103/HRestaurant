using HRestaurant.Data;
using HRestaurant.DTOS.Reports;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Exceptions;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class ReportService : IReportService
{
    private static readonly PaymentStatus[] SuccessfulPaymentStatuses =
        [PaymentStatus.Completed, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded];
    private readonly AppDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public ReportService(AppDbContext db, ICurrentUserContext currentUser,
        TimeProvider timeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<DashboardSummaryDTO>> GetDashboardAsync(
        ReportQuery query, CancellationToken cancellationToken = default)
    {
        var orders = ApplyOrderScope(ApplyDate(_db.Orders.AsNoTracking()
            .Where(x => !x.IsDeleted && x.Status != OrderStatus.Cancelled), query), query);
        var paidOrders = orders.Where(x => x.IsPaid);
        var revenue = await paidOrders.SumAsync(x => (decimal?)x.TotalAmount,
            cancellationToken) ?? 0;
        var orderCount = await paidOrders.CountAsync(cancellationToken);
        var reservations = ApplyReservationScope(ApplyDate(
            _db.Reservations.AsNoTracking().Where(x => !x.IsDeleted), query), query);
        var customers = ApplyCustomerScope(_db.BusinessUsers.AsNoTracking()
            .Where(x => !x.IsDeleted && x.Role == "Customer"));
        var inventory = ApplyInventoryScope(_db.InventoryItems.AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive), query);
        var refunds = ApplyPaymentScope(ApplyPaidDate(_db.Payments.AsNoTracking()
            .Where(x => !x.IsDeleted), query), query)
            .SelectMany(x => x.Refunds.Where(r => !r.IsDeleted));

        var recent = await orders.OrderByDescending(x => x.CreatAt).Take(10)
            .Select(x => new RecentOrderDTO
            {
                Id = x.ID,
                OrderNumber = x.OrderNumber,
                BranchName = x.Branch.Name,
                Total = x.TotalAmount,
                Status = x.Status.ToString(),
                CreatedAt = x.CreatAt
            }).ToListAsync(cancellationToken);
        var topItems = await ApplyOrderItemScope(_db.OrderItems.AsNoTracking()
                .Where(x => !x.IsDeleted && x.Order.IsPaid
                    && x.Order.Status != OrderStatus.Cancelled), query)
            .GroupBy(x => x.MenuItemName)
            .Select(group => new NamedValueDTO
            {
                Name = group.Key,
                Value = group.Sum(x => x.TotalPrice),
                Count = group.Sum(x => x.Quantity)
            }).OrderByDescending(x => x.Value).Take(5).ToListAsync(cancellationToken);
        var sales = await BuildSalesAsync(query, "day", cancellationToken);
        return ApiResponse.Ok(new DashboardSummaryDTO
        {
            Revenue = revenue,
            OrderCount = orderCount,
            AverageOrderValue = orderCount == 0 ? 0 : Money(revenue / orderCount),
            ReservationCount = await reservations.CountAsync(cancellationToken),
            CustomerCount = await customers.CountAsync(cancellationToken),
            LowStockCount = await inventory.CountAsync(
                x => x.CurrentQuantity <= x.MinimumQuantity, cancellationToken),
            RefundedAmount = await refunds.SumAsync(x => (decimal?)x.Amount,
                cancellationToken) ?? 0,
            RecentOrders = recent,
            TopItems = topItems,
            Sales = sales.ToList()
        }, "Dashboard report retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<TimeSeriesPointDTO>>> GetSalesAsync(
        ReportQuery query, string period, CancellationToken cancellationToken = default) =>
        ApiResponse.Ok(await BuildSalesAsync(query, period, cancellationToken),
            "Sales report retrieved successfully.");

    public async Task<PagedResponse<NamedValueDTO>> GetMenuItemSalesAsync(
        ReportQuery query, bool least, CancellationToken cancellationToken = default)
    {
        var grouped = ApplyOrderItemScope(_db.OrderItems.AsNoTracking()
                .Where(x => !x.IsDeleted && x.Order.IsPaid
                    && x.Order.Status != OrderStatus.Cancelled), query)
            .GroupBy(x => x.MenuItemName)
            .Select(group => new NamedValueDTO
            {
                Name = group.Key,
                Value = group.Sum(x => x.TotalPrice),
                Count = group.Sum(x => x.Quantity)
            });
        var total = await grouped.CountAsync(cancellationToken);
        var ordered = least ? grouped.OrderBy(x => x.Count).ThenBy(x => x.Name)
            : grouped.OrderByDescending(x => x.Count).ThenBy(x => x.Name);
        var data = await ordered.Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize).ToListAsync(cancellationToken);
        return PagedResponse<NamedValueDTO>.Create(data, query.PageNumber,
            query.PageSize, total, "Menu-item sales retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<NamedValueDTO>>> GetSalesByCategoryAsync(
        ReportQuery query, CancellationToken cancellationToken = default)
    {
        var data = await ApplyOrderItemScope(_db.OrderItems.AsNoTracking()
                .Where(x => !x.IsDeleted && x.Order.IsPaid
                    && x.Order.Status != OrderStatus.Cancelled), query)
            .GroupBy(x => x.MenuItem.Category.Name)
            .Select(group => new NamedValueDTO
            {
                Name = group.Key,
                Value = group.Sum(x => x.TotalPrice),
                Count = group.Sum(x => x.Quantity)
            }).OrderByDescending(x => x.Value).ToListAsync(cancellationToken);
        return ApiResponse.Ok<IReadOnlyCollection<NamedValueDTO>>(data);
    }

    public async Task<ApiResponse<IReadOnlyCollection<NamedValueDTO>>> GetSalesByPaymentMethodAsync(
        ReportQuery query, CancellationToken cancellationToken = default)
    {
        var data = await ApplyPaymentScope(ApplyPaidDate(_db.Payments.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && SuccessfulPaymentStatuses.Contains(x.PaymentStatus)), query), query)
            .GroupBy(x => x.PaymentMethod)
            .Select(group => new NamedValueDTO
            {
                Name = group.Key.ToString(),
                Value = group.Sum(x => x.Amount)
                    - group.SelectMany(x => x.Refunds.Where(r => !r.IsDeleted))
                        .Sum(x => x.Amount),
                Count = group.Count()
            }).OrderByDescending(x => x.Value).ToListAsync(cancellationToken);
        return ApiResponse.Ok<IReadOnlyCollection<NamedValueDTO>>(data);
    }

    public async Task<ApiResponse<IReadOnlyCollection<NamedValueDTO>>> GetSalesByBranchAsync(
        ReportQuery query, CancellationToken cancellationToken = default)
    {
        var data = await ApplyOrderScope(ApplyDate(_db.Orders.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsPaid
                    && x.Status != OrderStatus.Cancelled), query), query)
            .GroupBy(x => x.Branch.Name)
            .Select(group => new NamedValueDTO
            {
                Name = group.Key,
                Value = group.Sum(x => x.TotalAmount),
                Count = group.Count()
            }).OrderByDescending(x => x.Value).ToListAsync(cancellationToken);
        return ApiResponse.Ok<IReadOnlyCollection<NamedValueDTO>>(data);
    }

    public async Task<PagedResponse<EmployeePerformanceDTO>> GetEmployeePerformanceAsync(
        ReportQuery query, CancellationToken cancellationToken = default)
    {
        var grouped = ApplyOrderScope(ApplyDate(_db.Orders.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsPaid && x.WaiterId.HasValue
                    && x.Status != OrderStatus.Cancelled), query), query)
            .GroupBy(x => new { EmployeeId = x.WaiterId!.Value, x.Waiter!.Name })
            .Select(group => new EmployeePerformanceDTO
            {
                EmployeeId = group.Key.EmployeeId,
                EmployeeName = group.Key.Name,
                OrderCount = group.Count(),
                Revenue = group.Sum(x => x.TotalAmount),
                AverageOrderValue = group.Average(x => x.TotalAmount)
            });
        var total = await grouped.CountAsync(cancellationToken);
        var data = await grouped.OrderByDescending(x => x.Revenue)
            .Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);
        return PagedResponse<EmployeePerformanceDTO>.Create(data, query.PageNumber,
            query.PageSize, total, "Employee performance retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<NamedValueDTO>>> GetPeakHoursAsync(
        ReportQuery query, CancellationToken cancellationToken = default)
    {
        var raw = await ApplyOrderScope(ApplyDate(_db.Orders.AsNoTracking()
                .Where(x => !x.IsDeleted && x.Status != OrderStatus.Cancelled), query), query)
            .Select(x => new { x.CreatAt, x.TotalAmount }).ToListAsync(cancellationToken);
        IReadOnlyCollection<NamedValueDTO> data = raw.GroupBy(x => x.CreatAt.Hour)
            .Select(group => new NamedValueDTO
            {
                Name = $"{group.Key:00}:00",
                Count = group.Count(),
                Value = group.Sum(x => x.TotalAmount)
            }).OrderByDescending(x => x.Count).ThenBy(x => x.Name).ToList();
        return ApiResponse.Ok(data);
    }

    public async Task<PagedResponse<RecentOrderDTO>> GetCancelledOrdersAsync(
        ReportQuery query, CancellationToken cancellationToken = default)
    {
        var orders = ApplyOrderScope(ApplyDate(_db.Orders.AsNoTracking()
            .Where(x => !x.IsDeleted && x.Status == OrderStatus.Cancelled), query), query);
        var total = await orders.CountAsync(cancellationToken);
        var data = await orders.OrderByDescending(x => x.CancelledAt)
            .Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new RecentOrderDTO
            {
                Id = x.ID,
                OrderNumber = x.OrderNumber,
                BranchName = x.Branch.Name,
                Total = x.TotalAmount,
                Status = x.Status.ToString(),
                CreatedAt = x.CreatAt
            }).ToListAsync(cancellationToken);
        return PagedResponse<RecentOrderDTO>.Create(data, query.PageNumber,
            query.PageSize, total, "Cancelled orders retrieved successfully.");
    }

    public async Task<ApiResponse<ReservationStatisticsDTO>> GetReservationStatisticsAsync(
        ReportQuery query, CancellationToken cancellationToken = default)
    {
        var reservations = ApplyReservationScope(ApplyDate(
            _db.Reservations.AsNoTracking().Where(x => !x.IsDeleted), query), query);
        var counts = await reservations.GroupBy(x => x.Status)
            .Select(x => new { x.Key, Count = x.Count() }).ToListAsync(cancellationToken);
        var total = counts.Sum(x => x.Count);
        int Count(ReservationStatus status) => counts.FirstOrDefault(x => x.Key == status)?.Count ?? 0;
        var cancelled = Count(ReservationStatus.Cancelled);
        return ApiResponse.Ok(new ReservationStatisticsDTO
        {
            Total = total,
            Pending = Count(ReservationStatus.Pending),
            Confirmed = Count(ReservationStatus.Confirmed) + Count(ReservationStatus.Seated),
            Completed = Count(ReservationStatus.Completed),
            Cancelled = cancelled,
            NoShow = Count(ReservationStatus.NoShow),
            CancellationRate = total == 0 ? 0 : Money(cancelled * 100m / total)
        });
    }

    public async Task<ApiResponse<CustomerStatisticsDTO>> GetCustomerStatisticsAsync(
        ReportQuery query, CancellationToken cancellationToken = default)
    {
        var customers = ApplyCustomerScope(_db.BusinessUsers.AsNoTracking()
            .Where(x => !x.IsDeleted && x.Role == "Customer"));
        if (query.BranchId.HasValue)
            customers = customers.Where(x => x.BranchId == query.BranchId);
        var (start, end) = Range(query);
        var total = await customers.CountAsync(cancellationToken);
        var returning = await customers.CountAsync(x => x.TotalOrders > 1, cancellationToken);
        return ApiResponse.Ok(new CustomerStatisticsDTO
        {
            TotalCustomers = total,
            ReturningCustomers = returning,
            NewCustomers = await customers.CountAsync(x =>
                x.CreatAt >= start && x.CreatAt < end, cancellationToken),
            AverageCustomerSpend = total == 0 ? 0
                : Money(await customers.AverageAsync(x => x.TotalSpent, cancellationToken))
        });
    }

    public async Task<ApiResponse<IReadOnlyCollection<NamedValueDTO>>> GetLowStockSummaryAsync(
        ReportQuery query, CancellationToken cancellationToken = default)
    {
        var data = await ApplyInventoryScope(_db.InventoryItems.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive
                    && x.CurrentQuantity <= x.MinimumQuantity), query)
            .OrderBy(x => x.CurrentQuantity - x.MinimumQuantity)
            .Select(x => new NamedValueDTO
            {
                Name = x.Ingredient.Name + " (" + x.Branch.Name + ")",
                Value = x.CurrentQuantity,
                Count = 1
            }).Take(100).ToListAsync(cancellationToken);
        return ApiResponse.Ok<IReadOnlyCollection<NamedValueDTO>>(data);
    }

    private async Task<IReadOnlyCollection<TimeSeriesPointDTO>> BuildSalesAsync(
        ReportQuery query, string period, CancellationToken cancellationToken)
    {
        if (!new[] { "day", "week", "month" }.Contains(period,
            StringComparer.OrdinalIgnoreCase))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(period)] = ["Period must be day, week or month."]
            });
        var raw = await ApplyOrderScope(ApplyDate(_db.Orders.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsPaid
                    && x.Status != OrderStatus.Cancelled), query), query)
            .Select(x => new { x.CreatAt, x.TotalAmount }).ToListAsync(cancellationToken);
        DateTime Key(DateTime value) => period.ToLowerInvariant() switch
        {
            "month" => new DateTime(value.Year, value.Month, 1),
            "week" => value.Date.AddDays(-(((int)value.DayOfWeek + 6) % 7)),
            _ => value.Date
        };
        return raw.GroupBy(x => Key(x.CreatAt)).OrderBy(x => x.Key)
            .Select(group => new TimeSeriesPointDTO
            {
                Period = group.Key,
                Revenue = group.Sum(x => x.TotalAmount),
                OrderCount = group.Count(),
                AverageOrderValue = Money(group.Average(x => x.TotalAmount))
            }).ToList();
    }

    private IQueryable<Order> ApplyOrderScope(IQueryable<Order> query, ReportQuery request)
    {
        if (!_currentUser.IsSuperAdmin)
            query = query.Where(x => x.RestaurantId == _currentUser.RestaurantId);
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => x.Branch.ManagerId == userId);
        }
        if (request.BranchId.HasValue)
        {
            EnsureBranchAccess(request.BranchId.Value);
            query = query.Where(x => x.BranchId == request.BranchId);
        }
        return query;
    }

    private IQueryable<OrderItem> ApplyOrderItemScope(
        IQueryable<OrderItem> query, ReportQuery request)
    {
        if (!_currentUser.IsSuperAdmin)
            query = query.Where(x => x.Order.RestaurantId == _currentUser.RestaurantId);
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => x.Order.Branch.ManagerId == userId);
        }
        if (request.BranchId.HasValue)
        {
            EnsureBranchAccess(request.BranchId.Value);
            query = query.Where(x => x.Order.BranchId == request.BranchId);
        }
        var (start, end) = Range(request);
        return query.Where(x => x.Order.CreatAt >= start && x.Order.CreatAt < end);
    }

    private IQueryable<Reservation> ApplyReservationScope(
        IQueryable<Reservation> query, ReportQuery request)
    {
        if (!_currentUser.IsSuperAdmin)
            query = query.Where(x => x.Branch.RestaurantId == _currentUser.RestaurantId);
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => x.Branch.ManagerId == userId);
        }
        if (request.BranchId.HasValue)
        {
            EnsureBranchAccess(request.BranchId.Value);
            query = query.Where(x => x.BranchId == request.BranchId);
        }
        return query;
    }

    private IQueryable<Payment> ApplyPaymentScope(
        IQueryable<Payment> query, ReportQuery request)
    {
        if (!_currentUser.IsSuperAdmin)
            query = query.Where(x => x.RestaurantId == _currentUser.RestaurantId);
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => x.Branch.ManagerId == userId);
        }
        if (request.BranchId.HasValue)
        {
            EnsureBranchAccess(request.BranchId.Value);
            query = query.Where(x => x.BranchId == request.BranchId);
        }
        return query;
    }

    private IQueryable<InventoryItem> ApplyInventoryScope(
        IQueryable<InventoryItem> query, ReportQuery request)
    {
        if (!_currentUser.IsSuperAdmin)
            query = query.Where(x => x.RestaurantId == _currentUser.RestaurantId);
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => x.Branch.ManagerId == userId);
        }
        if (request.BranchId.HasValue)
        {
            EnsureBranchAccess(request.BranchId.Value);
            query = query.Where(x => x.BranchId == request.BranchId);
        }
        return query;
    }

    private IQueryable<User> ApplyCustomerScope(IQueryable<User> query)
    {
        if (!_currentUser.IsSuperAdmin)
            query = query.Where(x => x.RestaurantId == _currentUser.RestaurantId);
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => x.Branch != null && x.Branch.ManagerId == userId);
        }
        return query;
    }

    private IQueryable<T> ApplyDate<T>(IQueryable<T> query, ReportQuery request)
        where T : Models.BaseModels.BaseEntity
    {
        var (start, end) = Range(request);
        return query.Where(x => x.CreatAt >= start && x.CreatAt < end);
    }

    private IQueryable<Payment> ApplyPaidDate(IQueryable<Payment> query, ReportQuery request)
    {
        var (start, end) = Range(request);
        return query.Where(x => x.PaidAt >= start && x.PaidAt < end);
    }

    private (DateTime Start, DateTime End) Range(ReportQuery request)
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var from = request.From ?? today.AddDays(-29);
        var to = request.To ?? today;
        return (from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
    }

    private void EnsureBranchAccess(Guid branchId)
    {
        if (_currentUser.IsSuperAdmin) return;
        var restaurantId = _currentUser.RestaurantId;
        var userId = _currentUser.UserId;
        var allowed = _db.Branches.AsNoTracking().Any(x => x.ID == branchId
            && x.RestaurantId == restaurantId && !x.IsDeleted
            && (!_currentUser.IsManager || x.ManagerId == userId));
        if (!allowed)
            throw new ForbiddenException("The selected branch is outside your access scope.");
    }

    private static decimal Money(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
