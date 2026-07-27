using FluentValidation;
using HRestaurant.DTOS.MenuCategory;
using HRestaurant.Validators.Common;

namespace HRestaurant.Validators.MenuCategories;

public sealed class MenuCategoryCreateDTOValidator
    : AbstractValidator<MenuCategoryCreateDTO>
{
    public MenuCategoryCreateDTOValidator()
    {
        RuleFor(dto => dto.ResdaranId)
            .NotEmpty()
            .WithMessage("Restaurant id is required.");

        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithMessage("Category name cannot be empty.")
            .MaximumLength(ValidationConstants.NameMaximumLength)
            .WithMessage(
                $"Category name cannot exceed {ValidationConstants.NameMaximumLength} characters.");
    }
}

public sealed class MenuCategoryUpdateDTOValidator
    : AbstractValidator<MenuCategoryUpdateDTO>
{
    public MenuCategoryUpdateDTOValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithMessage("Category name cannot be empty.")
            .MaximumLength(ValidationConstants.NameMaximumLength)
            .WithMessage(
                $"Category name cannot exceed {ValidationConstants.NameMaximumLength} characters.")
            .When(dto => dto.Name is not null);
    }
}
