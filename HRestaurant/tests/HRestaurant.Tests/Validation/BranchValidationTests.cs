using FluentAssertions;
using HRestaurant.DTOS.Branch;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Validators.Branches;
using HRestaurant.Validators.Restaurants;

namespace HRestaurant.Tests.Validation;

public sealed class BranchValidationTests
{
    [Fact]
    public async Task CreateValidator_AcceptsValidCoordinates()
    {
        var dto = new BranchCreateDTO
        {
            RestaurantId = Guid.NewGuid(),
            Name = "Baku Branch",
            Address = "Nizami Street 1",
            Phone = "+994501234567",
            Email = "branch@example.com",
            Latitude = 40.4093m,
            Longitude = 49.8671m,
            TimeZoneId = "Asia/Baku"
        };

        var result = await new BranchCreateDTOValidator()
            .ValidateAsync(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateValidator_RejectsDuplicateWorkingHourDays()
    {
        var dto = new BranchCreateDTO
        {
            RestaurantId = Guid.NewGuid(),
            Name = "Baku Branch",
            Address = "Nizami Street 1",
            TimeZoneId = "Asia/Baku",
            WorkingHours = Enumerable.Range(0, 7)
                .Select(_ => new BranchWorkingHourDTO
                {
                    DayOfWeek = DayOfWeek.Monday,
                    IsClosed = true
                })
                .ToList()
        };

        var result = await new BranchCreateDTOValidator()
            .ValidateAsync(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error =>
            error.ErrorMessage == "Each day can appear only once.");
    }

    [Fact]
    public async Task RestaurantListValidator_RejectsUnknownSortField()
    {
        var request = new RestaurantListRequest
        {
            SortBy = "taxRate"
        };

        var result = await new RestaurantListRequestValidator()
            .ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }
}
