using HRestaurant.DTOS.Reservation;

namespace HRestaurant.Services.Interfaces;

public interface IReservationService :
    ICrudService<
        ReservationCreateDTO,
        ReservationUpdateDTO,
        ReservationGetDTO>;
