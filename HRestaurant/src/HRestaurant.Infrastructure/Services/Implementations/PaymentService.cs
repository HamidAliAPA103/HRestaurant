using System.Data;
using System.Text.Json;
using HRestaurant.Data;
using HRestaurant.DTOS.Payment;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Exceptions;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class PaymentService : IPaymentService
{
    private static readonly PaymentStatus[] SuccessfulStatuses =
        [PaymentStatus.Completed, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded];
    private readonly AppDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILoyaltyService _loyalty;
    private readonly TimeProvider _timeProvider;

    public PaymentService(AppDbContext db, ICurrentUserContext currentUser,
        ILoyaltyService loyalty, TimeProvider timeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _loyalty = loyalty;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<Guid>> CreateAsync(
        PaymentCreateDTO dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var order = await GetOrderAsync(dto.OrderId, cancellationToken);
        EnsureOrderPayable(order);
        await EnsureReferenceUniqueAsync(dto.TransactionReference, cancellationToken);
        var remaining = CalculateSummary(order).RemainingAmount;
        if (dto.Amount > remaining)
            throw new ConflictException("Payment amount cannot exceed the remaining order amount.");

        var payment = new Payment
        {
            OrderId = order.ID,
            RestaurantId = order.RestaurantId,
            BranchId = order.BranchId,
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            Amount = Money(dto.Amount),
            TransactionReference = NormalizeOptional(dto.TransactionReference),
            CreatedByUserId = _currentUser.UserId,
            CreatAt = UtcNow
        };
        _db.Payments.Add(payment);
        AddAudit("PaymentCreated", payment, null,
            new { payment.OrderId, payment.Amount, payment.PaymentMethod });
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Created(payment.ID, "Payment created successfully.");
    }

    public async Task<ApiResponse<OrderPaymentSummaryDTO>> CompleteAsync(
        Guid id, PaymentCompleteDTO dto,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var payment = await GetPaymentAsync(id, cancellationToken);
        EnsurePending(payment);
        ApplyVersion(payment, dto.RowVersion);
        EnsureOrderPayable(payment.Order);
        var remaining = CalculateSummary(payment.Order).RemainingAmount;
        if (payment.Amount > remaining)
            throw new ConflictException("The payment exceeds the current remaining amount.");

        await _loyalty.RedeemForPaymentAsync(payment.Order, payment, cancellationToken);
        payment.PaymentStatus = PaymentStatus.Completed;
        payment.PaidAt = UtcNow;
        payment.UpdateAt = UtcNow;
        var becameFullyPaid = await UpdateOrderTotalsAsync(payment.Order, cancellationToken);
        AddAudit("PaymentCompleted", payment,
            new { Status = PaymentStatus.Pending },
            new { Status = payment.PaymentStatus, payment.PaidAt });
        await SaveConcurrencyAsync(cancellationToken);
        if (becameFullyPaid)
            await _loyalty.EarnForFullyPaidOrderAsync(payment.Order, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Ok(await BuildSummaryAsync(payment.Order, cancellationToken),
            "Payment completed successfully.");
    }

    public async Task<ApiResponse<OrderPaymentSummaryDTO>> FailAsync(
        Guid id, PaymentFailedDTO dto,
        CancellationToken cancellationToken = default)
    {
        var payment = await GetPaymentAsync(id, cancellationToken);
        EnsurePending(payment);
        ApplyVersion(payment, dto.RowVersion);
        payment.PaymentStatus = PaymentStatus.Failed;
        payment.FailureReason = NormalizeOptional(dto.Reason);
        payment.UpdateAt = UtcNow;
        AddAudit("PaymentFailed", payment,
            new { Status = PaymentStatus.Pending },
            new { Status = payment.PaymentStatus, payment.FailureReason });
        await SaveConcurrencyAsync(cancellationToken);
        return ApiResponse.Ok(await BuildSummaryAsync(payment.Order, cancellationToken),
            "Payment marked as failed.");
    }

    public async Task<ApiResponse<OrderPaymentSummaryDTO>> SplitAsync(
        SplitPaymentDTO dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var order = await GetOrderAsync(dto.OrderId, cancellationToken);
        EnsureOrderPayable(order);
        _db.Entry(order).Property(x => x.RowVersion).OriginalValue = dto.OrderRowVersion;
        var total = Money(dto.Payments.Sum(x => x.Amount));
        var remaining = CalculateSummary(order).RemainingAmount;
        if (total > remaining)
            throw new ConflictException("Split payments exceed the remaining order amount.");

        foreach (var item in dto.Payments)
            await EnsureReferenceUniqueAsync(item.TransactionReference, cancellationToken);
        if (dto.Payments.Where(x => !string.IsNullOrWhiteSpace(x.TransactionReference))
            .GroupBy(x => x.TransactionReference!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Any(x => x.Count() > 1))
            throw new ConflictException("Split payment references must be unique.");

        foreach (var item in dto.Payments)
        {
            var payment = new Payment
            {
                OrderId = order.ID,
                RestaurantId = order.RestaurantId,
                BranchId = order.BranchId,
                PaymentMethod = item.PaymentMethod,
                PaymentStatus = PaymentStatus.Completed,
                Amount = Money(item.Amount),
                TransactionReference = NormalizeOptional(item.TransactionReference),
                PaidAt = UtcNow,
                CreatedByUserId = _currentUser.UserId,
                CreatAt = UtcNow
            };
            _db.Payments.Add(payment);
            await _loyalty.RedeemForPaymentAsync(order, payment, cancellationToken);
            AddAudit("SplitPaymentCompleted", payment, null,
                new { payment.OrderId, payment.Amount, payment.PaymentMethod });
        }

        var becameFullyPaid = await UpdateOrderTotalsAsync(order, cancellationToken);
        await SaveConcurrencyAsync(cancellationToken);
        if (becameFullyPaid)
            await _loyalty.EarnForFullyPaidOrderAsync(order, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Ok(await BuildSummaryAsync(order, cancellationToken),
            "Split payment completed successfully.");
    }

    public async Task<ApiResponse<OrderPaymentSummaryDTO>> GetOrderSummaryAsync(
        Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderAsync(orderId, cancellationToken);
        return ApiResponse.Ok(await BuildSummaryAsync(order, cancellationToken));
    }

    public async Task<ApiResponse<ReceiptDTO>> GetReceiptAsync(
        Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(x => x.Restaurant).Include(x => x.Branch).Include(x => x.Table)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .Include(x => x.Payments.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(x => x.ID == orderId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Order", orderId);
        await EnsureOrderAccessAsync(order, cancellationToken);
        var successful = order.Payments.Where(x =>
            SuccessfulStatuses.Contains(x.PaymentStatus)).ToList();
        if (successful.Count == 0)
            throw new ConflictException("A receipt is available only after a completed payment.");
        var cashierId = successful.OrderByDescending(x => x.PaidAt)
            .Select(x => x.CreatedByUserId).First();
        var cashier = await _db.Users.AsNoTracking().Where(x => x.Id == cashierId)
            .Select(x => x.FullName).FirstOrDefaultAsync(cancellationToken) ?? "Unknown";
        return ApiResponse.Ok(new ReceiptDTO
        {
            RestaurantName = order.Restaurant.Name,
            BranchName = order.Branch.Name,
            Address = order.Branch.Address,
            OrderNumber = order.OrderNumber,
            TableNumber = order.Table?.TableNumber,
            Items = order.Items.Select(x => new ReceiptItemDTO
            {
                Name = x.MenuItemName,
                UnitPrice = x.UnitPrice,
                Quantity = x.Quantity,
                Discount = x.DiscountAmount,
                Total = x.TotalPrice
            }).ToList(),
            Subtotal = order.Subtotal,
            Discount = order.DiscountAmount,
            Tax = order.TaxAmount,
            Total = order.TotalAmount,
            Payments = successful.Where(x => x.PaidAt.HasValue)
                .Select(x => new ReceiptPaymentDTO
                {
                    Method = x.PaymentMethod,
                    Amount = x.Amount,
                    TransactionReference = x.TransactionReference,
                    PaidAt = x.PaidAt!.Value
                }).ToList(),
            PaidAt = successful.Max(x => x.PaidAt),
            CashierName = cashier
        }, "Receipt retrieved successfully.");
    }

    public async Task<ApiResponse<OrderPaymentSummaryDTO>> RefundAsync(
        Guid paymentId, RefundCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var payment = await GetPaymentAsync(paymentId, cancellationToken);
        if (payment.PaymentStatus is not (
            PaymentStatus.Completed or PaymentStatus.PartiallyRefunded))
            throw new ConflictException("Only completed payments can be refunded.");
        ApplyVersion(payment, dto.RowVersion);
        var refunded = payment.Refunds.Where(x => !x.IsDeleted).Sum(x => x.Amount);
        if (dto.Amount > payment.Amount - refunded)
            throw new ConflictException("Refund amount exceeds the refundable payment amount.");

        var wasFullyPaid = payment.Order.IsPaid;
        var refund = new Refund
        {
            PaymentId = payment.ID,
            Amount = Money(dto.Amount),
            Reason = dto.Reason.Trim(),
            RefundedByUserId = _currentUser.UserId,
            RefundedAt = UtcNow,
            CreatAt = UtcNow
        };
        payment.Refunds.Add(refund);
        var totalRefunded = refunded + refund.Amount;
        payment.PaymentStatus = totalRefunded == payment.Amount
            ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
        payment.UpdateAt = UtcNow;
        await UpdateOrderTotalsAsync(payment.Order, cancellationToken);
        if (wasFullyPaid && !payment.Order.IsPaid && payment.Order.CustomerId.HasValue)
        {
            var customer = await _db.BusinessUsers.FirstOrDefaultAsync(
                x => x.ID == payment.Order.CustomerId, cancellationToken);
            if (customer is not null)
            {
                customer.TotalSpent = Math.Max(0, Money(customer.TotalSpent - refund.Amount));
                customer.UpdateAt = UtcNow;
            }
        }
        AddAudit("PaymentRefunded", payment, null,
            new { refund.Amount, refund.Reason, PaymentId = payment.ID });
        await SaveConcurrencyAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Ok(await BuildSummaryAsync(payment.Order, cancellationToken),
            "Refund completed successfully.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<RefundGetDTO>>> GetRefundHistoryAsync(
        Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await GetPaymentAsync(paymentId, cancellationToken);
        var refunds = payment.Refunds.Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.RefundedAt).ToList();
        var names = await _db.Users.AsNoTracking()
            .Where(x => refunds.Select(r => r.RefundedByUserId).Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);
        IReadOnlyCollection<RefundGetDTO> data = refunds.Select(x => new RefundGetDTO
        {
            Id = x.ID,
            PaymentId = x.PaymentId,
            Amount = x.Amount,
            Reason = x.Reason,
            RefundedByUserId = x.RefundedByUserId,
            RefundedByName = names.GetValueOrDefault(x.RefundedByUserId, "Unknown"),
            RefundedAt = x.RefundedAt
        }).ToList();
        return ApiResponse.Ok(data, "Refund history retrieved successfully.");
    }

    private async Task<Order> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.Include(x => x.Branch).Include(x => x.Restaurant)
            .Include(x => x.Payments.Where(p => !p.IsDeleted)).ThenInclude(x => x.Refunds)
            .FirstOrDefaultAsync(x => x.ID == orderId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Order", orderId);
        await EnsureOrderAccessAsync(order, cancellationToken);
        return order;
    }

    private async Task<Payment> GetPaymentAsync(Guid paymentId,
        CancellationToken cancellationToken)
    {
        var payment = await _db.Payments.Include(x => x.Refunds.Where(r => !r.IsDeleted))
            .Include(x => x.Order).ThenInclude(x => x.Branch)
            .Include(x => x.Order).ThenInclude(x => x.Restaurant)
            .Include(x => x.Order).ThenInclude(x => x.Payments).ThenInclude(x => x.Refunds)
            .FirstOrDefaultAsync(x => x.ID == paymentId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Payment", paymentId);
        await EnsureOrderAccessAsync(payment.Order, cancellationToken);
        return payment;
    }

    private async Task EnsureOrderAccessAsync(Order order, CancellationToken cancellationToken)
    {
        if (_currentUser.IsSuperAdmin) return;
        if (order.RestaurantId != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant's payment cannot be accessed.");
        if (_currentUser.IsManager && order.Branch.ManagerId != _currentUser.UserId)
            throw new ForbiddenException("Managers can access only their own branch payments.");
        if (_currentUser.IsInRole(AppRoles.Cashier))
        {
            var allowed = await _db.BusinessUsers.AsNoTracking().AnyAsync(x =>
                x.AppUserId == _currentUser.UserId && x.BranchId == order.BranchId
                && x.IsActive && !x.IsDeleted, cancellationToken);
            if (!allowed)
                throw new ForbiddenException("Cashiers can access only their own branch payments.");
        }
    }

    private static void EnsureOrderPayable(Order order)
    {
        if (order.Status == OrderStatus.Cancelled)
            throw new ConflictException("A cancelled order cannot be paid.");
        if (order.TotalAmount <= 0)
            throw new ConflictException("The order total must be greater than zero.");
    }

    private static void EnsurePending(Payment payment)
    {
        if (payment.PaymentStatus != PaymentStatus.Pending)
            throw new ConflictException("Only pending payments can change to completed or failed.");
    }

    private async Task<bool> UpdateOrderTotalsAsync(Order order,
        CancellationToken cancellationToken)
    {
        var wasFullyPaid = order.IsPaid;
        var totals = CalculateSummary(order);
        order.PaidAmount = totals.PaidAmount;
        order.IsPaid = totals.RemainingAmount == 0;
        order.PaymentStatus = order.IsPaid ? PaymentStatus.Completed
            : totals.PaidAmount > 0 ? PaymentStatus.PartiallyRefunded : PaymentStatus.Pending;
        if (order.IsPaid && order.Status == OrderStatus.Pending)
            order.Status = OrderStatus.Confirmed;
        order.UpdateAt = UtcNow;
        if (!wasFullyPaid && order.IsPaid && order.CustomerId.HasValue)
        {
            var customer = await _db.BusinessUsers.FirstOrDefaultAsync(
                x => x.ID == order.CustomerId, cancellationToken);
            if (customer is not null)
            {
                customer.TotalOrders++;
                customer.TotalSpent = Money(customer.TotalSpent + order.TotalAmount);
                customer.LastVisitDate = UtcNow;
                customer.UpdateAt = UtcNow;
            }
        }
        return !wasFullyPaid && order.IsPaid;
    }

    private OrderPaymentSummaryDTO CalculateSummary(Order order)
    {
        var payments = order.Payments.Where(x => !x.IsDeleted
            && SuccessfulStatuses.Contains(x.PaymentStatus)).ToList();
        var gross = payments.Sum(x => x.Amount);
        var refunds = payments.SelectMany(x => x.Refunds)
            .Where(x => !x.IsDeleted).Sum(x => x.Amount);
        var net = Money(Math.Max(0, gross - refunds));
        return new OrderPaymentSummaryDTO
        {
            OrderId = order.ID,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            PaidAmount = net,
            RefundedAmount = Money(refunds),
            RemainingAmount = Money(Math.Max(0, order.TotalAmount - net)),
            IsFullyPaid = net >= order.TotalAmount,
            PaymentStatus = net >= order.TotalAmount ? PaymentStatus.Completed
                : net > 0 ? PaymentStatus.PartiallyRefunded : PaymentStatus.Pending,
            OrderRowVersion = order.RowVersion
        };
    }

    private async Task<OrderPaymentSummaryDTO> BuildSummaryAsync(Order order,
        CancellationToken cancellationToken)
    {
        await _db.Entry(order).ReloadAsync(cancellationToken);
        await _db.Entry(order).Collection(x => x.Payments).Query()
            .Where(x => !x.IsDeleted).Include(x => x.Refunds.Where(r => !r.IsDeleted))
            .LoadAsync(cancellationToken);
        var summary = CalculateSummary(order);
        var payments = order.Payments.Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatAt).ToList();
        var names = await _db.Users.AsNoTracking()
            .Where(x => payments.Select(p => p.CreatedByUserId).Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);
        summary.Payments = payments.Select(x => new PaymentGetDTO
        {
            Id = x.ID,
            OrderId = x.OrderId,
            RestaurantId = x.RestaurantId,
            BranchId = x.BranchId,
            PaymentMethod = x.PaymentMethod,
            PaymentStatus = x.PaymentStatus,
            Amount = x.Amount,
            RefundedAmount = x.Refunds.Where(r => !r.IsDeleted).Sum(r => r.Amount),
            RefundableAmount = Math.Max(0,
                x.Amount - x.Refunds.Where(r => !r.IsDeleted).Sum(r => r.Amount)),
            TransactionReference = x.TransactionReference,
            FailureReason = x.FailureReason,
            PaidAt = x.PaidAt,
            CreatedByUserId = x.CreatedByUserId,
            CreatedByName = names.GetValueOrDefault(x.CreatedByUserId, "Unknown"),
            CreatedAt = x.CreatAt,
            RowVersion = x.RowVersion,
            Refunds = x.Refunds.Where(r => !r.IsDeleted).Select(r => new RefundGetDTO
            {
                Id = r.ID,
                PaymentId = r.PaymentId,
                Amount = r.Amount,
                Reason = r.Reason,
                RefundedByUserId = r.RefundedByUserId,
                RefundedAt = r.RefundedAt
            }).ToList()
        }).ToList();
        return summary;
    }

    private async Task EnsureReferenceUniqueAsync(string? reference,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizeOptional(reference);
        if (normalized is not null && await _db.Payments.AsNoTracking().AnyAsync(x =>
                x.TransactionReference == normalized, cancellationToken))
            throw new ConflictException("This payment transaction reference already exists.");
    }

    private void AddAudit(string action, Payment payment, object? oldValues, object? newValues)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = _currentUser.UserId,
            Action = action,
            EntityName = nameof(Payment),
            EntityId = payment.ID,
            OldValues = oldValues is null ? null : JsonSerializer.Serialize(oldValues),
            NewValues = newValues is null ? null : JsonSerializer.Serialize(newValues),
            CreatAt = UtcNow
        });
    }

    private void ApplyVersion(Payment payment, byte[] rowVersion) =>
        _db.Entry(payment).Property(x => x.RowVersion).OriginalValue = rowVersion;

    private async Task SaveConcurrencyAsync(CancellationToken cancellationToken)
    {
        try { await _db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The payment changed concurrently. Reload and retry.");
        }
    }

    private static decimal Money(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
