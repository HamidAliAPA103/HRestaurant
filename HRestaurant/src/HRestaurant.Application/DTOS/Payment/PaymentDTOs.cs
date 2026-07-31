using HRestaurant.Enum;

namespace HRestaurant.DTOS.Payment;

public sealed class PaymentCreateDTO
{
    public Guid OrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionReference { get; set; }
}

public sealed class PaymentCompleteDTO
{
    public byte[] RowVersion { get; set; } = [];
}

public sealed class PaymentFailedDTO
{
    public string? Reason { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class RefundCreateDTO
{
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = [];
}

public sealed class SplitPaymentItemDTO
{
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionReference { get; set; }
}

public sealed class SplitPaymentDTO
{
    public Guid OrderId { get; set; }
    public List<SplitPaymentItemDTO> Payments { get; set; } = [];
    public byte[] OrderRowVersion { get; set; } = [];
}

public sealed class RefundGetDTO
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid RefundedByUserId { get; set; }
    public string RefundedByName { get; set; } = string.Empty;
    public DateTime RefundedAt { get; set; }
}

public sealed class PaymentGetDTO
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal Amount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal RefundableAmount { get; set; }
    public string? TransactionReference { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public List<RefundGetDTO> Refunds { get; set; } = [];
}

public sealed class OrderPaymentSummaryDTO
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public bool IsFullyPaid { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public byte[] OrderRowVersion { get; set; } = [];
    public List<PaymentGetDTO> Payments { get; set; } = [];
}

public sealed class ReceiptItemDTO
{
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
}

public sealed class ReceiptPaymentDTO
{
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionReference { get; set; }
    public DateTime PaidAt { get; set; }
}

public sealed class ReceiptDTO
{
    public string RestaurantName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string? TableNumber { get; set; }
    public List<ReceiptItemDTO> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public List<ReceiptPaymentDTO> Payments { get; set; } = [];
    public DateTime? PaidAt { get; set; }
    public string CashierName { get; set; } = string.Empty;
}
