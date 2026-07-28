using FluentValidation;
using HRestaurant.DTOS.Auth;
using HRestaurant.Validators.Common;

namespace HRestaurant.Validators.Auth;

public sealed class RegisterRequestValidator
    : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.FullName)
            .NotEmpty()
            .WithMessage("Full name cannot be empty.")
            .MaximumLength(ValidationConstants.NameMaximumLength)
            .WithMessage(
                $"Full name cannot exceed {ValidationConstants.NameMaximumLength} characters.");

        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Email cannot be empty.")
            .EmailAddress()
            .WithMessage("Email format is invalid.")
            .MaximumLength(ValidationConstants.EmailMaximumLength)
            .WithMessage(
                $"Email cannot exceed {ValidationConstants.EmailMaximumLength} characters.");

        RuleFor(request => request.Password)
            .NotEmpty()
            .WithMessage("Password cannot be empty.")
            .MinimumLength(8)
            .WithMessage("Password must contain at least 8 characters.")
            .MaximumLength(128)
            .WithMessage("Password cannot exceed 128 characters.")
            .Matches("[A-Z]")
            .WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]")
            .WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]")
            .WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must contain a non-alphanumeric character.");

        RuleFor(request => request.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Password confirmation cannot be empty.")
            .Equal(request => request.Password)
            .WithMessage("Password confirmation does not match.");

        RuleFor(request => request.RestaurantId)
            .NotEmpty()
            .WithMessage("Restaurant id is required.");
    }
}

public sealed class LoginRequestValidator
    : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Email cannot be empty.")
            .EmailAddress()
            .WithMessage("Email format is invalid.")
            .MaximumLength(ValidationConstants.EmailMaximumLength)
            .WithMessage(
                $"Email cannot exceed {ValidationConstants.EmailMaximumLength} characters.");

        RuleFor(request => request.Password)
            .NotEmpty()
            .WithMessage("Password cannot be empty.")
            .MaximumLength(128)
            .WithMessage("Password cannot exceed 128 characters.");
    }
}

public sealed class RefreshTokenRequestValidator
    : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token cannot be empty.")
            .MaximumLength(512)
            .WithMessage("Refresh token is invalid.");
    }
}

public sealed class LogoutRequestValidator
    : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token cannot be empty.")
            .MaximumLength(512)
            .WithMessage("Refresh token is invalid.");
    }
}
