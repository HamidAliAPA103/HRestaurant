using HRestaurant.Data;
using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Enum;
using HRestaurant.Extentions;
using HRestaurant.Migrations;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implaments
{
    public class MenuService : IMenuService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _web;
        private readonly IHttpContextAccessor _accessor;

        public MenuService(AppDbContext context, IWebHostEnvironment web, IHttpContextAccessor accessor)
        {
            _context = context;
            _web = web;
            _accessor = accessor;
        }

        public async Task<ApiResponse> CreateAsync(MenuCreateDTO dto)
        {
            Menu menu = new()
            {
                CategoryId = dto.CategoryId,
                Image = await dto.Image.CreateFileAsync(_web.WebRootPath, "images", "menus"),
                Price = dto.Price,
                Desc = dto.Desc,
                Nutrition = dto.Nutrition
            };
            menu.ImageURL = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/images/menus/{menu.Image}";

            var result = await _context.AddAsync(menu);
            if (result.State != EntityState.Added) return new ApiResponse() { StatusCode = 500, Message = "Create failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 201, Message = "Created successfully!" } :
            new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> GetAllAsync(ViewType type)
        {
            var menus = (type == ViewType.notdeleted) ?

         await _context.Menus.Where(c => !c.IsDeleted).ToListAsync() :

         (type == ViewType.deleted) ? await _context.Menus.Where(c => c.IsDeleted).ToListAsync() :

         await _context.Menus.ToListAsync();

            var dtos = menus.Select(c => new MenuGetDTO { ID = c.ID, CategoryId = c.CategoryId, CreatAt = c.CreatAt, DeletedAt = c.DeletedAt, Desc = c.Desc, Nutrition = c.Nutrition, Price = c.Price, UpdateAt = c.UpdateAt }).ToList();

            return new ApiResponse { StatusCode = 200, Data = dtos, Message = $"Total: {dtos.Count.ToString()}" };
        }

        public async Task<ApiResponse> GetByID(Guid id)
        {
            var menus = await _context.Menus.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (menus == null) return new ApiResponse { StatusCode = 404, Message = "Menu not found!" };

            var dto = new MenuGetDTO()
            {
                ID = menus.ID,
                CategoryId = menus.CategoryId,
                CreatAt = menus.CreatAt,
                DeletedAt = menus.DeletedAt,
                Desc = menus.Desc,
                Nutrition = menus.Nutrition,
                Price = menus.Price,
                UpdateAt = menus.UpdateAt
            };
            return new ApiResponse { StatusCode = 200, Data = dto };

        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var menus = await _context.Menus.FindAsync(id);

            if (menus == null) return new ApiResponse { StatusCode = 404, Message = "Menu not found!" };

            menus.Image.DeleteFile(_web.WebRootPath, "images", "menus");


            var result = _context.Remove(menus);
            if (result.State != EntityState.Deleted) return new ApiResponse { StatusCode = 404, Message = "Menu not found!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 204, Message = "Deleted successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var menus = await _context.Menus.FindAsync(id);

            if (menus == null) return new ApiResponse { StatusCode = 404, Message = "Menu not found!" };

            menus.IsDeleted = !menus.IsDeleted;

            menus.DeletedAt = DateTime.Now;

            var result = _context.Update(menus);
            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Menu failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return (saveCount > 0 && menus.IsDeleted) ?
                new ApiResponse { StatusCode = 204, Message = "Deleted temporarily!" }
                :
                (saveCount > 0 && !menus.IsDeleted) ?
                new ApiResponse { StatusCode = 200, Message = "Restored successfully!" } :
                new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, MenuUpdateDTO dto)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
                return new ApiResponse
                {
                    StatusCode = 404,
                    Message = "Menu tapılmadı"
                };

            if (dto.Image != null)
            {
                menu.Image.DeleteFile(_web.WebRootPath, "images", "menus");
                menu.Image = await dto.Image.CreateFileAsync(_web.WebRootPath, "images", "menus");
                menu.ImageURL = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/images/menus/{menu.Image}";
            }


            if (!string.IsNullOrEmpty(dto.Desc)) menu.Desc = dto.Desc;
            if (!string.IsNullOrEmpty(dto.Nutrition)) menu.Nutrition = dto.Nutrition;

            menu.UpdateAt = DateTime.UtcNow;

            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0
                ? new ApiResponse { StatusCode = 200, Message = "Uğurla yeniləndi!" }
                : new ApiResponse { StatusCode = 500, Message = "Save uğursuz oldu!" };
        }
    }
}
