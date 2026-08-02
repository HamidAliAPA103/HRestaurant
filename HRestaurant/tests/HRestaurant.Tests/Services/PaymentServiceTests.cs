using FluentAssertions;
using HRestaurant.Data;
using HRestaurant.DTOS.Payment;
using HRestaurant.Enum;
using HRestaurant.Infrastructure.Identity;
using HRestaurant.Models;
using HRestaurant.Services.Implementations;
using HRestaurant.Services.Interfaces;
using HRestaurant.Tests.TestSupport;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HRestaurant.Tests.Services;

public sealed class PaymentServiceTests
{
    [Fact]
    public async Task Split_TracksEachPaymentOnce_AndReturnsPersistedTotals()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new SqlitePaymentDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var now = new DateTime(2026, 8, 2, 8, 0, 0, DateTimeKind.Utc);
        var restaurant = new Restaurant
        {
            ID = Guid.NewGuid(),
            Name = "Payment Test Restaurant",
            Slug = "payment-test-restaurant",
            Adres = "Baku",
            Number = "+994501234567",
            IsActive = true,
            CreatAt = now
        };
        var branch = new Branch
        {
            ID = Guid.NewGuid(),
            RestaurantId = restaurant.ID,
            Restaurant = restaurant,
            Name = "Main",
            NormalizedName = "MAIN",
            Slug = "main",
            Address = "Baku",
            TimeZoneId = "UTC",
            IsActive = true,
            CreatAt = now
        };
        var userId = Guid.NewGuid();
        var appUser = new AppUser
        {
            Id = userId,
            UserName = "payment.test@hrestaurant.az",
            NormalizedUserName = "PAYMENT.TEST@HRESTAURANT.AZ",
            Email = "payment.test@hrestaurant.az",
            NormalizedEmail = "PAYMENT.TEST@HRESTAURANT.AZ",
            FullName = "Payment Test User",
            RestaurantId = restaurant.ID,
            Restaurant = restaurant,
            CreatedAtUtc = now
        };
        var order = new Order
        {
            ID = Guid.NewGuid(),
            RestaurantId = restaurant.ID,
            Restaurant = restaurant,
            BranchId = branch.ID,
            Branch = branch,
            OrderNumber = "ORD-PAYMENT-TEST",
            Status = OrderStatus.Ready,
            Subtotal = 37m,
            TotalAmount = 37m,
            CreatAt = now
        };
        db.AddRange(restaurant, branch, appUser, order);
        await db.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserContext>();
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        currentUser.SetupGet(x => x.RestaurantId).Returns(restaurant.ID);
        currentUser.SetupGet(x => x.IsSuperAdmin).Returns(false);
        currentUser.SetupGet(x => x.IsManager).Returns(false);
        currentUser.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(false);
        var loyalty = new Mock<ILoyaltyService>();
        loyalty.Setup(x => x.RedeemForPaymentAsync(
                It.IsAny<Order>(), It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        loyalty.Setup(x => x.EarnForFullyPaidOrderAsync(
                It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(now));
        var service = new PaymentService(db, currentUser.Object, loyalty.Object, timeProvider);

        var response = await service.SplitAsync(new SplitPaymentDTO
        {
            OrderId = order.ID,
            OrderRowVersion = order.RowVersion,
            Payments =
            [
                new SplitPaymentItemDTO
                {
                    PaymentMethod = PaymentMethod.Cash,
                    Amount = 18.50m
                },
                new SplitPaymentItemDTO
                {
                    PaymentMethod = PaymentMethod.Card,
                    Amount = 18.50m,
                    TransactionReference = "PAYMENT-TEST-001"
                }
            ]
        });

        response.Data.Should().NotBeNull();
        response.Data!.Payments.Should().HaveCount(2);
        response.Data.PaidAmount.Should().Be(37m);
        response.Data.RemainingAmount.Should().Be(0m);
        (await db.Payments.CountAsync()).Should().Be(2);
        var persistedOrder = await db.Orders.AsNoTracking().SingleAsync(x => x.ID == order.ID);
        persistedOrder.PaidAmount.Should().Be(37m);
        persistedOrder.IsPaid.Should().BeTrue();
    }

    private sealed class SqlitePaymentDbContext(DbContextOptions<AppDbContext> options)
        : AppDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>().Property(x => x.RowVersion)
                .ValueGeneratedNever();
            modelBuilder.Entity<Payment>().Property(x => x.RowVersion)
                .ValueGeneratedNever();
        }
    }
}
