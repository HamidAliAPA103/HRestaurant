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

    Task<ApiResponse<CurrentUserResponse>> GetCurrentUserAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> RequestPasswordResetAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> ResendEmailVerificationAsync(
        ResendVerificationRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> VerifyEmailAsync(
        VerifyEmailRequest request,
        CancellationToken cancellationToken = default);
}
