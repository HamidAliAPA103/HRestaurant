using AutoMapper;
using HRestaurant.DTOS.User;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Users;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserGetDTO>();

        CreateMap<UserCreateDTO, User>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Orders, options => options.Ignore())
            .ForMember(destination => destination.Reservations, options => options.Ignore())
            .ForMember(destination => destination.Reviews, options => options.Ignore());

        CreateMap<UserUpdateDTO, User>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Orders, options => options.Ignore())
            .ForMember(destination => destination.Reservations, options => options.Ignore())
            .ForMember(destination => destination.Reviews, options => options.Ignore())
            .IgnoreNullSourceMembers();
    }
}
