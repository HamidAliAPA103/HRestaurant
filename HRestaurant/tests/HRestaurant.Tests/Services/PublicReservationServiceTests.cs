using FluentAssertions;
using HRestaurant.DTOS.Public;
using HRestaurant.Enum;
using HRestaurant.Exceptions;
using HRestaurant.Services.Implementations;
using HRestaurant.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Tests.Services;

public sealed class PublicReservationServiceTests
{
    [Fact]
    public async Task Create_CompletesReservation_AndStoresTokenHash()
    {
        await using var context =
            await PublicReservationTestContext.CreateAsync();
        var service = context.CreateReservationService();

        var response = await service.CreateAsync(CreateRequest(context));
        var stored = await context.DbContext.Reservations
            .AsNoTracking()
            .SingleAsync();

        response.Data.Should().NotBeNull();
        response.Data!.ConfirmationCode.Should().StartWith("RSV-");
        stored.PublicTrackingTokenHash
            .Should().NotBe(response.Data.TrackingToken);

        var confirmation = new ReservationConfirmationService();
        stored.PublicTrackingTokenHash.Should().Be(
            confirmation.HashTrackingToken(
                response.Data.TrackingToken));
    }

    [Fact]
    public async Task Create_RejectsSecondReservation_ForSameTableAndSlot()
    {
        await using var context =
            await PublicReservationTestContext.CreateAsync();
        var service = context.CreateReservationService();
        var request = CreateRequest(context);
        await service.CreateAsync(request);

        var action = () => service.CreateAsync(request);

        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("*no longer available*");
        (await context.DbContext.Reservations.CountAsync())
            .Should().Be(1);
    }

    [Fact]
    public async Task Create_RejectsTimeOutsideWorkingHours()
    {
        await using var context =
            await PublicReservationTestContext.CreateAsync();
        var service = context.CreateReservationService();
        var request = CreateRequest(
            context,
            startTime: new TimeOnly(23, 30));

        var action = () => service.CreateAsync(request);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Track_VerifiesHashedTrackingToken()
    {
        await using var context =
            await PublicReservationTestContext.CreateAsync();
        var service = context.CreateReservationService();
        var created = await service.CreateAsync(CreateRequest(context));

        var tracked = await service.TrackAsync(
            created.Data!.TrackingToken);

        tracked.Data!.ConfirmationCode
            .Should().Be(created.Data.ConfirmationCode);
        tracked.Data.MaskedPhone.Should().Contain("*");
    }

    [Fact]
    public async Task Lookup_ReturnsGenericError_ForWrongConfirmationCode()
    {
        await using var context =
            await PublicReservationTestContext.CreateAsync();
        var service = context.CreateReservationService();

        var action = () => service.LookupAsync(
            new PublicReservationLookupRequestDto
            {
                ConfirmationCode = "RSV-ABC123",
                Phone = "+994501234567"
            });

        var exception = await action.Should()
            .ThrowAsync<NotFoundException>();
        exception.Which.Message.Should().Be(
            "Reservation information could not be verified.");
    }

    [Fact]
    public async Task Cancel_ChangesEligibleReservationStatus()
    {
        await using var context =
            await PublicReservationTestContext.CreateAsync();
        var service = context.CreateReservationService();
        var created = await service.CreateAsync(CreateRequest(context));

        await service.CancelAsync(
            created.Data!.ConfirmationCode,
            new PublicCancelReservationDto
            {
                TrackingToken = created.Data.TrackingToken,
                Reason = "Plan changed"
            });

        var reservation = await context.DbContext.Reservations
            .AsNoTracking()
            .SingleAsync();
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
        reservation.CancelledAt.Should().NotBeNull();
        reservation.CancellationReason.Should().Be("Plan changed");
    }

    [Fact]
    public async Task Cancel_RejectsReservation_AfterCutoff()
    {
        await using var context =
            await PublicReservationTestContext.CreateAsync();
        var service = context.CreateReservationService();
        var request = CreateRequest(
            context,
            reservationDate: new DateOnly(2026, 7, 30),
            startTime: new TimeOnly(9, 30),
            durationMinutes: 60);
        var created = await service.CreateAsync(request);

        var action = () => service.CancelAsync(
            created.Data!.ConfirmationCode,
            new PublicCancelReservationDto
            {
                TrackingToken = created.Data.TrackingToken
            });

        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("*deadline*");
    }

    private static PublicCreateReservationDto CreateRequest(
        PublicReservationTestContext context,
        DateOnly? reservationDate = null,
        TimeOnly? startTime = null,
        int durationMinutes = 120)
    {
        return new PublicCreateReservationDto
        {
            BranchId = context.Branch.ID,
            TableId = context.Table.ID,
            ReservationDate =
                reservationDate ?? new DateOnly(2026, 8, 10),
            StartTime = startTime ?? new TimeOnly(19, 0),
            DurationMinutes = durationMinutes,
            GuestCount = 2,
            FullName = "Aydan Sharifova",
            Phone = "+994501234567",
            Email = "aydan@example.com",
            SpecialNotes = "<b>Window</b> seat",
            TermsAccepted = true
        };
    }
}
