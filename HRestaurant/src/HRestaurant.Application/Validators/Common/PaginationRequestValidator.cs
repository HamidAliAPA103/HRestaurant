using FluentValidation;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Validators.Common;

public sealed class PaginationRequestValidator :
    AbstractValidator<PaginationRequest>
{
    public PaginationRequestValidator()
    {
        RuleFor(request => request.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, PaginationRequest.MaxPageSize);
    }
}
