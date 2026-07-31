using FluentAssertions;
using HRestaurant.DTOS.Inventory;
using HRestaurant.DTOS.Supplier;
using HRestaurant.DTOS.Table;
using HRestaurant.Enum;
using HRestaurant.Validators.Inventory;
using HRestaurant.Validators.Suppliers;
using HRestaurant.Validators.Tables;

namespace HRestaurant.Tests.Validation;

public sealed class InventoryTableValidationTests
{
    [Fact]
    public void InventoryCreate_RejectsNegativeQuantity()
    {
        var dto = new InventoryItemCreateDTO
        {
            RestaurantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            IngredientId = Guid.NewGuid(),
            CurrentQuantity = -1,
            MinimumQuantity = 2,
            Unit = IngredientUnit.Gram,
            PurchasePrice = 3
        };

        new InventoryItemCreateDTOValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void StockMovement_RequiresPositiveQuantityAndRowVersion()
    {
        var dto = new StockMovementDTO
        {
            Quantity = 0,
            TransactionType = StockTransactionType.StockOut,
            Reason = "Usage"
        };

        var result = new StockMovementDTOValidator().Validate(dto);

        result.Errors.Select(x => x.PropertyName).Should().Contain([
            nameof(dto.Quantity), nameof(dto.RowVersion)
        ]);
    }

    [Fact]
    public void TableCreate_RejectsCapacityBelowOneAndInvalidDimensions()
    {
        var dto = new TableCreateDTO
        {
            RestaurantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            TableNumber = "T-1",
            Capacity = 0,
            Width = 0.1,
            Length = 0.1,
            Height = 0.1
        };

        new TableCreateDTOValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void BulkLayout_RejectsDuplicateTableIds()
    {
        var tableId = Guid.NewGuid();
        var dto = new TableLayoutBulkUpdateDTO
        {
            BranchId = Guid.NewGuid(),
            Tables =
            [
                Layout(tableId),
                Layout(tableId)
            ]
        };

        new TableLayoutBulkUpdateDTOValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void SupplierCreate_RejectsInvalidEmail()
    {
        var dto = new SupplierCreateDTO
        {
            RestaurantId = Guid.NewGuid(),
            Name = "Supplier",
            ContactPerson = "Contact",
            Phone = "+994501234567",
            Email = "invalid-email",
            Address = "Baku"
        };

        new SupplierCreateDTOValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    private static TableLayoutItemDTO Layout(Guid tableId) => new()
    {
        TableId = tableId,
        Width = 1.8,
        Length = 1.8
    };
}
