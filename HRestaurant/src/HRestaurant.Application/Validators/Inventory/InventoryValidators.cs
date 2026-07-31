using FluentValidation;
using HRestaurant.DTOS.Inventory;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Validators.Inventory;

public sealed class InventoryItemCreateDTOValidator : AbstractValidator<InventoryItemCreateDTO>
{
    public InventoryItemCreateDTOValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.IngredientId).NotEmpty();
        RuleFor(x => x.CurrentQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Unit).IsInEnum();
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BatchNumber).MaximumLength(100).When(x => x.BatchNumber is not null);
    }
}

public sealed class InventoryItemUpdateDTOValidator : AbstractValidator<InventoryItemUpdateDTO>
{
    public InventoryItemUpdateDTOValidator()
    {
        RuleFor(x => x.MinimumQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Unit).IsInEnum();
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BatchNumber).MaximumLength(100).When(x => x.BatchNumber is not null);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class InventoryListRequestValidator : AbstractValidator<InventoryListRequest>
{
    public InventoryListRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(150).When(x => x.Search is not null);
    }
}

public sealed class StockMovementDTOValidator : AbstractValidator<StockMovementDTO>
{
    public StockMovementDTOValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.TransactionType).IsInEnum();
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ReferenceNumber).MaximumLength(100).When(x => x.ReferenceNumber is not null);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class StockAdjustmentDTOValidator : AbstractValidator<StockAdjustmentDTO>
{
    public StockAdjustmentDTOValidator()
    {
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ReferenceNumber).MaximumLength(100).When(x => x.ReferenceNumber is not null);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

public sealed class InventoryNotificationListRequestValidator
    : AbstractValidator<InventoryNotificationListRequest>
{
    public InventoryNotificationListRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x.Type).IsInEnum().When(x => x.Type.HasValue);
    }
}
