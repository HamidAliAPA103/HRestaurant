using FluentValidation;
using HRestaurant.DTOS.Reservation;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Validators.Reservations;

public sealed class ReservationCreateDTOValidator
    : AbstractValidator<ReservationCreateDTO>
{
    public ReservationCreateDTOValidator(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        RuleFor(dto => dto.CustomerId)
            .NotEmpty()
            .WithMessage("Customer id is required.");

        RuleFor(dto => dto.TableId)
            .NotEmpty()
            .WithMessage("Table id is required.");

        RuleFor(dto => dto.BranchId)
            .NotEmpty()
            .WithMessage("Branch id is required.");

        RuleFor(dto => dto.ReservationTime)
            .Must(value => IsNotInPast(value, timeProvider))
            .WithMessage("Reservation date cannot be in the past.");

        RuleFor(dto => dto.GuestCount)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Guest count must be at least 1.");

        RuleFor(dto => dto.DurationMinutes)
            .InclusiveBetween(30, 240)
            .WithMessage(
                "Duration must be between 30 and 240 minutes.");

        RuleFor(dto => dto.Status)
            .IsInEnum()
            .WithMessage("Reservation status is invalid.");
    }

    private static bool IsNotInPast(
        DateTime reservationTime,
        TimeProvider timeProvider)
    {
        if (reservationTime == default)
        {
            return false;
        }

        var reservationUtc = reservationTime.Kind switch
        {
            DateTimeKind.Local => reservationTime.ToUniversalTime(),
            DateTimeKind.Utc => reservationTime,
            _ => DateTime.SpecifyKind(reservationTime, DateTimeKind.Utc)
        };

        return reservationUtc >= timeProvider.GetUtcNow().UtcDateTime;
    }
}

public sealed class ReservationListRequestValidator
    : AbstractValidator<ReservationListRequest>
{
    public ReservationListRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x).Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("From date must be before or equal to To date.");
    }
}

public sealed class ReservationStatusUpdateDTOValidator
    : AbstractValidator<ReservationStatusUpdateDTO>
{
    public ReservationStatusUpdateDTOValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Reason).MaximumLength(300);
    }
}

public sealed class ReservationUpdateDTOValidator
    : AbstractValidator<ReservationUpdateDTO>
{
    public ReservationUpdateDTOValidator(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        RuleFor(dto => dto.CustomerId)
            .NotEmpty()
            .WithMessage("Customer id is required.");

        RuleFor(dto => dto.TableId)
            .NotEmpty()
            .WithMessage("Table id is required.");

        RuleFor(dto => dto.BranchId)
            .NotEmpty()
            .WithMessage("Branch id is required.");

        RuleFor(dto => dto.ReservationTime)
            .Must(value => IsNotInPast(value, timeProvider))
            .WithMessage("Reservation date cannot be in the past.");

        RuleFor(dto => dto.GuestCount)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Guest count must be at least 1.");

        RuleFor(dto => dto.DurationMinutes)
            .InclusiveBetween(30, 240)
            .WithMessage(
                "Duration must be between 30 and 240 minutes.");

        RuleFor(dto => dto.Status)
            .IsInEnum()
            .WithMessage("Reservation status is invalid.");
    }

    private static bool IsNotInPast(
        DateTime reservationTime,
        TimeProvider timeProvider)
    {
        if (reservationTime == default)
        {
            return false;
        }

        var reservationUtc = reservationTime.Kind switch
        {
            DateTimeKind.Local => reservationTime.ToUniversalTime(),
            DateTimeKind.Utc => reservationTime,
            _ => DateTime.SpecifyKind(reservationTime, DateTimeKind.Utc)
        };

        return reservationUtc >= timeProvider.GetUtcNow().UtcDateTime;
    }
}
