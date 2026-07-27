using AutoMapper;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Extensions;
using HRestaurant.Models.BaseModels;
using HRestaurant.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public abstract class CrudServiceBase<
    TEntity,
    TCreate,
    TUpdate,
    TGet>
    where TEntity : BaseEntity
{
    protected CrudServiceBase(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        string resourceName)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);

        UnitOfWork = unitOfWork;
        Mapper = mapper;
        ResourceName = resourceName;
    }

    protected IUnitOfWork UnitOfWork { get; }

    protected IMapper Mapper { get; }

    protected IGenericRepository<TEntity> Repository =>
        UnitOfWork.Repository<TEntity>();

    protected string ResourceName { get; }

    public virtual async Task<ApiResponse<Guid>> CreateAsync(
        TCreate dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var entity = Mapper.Map<TEntity>(dto);

        await Repository.AddAsync(entity, cancellationToken);
        var saveCount = await UnitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? ApiResponse.Created(
                entity.ID,
                $"{ResourceName} created successfully.")
            : ApiResponse.PersistenceFailure<Guid>();
    }

    public virtual Task<PagedResponse<TGet>> GetAllAsync(
        ViewType type,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pagination);

        var query = Repository.GetQueryable();

        query = type switch
        {
            ViewType.deleted =>
                query.Where(entity => entity.IsDeleted),
            ViewType.notdeleted =>
                query.Where(entity => !entity.IsDeleted),
            _ => query
        };

        return query.ToPagedResponseAsync<TEntity, TGet>(
            Mapper,
            pagination,
            cancellationToken);
    }

    public virtual async Task<ApiResponse<TGet>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository
            .GetQueryable()
            .FirstOrDefaultAsync(
                item => !item.IsDeleted && item.ID == id,
                cancellationToken);

        return entity is null
            ? ApiResponse.NotFound<TGet>(ResourceName)
            : ApiResponse.Ok(
                Mapper.Map<TGet>(entity),
                $"{ResourceName} retrieved successfully.");
    }

    public virtual async Task<ApiResponse<object?>> RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            return ApiResponse.NotFound<object?>(ResourceName);
        }

        Repository.Delete(entity);
        var saveCount = await UnitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? ApiResponse.NoContent(
                $"{ResourceName} deleted successfully.")
            : ApiResponse.PersistenceFailure<object?>();
    }

    public virtual async Task<ApiResponse<object?>> ToggleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            return ApiResponse.NotFound<object?>(ResourceName);
        }

        entity.IsDeleted = !entity.IsDeleted;
        entity.DeletedAt = entity.IsDeleted ? DateTime.UtcNow : null;
        entity.UpdateAt = DateTime.UtcNow;

        Repository.Update(entity);
        var saveCount = await UnitOfWork.SaveChangesAsync(cancellationToken);

        if (saveCount <= 0)
        {
            return ApiResponse.PersistenceFailure<object?>();
        }

        return entity.IsDeleted
            ? ApiResponse.NoContent(
                $"{ResourceName} archived successfully.")
            : ApiResponse.Success(
                $"{ResourceName} restored successfully.");
    }

    public virtual async Task<ApiResponse<object?>> UpdateAsync(
        Guid id,
        TUpdate dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var entity = await Repository
            .GetQueryable()
            .FirstOrDefaultAsync(
                item => !item.IsDeleted && item.ID == id,
                cancellationToken);

        if (entity is null)
        {
            return ApiResponse.NotFound<object?>(ResourceName);
        }

        Mapper.Map(dto, entity);
        entity.UpdateAt = DateTime.UtcNow;

        Repository.Update(entity);
        var saveCount = await UnitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? ApiResponse.Success(
                $"{ResourceName} updated successfully.")
            : ApiResponse.PersistenceFailure<object?>();
    }
}
