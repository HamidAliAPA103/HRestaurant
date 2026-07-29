using Microsoft.AspNetCore.Authorization;

namespace HRestaurant.Infrastructure.Authorization;

public sealed record PermissionRequirement(string Permission)
    : IAuthorizationRequirement;
