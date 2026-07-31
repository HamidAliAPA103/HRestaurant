using FluentValidation;
using HRestaurant.DTOS.Loyalty;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Validators.Loyalty;

public sealed class LoyaltyAdjustmentDTOValidator : AbstractValidator<LoyaltyAdjustmentDTO>
{
    public LoyaltyAdjustmentDTOValidator()
    {
        RuleFor(x => x.Points).NotEqual(0).InclusiveBetween(-1_000_000, 1_000_000);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class LoyaltyHistoryRequestValidator : AbstractValidator<LoyaltyHistoryRequest>
{
    public LoyaltyHistoryRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationRequest.MaxPageSize);
    }
}
