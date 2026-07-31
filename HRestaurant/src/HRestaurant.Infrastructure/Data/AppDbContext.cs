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

    public DbSet<Shift> Shifts { get; set; }

    public DbSet<EmployeeShift> EmployeeShifts { get; set; }

    public DbSet<Ingredient> Ingredients { get; set; }

    public DbSet<MenuItemIngredient> MenuItemIngredients { get; set; }

    public DbSet<Supplier> Suppliers { get; set; }

    public DbSet<InventoryItem> InventoryItems { get; set; }

    public DbSet<StockTransaction> StockTransactions { get; set; }

    public DbSet<InventoryNotification> InventoryNotifications { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<Payment> Payments { get; set; }

    public DbSet<Refund> Refunds { get; set; }

    public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }

    public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

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

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);

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
