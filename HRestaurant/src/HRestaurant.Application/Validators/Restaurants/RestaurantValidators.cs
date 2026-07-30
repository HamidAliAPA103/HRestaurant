using FluentValidation;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Validators.Common;

namespace HRestaurant.Validators.Restaurants;

public sealed class RestaurantCreateDTOValidator
    : AbstractValidator<RestaurantCreateDTO>
{
    public RestaurantCreateDTOValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithMessage("Restaurant name cannot be empty.")
            .MaximumLength(ValidationConstants.NameMaximumLength)
            .WithMessage(
                $"Restaurant name cannot exceed {ValidationConstants.NameMaximumLength} characters.");

        RuleFor(dto => dto.Slug)
            .MaximumLength(120)
            .WithMessage("Slug cannot exceed 120 characters.")
            .Matches("^[A-Za-z0-9-]+$")
            .WithMessage(
                "Slug can contain letters, numbers and hyphens only.")
            .When(dto => !string.IsNullOrWhiteSpace(dto.Slug));

        RuleFor(dto => dto.Adres)
            .NotEmpty()
            .WithMessage("Restaurant address cannot be empty.")
            .MaximumLength(250)
            .WithMessage(
                "Restaurant address cannot exceed 250 characters.");

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

        RuleFor(dto => dto.Email)
            .EmailAddress()
            .WithMessage("Email format is invalid.")
            .MaximumLength(254)
            .WithMessage("Email cannot exceed 254 characters.")
            .When(dto => !string.IsNullOrWhiteSpace(dto.Email));

        RuleFor(dto => dto.Description)
            .MaximumLength(2000)
            .WithMessage(
                "Description cannot exceed 2000 characters.");

        RuleFor(dto => dto.LogoUrl)
            .MaximumLength(500)
            .WithMessage("Logo URL cannot exceed 500 characters.");

        RuleFor(dto => dto.CoverImageUrl)
            .MaximumLength(500)
            .WithMessage(
                "Cover image URL cannot exceed 500 characters.");

        RuleFor(dto => dto.Currency)
            .NotEmpty()
            .WithMessage("Currency cannot be empty.")
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code.")
            .Matches("^[A-Za-z]{3}$")
            .WithMessage("Currency must contain letters only.");

        RuleFor(dto => dto.TaxRate)
            .InclusiveBetween(0m, 100m)
            .WithMessage("Tax rate must be between 0 and 100.")
            .Must(HaveAtMostTwoDecimalPlaces)
            .WithMessage(
                "Tax rate can have at most 2 decimal places.");

        RuleFor(dto => dto.WorkingHours)
            .NotNull()
            .WithMessage("Working hours cannot be null.")
            .Must(hours => hours is null || hours.Count is 0 or 7)
            .WithMessage(
                "Working hours must be empty or contain all 7 days.")
            .Must(hours => hours is null || HaveUniqueDays(hours))
            .WithMessage("Each day can appear only once.");

        RuleForEach(dto => dto.WorkingHours)
            .SetValidator(new RestaurantWorkingHourDTOValidator());
    }

    private static bool HaveUniqueDays(
        IEnumerable<RestaurantWorkingHourDTO> hours)
    {
        var entries = hours.ToArray();

        return entries
            .Select(entry => entry.DayOfWeek)
            .Distinct()
            .Count() == entries.Length;
    }

    private static bool HaveAtMostTwoDecimalPlaces(decimal value)
    {
        return decimal.Round(value, 2) == value;
    }
}

public sealed class RestaurantUpdateDTOValidator
    : AbstractValidator<RestaurantUpdateDTO>
{
    public RestaurantUpdateDTOValidator()
    {
        RuleFor(dto => dto)
            .Must(dto =>
                dto.Name is not null
                || dto.Adres is not null
                || dto.Number is not null
                || dto.Email is not null
                || dto.Description is not null
                || dto.LogoUrl is not null
                || dto.CoverImageUrl is not null)
            .WithMessage(
                "At least one restaurant field must be supplied.");

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
            .WithMessage(
                "Restaurant address cannot exceed 250 characters.")
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

        RuleFor(dto => dto.Email)
            .EmailAddress()
            .WithMessage("Email format is invalid.")
            .MaximumLength(254)
            .WithMessage("Email cannot exceed 254 characters.")
            .When(dto => !string.IsNullOrWhiteSpace(dto.Email));

        RuleFor(dto => dto.Description)
            .MaximumLength(2000)
            .WithMessage(
                "Description cannot exceed 2000 characters.")
            .When(dto => dto.Description is not null);

        RuleFor(dto => dto.LogoUrl)
            .MaximumLength(500)
            .WithMessage("Logo URL cannot exceed 500 characters.")
            .When(dto => dto.LogoUrl is not null);

        RuleFor(dto => dto.CoverImageUrl)
            .MaximumLength(500)
            .WithMessage(
                "Cover image URL cannot exceed 500 characters.")
            .When(dto => dto.CoverImageUrl is not null);
    }
}

public sealed class RestaurantSettingsUpdateDTOValidator
    : AbstractValidator<RestaurantSettingsUpdateDTO>
{
    public RestaurantSettingsUpdateDTOValidator()
    {
        RuleFor(dto => dto.Currency)
            .NotEmpty()
            .WithMessage("Currency cannot be empty.")
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code.")
            .Matches("^[A-Za-z]{3}$")
            .WithMessage("Currency must contain letters only.");

        RuleFor(dto => dto.TaxRate)
            .InclusiveBetween(0m, 100m)
            .WithMessage("Tax rate must be between 0 and 100.")
            .Must(value => decimal.Round(value, 2) == value)
            .WithMessage(
                "Tax rate can have at most 2 decimal places.");
    }
}

public sealed class RestaurantWorkingHoursUpdateDTOValidator
    : AbstractValidator<RestaurantWorkingHoursUpdateDTO>
{
    public RestaurantWorkingHoursUpdateDTOValidator()
    {
        RuleFor(dto => dto.WorkingHours)
            .NotNull()
            .WithMessage("Working hours cannot be null.")
            .Must(hours => hours is null || hours.Count == 7)
            .WithMessage("Working hours must contain all 7 days.")
            .Must(hours =>
                hours is null
                || hours
                    .Select(entry => entry.DayOfWeek)
                    .Distinct()
                    .Count() == 7)
            .WithMessage("Each day must appear exactly once.");

        RuleForEach(dto => dto.WorkingHours)
            .SetValidator(new RestaurantWorkingHourDTOValidator());
    }
}

public sealed class RestaurantWorkingHourDTOValidator
    : AbstractValidator<RestaurantWorkingHourDTO>
{
    public RestaurantWorkingHourDTOValidator()
    {
        RuleFor(dto => dto.DayOfWeek)
            .IsInEnum()
            .WithMessage("Day of week is invalid.");

        RuleFor(dto => dto.OpensAt)
            .Null()
            .WithMessage(
                "Opening time must be empty when the restaurant is closed.")
            .When(dto => dto.IsClosed);

        RuleFor(dto => dto.ClosesAt)
            .Null()
            .WithMessage(
                "Closing time must be empty when the restaurant is closed.")
            .When(dto => dto.IsClosed);

        RuleFor(dto => dto.OpensAt)
            .NotNull()
            .WithMessage(
                "Opening time is required when the restaurant is open.")
            .When(dto => !dto.IsClosed);

        RuleFor(dto => dto.ClosesAt)
            .NotNull()
            .WithMessage(
                "Closing time is required when the restaurant is open.")
            .Must((dto, closesAt) =>
                dto.OpensAt.HasValue
                && closesAt.HasValue
                && dto.OpensAt.Value < closesAt.Value)
            .WithMessage("Closing time must be later than opening time.")
            .When(dto => !dto.IsClosed);
    }
}
