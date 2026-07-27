using AutoMapper;
using HRestaurant.DTOS.Reservation;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;

namespace HRestaurant.Services.Implementations;

public sealed class ReservationService :
    CrudServiceBase<
        Reservation,
        ReservationCreateDTO,
        ReservationUpdateDTO,
        ReservationGetDTO>,
    IReservationService
{
    public ReservationService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper, "Reservation")
    {
    }
}
