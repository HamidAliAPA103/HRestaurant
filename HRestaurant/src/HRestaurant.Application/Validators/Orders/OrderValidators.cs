using FluentValidation;
using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.Validators.Orders;

public sealed class OrderCreateDTOValidator : AbstractValidator<OrderCreatDTO>
{
    public OrderCreateDTOValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.OrderType).IsInEnum();
        RuleFor(x => x.TableId).NotEmpty()
            .When(x => x.OrderType == OrderType.DineIn)
            .WithMessage("A table is required for dine-in orders.");
        RuleFor(x => x.TableId).Null()
            .When(x => x.OrderType != OrderType.DineIn)
            .WithMessage("A table can be selected only for dine-in orders.");
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes is not null);
        RuleFor(x => x.Items).NotEmpty().Must(items =>
                items.Select(x => x.MenuItemId).Distinct().Count() == items.Count)
            .WithMessage("Duplicate menu items are not allowed in an order request.");
        RuleForEach(x => x.Items).SetValidator(new OrderItemCreateDTOValidator());
    }
}

public sealed class OrderUpdateDTOValidator : AbstractValidator<OrderUpdateDTO>
{
    public OrderUpdateDTOValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes is not null);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class OrderListRequestValidator : AbstractValidator<OrderListRequest>
{
    public OrderListRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(100).When(x => x.Search is not null);
        RuleFor(x => x.OrderType).IsInEnum().When(x => x.OrderType.HasValue);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
    }
}

public sealed class OrderStatusUpdateDTOValidator : AbstractValidator<OrderStatusUpdateDTO>
{
    public OrderStatusUpdateDTOValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class KitchenOrderStatusUpdateDTOValidator
    : AbstractValidator<KitchenOrderStatusUpdateDTO>
{
    public KitchenOrderStatusUpdateDTOValidator()
    {
        RuleFor(x => x.Status).Must(status =>
                status is OrderStatus.Confirmed or OrderStatus.Preparing or OrderStatus.Ready)
            .WithMessage("Kitchen status must be Confirmed, Preparing or Ready.");
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class OrderCancelDTOValidator : AbstractValidator<OrderCancelDTO>
{
    public OrderCancelDTOValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class OrderTableUpdateDTOValidator : AbstractValidator<OrderTableUpdateDTO>
{
    public OrderTableUpdateDTOValidator()
    {
        RuleFor(x => x.TableId).NotEmpty();
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class OrderDiscountDTOValidator : AbstractValidator<OrderDiscountDTO>
{
    public OrderDiscountDTOValidator()
    {
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class OrderMergeDTOValidator : AbstractValidator<OrderMergeDTO>
{
    public OrderMergeDTOValidator()
    {
        RuleFor(x => x.SourceOrderIds).NotEmpty().Must(ids => ids.Distinct().Count() == ids.Count);
        RuleForEach(x => x.SourceOrderIds).NotEmpty();
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class OrderSplitDTOValidator : AbstractValidator<OrderSplitDTO>
{
    public OrderSplitDTOValidator()
    {
        RuleFor(x => x.Items).NotEmpty().Must(items =>
            items.Select(x => x.OrderItemId).Distinct().Count() == items.Count);
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.OrderItemId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0);
        });
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class OrderConcurrencyDTOValidator : AbstractValidator<OrderConcurrencyDTO>
{
    public OrderConcurrencyDTOValidator() =>
        RuleFor(x => x.RowVersion).NotEmpty();
}
