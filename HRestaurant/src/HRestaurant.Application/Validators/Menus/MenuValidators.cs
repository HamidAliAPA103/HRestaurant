using FluentValidation;
using HRestaurant.DTOS.Menu;
using HRestaurant.Validators.Common;

namespace HRestaurant.Validators.Menus;

public sealed class MenuCreateDTOValidator
    : AbstractValidator<MenuCreateDTO>
{
    public MenuCreateDTOValidator()
    {
        RuleFor(dto => dto.Image)
            .NotNull()
            .WithMessage("Menu image is required.");

        RuleFor(dto => dto.Image.Length)
            .GreaterThan(0)
            .WithMessage("Menu image cannot be empty.")
            .When(dto => dto.Image is not null);

        RuleFor(dto => dto.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero.");

        RuleFor(dto => dto.Desc)
            .NotEmpty()
            .WithMessage("Menu description cannot be empty.")
            .MaximumLength(ValidationConstants.DescriptionMaximumLength)
            .WithMessage(
                $"Menu description cannot exceed {ValidationConstants.DescriptionMaximumLength} characters.");

        RuleFor(dto => dto.CategoryId)
            .NotEmpty()
            .WithMessage("Category id is required.");

        RuleFor(dto => dto.Nutrition)
            .NotEmpty()
            .WithMessage("Nutrition information cannot be empty.")
            .MaximumLength(ValidationConstants.DescriptionMaximumLength)
            .WithMessage(
                $"Nutrition information cannot exceed {ValidationConstants.DescriptionMaximumLength} characters.");
    }
}

public sealed class MenuUpdateDTOValidator
    : AbstractValidator<MenuUpdateDTO>
{
    public MenuUpdateDTOValidator()
    {
        RuleFor(dto => dto.Image!.Length)
            .GreaterThan(0)
            .WithMessage("Menu image cannot be empty.")
            .When(dto => dto.Image is not null);

        RuleFor(dto => dto.ImageURL)
            .MaximumLength(ValidationConstants.UrlMaximumLength)
            .WithMessage(
                $"Image URL cannot exceed {ValidationConstants.UrlMaximumLength} characters.")
            .When(dto => dto.ImageURL is not null);

        RuleFor(dto => dto.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero.")
            .When(dto => dto.Price.HasValue);

        RuleFor(dto => dto.Desc)
            .NotEmpty()
            .WithMessage("Menu description cannot be empty.")
            .MaximumLength(ValidationConstants.DescriptionMaximumLength)
            .WithMessage(
                $"Menu description cannot exceed {ValidationConstants.DescriptionMaximumLength} characters.")
            .When(dto => dto.Desc is not null);

        RuleFor(dto => dto.Nutrition)
            .NotEmpty()
            .WithMessage("Nutrition information cannot be empty.")
            .MaximumLength(ValidationConstants.DescriptionMaximumLength)
            .WithMessage(
                $"Nutrition information cannot exceed {ValidationConstants.DescriptionMaximumLength} characters.")
            .When(dto => dto.Nutrition is not null);
    }
}
