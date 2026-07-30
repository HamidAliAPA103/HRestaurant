using FluentValidation;
using HRestaurant.Configuration;
using HRestaurant.DTOS.Public;
using System.Linq.Expressions;

namespace HRestaurant.Validators.Public;

public sealed class TableAvailabilityRequestDtoValidator
    : AbstractValidator<TableAvailabilityRequestDto>
{
    public TableAvailabilityRequestDtoValidator(
        TimeProvider timeProvider,
        PublicReservationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(settings);

        RuleFor(dto => dto.ReservationDate)
            .NotEmpty()
            .WithMessage("Reservation date is required.")
            .GreaterThanOrEqualTo(
                DateOnly.FromDateTime(
                    timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage("Reservation date cannot be in the past.");

        RuleFor(dto => dto.StartTime)
            .NotEmpty()
            .WithMessage("Start time is required.");

        RuleFor(dto => dto.GuestCount)
            .InclusiveBetween(
                settings.MinimumGuestCount,
                settings.MaximumGuestCount)
            .WithMessage(
                $"Guest count must be between "
                + $"{settings.MinimumGuestCount} and "
                + $"{settings.MaximumGuestCount}.");

        RuleFor(dto => dto.DurationMinutes)
            .InclusiveBetween(
                settings.MinimumDurationMinutes,
                settings.MaximumDurationMinutes)
            .WithMessage(
                $"Duration must be between "
                + $"{settings.MinimumDurationMinutes} and "
                + $"{settings.MaximumDurationMinutes} minutes.")
            .Must(value =>
                value % settings.SlotIntervalMinutes == 0)
            .WithMessage(
                $"Duration must use "
                + $"{settings.SlotIntervalMinutes}-minute increments.");
    }
}

public sealed class PublicCreateReservationDtoValidator
    : AbstractValidator<PublicCreateReservationDto>
{
    public PublicCreateReservationDtoValidator(
        TimeProvider timeProvider,
        PublicReservationSettings settings)
    {
        Include(new PublicReservationTimingValidator<
            PublicCreateReservationDto>(
            dto => dto.ReservationDate,
            dto => dto.StartTime,
            dto => dto.GuestCount,
            dto => dto.DurationMinutes,
            timeProvider,
            settings));

        RuleFor(dto => dto.BranchId)
            .NotEmpty()
            .WithMessage("Branch id is required.");

        RuleFor(dto => dto.TableId)
            .NotEmpty()
            .WithMessage("Table id is required.");

        RuleFor(dto => dto.FullName)
            .NotEmpty()
            .WithMessage("Full name is required.")
            .MinimumLength(2)
            .WithMessage("Full name must contain at least 2 characters.")
            .MaximumLength(100)
            .WithMessage("Full name cannot exceed 100 characters.");

        RuleFor(dto => dto.Phone)
            .NotEmpty()
            .WithMessage("Phone is required.")
            .Matches(@"^\+?[0-9 ()-]{7,20}$")
            .WithMessage("Phone format is invalid.");

        RuleFor(dto => dto.Email)
            .EmailAddress()
            .WithMessage("Email format is invalid.")
            .MaximumLength(254)
            .WithMessage("Email cannot exceed 254 characters.")
            .When(dto => !string.IsNullOrWhiteSpace(dto.Email));

        RuleFor(dto => dto.SpecialNotes)
            .MaximumLength(500)
            .WithMessage("Special notes cannot exceed 500 characters.");

        RuleFor(dto => dto.TermsAccepted)
            .Equal(true)
            .WithMessage("Terms must be accepted.");
    }
}

public sealed class PublicReservationLookupRequestDtoValidator
    : AbstractValidator<PublicReservationLookupRequestDto>
{
    public PublicReservationLookupRequestDtoValidator()
    {
        RuleFor(dto => dto)
            .Must(HasValidLookupIdentity)
            .WithMessage(
                "Provide a tracking token or confirmation code and phone.");

        RuleFor(dto => dto.ConfirmationCode)
            .Matches(@"^RSV-[A-Z0-9]{6}$")
            .WithMessage("Reservation lookup information is invalid.")
            .When(dto =>
                !string.IsNullOrWhiteSpace(dto.ConfirmationCode));

        RuleFor(dto => dto.Phone)
            .Matches(@"^\+?[0-9 ()-]{7,20}$")
            .WithMessage("Reservation lookup information is invalid.")
            .When(dto => !string.IsNullOrWhiteSpace(dto.Phone));

        RuleFor(dto => dto.TrackingToken)
            .Matches(@"^[A-Fa-f0-9]{64}$")
            .WithMessage("Reservation lookup information is invalid.")
            .When(dto =>
                !string.IsNullOrWhiteSpace(dto.TrackingToken));
    }

    private static bool HasValidLookupIdentity(
        PublicReservationLookupRequestDto dto)
    {
        var hasToken =
            !string.IsNullOrWhiteSpace(dto.TrackingToken);
        var hasCodeAndPhone =
            !string.IsNullOrWhiteSpace(dto.ConfirmationCode)
            && !string.IsNullOrWhiteSpace(dto.Phone);

        return hasToken ^ hasCodeAndPhone;
    }
}

public sealed class PublicCancelReservationDtoValidator
    : AbstractValidator<PublicCancelReservationDto>
{
    public PublicCancelReservationDtoValidator()
    {
        RuleFor(dto => dto)
            .Must(dto =>
                !string.IsNullOrWhiteSpace(dto.Phone)
                ^ !string.IsNullOrWhiteSpace(dto.TrackingToken))
            .WithMessage(
                "Provide either phone or tracking token.");

        RuleFor(dto => dto.Phone)
            .Matches(@"^\+?[0-9 ()-]{7,20}$")
            .WithMessage("Cancellation information is invalid.")
            .When(dto => !string.IsNullOrWhiteSpace(dto.Phone));

        RuleFor(dto => dto.TrackingToken)
            .Matches(@"^[A-Fa-f0-9]{64}$")
            .WithMessage("Cancellation information is invalid.")
            .When(dto =>
                !string.IsNullOrWhiteSpace(dto.TrackingToken));

        RuleFor(dto => dto.Reason)
            .MaximumLength(300)
            .WithMessage(
                "Cancellation reason cannot exceed 300 characters.");
    }
}

internal sealed class PublicReservationTimingValidator<T>
    : AbstractValidator<T>
{
    public PublicReservationTimingValidator(
        Expression<Func<T, DateOnly>> date,
        Expression<Func<T, TimeOnly>> startTime,
        Expression<Func<T, int>> guestCount,
        Expression<Func<T, int>> duration,
        TimeProvider timeProvider,
        PublicReservationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(settings);

        RuleFor(date)
            .NotEmpty()
            .WithMessage("Reservation date is required.")
            .GreaterThanOrEqualTo(
                DateOnly.FromDateTime(
                    timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage("Reservation date cannot be in the past.");

        RuleFor(startTime)
            .NotEmpty()
            .WithMessage("Start time is required.");

        RuleFor(guestCount)
            .InclusiveBetween(
                settings.MinimumGuestCount,
                settings.MaximumGuestCount)
            .WithMessage(
                $"Guest count must be between "
                + $"{settings.MinimumGuestCount} and "
                + $"{settings.MaximumGuestCount}.");

        RuleFor(duration)
            .InclusiveBetween(
                settings.MinimumDurationMinutes,
                settings.MaximumDurationMinutes)
            .WithMessage(
                $"Duration must be between "
                + $"{settings.MinimumDurationMinutes} and "
                + $"{settings.MaximumDurationMinutes} minutes.")
            .Must(value =>
                value % settings.SlotIntervalMinutes == 0)
            .WithMessage(
                $"Duration must use "
                + $"{settings.SlotIntervalMinutes}-minute increments.");
    }
}
