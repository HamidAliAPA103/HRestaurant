using AutoMapper;
using HRestaurant.DTOS.Audit;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Audit;

public sealed class AuditProfile : Profile
{
    public AuditProfile()
    {
        CreateMap<AuditLog, AuditLogGetDTO>()
            .ForMember(x => x.Id, o => o.MapFrom(x => x.ID))
            .ForMember(x => x.CreatedAt, o => o.MapFrom(x => x.CreatAt))
            .ForMember(x => x.UserName, o => o.Ignore());
    }
}
