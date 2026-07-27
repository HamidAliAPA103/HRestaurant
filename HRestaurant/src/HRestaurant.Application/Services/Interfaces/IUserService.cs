using HRestaurant.DTOS.User;

namespace HRestaurant.Services.Interfaces;

public interface IUserService :
    ICrudService<UserCreateDTO, UserUpdateDTO, UserGetDTO>;
