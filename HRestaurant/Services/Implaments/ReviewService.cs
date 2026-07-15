using HRestaurant.Data;
using HRestaurant.DTOS.Reservation;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Review;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implaments
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse> CreateAsync(ReviewCreateDTO dto)
        {
            Review review = new()
            {
                CustomerId = dto.CustomerId,
                ResdaranId = dto.ResdaranId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            var result = await _context.AddAsync(review);
            if (result.State != EntityState.Added) return new ApiResponse() { StatusCode = 500, Message = "Create failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 201, Message = "Created successfully!" } :
            new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> GetAllAsync(ViewType type)
        {
            var reviews = (type == ViewType.notdeleted) ?

            await _context.Reviews.Where(c => !c.IsDeleted).ToListAsync() :

            (type == ViewType.deleted) ? await _context.Reviews.Where(c => c.IsDeleted).ToListAsync() :

            await _context.Reviews.ToListAsync();

            var dtos = reviews.Select(c => new ReviewGetDTO { ID = c.ID, CustomerId = c.CustomerId, Comment = c.Comment,Rating = c.Rating,ResdaranId = c.ResdaranId, CreatAt = c.CreatAt, DeletedAt = c.DeletedAt, IsDeleted = c.IsDeleted, UpdateAt = c.UpdateAt }).ToList();

            return new ApiResponse { StatusCode = 200, Data = dtos, Message = $"Total: {dtos.Count.ToString()}" };
        }

        public async Task<ApiResponse> GetByID(Guid id)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (review == null) return new ApiResponse { StatusCode = 404, Message = "Review not found!" };

            var dto = new ReviewGetDTO()
            {
                ID = review.ID,
                CustomerId = review.CustomerId,
                Comment = review.Comment,
                Rating = review.Rating,
                ResdaranId = review.ResdaranId,
                CreatAt = review.CreatAt,
                DeletedAt = review.DeletedAt,
                IsDeleted = review.IsDeleted,
                UpdateAt = review.UpdateAt
            };

            return new ApiResponse { StatusCode = 200, Data = dto };
        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null) return new ApiResponse { StatusCode = 404, Message = "Review not found!" };

            var result = _context.Remove(review);
            if (result.State != EntityState.Deleted) return new ApiResponse { StatusCode = 404, Message = "Review not found!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 204, Message = "Deleted successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null) return new ApiResponse { StatusCode = 404, Message = "Review not found!" };

            review.IsDeleted = !review.IsDeleted;

            review.DeletedAt = DateTime.Now;

            var result = _context.Update(review);
            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Review failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return (saveCount > 0 && review.IsDeleted) ?
                new ApiResponse { StatusCode = 204, Message = "Deleted temporarily!" }
                :
                (saveCount > 0 && !review.IsDeleted) ?
                new ApiResponse { StatusCode = 200, Message = "Restored successfully!" } :
                new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, ReviewUpdateDTO dto)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (review == null) return new ApiResponse { StatusCode = 404, Message = "Review not found!" };

            review.CustomerId = dto.CustomerId != null ? dto.CustomerId : review.CustomerId;

            review.ResdaranId = dto.ResdaranId != null ? dto.ResdaranId : review.ResdaranId;

            review.Rating = dto.Rating != null ? dto.Rating : review.Rating;

            review.Comment = dto.Comment != null ? dto.Comment : review.Comment;

            review.UpdateAt = DateTime.UtcNow;
            var result = _context.Update(review);

            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Updated failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 200, Message = "Updated successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }
    }
}
