using Microsoft.AspNetCore.Identity;

namespace HRestaurant.Infrastructure.Identity;

public sealed class AppRole : IdentityRole<Guid>
{
    public DateTime CreatedAtUtc { get; set; }
}
