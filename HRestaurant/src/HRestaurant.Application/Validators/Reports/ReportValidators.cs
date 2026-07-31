using FluentValidation;
using HRestaurant.DTOS.Reports;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Validators.Reports;

public sealed class ReportQueryValidator : AbstractValidator<ReportQuery>
{
    public ReportQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x).Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("From date must be before or equal to To date.");
        RuleFor(x => x).Must(x => !x.From.HasValue || !x.To.HasValue
                || x.To.Value.DayNumber - x.From.Value.DayNumber <= 730)
            .WithMessage("Report date range cannot exceed 730 days.");
    }
}
