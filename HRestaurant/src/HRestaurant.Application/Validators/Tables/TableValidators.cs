using FluentValidation;
using HRestaurant.DTOS.Table;

namespace HRestaurant.Validators.Tables;

public sealed class TableCreateDTOValidator
    : AbstractValidator<TableCreateDTO>
{
    public TableCreateDTOValidator()
    {
        RuleFor(dto => dto.RestaurantID)
            .NotEmpty()
            .WithMessage("Restaurant id is required.");

        RuleFor(dto => dto.Tutum)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Table capacity must be at least 1.");

        RuleFor(dto => dto.Status)
            .IsInEnum()
            .WithMessage("Table status is invalid.");
    }
}

public sealed class TableUpdateDTOValidator
    : AbstractValidator<TableUpdateDTO>
{
    public TableUpdateDTOValidator()
    {
        RuleFor(dto => dto.RestaurantID)
            .NotEmpty()
            .WithMessage("Restaurant id is required.");

        RuleFor(dto => dto.Tutum)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Table capacity must be at least 1.");

        RuleFor(dto => dto.Status)
            .IsInEnum()
            .WithMessage("Table status is invalid.");
    }
}
