using FluentValidation;
using HRestaurant.DTOS.User;
using HRestaurant.Validators.Common;

namespace HRestaurant.Validators.Users;

public sealed class UserCreateDTOValidator
    : AbstractValidator<UserCreateDTO>
{
    public UserCreateDTOValidator()
    {
        RuleFor(dto => dto.Email)
            .NotEmpty()
            .WithMessage("Email cannot be empty.")
            .EmailAddress()
            .WithMessage("Email format is invalid.")
            .MaximumLength(ValidationConstants.EmailMaximumLength)
            .WithMessage(
                $"Email cannot exceed {ValidationConstants.EmailMaximumLength} characters.");

        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithMessage("User name cannot be empty.")
            .MaximumLength(ValidationConstants.NameMaximumLength)
            .WithMessage(
                $"User name cannot exceed {ValidationConstants.NameMaximumLength} characters.");

        RuleFor(dto => dto.Role)
            .NotEmpty()
            .WithMessage("Role cannot be empty.")
            .MaximumLength(ValidationConstants.RoleMaximumLength)
            .WithMessage(
                $"Role cannot exceed {ValidationConstants.RoleMaximumLength} characters.");
    }
}

public sealed class UserUpdateDTOValidator
    : AbstractValidator<UserUpdateDTO>
{
    public UserUpdateDTOValidator()
    {
        RuleFor(dto => dto.Email)
            .NotEmpty()
            .WithMessage("Email cannot be empty.")
            .EmailAddress()
            .WithMessage("Email format is invalid.")
            .MaximumLength(ValidationConstants.EmailMaximumLength)
            .WithMessage(
                $"Email cannot exceed {ValidationConstants.EmailMaximumLength} characters.")
            .When(dto => dto.Email is not null);

        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithMessage("User name cannot be empty.")
            .MaximumLength(ValidationConstants.NameMaximumLength)
            .WithMessage(
                $"User name cannot exceed {ValidationConstants.NameMaximumLength} characters.")
            .When(dto => dto.Name is not null);

        RuleFor(dto => dto.Role)
            .NotEmpty()
            .WithMessage("Role cannot be empty.")
            .MaximumLength(ValidationConstants.RoleMaximumLength)
            .WithMessage(
                $"Role cannot exceed {ValidationConstants.RoleMaximumLength} characters.")
            .When(dto => dto.Role is not null);
    }
}
