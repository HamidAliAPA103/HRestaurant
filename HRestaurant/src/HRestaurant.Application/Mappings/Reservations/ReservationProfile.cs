using AutoMapper;
using HRestaurant.DTOS.Reservation;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Reservations;

public sealed class ReservationProfile : Profile
{
    public ReservationProfile()
    {
        CreateMap<Reservation, ReservationGetDTO>();

        CreateMap<ReservationCreateDTO, Reservation>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Table, options => options.Ignore())
            .ForMember(destination => destination.Customer, options => options.Ignore());

        CreateMap<ReservationUpdateDTO, Reservation>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Table, options => options.Ignore())
            .ForMember(destination => destination.Customer, options => options.Ignore());
    }
}
