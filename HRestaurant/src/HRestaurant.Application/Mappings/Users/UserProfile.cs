using AutoMapper;
using HRestaurant.DTOS.User;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Users;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserGetDTO>()
            .ForMember(x => x.RestaurantId,
                o => o.MapFrom(x => x.RestaurantId ?? Guid.Empty))
            .ForMember(x => x.BranchId,
                o => o.MapFrom(x => x.BranchId ?? Guid.Empty))
            .ForMember(x => x.BranchName,
                o => o.MapFrom(x => x.Branch == null ? string.Empty : x.Branch.Name));

        CreateMap<UserCreateDTO, User>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.NormalizedEmail, o => o.Ignore())
            .ForMember(x => x.NormalizedPhone, o => o.Ignore())
            .ForMember(x => x.AppUserId, o => o.Ignore())
            .ForMember(x => x.IsActive, o => o.MapFrom(_ => true))
            .ForMember(x => x.Birthday, o => o.Ignore())
            .ForMember(x => x.Notes, o => o.Ignore())
            .ForMember(x => x.TotalOrders, o => o.Ignore())
            .ForMember(x => x.TotalSpent, o => o.Ignore())
            .ForMember(x => x.LastVisitDate, o => o.Ignore())
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Branch, o => o.Ignore())
            .ForMember(x => x.EmployeeShifts, o => o.Ignore())
            .ForMember(x => x.Orders, o => o.Ignore())
            .ForMember(x => x.WaiterOrders, o => o.Ignore())
            .ForMember(x => x.Reservations, o => o.Ignore())
            .ForMember(x => x.Reviews, o => o.Ignore())
            .ForMember(x => x.LoyaltyAccount, o => o.Ignore());

        CreateMap<UserUpdateDTO, User>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.RestaurantId, o => o.Ignore())
            .ForMember(x => x.BranchId, o => o.Ignore())
            .ForMember(x => x.AppUserId, o => o.Ignore())
            .ForMember(x => x.NormalizedEmail, o => o.Ignore())
            .ForMember(x => x.NormalizedPhone, o => o.Ignore())
            .ForMember(x => x.IsActive, o => o.Ignore())
            .ForMember(x => x.Birthday, o => o.Ignore())
            .ForMember(x => x.Notes, o => o.Ignore())
            .ForMember(x => x.TotalOrders, o => o.Ignore())
            .ForMember(x => x.TotalSpent, o => o.Ignore())
            .ForMember(x => x.LastVisitDate, o => o.Ignore())
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Branch, o => o.Ignore())
            .ForMember(x => x.EmployeeShifts, o => o.Ignore())
            .ForMember(x => x.Orders, o => o.Ignore())
            .ForMember(x => x.WaiterOrders, o => o.Ignore())
            .ForMember(x => x.Reservations, o => o.Ignore())
            .ForMember(x => x.Reviews, o => o.Ignore())
            .ForMember(x => x.LoyaltyAccount, o => o.Ignore())
            .IgnoreNullSourceMembers();
    }
}
