using AutoMapper;
using HRestaurant.DTOS.Table;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Tables;

public sealed class TableProfile : Profile
{
    public TableProfile()
    {
        CreateMap<Table, TableGetDTO>()
            .ForMember(x => x.RestaurantId, o => o.MapFrom(x => x.RestaurantID))
            .ForMember(x => x.BranchId, o => o.MapFrom(x => x.BranchId ?? Guid.Empty))
            .ForMember(x => x.BranchName, o => o.MapFrom(x => x.Branch == null ? string.Empty : x.Branch.Name))
            .ForMember(x => x.Capacity, o => o.MapFrom(x => x.Tutum))
            .ForMember(x => x.PositionX, o => o.MapFrom(x => x.PositionX ?? 0))
            .ForMember(x => x.PositionY, o => o.MapFrom(x => x.PositionY ?? 0))
            .ForMember(x => x.PositionZ, o => o.MapFrom(x => x.PositionZ ?? 0))
            .ForMember(x => x.RotationX, o => o.MapFrom(x => x.RotationX ?? 0))
            .ForMember(x => x.RotationY, o => o.MapFrom(x => x.RotationY ?? 0))
            .ForMember(x => x.RotationZ, o => o.MapFrom(x => x.RotationZ ?? 0));

        CreateMap<TableCreateDTO, Table>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.RestaurantID, o => o.MapFrom(x => x.RestaurantId))
            .ForMember(x => x.Tutum, o => o.MapFrom(x => x.Capacity))
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Branch, o => o.Ignore())
            .ForMember(x => x.Orders, o => o.Ignore())
            .ForMember(x => x.Reservations, o => o.Ignore());

        CreateMap<TableUpdateDTO, Table>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.RestaurantID, o => o.Ignore())
            .ForMember(x => x.BranchId, o => o.Ignore())
            .ForMember(x => x.Tutum, o => o.MapFrom(x => x.Capacity))
            .ForMember(x => x.Status, o => o.Ignore())
            .ForMember(x => x.IsActive, o => o.Ignore())
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Branch, o => o.Ignore())
            .ForMember(x => x.Orders, o => o.Ignore())
            .ForMember(x => x.Reservations, o => o.Ignore());
    }
}
