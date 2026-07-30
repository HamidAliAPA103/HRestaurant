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

        RuleFor(dto => dto.BranchId)
            .NotNull()
            .WithMessage("Branch id is required.")
            .Must(value => value.HasValue && value.Value != Guid.Empty)
            .WithMessage("Branch id is required.");

        RuleFor(dto => dto.Tutum)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Table capacity must be at least 1.");

        RuleFor(dto => dto.TableNumber)
            .NotEmpty()
            .WithMessage("Table number is required.")
            .MaximumLength(30)
            .WithMessage("Table number cannot exceed 30 characters.");

        RuleFor(dto => dto.Status)
            .IsInEnum()
            .WithMessage("Table status is invalid.");

        RuleFor(dto => dto.Shape)
            .IsInEnum()
            .WithMessage("Table shape is invalid.");

        RuleFor(dto => dto.Width)
            .InclusiveBetween(0.5, 10)
            .WithMessage("Table width must be between 0.5 and 10.");

        RuleFor(dto => dto.Length)
            .InclusiveBetween(0.5, 10)
            .WithMessage("Table length must be between 0.5 and 10.");
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

        RuleFor(dto => dto.BranchId)
            .NotNull()
            .WithMessage("Branch id is required.")
            .Must(value => value.HasValue && value.Value != Guid.Empty)
            .WithMessage("Branch id is required.");

        RuleFor(dto => dto.Tutum)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Table capacity must be at least 1.");

        RuleFor(dto => dto.TableNumber)
            .NotEmpty()
            .WithMessage("Table number is required.")
            .MaximumLength(30)
            .WithMessage("Table number cannot exceed 30 characters.");

        RuleFor(dto => dto.Status)
            .IsInEnum()
            .WithMessage("Table status is invalid.");

        RuleFor(dto => dto.Shape)
            .IsInEnum()
            .WithMessage("Table shape is invalid.");

        RuleFor(dto => dto.Width)
            .InclusiveBetween(0.5, 10)
            .WithMessage("Table width must be between 0.5 and 10.");

        RuleFor(dto => dto.Length)
            .InclusiveBetween(0.5, 10)
            .WithMessage("Table length must be between 0.5 and 10.");
    }
}
