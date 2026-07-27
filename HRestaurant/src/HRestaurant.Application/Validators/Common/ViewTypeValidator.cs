using FluentValidation;
using HRestaurant.Enum;

namespace HRestaurant.Validators.Common;

public sealed class ViewTypeValidator : AbstractValidator<ViewType>
{
    public ViewTypeValidator()
    {
        RuleFor(value => value).IsInEnum();
    }
}
