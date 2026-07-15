using HRestaurant.Data;
using HRestaurant.DTOS.OrderItem;
using HRestaurant.DTOS.Reservation;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implaments
{
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;

        public ReservationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse> CreateAsync(ReservationCreateDTO dto)
        {
            Reservation reservation = new()
            {
                CustomerId = dto.CustomerId,
                TableId = dto.TableId,
                ReservationTime = dto.ReservationTime,
                GuestCount = dto.GuestCount,
                Status = dto.Status
            };

            var result = await _context.AddAsync(reservation);
            if (result.State != EntityState.Added) return new ApiResponse() { StatusCode = 500, Message = "Create failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 201, Message = "Created successfully!" } :
            new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> GetAllAsync(ViewType type)
        {
            var reservations = (type == ViewType.notdeleted) ?

            await _context.Reservations.Where(c => !c.IsDeleted).ToListAsync() :

            (type == ViewType.deleted) ? await _context.Reservations.Where(c => c.IsDeleted).ToListAsync() :

            await _context.Reservations.ToListAsync();

            var dtos = reservations.Select(c => new ReservationGetDTO { ID = c.ID,CustomerId = c.CustomerId,GuestCount = c.GuestCount,ReservationTime = c.ReservationTime,Status = c.Status,TableId = c.TableId , CreatAt = c.CreatAt, DeletedAt = c.DeletedAt, IsDeleted = c.IsDeleted, UpdateAt = c.UpdateAt }).ToList();

            return new ApiResponse { StatusCode = 200, Data = dtos, Message = $"Total: {dtos.Count.ToString()}" };
        }

        public async Task<ApiResponse> GetByID(Guid id)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (reservation == null) return new ApiResponse { StatusCode = 404, Message = "Reservation not found!" };

            var dto = new ReservationGetDTO()
            {
                ID = reservation.ID,
                CustomerId = reservation.CustomerId,
                GuestCount = reservation.GuestCount,
                ReservationTime = reservation.ReservationTime,
                Status = reservation.Status,
                TableId = reservation.TableId,
                CreatAt = reservation.CreatAt,
                DeletedAt = reservation.DeletedAt,
                UpdateAt = reservation.UpdateAt
            };

            return new ApiResponse { StatusCode = 200, Data = dto };
        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null) return new ApiResponse { StatusCode = 404, Message = "Reservatoin not found!" };

            var result = _context.Remove(reservation);
            if (result.State != EntityState.Deleted) return new ApiResponse { StatusCode = 404, Message = "Reservatoin not found!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 204, Message = "Deleted successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null) return new ApiResponse { StatusCode = 404, Message = "Reservatoin not found!" };

            reservation.IsDeleted = !reservation.IsDeleted;

            reservation.DeletedAt = DateTime.Now;

            var result = _context.Update(reservation);
            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Reservation failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return (saveCount > 0 && reservation.IsDeleted) ?
                new ApiResponse { StatusCode = 204, Message = "Deleted temporarily!" }
                :
                (saveCount > 0 && !reservation.IsDeleted) ?
                new ApiResponse { StatusCode = 200, Message = "Restored successfully!" } :
                new ApiResponse { StatusCode = 500, Message = "Save failed!" };
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, ReservationUpdateDTO dto)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(c => !c.IsDeleted && c.ID == id);

            if (reservation == null) return new ApiResponse { StatusCode = 404, Message = "Reservatoin not found!" };

            reservation.CustomerId = dto.CustomerId != null ? dto.CustomerId : reservation.CustomerId;

            reservation.TableId = dto.TableId != null ? dto.TableId : reservation.TableId;

            reservation.ReservationTime = dto.ReservationTime != null ? dto.ReservationTime : reservation.ReservationTime;

            reservation.GuestCount = dto.GuestCount != null ? dto.GuestCount : reservation.GuestCount;

            reservation.Status = dto.Status != null ? dto.Status : reservation.Status;

            reservation.UpdateAt = DateTime.UtcNow;
            var result = _context.Update(reservation);

            if (result.State != EntityState.Modified) return new ApiResponse { StatusCode = 500, Message = "Updated failed!" };
            var saveCount = await _context.SaveChangesAsync();
            return saveCount > 0 ? new ApiResponse { StatusCode = 200, Message = "Updated successfully!" } :
            new ApiResponse() { StatusCode = 500, Message = "Save failed!" };
        }
    }
}
