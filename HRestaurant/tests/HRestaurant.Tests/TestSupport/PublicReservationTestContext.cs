using AutoMapper;
using HRestaurant.Configuration;
using HRestaurant.Data;
using HRestaurant.Enum;
using HRestaurant.Mappings.Public;
using HRestaurant.Models;
using HRestaurant.Services.Implementations;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HRestaurant.Tests.TestSupport;

internal sealed class PublicReservationTestContext : IAsyncDisposable
{
    private PublicReservationTestContext(
        SqliteConnection connection,
        AppDbContext dbContext,
        IMapper mapper,
        PublicReservationSettings settings,
        FixedTimeProvider timeProvider,
        Branch branch,
        Table table)
    {
        Connection = connection;
        DbContext = dbContext;
        Mapper = mapper;
        Settings = settings;
        TimeProvider = timeProvider;
        Branch = branch;
        Table = table;
    }

    public SqliteConnection Connection { get; }

    public AppDbContext DbContext { get; }

    public IMapper Mapper { get; }

    public PublicReservationSettings Settings { get; }

    public FixedTimeProvider TimeProvider { get; }

    public Branch Branch { get; }

    public Table Table { get; }

    public static async Task<PublicReservationTestContext> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(
                2026,
                7,
                30,
                8,
                0,
                0,
                TimeSpan.Zero));
        var settings = new PublicReservationSettings
        {
            BufferMinutes = 15,
            CancellationCutoffMinutes = 120,
            PublicBaseUrl = "https://restaurant.example"
        };
        var mapperConfig = new MapperConfiguration(
            configuration =>
                configuration.AddProfile<PublicReservationProfile>(),
            NullLoggerFactory.Instance);
        mapperConfig.AssertConfigurationIsValid();
        var mapper = mapperConfig.CreateMapper();

        var restaurant = new Restaurant
        {
            ID = Guid.NewGuid(),
            Name = "Test Restaurant",
            Slug = "test-restaurant",
            Adres = "Baku",
            Number = "+994501234567",
            IsActive = true,
            CreatAt = timeProvider.GetUtcNow().UtcDateTime
        };
        var branch = new Branch
        {
            ID = Guid.NewGuid(),
            RestaurantId = restaurant.ID,
            Restaurant = restaurant,
            Name = "Main",
            Slug = "main",
            Address = "Baku",
            TimeZoneId = "UTC",
            IsActive = true,
            CreatAt = timeProvider.GetUtcNow().UtcDateTime
        };
        branch.WorkingHours = System.Enum
            .GetValues<DayOfWeek>()
            .Select(day => new BranchWorkingHour
            {
                ID = Guid.NewGuid(),
                BranchId = branch.ID,
                DayOfWeek = day,
                OpensAt = new TimeOnly(8, 0),
                ClosesAt = new TimeOnly(23, 0),
                IsClosed = false,
                CreatAt = timeProvider.GetUtcNow().UtcDateTime
            })
            .ToList();
        var table = new Table
        {
            ID = Guid.NewGuid(),
            RestaurantID = restaurant.ID,
            BranchId = branch.ID,
            TableNumber = "T-1",
            Tutum = 4,
            Shape = TableShape.Round,
            Status = TableStatus.Available,
            IsActive = true,
            Width = 1.8,
            Length = 1.8,
            CreatAt = timeProvider.GetUtcNow().UtcDateTime
        };

        dbContext.AddRange(restaurant, branch, table);
        await dbContext.SaveChangesAsync();

        return new PublicReservationTestContext(
            connection,
            dbContext,
            mapper,
            settings,
            timeProvider,
            branch,
            table);
    }

    public TableAvailabilityService CreateAvailabilityService()
    {
        return new TableAvailabilityService(
            DbContext,
            Mapper,
            Settings,
            TimeProvider);
    }

    public PublicReservationService CreateReservationService(
        Mock<IReservationEmailQueue>? emailQueue = null)
    {
        var challenge = new Mock<IPublicRequestChallengeValidator>();
        challenge
            .Setup(service => service.EnsureValidAsync(
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new PublicReservationService(
            DbContext,
            CreateAvailabilityService(),
            new ReservationConfirmationService(),
            (emailQueue ?? CreateEmailQueueMock()).Object,
            challenge.Object,
            Settings,
            TimeProvider,
            new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            },
            NullLogger<PublicReservationService>.Instance);
    }

    public Reservation CreateReservation(
        ReservationStatus status,
        DateTime startUtc,
        DateTime endUtc)
    {
        return new Reservation
        {
            ID = Guid.NewGuid(),
            BranchId = Branch.ID,
            TableId = Table.ID,
            ReservationTime = startUtc,
            EndTime = endUtc,
            DurationMinutes = (int)(endUtc - startUtc).TotalMinutes,
            GuestCount = 2,
            FullName = "Test Guest",
            PhoneNormalized = "+994501234567",
            ConfirmationCode =
                $"RSV-{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
            PublicTrackingTokenHash =
                Convert.ToHexString(
                    System.Security.Cryptography.SHA256.HashData(
                        Guid.NewGuid().ToByteArray()))
                    .ToLowerInvariant(),
            Status = status,
            CreatAt = TimeProvider.GetUtcNow().UtcDateTime
        };
    }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await Connection.DisposeAsync();
    }

    private static Mock<IReservationEmailQueue> CreateEmailQueueMock()
    {
        var mock = new Mock<IReservationEmailQueue>();
        mock.Setup(queue => queue.QueueAsync(
                It.IsAny<ReservationEmailMessage>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        return mock;
    }
}
