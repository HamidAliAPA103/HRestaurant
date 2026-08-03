using AutoMapper;
using FluentAssertions;
using HRestaurant.Data;
using HRestaurant.Enum;
using HRestaurant.Mappings.Public;
using HRestaurant.Models;
using HRestaurant.Services.Implementations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace HRestaurant.Tests.Services;

public sealed class PublicRestaurantExperienceServiceTests
{
    [Fact]
    public async Task ExperienceAndScene_ReturnOnlyPersistedBranchAndTableLayout()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var restaurantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        dbContext.Restaurants.Add(new Restaurant
        {
            ID = restaurantId,
            Name = "Tour Restaurant",
            Slug = "tour-restaurant",
            Adres = "Baku",
            Number = "+994501112244",
            IsActive = true
        });
        dbContext.Branches.Add(new Branch
        {
            ID = branchId,
            RestaurantId = restaurantId,
            Name = "Main Branch",
            NormalizedName = "MAIN BRANCH",
            Slug = "main",
            Address = "Baku",
            TimeZoneId = "Asia/Baku",
            IsActive = true
        });
        dbContext.Tables.Add(new Table
        {
            ID = tableId,
            RestaurantID = restaurantId,
            BranchId = branchId,
            TableNumber = "T-01",
            Tutum = 4,
            Shape = TableShape.Round,
            Status = TableStatus.Available,
            IsActive = true,
            PositionX = 2.5,
            PositionY = 0,
            PositionZ = -1.25,
            RotationY = 0.4,
            Width = 1.9,
            Length = 1.9,
            Height = 0.78
        });
        await dbContext.SaveChangesAsync();

        var mapperConfiguration = new MapperConfiguration(
            configuration => configuration.AddProfile<PublicReservationProfile>(),
            NullLoggerFactory.Instance);
        var service = new PublicRestaurantService(
            dbContext,
            mapperConfiguration.CreateMapper(),
            TimeProvider.System);

        var experience = await service.GetExperienceAsync("tour-restaurant");
        var scene = await service.GetSceneAsync("tour-restaurant");

        experience.Data.Should().NotBeNull();
        experience.Data!.DefaultBranchId.Should().Be(branchId);
        scene.Data.Should().NotBeNull();
        scene.Data!.Branches.Should().ContainSingle();
        var branchScene = scene.Data.Branches.Single();
        branchScene.Hotspots.Should().HaveCount(7);
        branchScene.Tables.Should().ContainSingle();
        branchScene.Tables.Single().Should().BeEquivalentTo(new
        {
            Id = tableId,
            TableNumber = "T-01",
            Capacity = 4,
            Status = "Available",
            PositionX = 2.5,
            PositionZ = -1.25,
            Width = 1.9,
            Height = 0.78
        });
        branchScene.Hotspots.SelectMany(hotspot => hotspot.TableIds)
            .Should().ContainSingle(id => id == tableId);
    }
}
