using AutoMapper;
using HRestaurant.DTOS.Review;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;

namespace HRestaurant.Services.Implementations;

public sealed class ReviewService :
    CrudServiceBase<
        Review,
        ReviewCreateDTO,
        ReviewUpdateDTO,
        ReviewGetDTO>,
    IReviewService
{
    public ReviewService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper, "Review")
    {
    }
}
