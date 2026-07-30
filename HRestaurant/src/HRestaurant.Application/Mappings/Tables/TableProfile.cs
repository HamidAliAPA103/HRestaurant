using AutoMapper;
using HRestaurant.DTOS.Table;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Tables;

public sealed class TableProfile : Profile
{
    public TableProfile()
    {
        CreateMap<Table, TableGetDTO>();

        CreateMap<TableCreateDTO, Table>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Restaurant, options => options.Ignore())
            .ForMember(destination => destination.Branch, options => options.Ignore())
            .ForMember(destination => destination.Orders, options => options.Ignore())
            .ForMember(destination => destination.Reservations, options => options.Ignore());

        CreateMap<TableUpdateDTO, Table>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Restaurant, options => options.Ignore())
            .ForMember(destination => destination.Branch, options => options.Ignore())
            .ForMember(destination => destination.Orders, options => options.Ignore())
            .ForMember(destination => destination.Reservations, options => options.Ignore());
    }
}
