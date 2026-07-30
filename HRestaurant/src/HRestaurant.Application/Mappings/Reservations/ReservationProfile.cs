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
            .ForMember(
                destination => destination.EndTime,
                options => options.MapFrom(source =>
                    source.ReservationTime.AddMinutes(
                        source.DurationMinutes)))
            .ForMember(destination => destination.FullName, options => options.Ignore())
            .ForMember(destination => destination.PhoneNormalized, options => options.Ignore())
            .ForMember(destination => destination.Email, options => options.Ignore())
            .ForMember(destination => destination.SpecialNotes, options => options.Ignore())
            .ForMember(destination => destination.ConfirmationCode, options => options.Ignore())
            .ForMember(destination => destination.PublicTrackingTokenHash, options => options.Ignore())
            .ForMember(destination => destination.CancelledAt, options => options.Ignore())
            .ForMember(destination => destination.CancellationReason, options => options.Ignore())
            .ForMember(destination => destination.Table, options => options.Ignore())
            .ForMember(destination => destination.Branch, options => options.Ignore())
            .ForMember(destination => destination.Customer, options => options.Ignore())
            .ForMember(destination => destination.AuditLogs, options => options.Ignore());

        CreateMap<ReservationUpdateDTO, Reservation>()
            .IgnoreBaseEntityMembers()
            .ForMember(
                destination => destination.EndTime,
                options => options.MapFrom(source =>
                    source.ReservationTime.AddMinutes(
                        source.DurationMinutes)))
            .ForMember(destination => destination.FullName, options => options.Ignore())
            .ForMember(destination => destination.PhoneNormalized, options => options.Ignore())
            .ForMember(destination => destination.Email, options => options.Ignore())
            .ForMember(destination => destination.SpecialNotes, options => options.Ignore())
            .ForMember(destination => destination.ConfirmationCode, options => options.Ignore())
            .ForMember(destination => destination.PublicTrackingTokenHash, options => options.Ignore())
            .ForMember(destination => destination.CancelledAt, options => options.Ignore())
            .ForMember(destination => destination.CancellationReason, options => options.Ignore())
            .ForMember(destination => destination.Table, options => options.Ignore())
            .ForMember(destination => destination.Branch, options => options.Ignore())
            .ForMember(destination => destination.Customer, options => options.Ignore())
            .ForMember(destination => destination.AuditLogs, options => options.Ignore());
    }
}
