using AutoMapper;
using HRestaurant.DTOS.Review;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Reviews;

public sealed class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<Review, ReviewGetDTO>();

        CreateMap<ReviewCreateDTO, Review>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Restaurant, options => options.Ignore())
            .ForMember(destination => destination.Customer, options => options.Ignore());

        CreateMap<ReviewUpdateDTO, Review>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Restaurant, options => options.Ignore())
            .ForMember(destination => destination.Customer, options => options.Ignore())
            .IgnoreNullSourceMembers();
    }
}
