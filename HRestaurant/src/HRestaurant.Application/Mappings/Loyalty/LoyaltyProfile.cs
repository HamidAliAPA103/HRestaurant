using AutoMapper;
using HRestaurant.DTOS.Loyalty;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Loyalty;

public sealed class LoyaltyProfile : Profile
{
    public LoyaltyProfile()
    {
        CreateMap<LoyaltyTransaction, LoyaltyTransactionGetDTO>()
            .ForMember(x => x.Id, o => o.MapFrom(x => x.ID))
            .ForMember(x => x.CreatedAt, o => o.MapFrom(x => x.CreatAt));
    }
}
