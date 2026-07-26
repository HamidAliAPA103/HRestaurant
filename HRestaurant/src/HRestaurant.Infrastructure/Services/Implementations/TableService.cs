using AutoMapper;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Table;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class TableService : ITableService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TableService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(mapper);

        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse> CreateAsync(
        TableCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var table = _mapper.Map<Table>(dto);

        await _unitOfWork.Tables.AddAsync(table, cancellationToken);
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
        var query = _unitOfWork.Tables.GetQueryable();

        query = type switch
        {
            ViewType.deleted => query.Where(entity => entity.IsDeleted),
            ViewType.notdeleted => query.Where(entity => !entity.IsDeleted),
            _ => query
        };

        var tables = await query.ToListAsync(cancellationToken);
        var dtos = _mapper.Map<List<TableGetDTO>>(tables);

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
        var table = await _unitOfWork.Tables
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (table is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Table not found!"
            };
        }

        return new ApiResponse
        {
            StatusCode = 200,
            Data = _mapper.Map<TableGetDTO>(table)
        };
    }

    public async Task<ApiResponse> RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var table = await _unitOfWork.Tables.GetByIdAsync(
            id,
            cancellationToken);

        if (table is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Table not found!"
            };
        }

        _unitOfWork.Tables.Delete(table);
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
        var table = await _unitOfWork.Tables.GetByIdAsync(
            id,
            cancellationToken);

        if (table is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Table not found!"
            };
        }

        table.IsDeleted = !table.IsDeleted;
        table.DeletedAt = table.IsDeleted ? DateTime.UtcNow : null;
        table.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Tables.Update(table);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? table.IsDeleted
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
        TableUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var table = await _unitOfWork.Tables
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (table is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Table not found!"
            };
        }

        _mapper.Map(dto, table);
        table.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Tables.Update(table);
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
