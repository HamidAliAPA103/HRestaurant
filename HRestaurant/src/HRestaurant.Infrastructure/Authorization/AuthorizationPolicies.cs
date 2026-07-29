namespace HRestaurant.Infrastructure.Authorization;

public static class AuthorizationPolicies
{
    public const string EmployeeManagement =
        nameof(EmployeeManagement);

    public const string PaymentProcessing =
        nameof(PaymentProcessing);
}
