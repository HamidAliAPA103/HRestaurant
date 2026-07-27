using AutoMapper;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;

namespace HRestaurant.Services.Implementations;

public sealed class RestaurantService :
    CrudServiceBase<
        Restaurant,
        RestaurantCreatDTO,
        RestaurantUpdateDTO,
        RestaurantGetDTO>,
    IRestaurantService
{
    public RestaurantService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper, "Restaurant")
    {
    }
}
