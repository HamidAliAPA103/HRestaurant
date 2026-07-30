using AutoMapper;
using HRestaurant.DTOS.Public;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Public;

public sealed class PublicReservationProfile : Profile
{
    public PublicReservationProfile()
    {
        CreateMap<Restaurant, PublicRestaurantDto>()
            .ForMember(
                destination => destination.Id,
                options => options.MapFrom(source => source.ID))
            .ForMember(
                destination => destination.Address,
                options => options.MapFrom(source => source.Adres))
            .ForMember(
                destination => destination.Phone,
                options => options.MapFrom(source => source.Number))
            .ForMember(
                destination => destination.IsOpenNow,
                options => options.Ignore())
            .ForMember(
                destination => destination.WorkingHours,
                options => options.Ignore())
            .ForMember(
                destination => destination.Branches,
                options => options.Ignore());

        CreateMap<Branch, PublicBranchDto>()
            .ForMember(
                destination => destination.Id,
                options => options.MapFrom(source => source.ID))
            .ForMember(
                destination => destination.IsOpenNow,
                options => options.Ignore())
            .ForMember(
                destination => destination.WorkingHours,
                options => options.Ignore());

        CreateMap<RestaurantWorkingHour, PublicWorkingHourDto>()
            .ForMember(
                destination => destination.DayName,
                options => options.MapFrom(source =>
                    source.DayOfWeek.ToString()));

        CreateMap<BranchWorkingHour, PublicWorkingHourDto>()
            .ForMember(
                destination => destination.DayName,
                options => options.MapFrom(source =>
                    source.DayOfWeek.ToString()));

        CreateMap<Table, PublicRestaurantTableDto>()
            .ForMember(
                destination => destination.Id,
                options => options.MapFrom(source => source.ID))
            .ForMember(
                destination => destination.Capacity,
                options => options.MapFrom(source => source.Tutum))
            .ForMember(
                destination => destination.Shape,
                options => options.MapFrom(source =>
                    source.Shape.ToString()))
            .ForMember(
                destination => destination.PositionX,
                options => options.MapFrom(source =>
                    source.PositionX ?? 0))
            .ForMember(
                destination => destination.PositionY,
                options => options.MapFrom(source =>
                    source.PositionY ?? 0))
            .ForMember(
                destination => destination.PositionZ,
                options => options.MapFrom(source =>
                    source.PositionZ ?? 0))
            .ForMember(
                destination => destination.RotationX,
                options => options.MapFrom(source =>
                    source.RotationX ?? 0))
            .ForMember(
                destination => destination.RotationY,
                options => options.MapFrom(source =>
                    source.RotationY ?? 0))
            .ForMember(
                destination => destination.RotationZ,
                options => options.MapFrom(source =>
                    source.RotationZ ?? 0))
            .ForMember(
                destination => destination.Status,
                options => options.Ignore())
            .ForMember(
                destination => destination.IsAvailable,
                options => options.Ignore())
            .ForMember(
                destination => destination.UnavailableReason,
                options => options.Ignore());
    }
}
