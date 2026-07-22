using HRestaurant.Data;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Review;
using HRestaurant.DTOS.Table;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations
{
    public class TableService : ITableService
    {
        private readonly AppDbContext _context;

        public TableService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse> CreateAsync(TableCreateDTO dto)
        {
            Table table = new()
            {
                RestaurantID = dto.RestaurantID,
                Status = dto.Status,
                Tutum = dto.Tutum
            };

            var result = await _context.AddAsync(table);
            if (result.State != EntityState.Added) return new ApiResponse() { StatusCode = 500, Message = "Create failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 201, Message = "Created successfully!" } :
            new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> GetAllAsync(ViewType type)
        {
            var table = (type == ViewType.notdeleted) ?

            await _context.Tables.Where(c => !c.IsDeleted).ToListAsync() :

            (type == ViewType.deleted) ? await _context.Tables.Where(c => c.IsDeleted).ToListAsync() :

            await _context.Tables.ToListAsync();

            var dtos = table.Select(c => new TableGetDTO { ID = c.ID,RestaurantID = c.RestaurantID,Status = c.Status,Tutum = c.Tutum, CreatAt = c.CreatAt, DeletedAt = c.DeletedAt, IsDeleted = c.IsDeleted, UpdateAt = c.UpdateAt }).ToList();

            return new ApiResponse { StatusCode = 200, Data = dtos, Message = $"Total: {dtos.Count.ToString()}" };
        }

        public async Task<ApiResponse> GetByID(Guid id)
        {
            var table = await _context.Tables.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (table == null) return new ApiResponse { StatusCode = 404, Message = "Table not found!" };

            var dto = new TableGetDTO()
            {
                ID = table.ID,
                Tutum = table.Tutum,
                CreatAt = table.CreatAt,
                DeletedAt = table.DeletedAt,
                UpdateAt = table.UpdateAt,
                RestaurantID= table.RestaurantID,
                Status = table.Status,
                IsDeleted = table.IsDeleted
                
            };

            return new ApiResponse { StatusCode = 200, Data = dto };

        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var table = await _context.Tables.FindAsync(id);

            if (table == null) return new ApiResponse { StatusCode = 404, Message = "Table not found!" };

            var result = _context.Remove(table);
            if (result.State != EntityState.Deleted) return new ApiResponse { StatusCode = 404, Message = "Review not found!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 204, Message = "Deleted successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var table = await _context.Tables.FindAsync(id);

            if (table == null) return new ApiResponse { StatusCode = 404, Message = "Table not found!" };

            table.IsDeleted = !table.IsDeleted;

            table.DeletedAt = DateTime.Now;

            var result = _context.Update(table);
            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Table failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return (saveCount > 0 && table.IsDeleted) ?
                new ApiResponse { StatusCode = 204, Message = "Deleted temporarily!" }
                :
                (saveCount > 0 && !table.IsDeleted) ?
                new ApiResponse { StatusCode = 200, Message = "Restored successfully!" } :
                new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, TableUpdateDTO dto)
        {
            var table = await _context.Tables.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (table == null) return new ApiResponse { StatusCode = 404, Message = "Table not found!" };

            table.RestaurantID = dto.RestaurantID != null ? dto.RestaurantID : table.RestaurantID;

            table.Tutum = dto.Tutum != null ? dto.Tutum : table.Tutum;

            table.Status = dto.Status != null ? dto.Status : table.Status;

            table.UpdateAt = DateTime.UtcNow;
            var result = _context.Update(table);

            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Updated failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 200, Message = "Updated successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }
    }
}
