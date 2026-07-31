using HRestaurant.DTOS.Audit;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IAuditLogService
{
    Task<PagedResponse<AuditLogGetDTO>> GetAllAsync(AuditLogRequest request,
        CancellationToken cancellationToken = default);
}
