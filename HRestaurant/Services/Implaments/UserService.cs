using HRestaurant.Data;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Table;
using HRestaurant.DTOS.User;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HRestaurant.Services.Implaments
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse> CreateAsync(UserCreateDTO dto)
        {
            User user = new()
            {
                Email = dto.Email,
                Name = dto.Name,
                Role = dto.Role
            };

            var result = await _context.AddAsync(user);
            if (result.State != EntityState.Added) return new ApiResponse() { StatusCode = 500, Message = "Create failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 201, Message = "Created successfully!" } :
            new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> GetAllAsync(ViewType type)
        {
            var user = (type == ViewType.notdeleted) ?

            await _context.Users.Where(c => !c.IsDeleted).ToListAsync() :

            (type == ViewType.deleted) ? await _context.Users.Where(c => c.IsDeleted).ToListAsync() :

            await _context.Users.ToListAsync();

            var dtos = user.Select(c => new UserGetDTO { ID = c.ID,Role = c.Role,Name = c.Name,Email = c.Email, CreatAt = c.CreatAt, DeletedAt = c.DeletedAt, IsDeleted = c.IsDeleted, UpdateAt = c.UpdateAt }).ToList();

            return new ApiResponse { StatusCode = 200, Data = dtos, Message = $"Total: {dtos.Count.ToString()}" };
        }

        public async Task<ApiResponse> GetByID(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (user == null) return new ApiResponse { StatusCode = 404, Message = "User not found!" };

            var dto = new UserGetDTO()
            {
                ID = user.ID,
                Role = user.Role,
                Name = user.Name,
                Email = user.Email,
                CreatAt = user.CreatAt,
                DeletedAt = user.DeletedAt,
                UpdateAt = user.UpdateAt
            };

            return new ApiResponse { StatusCode = 200, Data = dto };

        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) return new ApiResponse { StatusCode = 404, Message = "User not found!" };

            var result = _context.Remove(user);
            if (result.State != EntityState.Deleted) return new ApiResponse { StatusCode = 404, Message = "User not found!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 204, Message = "Deleted successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) return new ApiResponse { StatusCode = 404, Message = "User not found!" };

            user.IsDeleted = !user.IsDeleted;

            user.DeletedAt = DateTime.Now;

            var result = _context.Update(user);
            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "User failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return (saveCount > 0 && user.IsDeleted) ?
                new ApiResponse { StatusCode = 204, Message = "Deleted temporarily!" }
                :
                (saveCount > 0 && !user.IsDeleted) ?
                new ApiResponse { StatusCode = 200, Message = "Restored successfully!" } :
                new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, UserUpdateDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (user == null) return new ApiResponse { StatusCode = 404, Message = "Table not found!" };

            user.Email = dto.Email != null ? dto.Email : user.Email;

            user.Name = dto.Name != null ? dto.Name : user.Name;

            user.Role = dto.Role != null ? dto.Role : user.Role;

            user.UpdateAt = DateTime.UtcNow;
            var result = _context.Update(user);

            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Updated failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 200, Message = "Updated successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }
    }
}
