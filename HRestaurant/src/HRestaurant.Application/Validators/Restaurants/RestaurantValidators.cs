using FluentValidation;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Validators.Common;

namespace HRestaurant.Validators.Restaurants;

public sealed class RestaurantCreateDTOValidator
    : AbstractValidator<RestaurantCreatDTO>
{
    public RestaurantCreateDTOValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithMessage("Restaurant name cannot be empty.")
            .MaximumLength(ValidationConstants.NameMaximumLength)
            .WithMessage(
                $"Restaurant name cannot exceed {ValidationConstants.NameMaximumLength} characters.");

        RuleFor(dto => dto.Adres)
            .NotEmpty()
            .WithMessage("Restaurant address cannot be empty.")
            .MaximumLength(250)
            .WithMessage("Restaurant address cannot exceed 250 characters.");

        RuleFor(dto => dto.Number)
            .NotEmpty()
            .WithMessage("Phone cannot be empty.")
            .Length(
                ValidationConstants.PhoneMinimumLength,
                ValidationConstants.PhoneMaximumLength)
            .WithMessage(
                $"Phone length must be between {ValidationConstants.PhoneMinimumLength} and {ValidationConstants.PhoneMaximumLength} characters.")
            .Matches(ValidationConstants.PhonePattern)
            .WithMessage("Phone format is invalid.");
    }
}

public sealed class RestaurantUpdateDTOValidator
    : AbstractValidator<RestaurantUpdateDTO>
{
    public RestaurantUpdateDTOValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithMessage("Restaurant name cannot be empty.")
            .MaximumLength(ValidationConstants.NameMaximumLength)
            .WithMessage(
                $"Restaurant name cannot exceed {ValidationConstants.NameMaximumLength} characters.")
            .When(dto => dto.Name is not null);

        RuleFor(dto => dto.Adres)
            .NotEmpty()
            .WithMessage("Restaurant address cannot be empty.")
            .MaximumLength(250)
            .WithMessage("Restaurant address cannot exceed 250 characters.")
            .When(dto => dto.Adres is not null);

        RuleFor(dto => dto.Number)
            .NotEmpty()
            .WithMessage("Phone cannot be empty.")
            .Length(
                ValidationConstants.PhoneMinimumLength,
                ValidationConstants.PhoneMaximumLength)
            .WithMessage(
                $"Phone length must be between {ValidationConstants.PhoneMinimumLength} and {ValidationConstants.PhoneMaximumLength} characters.")
            .Matches(ValidationConstants.PhonePattern)
            .WithMessage("Phone format is invalid.")
            .When(dto => dto.Number is not null);
    }
}
