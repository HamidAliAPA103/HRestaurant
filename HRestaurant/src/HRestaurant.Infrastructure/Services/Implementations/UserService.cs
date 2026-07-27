using AutoMapper;
using HRestaurant.DTOS.User;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;

namespace HRestaurant.Services.Implementations;

public sealed class UserService :
    CrudServiceBase<
        User,
        UserCreateDTO,
        UserUpdateDTO,
        UserGetDTO>,
    IUserService
{
    public UserService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper, "User")
    {
    }
}
