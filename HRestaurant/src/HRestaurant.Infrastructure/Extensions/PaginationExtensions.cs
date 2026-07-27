using AutoMapper;
using HRestaurant.DTOS.Responses;
using HRestaurant.Models.BaseModels;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Extensions;

internal static class PaginationExtensions
{
    public static async Task<PagedResponse<TDestination>>
        ToPagedResponseAsync<TEntity, TDestination>(
            this IQueryable<TEntity> query,
            IMapper mapper,
            PaginationRequest pagination,
            CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(pagination);

        ArgumentOutOfRangeException.ThrowIfLessThan(
            pagination.PageNumber,
            1);
        ArgumentOutOfRangeException.ThrowIfLessThan(
            pagination.PageSize,
            1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            pagination.PageSize,
            PaginationRequest.MaxPageSize);

        var totalCount = await query.CountAsync(cancellationToken);
        var skip = (long)(pagination.PageNumber - 1)
            * pagination.PageSize;

        List<TEntity> entities;

        if (skip >= totalCount)
        {
            entities = [];
        }
        else
        {
            entities = await query
                .OrderByDescending(entity => entity.CreatAt)
                .ThenBy(entity => entity.ID)
                .Skip((int)skip)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);
        }

        var data = mapper.Map<List<TDestination>>(entities);

        return PagedResponse<TDestination>.Create(
            data,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount);
    }
}
