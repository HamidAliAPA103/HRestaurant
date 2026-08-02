using HRestaurant.Data;
using HRestaurant.DTOS.Audit;
using HRestaurant.DTOS.Responses;
using HRestaurant.Exceptions;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public AuditLogService(AppDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResponse<AuditLogGetDTO>> GetAllAsync(
        AuditLogRequest request, CancellationToken cancellationToken = default)
    {
        var query = _db.AuditLogs.AsNoTracking().Where(x => !x.IsDeleted);
        if (!_currentUser.IsSuperAdmin)
        {
            var restaurantId = _currentUser.RestaurantId;
            query = query.Where(x => x.UserId.HasValue && _db.Users.Any(user =>
                user.Id == x.UserId.Value && user.RestaurantId == restaurantId));
        }
        if (request.UserId.HasValue) query = query.Where(x => x.UserId == request.UserId);
        if (!string.IsNullOrWhiteSpace(request.EntityName))
            query = query.Where(x => x.EntityName == request.EntityName.Trim());
        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(x => x.Action == request.Action.Trim());
        if (request.From.HasValue)
        {
            var from = request.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.CreatAt >= from);
        }
        if (request.To.HasValue)
        {
            var to = request.To.Value.AddDays(1)
                .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.CreatAt < to);
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(x => x.CreatAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize).ToListAsync(cancellationToken);
        var userIds = rows.Where(x => x.UserId.HasValue).Select(x => x.UserId!.Value)
            .Distinct().ToArray();
        var names = await _db.Users.AsNoTracking().Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);
        var data = rows.Select(x => new AuditLogGetDTO
        {
            Id = x.ID,
            UserId = x.UserId,
            UserName = x.UserId.HasValue ? names.GetValueOrDefault(x.UserId.Value) : null,
            Action = x.Action,
            EntityName = x.EntityName,
            EntityId = x.EntityId,
            OldValues = x.OldValues,
            NewValues = x.NewValues,
            IpAddress = x.IpAddress,
            UserAgent = x.UserAgent,
            CreatedAt = x.CreatAt
        }).ToList();
        return PagedResponse<AuditLogGetDTO>.Create(data, request.PageNumber,
            request.PageSize, total, "Audit logs retrieved successfully.");
    }
}
