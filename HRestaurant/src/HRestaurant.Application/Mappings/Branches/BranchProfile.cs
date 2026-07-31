using AutoMapper;
using HRestaurant.DTOS.Branch;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Branches;

public sealed class BranchProfile : Profile
{
    public BranchProfile()
    {
        CreateMap<BranchWorkingHour, BranchWorkingHourDTO>();

        CreateMap<BranchWorkingHourDTO, BranchWorkingHour>()
            .IgnoreBaseEntityMembers()
            .ForMember(
                destination => destination.BranchId,
                options => options.Ignore())
            .ForMember(
                destination => destination.Branch,
                options => options.Ignore());

        CreateMap<Branch, BranchGetDTO>()
            .ForMember(
                destination => destination.RestaurantName,
                options => options.MapFrom(source =>
                    source.Restaurant.Name))
            .ForMember(
                destination => destination.ManagerName,
                options => options.Ignore())
            .ForMember(
                destination => destination.ManagerEmail,
                options => options.Ignore());

        CreateMap<BranchCreateDTO, Branch>()
            .IgnoreBaseEntityMembers()
            .ForMember(
                destination => destination.NormalizedName,
                options => options.Ignore())
            .ForMember(
                destination => destination.ManagerId,
                options => options.Ignore())
            .ForMember(
                destination => destination.IsActive,
                options => options.MapFrom(_ => true))
            .ForMember(
                destination => destination.Restaurant,
                options => options.Ignore())
            .ForMember(
                destination => destination.Tables,
                options => options.Ignore())
            .ForMember(
                destination => destination.Reservations,
                options => options.Ignore());

        CreateMap<BranchUpdateDTO, Branch>()
            .IgnoreBaseEntityMembers()
            .ForMember(
                destination => destination.RestaurantId,
                options => options.Ignore())
            .ForMember(
                destination => destination.NormalizedName,
                options => options.Ignore())
            .ForMember(
                destination => destination.Slug,
                options => options.Ignore())
            .ForMember(
                destination => destination.ManagerId,
                options => options.Ignore())
            .ForMember(
                destination => destination.IsActive,
                options => options.Ignore())
            .ForMember(
                destination => destination.Restaurant,
                options => options.Ignore())
            .ForMember(
                destination => destination.WorkingHours,
                options => options.Ignore())
            .ForMember(
                destination => destination.Tables,
                options => options.Ignore())
            .ForMember(
                destination => destination.Reservations,
                options => options.Ignore());
    }
}
