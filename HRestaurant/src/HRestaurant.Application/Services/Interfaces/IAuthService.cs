using HRestaurant.DTOS.Auth;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default);
}
