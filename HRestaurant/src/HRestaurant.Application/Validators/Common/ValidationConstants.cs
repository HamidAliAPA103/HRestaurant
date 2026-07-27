namespace HRestaurant.Validators.Common;

internal static class ValidationConstants
{
    public const int NameMaximumLength = 100;
    public const int PhoneMinimumLength = 7;
    public const int PhoneMaximumLength = 15;
    public const int DescriptionMaximumLength = 500;
    public const int CommentMaximumLength = 1000;
    public const int EmailMaximumLength = 254;
    public const int RoleMaximumLength = 50;
    public const int UrlMaximumLength = 2048;

    public const string PhonePattern = @"^\+?[0-9 ()-]+$";
}
