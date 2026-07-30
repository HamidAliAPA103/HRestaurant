using AutoMapper;
using HRestaurant.Data;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Table;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class TableService :
    CrudServiceBase<
        Table,
        TableCreateDTO,
        TableUpdateDTO,
        TableGetDTO>,
    ITableService
{
    private readonly AppDbContext _dbContext;

    public TableService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        AppDbContext dbContext)
        : base(unitOfWork, mapper, "Table")
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public override async Task<ApiResponse<Guid>> CreateAsync(
        TableCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureBranchBelongsToRestaurantAsync(
            dto.RestaurantID,
            dto.BranchId,
            cancellationToken);

        return await base.CreateAsync(dto, cancellationToken);
    }

    public override async Task<ApiResponse<object?>> UpdateAsync(
        Guid id,
        TableUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureBranchBelongsToRestaurantAsync(
            dto.RestaurantID,
            dto.BranchId,
            cancellationToken);

        return await base.UpdateAsync(id, dto, cancellationToken);
    }

    private async Task EnsureBranchBelongsToRestaurantAsync(
        Guid restaurantId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        if (!branchId.HasValue
            || !await _dbContext.Branches
                .AsNoTracking()
                .AnyAsync(
                    branch =>
                        branch.ID == branchId.Value
                        && branch.RestaurantId == restaurantId
                        && branch.IsActive
                        && !branch.IsDeleted,
                    cancellationToken))
        {
            throw new NotFoundException(
                "The selected active branch was not found for "
                + "this restaurant.");
        }
    }
}
