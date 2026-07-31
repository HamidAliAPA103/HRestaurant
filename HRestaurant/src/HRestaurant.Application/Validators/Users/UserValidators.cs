using FluentValidation;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.User;
using HRestaurant.Validators.Common;

namespace HRestaurant.Validators.Users;

public sealed class UserCreateDTOValidator : AbstractValidator<UserCreateDTO>
{
    public UserCreateDTOValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress()
            .MaximumLength(ValidationConstants.EmailMaximumLength);
        RuleFor(x => x.Name).NotEmpty()
            .MaximumLength(ValidationConstants.NameMaximumLength);
        RuleFor(x => x.Phone).NotEmpty()
            .Matches(ValidationConstants.PhonePattern)
            .MaximumLength(ValidationConstants.PhoneMaximumLength);
        RuleFor(x => x.Role).Must(EmployeeRoleRules.IsAllowed)
            .WithMessage("Role must be Manager, Cashier, Waiter or Chef.");
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.HireDate).NotEmpty()
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.AvatarUrl).MaximumLength(500)
            .When(x => x.AvatarUrl is not null);
        RuleFor(x => x.EmergencyContact).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").Matches("[a-z]").Matches("[0-9]")
            .Matches("[^a-zA-Z0-9]");
    }
}

public sealed class UserUpdateDTOValidator : AbstractValidator<UserUpdateDTO>
{
    public UserUpdateDTOValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress()
            .MaximumLength(ValidationConstants.EmailMaximumLength)
            .When(x => x.Email is not null);
        RuleFor(x => x.Name).NotEmpty()
            .MaximumLength(ValidationConstants.NameMaximumLength)
            .When(x => x.Name is not null);
        RuleFor(x => x.Phone).NotEmpty()
            .Matches(ValidationConstants.PhonePattern)
            .MaximumLength(ValidationConstants.PhoneMaximumLength)
            .When(x => x.Phone is not null);
        RuleFor(x => x.Role).Must(EmployeeRoleRules.IsAllowed)
            .WithMessage("Role must be Manager, Cashier, Waiter or Chef.")
            .When(x => x.Role is not null);
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0)
            .When(x => x.Salary.HasValue);
        RuleFor(x => x.HireDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.HireDate.HasValue);
        RuleFor(x => x.AvatarUrl).MaximumLength(500)
            .When(x => x.AvatarUrl is not null);
        RuleFor(x => x.EmergencyContact).NotEmpty().MaximumLength(150)
            .When(x => x.EmergencyContact is not null);
    }
}

public sealed class EmployeeListRequestValidator
    : AbstractValidator<EmployeeListRequest>
{
    public EmployeeListRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(150)
            .When(x => x.Search is not null);
        RuleFor(x => x.Role).Must(EmployeeRoleRules.IsAllowed)
            .When(x => x.Role is not null);
        RuleFor(x => x.SortBy).Cascade(CascadeMode.Stop).NotEmpty()
            .Must(x => x.Equals("name", StringComparison.OrdinalIgnoreCase)
                || x.Equals("hireDate", StringComparison.OrdinalIgnoreCase));
        RuleFor(x => x.SortDirection).Cascade(CascadeMode.Stop).NotEmpty()
            .Must(x => x.Equals("asc", StringComparison.OrdinalIgnoreCase)
                || x.Equals("desc", StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class EmployeeBranchAssignmentDTOValidator
    : AbstractValidator<EmployeeBranchAssignmentDTO>
{
    public EmployeeBranchAssignmentDTOValidator() =>
        RuleFor(x => x.BranchId).NotEmpty();
}

public sealed class EmployeeRoleAssignmentDTOValidator
    : AbstractValidator<EmployeeRoleAssignmentDTO>
{
    public EmployeeRoleAssignmentDTOValidator() =>
        RuleFor(x => x.Role).Must(EmployeeRoleRules.IsAllowed)
            .WithMessage("Role must be Manager, Cashier, Waiter or Chef.");
}

internal static class EmployeeRoleRules
{
    private static readonly HashSet<string> Allowed = new(
        ["Manager", "Cashier", "Waiter", "Chef"],
        StringComparer.OrdinalIgnoreCase);

    public static bool IsAllowed(string? role) =>
        !string.IsNullOrWhiteSpace(role) && Allowed.Contains(role);
}
