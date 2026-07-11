using HRestaurant.Data;
using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Enum;
using HRestaurant.Migrations;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implaments
{
    public class OrderService : IOrderService
    {

        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse> CreateAsync(OrderCreatDTO dto)
        {
            Order order = new()
            {
                CustomerID = dto.CustomerID,
                TableID = dto.TableID,
                Items = new List<OrderItem>()
            };

            foreach (var itemDto in dto.Items)
            {
                var menu = await _context.Menus.FindAsync(itemDto.MenuId);

                if (menu == null)
                    return new ApiResponse { StatusCode = 404, Message = "Yemək tapılmadı!" };

                OrderItem orderItem = new()
                {
                    MenuId = itemDto.MenuId,
                    Say = itemDto.Say,
                    Prices = menu.Price
                };

                order.Items.Add(orderItem);
                order.TotalPrices += (orderItem.Prices * orderItem.Say);
            }

            var result = await _context.AddAsync(order);

            if (result.State != EntityState.Added)
                return new ApiResponse() { StatusCode = 500, Message = "Create failed!" };

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0
                ? new ApiResponse { StatusCode = 201, Message = "Created successfully!" }
                : new ApiResponse { StatusCode = 500, Message = "Save failed!" };

        }

        public async Task<ApiResponse> GetAllAsync(ViewType type)
        {
            var orders = (type == ViewType.notdeleted) ?

            await _context.Orders.Where(c => !c.IsDeleted).ToListAsync() :

            (type == ViewType.deleted) ? await _context.Orders.Where(c => c.IsDeleted).ToListAsync() :

            await _context.Orders.ToListAsync();

            var dtos = orders.Select(c => new OrderGetDTO {CustomerID = c.CustomerID , Status=c.Status, TableID=c.TableID, CreatAt = c.CreatAt, ID = c.ID, IsDeleted = c.IsDeleted,TotalPrices=c.TotalPrices }).ToList();

            return new ApiResponse { StatusCode = 200, Data = dtos, Message = $"Total: {dtos.Count.ToString()}" };
        }

        public async Task<ApiResponse> GetByID(Guid id)
        {
            var orders = await _context.Orders.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (orders == null) return new ApiResponse { StatusCode = 404, Message = "Order not found!" };

            var dto = new OrderGetDTO()
            {
                ID = orders.ID,
                Status = orders.Status,
                TableID = orders.TableID,
                CreatAt = orders.CreatAt,
                TotalPrices = orders.TotalPrices,
                CustomerID = orders.CustomerID,
                IsDeleted = orders.IsDeleted,
             
            };
            return new ApiResponse { StatusCode = 200, Data = dto };
        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {

            var orders = await _context.Orders.FindAsync(id);

            if (orders == null) return new ApiResponse { StatusCode = 404, Message = "Order not found!" };

            var result = _context.Remove(orders);
            if (result.State != EntityState.Deleted) return new ApiResponse { StatusCode = 404, Message = "Order not found!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 204, Message = "Deleted successfully!" } :
                new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var orders = await _context.Orders.FindAsync(id);

            if (orders == null) return new ApiResponse { StatusCode = 404, Message = "Order not found!" };

            orders.IsDeleted = !orders.IsDeleted;

            orders.DeletedAt = DateTime.Now;

            var result = _context.Update(orders);
            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Order failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return (saveCount > 0 && orders.IsDeleted) ?
                new ApiResponse { StatusCode = 204, Message = "Deleted temporarily!" }
                :
                (saveCount > 0 && !orders.IsDeleted) ?
                new ApiResponse { StatusCode = 200, Message = "Restored successfully!" } :
                new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, OrderUpdateDTO dto)
        {
            var orders = await _context.Orders.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (orders == null) return new ApiResponse { StatusCode = 404, Message = "Order not found!" };

            orders.TableID = dto.TableID != null ? dto.TableID : orders.TableID;

            orders.Status = dto.Status != null ? dto.Status : orders.Status;

         

            orders.UpdateAt = DateTime.UtcNow;
            var result = _context.Update(orders);

            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Updated failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 200, Message = "Updated successfully!" } :
                new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }
    }
}
