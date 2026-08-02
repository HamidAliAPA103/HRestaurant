namespace HRestaurant.DTOS.Auth;

public sealed class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class VerifyEmailRequest
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
}

public sealed class ResendVerificationRequest
{
    public string Email { get; set; } = string.Empty;
}
