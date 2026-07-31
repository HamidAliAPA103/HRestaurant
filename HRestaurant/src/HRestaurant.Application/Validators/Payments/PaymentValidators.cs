using FluentValidation;
using HRestaurant.DTOS.Payment;

namespace HRestaurant.Validators.Payments;

public sealed class PaymentCreateDTOValidator : AbstractValidator<PaymentCreateDTO>
{
    public PaymentCreateDTOValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.TransactionReference).MaximumLength(150);
    }
}

public sealed class PaymentCompleteDTOValidator : AbstractValidator<PaymentCompleteDTO>
{
    public PaymentCompleteDTOValidator() => RuleFor(x => x.RowVersion).NotEmpty();
}

public sealed class PaymentFailedDTOValidator : AbstractValidator<PaymentFailedDTO>
{
    public PaymentFailedDTOValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(300);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class RefundCreateDTOValidator : AbstractValidator<RefundCreateDTO>
{
    public RefundCreateDTOValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class SplitPaymentItemDTOValidator : AbstractValidator<SplitPaymentItemDTO>
{
    public SplitPaymentItemDTOValidator()
    {
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.TransactionReference).MaximumLength(150);
    }
}

public sealed class SplitPaymentDTOValidator : AbstractValidator<SplitPaymentDTO>
{
    public SplitPaymentDTOValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderRowVersion).NotEmpty();
        RuleFor(x => x.Payments).NotEmpty().Must(x => x.Count <= 10);
        RuleForEach(x => x.Payments).SetValidator(new SplitPaymentItemDTOValidator());
    }
}
