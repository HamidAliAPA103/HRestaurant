using System.Data;
using HRestaurant.Configuration;
using HRestaurant.Data;
using HRestaurant.DTOS.Loyalty;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class LoyaltyService : ILoyaltyService
{
    private const string CustomerRole = "Customer";
    private readonly AppDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly LoyaltySettings _settings;
    private readonly TimeProvider _timeProvider;

    public LoyaltyService(AppDbContext db, ICurrentUserContext currentUser,
        LoyaltySettings settings, TimeProvider timeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _settings = settings;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<LoyaltySummaryDTO>> GetSummaryAsync(
        Guid customerId, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerAccessAsync(customerId, cancellationToken);
        var account = await _db.LoyaltyAccounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && !x.IsDeleted,
                cancellationToken);
        return ApiResponse.Ok(account is null
            ? EmptySummary(customerId)
            : MapSummary(account));
    }

    public async Task<PagedResponse<LoyaltyTransactionGetDTO>> GetHistoryAsync(
        Guid customerId, LoyaltyHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCustomerAccessAsync(customerId, cancellationToken);
        var query = _db.LoyaltyTransactions.AsNoTracking().Where(x =>
            x.LoyaltyAccount.CustomerId == customerId && !x.IsDeleted);
        var total = await query.CountAsync(cancellationToken);
        var data = await query.OrderByDescending(x => x.CreatAt)
            .ThenByDescending(x => x.ID)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new LoyaltyTransactionGetDTO
            {
                Id = x.ID,
                OrderId = x.OrderId,
                Type = x.Type,
                Points = x.Points,
                Description = x.Description,
                CreatedAt = x.CreatAt
            }).ToListAsync(cancellationToken);
        return PagedResponse<LoyaltyTransactionGetDTO>.Create(
            data, request.PageNumber, request.PageSize, total,
            "Loyalty history retrieved successfully.");
    }

    public async Task<ApiResponse<LoyaltySummaryDTO>> AdjustAsync(
        Guid customerId, LoyaltyAdjustmentDTO dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureCustomerAccessAsync(customerId, cancellationToken);
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var account = await GetOrCreateAccountAsync(customerId, cancellationToken);
        if (account.RowVersion.Length > 0)
            _db.Entry(account).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;
        if (account.CurrentPoints + dto.Points < 0)
            throw new ConflictException("The loyalty adjustment would create a negative balance.");

        account.CurrentPoints += dto.Points;
        if (dto.Points > 0) account.LifetimeEarnedPoints += dto.Points;
        else account.LifetimeRedeemedPoints += Math.Abs(dto.Points);
        account.UpdateAt = UtcNow;
        _db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            LoyaltyAccountId = account.ID,
            Type = LoyaltyTransactionType.Adjustment,
            Points = dto.Points,
            Description = dto.Description.Trim(),
            CreatAt = UtcNow
        });
        await SaveConcurrencyAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Ok(MapSummary(account), "Loyalty balance adjusted successfully.");
    }

    public async Task RedeemForPaymentAsync(Order order, Payment payment,
        CancellationToken cancellationToken = default)
    {
        if (payment.PaymentMethod != PaymentMethod.LoyaltyPoints) return;
        if (!order.CustomerId.HasValue)
            throw new ConflictException("A customer is required for a loyalty-points payment.");

        var account = await GetOrCreateAccountAsync(order.CustomerId.Value, cancellationToken);
        var points = (int)Math.Ceiling(payment.Amount / _settings.CurrencyValuePerPoint);
        if (points <= 0 || account.CurrentPoints < points)
            throw new ConflictException("The customer does not have enough loyalty points.");
        var exists = await _db.LoyaltyTransactions.AnyAsync(x =>
            x.OrderId == order.ID && x.Type == LoyaltyTransactionType.Redeemed,
            cancellationToken);
        if (exists)
            throw new ConflictException("Loyalty points have already been redeemed for this order.");

        account.CurrentPoints -= points;
        account.LifetimeRedeemedPoints += points;
        account.UpdateAt = UtcNow;
        _db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            LoyaltyAccountId = account.ID,
            OrderId = order.ID,
            Type = LoyaltyTransactionType.Redeemed,
            Points = -points,
            Description = $"Redeemed for order {order.OrderNumber}.",
            CreatAt = UtcNow
        });
        await SaveConcurrencyAsync(cancellationToken);
    }

    public async Task EarnForFullyPaidOrderAsync(Order order,
        CancellationToken cancellationToken = default)
    {
        if (!order.CustomerId.HasValue || !order.IsPaid) return;
        var exists = await _db.LoyaltyTransactions.AnyAsync(x =>
            x.OrderId == order.ID && x.Type == LoyaltyTransactionType.Earned,
            cancellationToken);
        if (exists) return;
        var points = (int)Math.Floor(order.TotalAmount * _settings.EarnPointsPerCurrencyUnit);
        if (points <= 0) return;

        var account = await GetOrCreateAccountAsync(order.CustomerId.Value, cancellationToken);
        account.CurrentPoints += points;
        account.LifetimeEarnedPoints += points;
        account.UpdateAt = UtcNow;
        _db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            LoyaltyAccountId = account.ID,
            OrderId = order.ID,
            Type = LoyaltyTransactionType.Earned,
            Points = points,
            Description = $"Earned from order {order.OrderNumber}.",
            CreatAt = UtcNow
        });
        await SaveConcurrencyAsync(cancellationToken);
    }

    private async Task<LoyaltyAccount> GetOrCreateAccountAsync(
        Guid customerId, CancellationToken cancellationToken)
    {
        var account = await _db.LoyaltyAccounts.FirstOrDefaultAsync(x =>
            x.CustomerId == customerId && !x.IsDeleted, cancellationToken);
        if (account is not null) return account;
        account = new LoyaltyAccount { CustomerId = customerId, CreatAt = UtcNow };
        _db.LoyaltyAccounts.Add(account);
        await _db.SaveChangesAsync(cancellationToken);
        return account;
    }

    private async Task EnsureCustomerAccessAsync(Guid customerId,
        CancellationToken cancellationToken)
    {
        var customer = await _db.BusinessUsers.AsNoTracking().FirstOrDefaultAsync(x =>
            x.ID == customerId && x.Role == CustomerRole && !x.IsDeleted,
            cancellationToken) ?? throw new NotFoundException("Customer", customerId);
        if (_currentUser.IsSuperAdmin) return;
        if (customer.RestaurantId != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant's loyalty account cannot be accessed.");
        if (_currentUser.IsManager)
        {
            var allowed = customer.BranchId.HasValue && await _db.Branches.AsNoTracking()
                .AnyAsync(x => x.ID == customer.BranchId && x.ManagerId == _currentUser.UserId,
                    cancellationToken);
            if (!allowed)
                throw new ForbiddenException("Managers can access only their own branch loyalty accounts.");
        }
    }

    private LoyaltySummaryDTO EmptySummary(Guid customerId) => new()
    {
        CustomerId = customerId,
        CurrencyValue = 0m
    };

    private LoyaltySummaryDTO MapSummary(LoyaltyAccount account) => new()
    {
        CustomerId = account.CustomerId,
        CurrentPoints = account.CurrentPoints,
        LifetimeEarnedPoints = account.LifetimeEarnedPoints,
        LifetimeRedeemedPoints = account.LifetimeRedeemedPoints,
        CurrencyValue = decimal.Round(
            account.CurrentPoints * _settings.CurrencyValuePerPoint, 2),
        RowVersion = account.RowVersion
    };

    private async Task SaveConcurrencyAsync(CancellationToken cancellationToken)
    {
        try { await _db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The loyalty balance changed concurrently. Reload and retry.");
        }
    }

    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
