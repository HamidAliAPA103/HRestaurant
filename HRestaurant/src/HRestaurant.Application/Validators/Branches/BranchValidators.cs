using FluentValidation;
using HRestaurant.DTOS.Branch;
using HRestaurant.DTOS.Responses;
using HRestaurant.Validators.Common;

namespace HRestaurant.Validators.Branches;

public sealed class BranchCreateDTOValidator
    : AbstractValidator<BranchCreateDTO>
{
    public BranchCreateDTOValidator()
    {
        RuleFor(dto => dto.RestaurantId)
            .NotEmpty()
            .WithMessage("Restaurant id is required.");

        Include(new BranchDetailsValidator<BranchCreateDTO>());

        RuleFor(dto => dto.WorkingHours)
            .NotNull()
            .WithMessage("Working hours cannot be null.")
            .Must(hours => hours is null || hours.Count is 0 or 7)
            .WithMessage(
                "Working hours must be empty or contain all 7 days.")
            .Must(hours => hours is null || HaveUniqueDays(hours))
            .WithMessage("Each day can appear only once.");

        RuleForEach(dto => dto.WorkingHours)
            .SetValidator(new BranchWorkingHourDTOValidator());
    }

    private static bool HaveUniqueDays(
        IEnumerable<BranchWorkingHourDTO> hours)
    {
        var entries = hours.ToArray();

        return entries.Select(entry => entry.DayOfWeek)
            .Distinct()
            .Count() == entries.Length;
    }
}

public sealed class BranchUpdateDTOValidator
    : AbstractValidator<BranchUpdateDTO>
{
    public BranchUpdateDTOValidator()
    {
        Include(new BranchDetailsValidator<BranchUpdateDTO>());
    }
}

public sealed class BranchListRequestValidator
    : AbstractValidator<BranchListRequest>
{
    public BranchListRequestValidator()
    {
        RuleFor(request => request.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, PaginationRequest.MaxPageSize);

        RuleFor(request => request.Search)
            .MaximumLength(100)
            .When(request => request.Search is not null);

        RuleFor(request => request.ManagerId)
            .NotEmpty()
            .When(request => request.ManagerId.HasValue);

        RuleFor(request => request.RestaurantId)
            .NotEmpty()
            .When(request => request.RestaurantId.HasValue);
    }
}

public sealed class BranchManagerAssignmentDTOValidator
    : AbstractValidator<BranchManagerAssignmentDTO>
{
    public BranchManagerAssignmentDTOValidator()
    {
        RuleFor(dto => dto.ManagerId)
            .NotEmpty()
            .WithMessage("Manager id is required.");
    }
}

public sealed class BranchWorkingHoursUpdateDTOValidator
    : AbstractValidator<BranchWorkingHoursUpdateDTO>
{
    public BranchWorkingHoursUpdateDTOValidator()
    {
        RuleFor(dto => dto.WorkingHours)
            .NotNull()
            .WithMessage("Working hours cannot be null.")
            .Must(hours => hours is null || hours.Count == 7)
            .WithMessage("Working hours must contain all 7 days.")
            .Must(hours =>
                hours is null
                || hours.Select(entry => entry.DayOfWeek)
                    .Distinct()
                    .Count() == 7)
            .WithMessage("Each day must appear exactly once.");

        RuleForEach(dto => dto.WorkingHours)
            .SetValidator(new BranchWorkingHourDTOValidator());
    }
}

public sealed class BranchWorkingHourDTOValidator
    : AbstractValidator<BranchWorkingHourDTO>
{
    public BranchWorkingHourDTOValidator()
    {
        RuleFor(dto => dto.DayOfWeek)
            .IsInEnum()
            .WithMessage("Day of week is invalid.");

        RuleFor(dto => dto.OpensAt)
            .Null()
            .When(dto => dto.IsClosed)
            .WithMessage(
                "Opening time must be empty when the branch is closed.");

        RuleFor(dto => dto.ClosesAt)
            .Null()
            .When(dto => dto.IsClosed)
            .WithMessage(
                "Closing time must be empty when the branch is closed.");

        RuleFor(dto => dto.OpensAt)
            .NotNull()
            .When(dto => !dto.IsClosed)
            .WithMessage(
                "Opening time is required when the branch is open.");

        RuleFor(dto => dto.ClosesAt)
            .NotNull()
            .When(dto => !dto.IsClosed)
            .WithMessage(
                "Closing time is required when the branch is open.")
            .Must((dto, closesAt) =>
                dto.OpensAt.HasValue
                && closesAt.HasValue
                && dto.OpensAt.Value < closesAt.Value)
            .When(dto => !dto.IsClosed)
            .WithMessage("Closing time must be later than opening time.");
    }
}

internal sealed class BranchDetailsValidator<T> : AbstractValidator<T>
    where T : class
{
    public BranchDetailsValidator()
    {
        RuleFor(dto => GetName(dto))
            .NotEmpty()
            .WithName("Name")
            .WithMessage("Branch name cannot be empty.")
            .MaximumLength(ValidationConstants.NameMaximumLength)
            .WithMessage(
                $"Branch name cannot exceed {ValidationConstants.NameMaximumLength} characters.");

        RuleFor(dto => GetSlug(dto))
            .MaximumLength(120)
            .WithName("Slug")
            .WithMessage("Slug cannot exceed 120 characters.")
            .Matches("^[A-Za-z0-9-]+$")
            .WithMessage(
                "Slug can contain letters, numbers and hyphens only.")
            .When(dto => !string.IsNullOrWhiteSpace(GetSlug(dto)));

        RuleFor(dto => GetAddress(dto))
            .NotEmpty()
            .WithName("Address")
            .WithMessage("Branch address cannot be empty.")
            .MaximumLength(250)
            .WithMessage("Branch address cannot exceed 250 characters.");

        RuleFor(dto => GetPhone(dto))
            .Length(
                ValidationConstants.PhoneMinimumLength,
                ValidationConstants.PhoneMaximumLength)
            .WithName("Phone")
            .WithMessage(
                $"Phone length must be between {ValidationConstants.PhoneMinimumLength} and {ValidationConstants.PhoneMaximumLength} characters.")
            .Matches(ValidationConstants.PhonePattern)
            .WithMessage("Phone format is invalid.")
            .When(dto => !string.IsNullOrWhiteSpace(GetPhone(dto)));

        RuleFor(dto => GetEmail(dto))
            .EmailAddress()
            .WithName("Email")
            .WithMessage("Email format is invalid.")
            .MaximumLength(254)
            .WithMessage("Email cannot exceed 254 characters.")
            .When(dto => !string.IsNullOrWhiteSpace(GetEmail(dto)));

        RuleFor(dto => GetLatitude(dto))
            .InclusiveBetween(-90m, 90m)
            .WithName("Latitude")
            .WithMessage("Latitude must be between -90 and 90.")
            .When(dto => GetLatitude(dto).HasValue);

        RuleFor(dto => GetLongitude(dto))
            .InclusiveBetween(-180m, 180m)
            .WithName("Longitude")
            .WithMessage("Longitude must be between -180 and 180.")
            .When(dto => GetLongitude(dto).HasValue);

        RuleFor(dto => GetTimeZoneId(dto))
            .NotEmpty()
            .WithName("TimeZoneId")
            .WithMessage("Time zone id is required.")
            .MaximumLength(100)
            .WithMessage("Time zone id cannot exceed 100 characters.")
            .Must(IsValidTimeZone)
            .WithMessage("Time zone id is invalid.");
    }

    private static string GetName(T dto) => dto switch
    {
        BranchCreateDTO create => create.Name,
        BranchUpdateDTO update => update.Name,
        _ => string.Empty
    };

    private static string? GetSlug(T dto) => dto switch
    {
        BranchCreateDTO create => create.Slug,
        BranchUpdateDTO update => update.Slug,
        _ => null
    };

    private static string GetAddress(T dto) => dto switch
    {
        BranchCreateDTO create => create.Address,
        BranchUpdateDTO update => update.Address,
        _ => string.Empty
    };

    private static string? GetPhone(T dto) => dto switch
    {
        BranchCreateDTO create => create.Phone,
        BranchUpdateDTO update => update.Phone,
        _ => null
    };

    private static string? GetEmail(T dto) => dto switch
    {
        BranchCreateDTO create => create.Email,
        BranchUpdateDTO update => update.Email,
        _ => null
    };

    private static decimal? GetLatitude(T dto) => dto switch
    {
        BranchCreateDTO create => create.Latitude,
        BranchUpdateDTO update => update.Latitude,
        _ => null
    };

    private static decimal? GetLongitude(T dto) => dto switch
    {
        BranchCreateDTO create => create.Longitude,
        BranchUpdateDTO update => update.Longitude,
        _ => null
    };

    private static string GetTimeZoneId(T dto) => dto switch
    {
        BranchCreateDTO create => create.TimeZoneId,
        BranchUpdateDTO update => update.TimeZoneId,
        _ => string.Empty
    };

    private static bool IsValidTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return false;
        }

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId.Trim());
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}
