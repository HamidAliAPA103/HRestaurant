using HRestaurant.Data;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implaments
{
    public class RestaurantService : IRestaurantService
    {

        private readonly AppDbContext _context;

        public RestaurantService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse> CreateAsync(RestaurantCreatDTO dto)
        {
            Restaurant restaurant = new()
            {
                Name = dto.Name,
                Adres = dto.Adres,
                Number = dto.Number,
                CreatAt = DateTime.UtcNow
            };

            var result = await _context.AddAsync(restaurant);
            if (result.State != EntityState.Added) return new ApiResponse() { StatusCode = 500, Message = "Create failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 201, Message = "Created successfully!" } :
            new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> GetAllAsync(ViewType type)
        {
            var restaurants = (type == ViewType.notdeleted) ?

        await _context.Restaurants.Where(c => !c.IsDeleted).ToListAsync() :

        (type == ViewType.deleted) ? await _context.Restaurants.Where(c => c.IsDeleted).ToListAsync() :

        await _context.Restaurants.ToListAsync();

            var dtos = restaurants.Select(c => new RestaurantGetDTO { Name = c.Name,Adres=c.Adres,Number=c.Number, CreatAt = c.CreatAt, ID = c.ID, IsDeleted = c.IsDeleted}).ToList();

            return new ApiResponse { StatusCode = 200, Data = dtos, Message = $"Total: {dtos.Count.ToString()}" };

        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);

            if (restaurant == null) return new ApiResponse { StatusCode = 404, Message = "Restaurant not found!" };

            var result = _context.Remove(restaurant);
            if (result.State != EntityState.Deleted) return new ApiResponse { StatusCode = 404, Message = "Restaurant not found!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 204, Message = "Deleted successfully!" } :
                new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, RestaurantUpdateDTO dto)
        {
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (restaurant == null) return new ApiResponse { StatusCode = 404, Message = "Restaurant not found!" };

            restaurant.Name = dto.Name != null ? dto.Name : restaurant.Name;

            restaurant.Adres = dto.Adres != null ? dto.Adres : restaurant.Adres;

            restaurant.Number = dto.Number != null ? dto.Number : restaurant.Number;

            restaurant.UpdateAt = DateTime.UtcNow;
            var result = _context.Update(restaurant);

            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Updated failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 200, Message = "Updated successfully!" } :
                new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }
    }
}
