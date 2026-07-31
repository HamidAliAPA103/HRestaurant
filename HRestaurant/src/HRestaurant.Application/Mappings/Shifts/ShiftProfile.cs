using AutoMapper;
using HRestaurant.DTOS.Shift;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Shifts;

public sealed class ShiftProfile : Profile
{
    public ShiftProfile()
    {
        CreateMap<Shift, ShiftGetDTO>()
            .ForMember(x => x.BranchName, o => o.MapFrom(x => x.Branch.Name));
        CreateMap<EmployeeShift, EmployeeShiftGetDTO>()
            .ForMember(x => x.EmployeeName, o => o.MapFrom(x => x.Employee.Name))
            .ForMember(x => x.ShiftName, o => o.MapFrom(x => x.Shift.Name))
            .ForMember(x => x.BranchId, o => o.MapFrom(x => x.Shift.BranchId))
            .ForMember(x => x.BranchName, o => o.MapFrom(x => x.Shift.Branch.Name));
        CreateMap<ShiftCreateDTO, Shift>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.IsActive, o => o.MapFrom(_ => true))
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Branch, o => o.Ignore())
            .ForMember(x => x.EmployeeShifts, o => o.Ignore());
        CreateMap<ShiftUpdateDTO, Shift>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.RestaurantId, o => o.Ignore())
            .ForMember(x => x.BranchId, o => o.Ignore())
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Branch, o => o.Ignore())
            .ForMember(x => x.EmployeeShifts, o => o.Ignore());
    }
}
