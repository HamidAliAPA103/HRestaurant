using HRestaurant.Data;
using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Enum;
using HRestaurant.Migrations;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implaments
{
    public class MenuCategoryService : IMenuCategoryService
    {
        private readonly AppDbContext _context;

        public MenuCategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse> CreateAsync(MenuCategoryCreateDTO dto)
        {
            MenuCategory menuCategory = new()
            {
                ResdaranId = dto.ResdaranId,
                Name = dto.Name
            };

            var result = await _context.AddAsync(menuCategory);
            if (result.State != EntityState.Added) return new ApiResponse() { StatusCode = 500, Message = "Create failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 201, Message = "Created successfully!" } :
            new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> GetAllAsync(ViewType type)
        {
            var menuCategories = (type == ViewType.notdeleted) ?

            await _context.MenuCategories.Where(c => !c.IsDeleted).ToListAsync() :

            (type == ViewType.deleted) ? await _context.MenuCategories.Where(c => c.IsDeleted).ToListAsync() :

            await _context.MenuCategories.ToListAsync();

            var dtos = menuCategories.Select(c => new MenuCategoryGetDTO { ID = c.ID, ResdaranId = c.ResdaranId ,Name = c.Name,CreatAt = c.CreatAt ,DeletedAt = c.DeletedAt ,IsDeleted = c.IsDeleted,UpdateAt=c.UpdateAt}).ToList();

            return new ApiResponse { StatusCode = 200, Data = dtos, Message = $"Total: {dtos.Count.ToString()}" };
        }

        public async Task<ApiResponse> GetByID(Guid id)
        {
            var menuCategory = await _context.MenuCategories.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (menuCategory == null) return new ApiResponse { StatusCode = 404, Message = "MenuCategory not found!" };

            var dto = new MenuCategoryGetDTO()
            {
                ID = menuCategory.ID,
                UpdateAt = menuCategory.UpdateAt,
                CreatAt = menuCategory.CreatAt,
                DeletedAt = menuCategory.DeletedAt,
                IsDeleted = menuCategory.IsDeleted,
                Name = menuCategory.Name,
                ResdaranId = menuCategory.ResdaranId

            };

            return new ApiResponse { StatusCode = 200, Data = dto };

        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var menuCategory = await _context.MenuCategories.FindAsync(id);

            if (menuCategory == null) return new ApiResponse { StatusCode = 404, Message = "MenuCategory not found!" };

            var result = _context.Remove(menuCategory);
            if (result.State != EntityState.Deleted) return new ApiResponse { StatusCode = 404, Message = "MenuCategory not found!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 204, Message = "Deleted successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var orders = await _context.MenuCategories.FindAsync(id);

            if (orders == null) return new ApiResponse { StatusCode = 404, Message = "MenuCategory not found!" };

            orders.IsDeleted = !orders.IsDeleted;

            orders.DeletedAt = DateTime.Now;

            var result = _context.Update(orders);
            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "MenuCategory failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return (saveCount > 0 && orders.IsDeleted) ?
                new ApiResponse { StatusCode = 204, Message = "Deleted temporarily!" }
                :
                (saveCount > 0 && !orders.IsDeleted) ?
                new ApiResponse { StatusCode = 200, Message = "Restored successfully!" } :
                new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, MenuCategoryUpdateDTO dto)
        {
            var menuCategory = await _context.MenuCategories.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (menuCategory == null) return new ApiResponse { StatusCode = 404, Message = "MenuCategory not found!" };

            menuCategory.Name = dto.Name != null ? dto.Name : menuCategory.Name;

            menuCategory.UpdateAt = DateTime.UtcNow;
            var result = _context.Update(menuCategory);

            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Updated failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 200, Message = "Updated successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }
    }
}
