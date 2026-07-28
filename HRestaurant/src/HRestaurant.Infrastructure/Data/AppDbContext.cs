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

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<Reservation> Reservations { get; set; }

    public DbSet<Restaurant> Restaurants { get; set; }

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

        modelBuilder.Entity<MenuCategory>()
            .HasOne(category => category.Restaurant)
            .WithMany()
            .HasForeignKey(category => category.ResdaranId);

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
