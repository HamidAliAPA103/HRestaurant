using HRestaurant.DTOS.Auth;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        ArgumentNullException.ThrowIfNull(authService);
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _authService.RegisterAsync(
                request,
                cancellationToken));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _authService.LoginAsync(
                request,
                cancellationToken));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _authService.RefreshTokenAsync(
                request,
                cancellationToken));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        LogoutRequest request,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _authService.LogoutAsync(
                request,
                cancellationToken));
    }
}
