using AutoMapper;
using HRestaurant.DTOS.Reservation;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class ReservationService : IReservationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReservationService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(mapper);

        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse> CreateAsync(
        ReservationCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var reservation = _mapper.Map<Reservation>(dto);

        await _unitOfWork.Reservations.AddAsync(
            reservation,
            cancellationToken);

        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? new ApiResponse
            {
                StatusCode = 201,
                Message = "Created successfully!"
            }
            : new ApiResponse
            {
                StatusCode = 500,
                Message = "Save failed!"
            };
    }

    public async Task<ApiResponse> GetAllAsync(
        ViewType type,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Reservations.GetQueryable();

        query = type switch
        {
            ViewType.deleted => query.Where(entity => entity.IsDeleted),
            ViewType.notdeleted => query.Where(entity => !entity.IsDeleted),
            _ => query
        };

        var reservations = await query.ToListAsync(cancellationToken);
        var dtos = _mapper.Map<List<ReservationGetDTO>>(reservations);

        return new ApiResponse
        {
            StatusCode = 200,
            Data = dtos,
            Message = $"Total: {dtos.Count}"
        };
    }

    public async Task<ApiResponse> GetByID(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _unitOfWork.Reservations
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (reservation is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Reservation not found!"
            };
        }

        return new ApiResponse
        {
            StatusCode = 200,
            Data = _mapper.Map<ReservationGetDTO>(reservation)
        };
    }

    public async Task<ApiResponse> RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(
            id,
            cancellationToken);

        if (reservation is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Reservation not found!"
            };
        }

        _unitOfWork.Reservations.Delete(reservation);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? new ApiResponse
            {
                StatusCode = 204,
                Message = "Deleted successfully!"
            }
            : new ApiResponse
            {
                StatusCode = 500,
                Message = "Save failed!"
            };
    }

    public async Task<ApiResponse> ToggleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(
            id,
            cancellationToken);

        if (reservation is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Reservation not found!"
            };
        }

        reservation.IsDeleted = !reservation.IsDeleted;
        reservation.DeletedAt = reservation.IsDeleted ? DateTime.UtcNow : null;
        reservation.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Reservations.Update(reservation);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? reservation.IsDeleted
                ? new ApiResponse
                {
                    StatusCode = 204,
                    Message = "Deleted temporarily!"
                }
                : new ApiResponse
                {
                    StatusCode = 200,
                    Message = "Restored successfully!"
                }
            : new ApiResponse
            {
                StatusCode = 500,
                Message = "Save failed!"
            };
    }

    public async Task<ApiResponse> UpdateAsync(
        Guid id,
        ReservationUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _unitOfWork.Reservations
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (reservation is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Reservation not found!"
            };
        }

        _mapper.Map(dto, reservation);
        reservation.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Reservations.Update(reservation);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? new ApiResponse
            {
                StatusCode = 200,
                Message = "Updated successfully!"
            }
            : new ApiResponse
            {
                StatusCode = 500,
                Message = "Save failed!"
            };
    }

}
