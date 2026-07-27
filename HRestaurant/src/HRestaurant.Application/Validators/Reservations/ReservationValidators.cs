using FluentValidation;
using HRestaurant.DTOS.Reservation;

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

        RuleFor(dto => dto.ReservationTime)
            .Must(value => IsNotInPast(value, timeProvider))
            .WithMessage("Reservation date cannot be in the past.");

        RuleFor(dto => dto.GuestCount)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Guest count must be at least 1.");

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

        RuleFor(dto => dto.ReservationTime)
            .Must(value => IsNotInPast(value, timeProvider))
            .WithMessage("Reservation date cannot be in the past.");

        RuleFor(dto => dto.GuestCount)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Guest count must be at least 1.");

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
