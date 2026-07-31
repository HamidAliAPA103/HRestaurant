using AutoMapper;
using HRestaurant.Data;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Supplier;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class SupplierService : ISupplierService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public SupplierService(AppDbContext db, IMapper mapper,
        ICurrentUserContext currentUser, TimeProvider timeProvider)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<Guid>> CreateAsync(
        SupplierCreateDTO dto, CancellationToken cancellationToken = default)
    {
        EnsureRestaurantAccess(dto.RestaurantId);
        if (!await _db.Restaurants.AsNoTracking().AnyAsync(x =>
                x.ID == dto.RestaurantId && !x.IsDeleted, cancellationToken))
            throw new NotFoundException("Restaurant", dto.RestaurantId);
        var name = dto.Name.Trim();
        var normalized = Normalize(name);
        await EnsureUniqueAsync(dto.RestaurantId, normalized, null, cancellationToken);
        var entity = _mapper.Map<Supplier>(dto);
        entity.Name = name;
        entity.NormalizedName = normalized;
        NormalizeContact(entity);
        entity.CreatAt = UtcNow;
        _db.Suppliers.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Created(entity.ID, "Supplier created successfully.");
    }

    public Task<PagedResponse<SupplierGetDTO>> GetAllAsync(
        SupplierListRequest request, CancellationToken cancellationToken = default) =>
        GetListAsync(request, null, cancellationToken);

    public Task<PagedResponse<SupplierGetDTO>> GetByRestaurantAsync(
        Guid restaurantId, SupplierListRequest request,
        CancellationToken cancellationToken = default) =>
        GetListAsync(request, restaurantId, cancellationToken);

    public async Task<ApiResponse<SupplierGetDTO>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Supplier", id);
        EnsureRestaurantAccess(entity.RestaurantId);
        return ApiResponse.Ok(_mapper.Map<SupplierGetDTO>(entity));
    }

    public async Task<ApiResponse<object?>> UpdateAsync(
        Guid id, SupplierUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        var name = dto.Name.Trim();
        var normalized = Normalize(name);
        await EnsureUniqueAsync(entity.RestaurantId, normalized, entity.ID, cancellationToken);
        _mapper.Map(dto, entity);
        entity.Name = name;
        entity.NormalizedName = normalized;
        NormalizeContact(entity);
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Supplier updated successfully.");
    }

    public async Task<ApiResponse<object?>> SoftDeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.DeletedAt = UtcNow;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.NoContent("Supplier deleted successfully.");
    }

    public Task<ApiResponse<object?>> ActivateAsync(
        Guid id, CancellationToken cancellationToken = default) =>
        SetActiveAsync(id, true, cancellationToken);

    public Task<ApiResponse<object?>> DeactivateAsync(
        Guid id, CancellationToken cancellationToken = default) =>
        SetActiveAsync(id, false, cancellationToken);

    private async Task<PagedResponse<SupplierGetDTO>> GetListAsync(
        SupplierListRequest request, Guid? fixedRestaurantId,
        CancellationToken cancellationToken)
    {
        var restaurantId = fixedRestaurantId ?? request.RestaurantId;
        if (!_currentUser.IsSuperAdmin)
        {
            if (restaurantId.HasValue) EnsureRestaurantAccess(restaurantId.Value);
            restaurantId = _currentUser.RestaurantId;
        }
        var query = _db.Suppliers.AsNoTracking().Where(x => !x.IsDeleted);
        if (restaurantId.HasValue) query = query.Where(x => x.RestaurantId == restaurantId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            var normalized = Normalize(search);
            query = query.Where(x => x.NormalizedName.Contains(normalized)
                || x.ContactPerson.Contains(search) || x.Email.Contains(search)
                || x.Phone.Contains(search));
        }
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Name).ThenBy(x => x.ID)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return PagedResponse<SupplierGetDTO>.Create(
            _mapper.Map<List<SupplierGetDTO>>(items), request.PageNumber,
            request.PageSize, total, "Suppliers retrieved successfully.");
    }

    private async Task<Supplier> GetForMutationAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.Suppliers.FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Supplier", id);
        EnsureRestaurantAccess(entity.RestaurantId);
        return entity;
    }

    private async Task<ApiResponse<object?>> SetActiveAsync(
        Guid id, bool active, CancellationToken cancellationToken)
    {
        var entity = await GetForMutationAsync(id, cancellationToken);
        entity.IsActive = active;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success(active ? "Supplier activated successfully." : "Supplier deactivated successfully.");
    }

    private async Task EnsureUniqueAsync(
        Guid restaurantId, string normalized, Guid? excludedId,
        CancellationToken cancellationToken)
    {
        if (await _db.Suppliers.AsNoTracking().AnyAsync(x =>
                x.RestaurantId == restaurantId && x.NormalizedName == normalized
                && !x.IsDeleted && (!excludedId.HasValue || x.ID != excludedId.Value),
                cancellationToken))
            throw new ConflictException("A supplier with the same name already exists in this restaurant.");
    }

    private void EnsureRestaurantAccess(Guid restaurantId)
    {
        if (!_currentUser.IsSuperAdmin && _currentUser.RestaurantId != restaurantId)
            throw new ForbiddenException("Another restaurant's suppliers cannot be accessed or modified.");
    }

    private static void NormalizeContact(Supplier entity)
    {
        entity.ContactPerson = entity.ContactPerson.Trim();
        entity.Phone = entity.Phone.Trim();
        entity.Email = entity.Email.Trim().ToLowerInvariant();
        entity.Address = entity.Address.Trim();
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
