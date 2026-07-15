using HRestaurant.Data;
using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.OrderItem;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implaments
{
    public class OrderItemService : IOrderItemService
    {
           private readonly AppDbContext _context;

        public OrderItemService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse> CreateAsync(OrderItemCreatDTO dto)
        {
            OrderItem orderItem = new()
            {
                OrderId = dto.OrderId,
                Prices = dto.Prices,
                Say = dto.Say,
                MenuId = dto.MenuId
            };

            var result = await _context.AddAsync(orderItem);
            if (result.State != EntityState.Added) return new ApiResponse() { StatusCode = 500, Message = "Create failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 201, Message = "Created successfully!" } :
            new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> GetAllAsync(ViewType type)
        {
            var orderItemsq = (type == ViewType.notdeleted) ?

            await _context.OrderItems.Where(c => !c.IsDeleted).ToListAsync() :

            (type == ViewType.deleted) ? await _context.OrderItems.Where(c => c.IsDeleted).ToListAsync() :

            await _context.OrderItems.ToListAsync();

            var dtos = orderItemsq.Select(c => new OrderItemGetDTO { ID = c.ID,MenuId =c.MenuId,OrderId = c.OrderId,Prices = c.Prices ,Say = c.Say, CreatAt = c.CreatAt, DeletedAt = c.DeletedAt, IsDeleted = c.IsDeleted, UpdateAt = c.UpdateAt }).ToList();

            return new ApiResponse { StatusCode = 200, Data = dtos, Message = $"Total: {dtos.Count.ToString()}" };
        }

        public async Task<ApiResponse> GetByID(Guid id)
        {
            var orderItem = await _context.OrderItems.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (orderItem == null) return new ApiResponse { StatusCode = 404, Message = "OrderItem not found!" };

            var dto = new OrderItemGetDTO()
            {
                ID = orderItem.ID,
                MenuId = orderItem.MenuId,
                Say = orderItem.Say,
                OrderId = orderItem.OrderId,
                Prices = orderItem.Prices,
                CreatAt = orderItem.CreatAt,
                DeletedAt = orderItem.DeletedAt,
                IsDeleted = orderItem.IsDeleted,
                UpdateAt = orderItem.UpdateAt
            };

            return new ApiResponse { StatusCode = 200, Data = dto };
        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);

            if (orderItem == null) return new ApiResponse { StatusCode = 404, Message = "OrderItem not found!" };

            var result = _context.Remove(orderItem);
            if (result.State != EntityState.Deleted) return new ApiResponse { StatusCode = 404, Message = "OrderItem not found!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 204, Message = "Deleted successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);

            if (orderItem == null) return new ApiResponse { StatusCode = 404, Message = "OrderItem not found!" };

            orderItem.IsDeleted = !orderItem.IsDeleted;

            orderItem.DeletedAt = DateTime.Now;

            var result = _context.Update(orderItem);
            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "OrderItem failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return (saveCount > 0 && orderItem.IsDeleted) ?
                new ApiResponse { StatusCode = 204, Message = "Deleted temporarily!" }
                :
                (saveCount > 0 && !orderItem.IsDeleted) ?
                new ApiResponse { StatusCode = 200, Message = "Restored successfully!" } :
                new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, OrderItemUpdateDTO dto)
        {
            var orderItem = await _context.OrderItems.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (orderItem == null) return new ApiResponse { StatusCode = 404, Message = "OrderItem not found!" };

            orderItem.Say = dto.Say != null ? dto.Say : orderItem.Say;

            orderItem.Prices = dto.Prices != null ? dto.Prices : orderItem.Prices;

            orderItem.UpdateAt = DateTime.UtcNow;
            var result = _context.Update(orderItem);

            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Updated failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 200, Message = "Updated successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }
    }
}
