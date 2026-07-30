using FluentAssertions;
using HRestaurant.Configuration;
using HRestaurant.DTOS.Public;
using HRestaurant.Services.Implementations;
using HRestaurant.Tests.TestSupport;
using HRestaurant.Validators.Public;

namespace HRestaurant.Tests.Validation;

public sealed class PublicReservationValidationTests
{
    [Fact]
    public void Validator_RejectsPastReservationDate()
    {
        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(
                2026,
                7,
                30,
                8,
                0,
                0,
                TimeSpan.Zero));
        var validator = new PublicCreateReservationDtoValidator(
            timeProvider,
            new PublicReservationSettings());
        var dto = new PublicCreateReservationDto
        {
            BranchId = Guid.NewGuid(),
            TableId = Guid.NewGuid(),
            ReservationDate = new DateOnly(2026, 7, 29),
            StartTime = new TimeOnly(19, 0),
            GuestCount = 2,
            DurationMinutes = 120,
            FullName = "Test Guest",
            Phone = "+994501234567",
            TermsAccepted = true
        };

        var result = validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error =>
            error.PropertyName.Contains("ReservationDate"));
    }

    [Fact]
    public void ConfirmationService_GeneratesAndHashesSecureToken()
    {
        var service = new ReservationConfirmationService();

        var token = service.GenerateTrackingToken();
        var hash = service.HashTrackingToken(token);

        token.Should().HaveLength(64);
        hash.Should().HaveLength(64);
        hash.Should().NotBe(token);
        service.HashTrackingToken(token).Should().Be(hash);
        service.GenerateConfirmationCode()
            .Should().MatchRegex("^RSV-[A-Z0-9]{6}$");
    }
}
