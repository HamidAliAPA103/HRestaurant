using HRestaurant.DTOS.Review;

namespace HRestaurant.Services.Interfaces;

public interface IReviewService :
    ICrudService<ReviewCreateDTO, ReviewUpdateDTO, ReviewGetDTO>;
