using HRestaurant.Models;
using Microsoft.AspNetCore.Identity;

namespace HRestaurant.Infrastructure.Identity;

public sealed class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public Guid RestaurantId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Restaurant Restaurant { get; set; } = null!;

    public ICollection<RefreshToken> RefreshTokens { get; set; } =
        new List<RefreshToken>();
}
