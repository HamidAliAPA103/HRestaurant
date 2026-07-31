using AutoMapper;
using HRestaurant.DTOS.Payment;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Payments;

public sealed class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<Payment, PaymentGetDTO>()
            .ForMember(x => x.Id, o => o.MapFrom(x => x.ID))
            .ForMember(x => x.CreatedAt, o => o.MapFrom(x => x.CreatAt))
            .ForMember(x => x.RefundedAmount,
                o => o.MapFrom(x => x.Refunds.Where(r => !r.IsDeleted).Sum(r => r.Amount)))
            .ForMember(x => x.RefundableAmount,
                o => o.MapFrom(x => x.Amount - x.Refunds.Where(r => !r.IsDeleted).Sum(r => r.Amount)))
            .ForMember(x => x.CreatedByName, o => o.Ignore());
        CreateMap<Refund, RefundGetDTO>()
            .ForMember(x => x.Id, o => o.MapFrom(x => x.ID))
            .ForMember(x => x.RefundedByName, o => o.Ignore());
    }
}
