using FluentAssertions;
using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.OrderItem;
using HRestaurant.Enum;
using HRestaurant.Validators.Orders;

namespace HRestaurant.Tests.Validation;

public sealed class OrderValidationTests
{
    [Fact]
    public void DineInOrder_RequiresTableAndItems()
    {
        var dto = ValidOrder();
        dto.TableId = null;
        dto.Items = [];

        var result = new OrderCreateDTOValidator().Validate(dto);

        result.Errors.Select(x => x.PropertyName).Should().Contain([
            nameof(dto.TableId), nameof(dto.Items)
        ]);
    }

    [Fact]
    public void OrderCreate_RejectsDuplicateMenuItemsAndInvalidDiscount()
    {
        var dto = ValidOrder();
        dto.DiscountPercentage = 101;
        dto.Items.Add(new OrderItemCreatDTO
        {
            MenuItemId = dto.Items[0].MenuItemId,
            Quantity = 2
        });

        var result = new OrderCreateDTOValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.PropertyName).Should().Contain([
            nameof(dto.DiscountPercentage), nameof(dto.Items)
        ]);
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Preparing, true)]
    [InlineData(OrderStatus.Ready, true)]
    [InlineData(OrderStatus.Served, false)]
    [InlineData(OrderStatus.Cancelled, false)]
    public void KitchenStatus_AllowsOnlyKitchenWorkflowStates(
        OrderStatus status, bool expectedValid)
    {
        var dto = new KitchenOrderStatusUpdateDTO
        {
            Status = status,
            RowVersion = [1]
        };

        new KitchenOrderStatusUpdateDTOValidator().Validate(dto).IsValid
            .Should().Be(expectedValid);
    }

    [Fact]
    public void OrderItem_RequiresPositiveQuantityAndConcurrencyToken()
    {
        var dto = new OrderItemAddDTO
        {
            MenuItemId = Guid.NewGuid(),
            Quantity = 0,
            RowVersion = []
        };

        var result = new OrderItemAddDTOValidator().Validate(dto);

        result.Errors.Select(x => x.PropertyName).Should().Contain([
            nameof(dto.Quantity), nameof(dto.RowVersion)
        ]);
    }

    private static OrderCreatDTO ValidOrder() => new()
    {
        RestaurantId = Guid.NewGuid(),
        BranchId = Guid.NewGuid(),
        TableId = Guid.NewGuid(),
        OrderType = OrderType.DineIn,
        DiscountPercentage = 10,
        Items =
        [
            new OrderItemCreatDTO
            {
                MenuItemId = Guid.NewGuid(),
                Quantity = 1
            }
        ]
    };
}
