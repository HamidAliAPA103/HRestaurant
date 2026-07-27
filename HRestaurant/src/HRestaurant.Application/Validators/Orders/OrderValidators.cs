using FluentValidation;
using HRestaurant.DTOS.Order;

namespace HRestaurant.Validators.Orders;

public sealed class OrderCreateDTOValidator
    : AbstractValidator<OrderCreatDTO>
{
    public OrderCreateDTOValidator()
    {
        RuleFor(dto => dto.CustomerID)
            .NotEmpty()
            .WithMessage("Customer id is required.");

        RuleFor(dto => dto.TableID)
            .NotEqual(Guid.Empty)
            .WithMessage("Table id is invalid.")
            .When(dto => dto.TableID.HasValue);

        RuleFor(dto => dto.Items)
            .NotNull()
            .WithMessage("Order items are required.")
            .NotEmpty()
            .WithMessage("An order must contain at least one item.");

        RuleForEach(dto => dto.Items)
            .ChildRules(item =>
            {
                item.RuleFor(value => value.MenuId)
                    .NotEmpty()
                    .WithMessage("Menu item id is required.");

                item.RuleFor(value => value.Say)
                    .GreaterThanOrEqualTo(1)
                    .WithMessage("Order item quantity must be at least 1.");
            });
    }
}

public sealed class OrderUpdateDTOValidator
    : AbstractValidator<OrderUpdateDTO>
{
    public OrderUpdateDTOValidator()
    {
        RuleFor(dto => dto.TableID)
            .NotEqual(Guid.Empty)
            .WithMessage("Table id is invalid.")
            .When(dto => dto.TableID.HasValue);

        RuleFor(dto => dto.Status)
            .IsInEnum()
            .WithMessage("Order status is invalid.");
    }
}
