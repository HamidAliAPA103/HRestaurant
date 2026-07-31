using FluentValidation;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Shift;

namespace HRestaurant.Validators.Shifts;

public sealed class ShiftCreateDTOValidator : AbstractValidator<ShiftCreateDTO>
{
    public ShiftCreateDTOValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
    }
}

public sealed class ShiftUpdateDTOValidator : AbstractValidator<ShiftUpdateDTO>
{
    public ShiftUpdateDTOValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
    }
}

public sealed class EmployeeShiftAssignDTOValidator
    : AbstractValidator<EmployeeShiftAssignDTO>
{
    public EmployeeShiftAssignDTOValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.WorkDate).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(500)
            .When(x => x.Notes is not null);
        RuleFor(x => x).Must(x => !x.StartTime.HasValue || !x.EndTime.HasValue
                || x.EndTime > x.StartTime)
            .WithMessage("EndTime must be after StartTime.");
    }
}

public sealed class ShiftListRequestValidator : AbstractValidator<ShiftListRequest>
{
    public ShiftListRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x).Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue
                || x.FromDate <= x.ToDate)
            .WithMessage("FromDate cannot be after ToDate.");
    }
}
