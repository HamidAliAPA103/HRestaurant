using AutoMapper;
using HRestaurant.DTOS.Inventory;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Inventory;

public sealed class InventoryProfile : Profile
{
    public InventoryProfile()
    {
        CreateMap<InventoryItem, InventoryItemGetDTO>()
            .ForMember(x => x.BranchName, o => o.MapFrom(x => x.Branch.Name))
            .ForMember(x => x.IngredientName, o => o.MapFrom(x => x.Ingredient.Name))
            .ForMember(x => x.SupplierName, o => o.MapFrom(x => x.Supplier == null ? null : x.Supplier.Name));
        CreateMap<InventoryItemCreateDTO, InventoryItem>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.IsActive, o => o.MapFrom(_ => true))
            .ForMember(x => x.RowVersion, o => o.Ignore())
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Branch, o => o.Ignore())
            .ForMember(x => x.Ingredient, o => o.Ignore())
            .ForMember(x => x.Supplier, o => o.Ignore())
            .ForMember(x => x.Transactions, o => o.Ignore())
            .ForMember(x => x.Notifications, o => o.Ignore());
        CreateMap<InventoryItemUpdateDTO, InventoryItem>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.RestaurantId, o => o.Ignore())
            .ForMember(x => x.BranchId, o => o.Ignore())
            .ForMember(x => x.IngredientId, o => o.Ignore())
            .ForMember(x => x.CurrentQuantity, o => o.Ignore())
            .ForMember(x => x.RowVersion, o => o.Ignore())
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Branch, o => o.Ignore())
            .ForMember(x => x.Ingredient, o => o.Ignore())
            .ForMember(x => x.Supplier, o => o.Ignore())
            .ForMember(x => x.Transactions, o => o.Ignore())
            .ForMember(x => x.Notifications, o => o.Ignore());
        CreateMap<StockTransaction, StockTransactionGetDTO>();
        CreateMap<InventoryNotification, InventoryNotificationGetDTO>()
            .ForMember(x => x.IngredientName,
                o => o.MapFrom(x => x.InventoryItem == null
                    ? null
                    : x.InventoryItem.Ingredient.Name));
    }
}
