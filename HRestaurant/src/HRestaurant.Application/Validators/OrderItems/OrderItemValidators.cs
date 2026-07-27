using FluentValidation;
using HRestaurant.DTOS.OrderItem;

namespace HRestaurant.Validators.OrderItems;

public sealed class OrderItemCreateDTOValidator
    : AbstractValidator<OrderItemCreatDTO>
{
    public OrderItemCreateDTOValidator()
    {
        RuleFor(dto => dto.MenuId)
            .NotEmpty()
            .WithMessage("Menu item id is required.");

        RuleFor(dto => dto.Say)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Order item quantity must be at least 1.");

        RuleFor(dto => dto.Prices)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero.");
    }
}

public sealed class OrderItemUpdateDTOValidator
    : AbstractValidator<OrderItemUpdateDTO>
{
    public OrderItemUpdateDTOValidator()
    {
        RuleFor(dto => dto.Say)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Order item quantity must be at least 1.");

        RuleFor(dto => dto.Prices)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero.");
    }
}
