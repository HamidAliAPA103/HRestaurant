using HRestaurant.Infrastructure.Identity;
using HRestaurant.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Data;

public class AppDbContext
    : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Menu> Menus { get; set; }

    public DbSet<MenuCategory> MenuCategories { get; set; }

    public DbSet<Branch> Branches { get; set; }

    public DbSet<BranchWorkingHour> BranchWorkingHours { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<Reservation> Reservations { get; set; }

    public DbSet<ReservationAuditLog> ReservationAuditLogs { get; set; }

    public DbSet<Restaurant> Restaurants { get; set; }

    public DbSet<RestaurantWorkingHour> RestaurantWorkingHours { get; set; }

    public DbSet<Review> Reviews { get; set; }

    public DbSet<Table> Tables { get; set; }

    public DbSet<User> BusinessUsers { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Menu>()
            .Property(menu => menu.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(order => order.TotalPrices)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(orderItem => orderItem.Prices)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.Property(restaurant => restaurant.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(restaurant => restaurant.Adres)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(restaurant => restaurant.Slug)
                .HasMaxLength(120)
                .IsUnicode(false)
                .IsRequired();

            entity.HasIndex(restaurant => restaurant.Slug)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.Property(restaurant => restaurant.Number)
                .HasMaxLength(15)
                .IsRequired();

            entity.Property(restaurant => restaurant.Email)
                .HasMaxLength(254);

            entity.Property(restaurant => restaurant.Description)
                .HasMaxLength(2000);

            entity.Property(restaurant => restaurant.LogoUrl)
                .HasMaxLength(500);

            entity.Property(restaurant => restaurant.CoverImageUrl)
                .HasMaxLength(500);

            entity.Property(restaurant => restaurant.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(restaurant => restaurant.TaxRate)
                .HasPrecision(5, 2);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches");

            entity.Property(branch => branch.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(branch => branch.Slug)
                .HasMaxLength(120)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(branch => branch.Address)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(branch => branch.Phone)
                .HasMaxLength(20);

            entity.Property(branch => branch.Email)
                .HasMaxLength(254);

            entity.Property(branch => branch.TimeZoneId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            entity.HasIndex(branch => new
            {
                branch.RestaurantId,
                branch.Slug
            })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.HasOne(branch => branch.Restaurant)
                .WithMany(restaurant => restaurant.Branches)
                .HasForeignKey(branch => branch.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BranchWorkingHour>(entity =>
        {
            entity.ToTable("BranchWorkingHours");

            entity.HasIndex(workingHour => new
            {
                workingHour.BranchId,
                workingHour.DayOfWeek
            })
                .IsUnique();

            entity.Property(workingHour => workingHour.OpensAt)
                .HasColumnType("time");

            entity.Property(workingHour => workingHour.ClosesAt)
                .HasColumnType("time");

            entity.HasOne(workingHour => workingHour.Branch)
                .WithMany(branch => branch.WorkingHours)
                .HasForeignKey(workingHour => workingHour.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RestaurantWorkingHour>(entity =>
        {
            entity.ToTable("RestaurantWorkingHours");

            entity.HasIndex(workingHour => new
            {
                workingHour.RestaurantId,
                workingHour.DayOfWeek
            })
                .IsUnique();

            entity.Property(workingHour => workingHour.OpensAt)
                .HasColumnType("time");

            entity.Property(workingHour => workingHour.ClosesAt)
                .HasColumnType("time");

            entity.HasOne(workingHour => workingHour.Restaurant)
                .WithMany(restaurant => restaurant.WorkingHours)
                .HasForeignKey(workingHour => workingHour.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MenuCategory>()
            .HasOne(category => category.Restaurant)
            .WithMany()
            .HasForeignKey(category => category.ResdaranId);

        modelBuilder.Entity<Table>(entity =>
        {
            entity.Property(table => table.TableNumber)
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(table => new
            {
                table.BranchId,
                table.TableNumber
            })
                .IsUnique()
                .HasFilter(
                    "[BranchId] IS NOT NULL AND [IsDeleted] = 0");

            entity.HasOne(table => table.Restaurant)
                .WithMany(restaurant => restaurant.Tables)
                .HasForeignKey(table => table.RestaurantID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(table => table.Branch)
                .WithMany(branch => branch.Tables)
                .HasForeignKey(table => table.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.Property(reservation => reservation.FullName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(reservation => reservation.PhoneNormalized)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(reservation => reservation.Email)
                .HasMaxLength(254);

            entity.Property(reservation => reservation.SpecialNotes)
                .HasMaxLength(500);

            entity.Property(reservation => reservation.ConfirmationCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(
                    reservation =>
                        reservation.PublicTrackingTokenHash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(reservation => reservation.CancellationReason)
                .HasMaxLength(300);

            entity.HasIndex(reservation =>
                    reservation.ConfirmationCode)
                .IsUnique();

            entity.HasIndex(reservation =>
                    reservation.PublicTrackingTokenHash)
                .IsUnique();

            entity.HasIndex(reservation => new
            {
                reservation.BranchId,
                reservation.ReservationTime
            });

            entity.HasIndex(reservation => new
            {
                reservation.TableId,
                reservation.ReservationTime,
                reservation.EndTime
            });

            entity.HasOne(reservation => reservation.Table)
                .WithMany(table => table.Reservations)
                .HasForeignKey(reservation => reservation.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(reservation => reservation.Branch)
                .WithMany(branch => branch.Reservations)
                .HasForeignKey(reservation => reservation.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(reservation => reservation.Customer)
                .WithMany(customer => customer.Reservations)
                .HasForeignKey(reservation => reservation.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReservationAuditLog>(entity =>
        {
            entity.ToTable("ReservationAuditLogs");

            entity.Property(audit => audit.Action)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(audit => audit.Reason)
                .HasMaxLength(300);

            entity.Property(audit => audit.IpAddressHash)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.HasIndex(audit => audit.ReservationId);

            entity.HasOne(audit => audit.Reservation)
                .WithMany(reservation => reservation.AuditLogs)
                .HasForeignKey(audit => audit.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>()
            .ToTable("Users");

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.Property(user => user.FullName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(user => user.CreatedAtUtc)
                .IsRequired();

            entity.HasOne(user => user.Restaurant)
                .WithMany()
                .HasForeignKey(user => user.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppRole>()
            .Property(role => role.CreatedAtUtc)
            .IsRequired();

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");

            entity.HasKey(token => token.Id);

            entity.Property(token => token.TokenHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(token => token.TokenHash)
                .IsUnique();

            entity.Property(token => token.ReplacedByTokenHash)
                .HasMaxLength(64);

            entity.Property(token => token.RevocationReason)
                .HasMaxLength(200);

            entity.Property(token => token.RowVersion)
                .IsRowVersion();

            entity.HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
