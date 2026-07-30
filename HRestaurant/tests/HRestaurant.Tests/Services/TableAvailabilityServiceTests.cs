using FluentAssertions;
using HRestaurant.DTOS.Public;
using HRestaurant.Enum;
using HRestaurant.Tests.TestSupport;

namespace HRestaurant.Tests.Services;

public sealed class TableAvailabilityServiceTests
{
    [Fact]
    public async Task GetTables_ReturnsAvailableTable_WhenSlotIsEmpty()
    {
        await using var context =
            await PublicReservationTestContext.CreateAsync();

        var response = await context
            .CreateAvailabilityService()
            .GetTablesAsync(
                context.Branch.ID,
                CreateRequest());

        response.Data.Should().ContainSingle();
        response.Data!.Single().IsAvailable.Should().BeTrue();
        response.Data!.Single().UnavailableReason.Should().BeNull();
    }

    [Fact]
    public async Task GetTables_ReturnsReserved_WhenIntervalsOverlap()
    {
        await using var context =
            await PublicReservationTestContext.CreateAsync();
        context.DbContext.Reservations.Add(
            context.CreateReservation(
                ReservationStatus.Confirmed,
                new DateTime(2026, 8, 10, 18, 30, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 10, 20, 30, 0, DateTimeKind.Utc)));
        await context.DbContext.SaveChangesAsync();

        var response = await context
            .CreateAvailabilityService()
            .GetTablesAsync(
                context.Branch.ID,
                CreateRequest());

        response.Data!.Single().IsAvailable.Should().BeFalse();
        response.Data!.Single().UnavailableReason.Should().Be("Reserved");
    }

    [Fact]
    public async Task GetTables_DoesNotBlockSlot_ForCancelledReservation()
    {
        await using var context =
            await PublicReservationTestContext.CreateAsync();
        context.DbContext.Reservations.Add(
            context.CreateReservation(
                ReservationStatus.Cancelled,
                new DateTime(2026, 8, 10, 18, 30, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 10, 20, 30, 0, DateTimeKind.Utc)));
        await context.DbContext.SaveChangesAsync();

        var response = await context
            .CreateAvailabilityService()
            .GetTablesAsync(
                context.Branch.ID,
                CreateRequest());

        response.Data!.Single().IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task GetTables_ReturnsCapacityReason_WhenTableIsTooSmall()
    {
        await using var context =
            await PublicReservationTestContext.CreateAsync();

        var response = await context
            .CreateAvailabilityService()
            .GetTablesAsync(
                context.Branch.ID,
                CreateRequest(guestCount: 6));

        response.Data!.Single().IsAvailable.Should().BeFalse();
        response.Data!.Single().UnavailableReason
            .Should().Be("CapacityNotSuitable");
    }

    private static TableAvailabilityRequestDto CreateRequest(
        int guestCount = 2)
    {
        return new TableAvailabilityRequestDto
        {
            ReservationDate = new DateOnly(2026, 8, 10),
            StartTime = new TimeOnly(19, 0),
            DurationMinutes = 120,
            GuestCount = guestCount
        };
    }
}
