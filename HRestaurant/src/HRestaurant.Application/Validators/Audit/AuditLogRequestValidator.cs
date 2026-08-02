using FluentValidation;
using HRestaurant.DTOS.Audit;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Validators.Audit;

public sealed class AuditLogRequestValidator : AbstractValidator<AuditLogRequest>
{
    public AuditLogRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x.EntityName).MaximumLength(100);
        RuleFor(x => x.Action).MaximumLength(50);
        RuleFor(x => x).Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("From date must be before or equal to To date.");
    }
}
