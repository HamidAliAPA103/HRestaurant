using FluentValidation;
using HRestaurant.DTOS.OrderItem;

namespace HRestaurant.Validators.Orders;

public sealed class OrderItemCreateDTOValidator : AbstractValidator<OrderItemCreatDTO>
{
    public OrderItemCreateDTOValidator()
    {
        RuleFor(x => x.MenuItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.KitchenNote).MaximumLength(300).When(x => x.KitchenNote is not null);
    }
}

public sealed class OrderItemUpdateDTOValidator : AbstractValidator<OrderItemUpdateDTO>
{
    public OrderItemUpdateDTOValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class OrderItemAddDTOValidator : AbstractValidator<OrderItemAddDTO>
{
    public OrderItemAddDTOValidator()
    {
        RuleFor(x => x.MenuItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.KitchenNote).MaximumLength(300).When(x => x.KitchenNote is not null);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class OrderItemKitchenNoteDTOValidator
    : AbstractValidator<OrderItemKitchenNoteDTO>
{
    public OrderItemKitchenNoteDTOValidator()
    {
        RuleFor(x => x.KitchenNote).MaximumLength(300).When(x => x.KitchenNote is not null);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}
