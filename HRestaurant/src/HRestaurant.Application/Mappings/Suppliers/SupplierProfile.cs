using AutoMapper;
using HRestaurant.DTOS.Supplier;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Suppliers;

public sealed class SupplierProfile : Profile
{
    public SupplierProfile()
    {
        CreateMap<Supplier, SupplierGetDTO>();
        CreateMap<SupplierCreateDTO, Supplier>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.NormalizedName, o => o.Ignore())
            .ForMember(x => x.IsActive, o => o.MapFrom(_ => true))
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.InventoryItems, o => o.Ignore());
        CreateMap<SupplierUpdateDTO, Supplier>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.RestaurantId, o => o.Ignore())
            .ForMember(x => x.NormalizedName, o => o.Ignore())
            .ForMember(x => x.IsActive, o => o.Ignore())
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.InventoryItems, o => o.Ignore());
    }
}
