using AutoMapper;
using HRestaurant.DTOS.Customer;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Customers;

public sealed class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<User, CustomerGetDTO>()
            .ForMember(x => x.Id, o => o.MapFrom(x => x.ID))
            .ForMember(x => x.RestaurantId, o => o.MapFrom(x => x.RestaurantId!.Value))
            .ForMember(x => x.FullName, o => o.MapFrom(x => x.Name))
            .ForMember(x => x.Phone, o => o.MapFrom(x => x.Phone ?? string.Empty))
            .ForMember(x => x.Email,
                o => o.MapFrom(x => string.IsNullOrEmpty(x.Email) ? null : x.Email))
            .ForMember(x => x.CreatedAt, o => o.MapFrom(x => x.CreatAt))
            .ForMember(x => x.UpdatedAt, o => o.MapFrom(x => x.UpdateAt));
        CreateMap<User, CustomerDetailDTO>()
            .IncludeBase<User, CustomerGetDTO>()
            .ForMember(x => x.FavoriteMenuItems, o => o.Ignore());
    }
}
