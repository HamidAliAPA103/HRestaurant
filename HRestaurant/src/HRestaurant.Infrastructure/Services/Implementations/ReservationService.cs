using System.Data;
using AutoMapper;
using HRestaurant.Data;
using HRestaurant.DTOS.Reservation;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Exceptions;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class ReservationService :
    CrudServiceBase<Reservation, ReservationCreateDTO, ReservationUpdateDTO, ReservationGetDTO>,
    IReservationService
{
    private static readonly ReservationStatus[] BlockingStatuses =
        [ReservationStatus.Pending, ReservationStatus.Confirmed, ReservationStatus.Seated];
    private readonly AppDbContext _db;
    private readonly IReservationConfirmationService _confirmation;
    private readonly ITableAvailabilityService _availability;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public ReservationService(IUnitOfWork unitOfWork, IMapper mapper,
        AppDbContext dbContext, IReservationConfirmationService confirmationService,
        ITableAvailabilityService availability, ICurrentUserContext currentUser,
        TimeProvider timeProvider)
        : base(unitOfWork, mapper, "Reservation")
    {
        _db = dbContext;
        _confirmation = confirmationService;
        _availability = availability;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public override async Task<ApiResponse<Guid>> CreateAsync(
        ReservationCreateDTO dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var branch = await GetAccessibleBranchAsync(dto.BranchId, cancellationToken);
        var customer = await GetCustomerAsync(dto.CustomerId, branch.RestaurantId,
            cancellationToken);
        var table = await _db.Tables.AsNoTracking().FirstOrDefaultAsync(x =>
            x.ID == dto.TableId && x.BranchId == branch.ID && !x.IsDeleted && x.IsActive,
            cancellationToken) ?? throw new NotFoundException("Table", dto.TableId);
        if (table.Tutum < dto.GuestCount)
            throw new ConflictException("The selected table does not have enough capacity.");
        var end = dto.ReservationTime.AddMinutes(dto.DurationMinutes);
        EnsureWithinWorkingHours(branch, dto.ReservationTime, dto.DurationMinutes);
        var check = await _availability.CheckAsync(branch.ID, table.ID,
            dto.ReservationTime, end, dto.GuestCount, cancellationToken);
        if (!check.IsAvailable)
            throw new ConflictException(check.UnavailableReason
                ?? "The selected table is unavailable for this time.");
        if (dto.Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed))
            throw new ConflictException("New reservations must be Pending or Confirmed.");

        var token = _confirmation.GenerateTrackingToken();
        var reservation = Mapper.Map<Reservation>(dto);
        reservation.FullName = customer.Name;
        reservation.Email = string.IsNullOrWhiteSpace(customer.Email) ? null : customer.Email;
        reservation.PhoneNormalized = customer.NormalizedPhone ?? customer.Phone ?? string.Empty;
        reservation.ConfirmationCode = await CreateUniqueCodeAsync(cancellationToken);
        reservation.PublicTrackingTokenHash = _confirmation.HashTrackingToken(token);
        reservation.CreatAt = UtcNow;
        reservation.AuditLogs.Add(new ReservationAuditLog
        {
            Action = "CreatedByStaff",
            CreatAt = UtcNow
        });
        _db.Reservations.Add(reservation);
        _db.InventoryNotifications.Add(SystemNotificationFactory.ReservationCreated(
            reservation.ID,
            branch.RestaurantId,
            branch.ID,
            reservation.FullName,
            reservation.ConfirmationCode,
            reservation.ReservationTime,
            UtcNow));
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Created(reservation.ID, "Reservation created successfully.");
    }

    public Task<PagedResponse<ReservationGetDTO>> GetAllAsync(
        ReservationListRequest request, CancellationToken cancellationToken = default) =>
        GetListAsync(request, cancellationToken);

    public override Task<PagedResponse<ReservationGetDTO>> GetAllAsync(
        ViewType type, PaginationRequest pagination,
        CancellationToken cancellationToken = default) =>
        GetListAsync(new ReservationListRequest
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        }, cancellationToken);

    public override async Task<ApiResponse<ReservationGetDTO>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await _db.Reservations.AsNoTracking()
            .Include(x => x.Branch).FirstOrDefaultAsync(x =>
                x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Reservation", id);
        await EnsureBranchAccessAsync(reservation.Branch, cancellationToken);
        return ApiResponse.Ok(Mapper.Map<ReservationGetDTO>(reservation));
    }

    public override async Task<ApiResponse<object?>> UpdateAsync(
        Guid id, ReservationUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var reservation = await GetForMutationAsync(id, cancellationToken);
        if (reservation.Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed))
            throw new ConflictException("Only pending or confirmed reservations can be edited.");
        var branch = await GetAccessibleBranchAsync(dto.BranchId, cancellationToken);
        var customer = await GetCustomerAsync(dto.CustomerId, branch.RestaurantId,
            cancellationToken);
        var table = await _db.Tables.AsNoTracking().FirstOrDefaultAsync(x =>
            x.ID == dto.TableId && x.BranchId == branch.ID && !x.IsDeleted && x.IsActive,
            cancellationToken) ?? throw new NotFoundException("Table", dto.TableId);
        if (table.Tutum < dto.GuestCount)
            throw new ConflictException("The selected table does not have enough capacity.");
        if (table.Status is TableStatus.Disabled or TableStatus.Occupied or TableStatus.Cleaning)
            throw new ConflictException("The selected table is not available for reservations.");
        var end = dto.ReservationTime.AddMinutes(dto.DurationMinutes);
        EnsureWithinWorkingHours(branch, dto.ReservationTime, dto.DurationMinutes);
        var overlaps = await _db.Reservations.AsNoTracking().AnyAsync(x =>
            x.ID != id && x.TableId == dto.TableId && !x.IsDeleted
            && BlockingStatuses.Contains(x.Status)
            && dto.ReservationTime < x.EndTime && end > x.ReservationTime,
            cancellationToken);
        if (overlaps)
            throw new ConflictException("The selected table is already reserved for this time.");
        var currentStatus = reservation.Status;
        Mapper.Map(dto, reservation);
        reservation.Status = currentStatus;
        reservation.FullName = customer.Name;
        reservation.Email = string.IsNullOrWhiteSpace(customer.Email) ? null : customer.Email;
        reservation.PhoneNormalized = customer.NormalizedPhone ?? customer.Phone ?? string.Empty;
        reservation.UpdateAt = UtcNow;
        reservation.AuditLogs.Add(new ReservationAuditLog
        {
            Action = "UpdatedByStaff",
            CreatAt = UtcNow
        });
        _db.InventoryNotifications.Add(SystemNotificationFactory.ReservationStatusChanged(
            reservation,
            reservation.Branch.RestaurantId,
            UtcNow));
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Success("Reservation updated successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateStatusAsync(
        Guid id, ReservationStatusUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetForMutationAsync(id, cancellationToken);
        if (!AllowedTransitions(reservation.Status).Contains(dto.Status))
            throw new ConflictException(
                $"Reservation cannot change from {reservation.Status} to {dto.Status}.");
        var old = reservation.Status;
        reservation.Status = dto.Status;
        reservation.UpdateAt = UtcNow;
        if (dto.Status == ReservationStatus.Cancelled)
        {
            reservation.CancelledAt = UtcNow;
            reservation.CancellationReason = string.IsNullOrWhiteSpace(dto.Reason)
                ? "Cancelled by staff." : dto.Reason.Trim();
        }
        reservation.AuditLogs.Add(new ReservationAuditLog
        {
            Action = $"StatusChanged:{old}->{dto.Status}",
            Reason = string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim(),
            CreatAt = UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Reservation status updated successfully.");
    }

    public override async Task<ApiResponse<object?>> RemoveAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await GetForMutationAsync(id, cancellationToken);
        if (BlockingStatuses.Contains(reservation.Status))
            throw new ConflictException("An active reservation must be cancelled before deletion.");
        reservation.IsDeleted = true;
        reservation.DeletedAt = UtcNow;
        reservation.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.NoContent("Reservation deleted successfully.");
    }

    public override async Task<ApiResponse<object?>> ToggleAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await _db.Reservations.Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.ID == id, cancellationToken)
            ?? throw new NotFoundException("Reservation", id);
        await EnsureBranchAccessAsync(reservation.Branch, cancellationToken);
        if (!reservation.IsDeleted)
            return await RemoveAsync(id, cancellationToken);
        reservation.IsDeleted = false;
        reservation.DeletedAt = null;
        reservation.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Reservation restored successfully.");
    }

    private async Task<PagedResponse<ReservationGetDTO>> GetListAsync(
        ReservationListRequest request, CancellationToken cancellationToken)
    {
        var query = ApplyScope(_db.Reservations.AsNoTracking()
            .Include(x => x.Branch).Where(x => !x.IsDeleted));
        if (request.BranchId.HasValue)
        {
            var branch = await GetAccessibleBranchAsync(request.BranchId.Value,
                cancellationToken);
            query = query.Where(x => x.BranchId == branch.ID);
        }
        if (request.TableId.HasValue) query = query.Where(x => x.TableId == request.TableId);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.From.HasValue)
        {
            var from = request.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.ReservationTime >= from);
        }
        if (request.To.HasValue)
        {
            var to = request.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.ReservationTime < to);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.FullName.Contains(search)
                || x.PhoneNormalized.Contains(search)
                || x.ConfirmationCode.Contains(search));
        }
        var total = await query.CountAsync(cancellationToken);
        var data = await query.OrderBy(x => x.ReservationTime).ThenBy(x => x.ID)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return PagedResponse<ReservationGetDTO>.Create(
            Mapper.Map<List<ReservationGetDTO>>(data), request.PageNumber,
            request.PageSize, total, "Reservations retrieved successfully.");
    }

    private IQueryable<Reservation> ApplyScope(IQueryable<Reservation> query)
    {
        if (_currentUser.IsSuperAdmin) return query;
        query = query.Where(x => x.Branch.RestaurantId == _currentUser.RestaurantId);
        if (_currentUser.IsManager)
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => x.Branch.ManagerId == userId);
        }
        if (_currentUser.IsInRole(AppRoles.Waiter))
        {
            var userId = _currentUser.UserId;
            query = query.Where(x => _db.BusinessUsers.Any(user =>
                user.AppUserId == userId && user.BranchId == x.BranchId
                && user.IsActive && !user.IsDeleted));
        }
        return query;
    }

    private async Task<Reservation> GetForMutationAsync(Guid id,
        CancellationToken cancellationToken)
    {
        var reservation = await _db.Reservations.Include(x => x.Branch)
            .Include(x => x.AuditLogs).FirstOrDefaultAsync(x =>
                x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Reservation", id);
        await EnsureBranchAccessAsync(reservation.Branch, cancellationToken);
        return reservation;
    }

    private async Task<Branch> GetAccessibleBranchAsync(Guid id,
        CancellationToken cancellationToken)
    {
        var branch = await _db.Branches.AsNoTracking().Include(x => x.WorkingHours)
            .FirstOrDefaultAsync(x =>
            x.ID == id && !x.IsDeleted && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Branch", id);
        await EnsureBranchAccessAsync(branch, cancellationToken);
        return branch;
    }

    private async Task EnsureBranchAccessAsync(Branch branch,
        CancellationToken cancellationToken)
    {
        if (_currentUser.IsSuperAdmin) return;
        if (branch.RestaurantId != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant's reservation cannot be accessed.");
        if (_currentUser.IsManager && branch.ManagerId != _currentUser.UserId)
            throw new ForbiddenException("Managers can access only their own branch reservations.");
        if (_currentUser.IsInRole(AppRoles.Waiter))
        {
            var allowed = await _db.BusinessUsers.AsNoTracking().AnyAsync(x =>
                x.AppUserId == _currentUser.UserId && x.BranchId == branch.ID
                && x.IsActive && !x.IsDeleted, cancellationToken);
            if (!allowed)
                throw new ForbiddenException("Waiters can access only their own branch reservations.");
        }
    }

    private async Task<User> GetCustomerAsync(Guid id, Guid restaurantId,
        CancellationToken cancellationToken) =>
        await _db.BusinessUsers.AsNoTracking().FirstOrDefaultAsync(x =>
            x.ID == id && x.RestaurantId == restaurantId && x.Role == "Customer"
            && !x.IsDeleted && x.IsActive, cancellationToken)
        ?? throw new NotFoundException("Customer", id);

    private async Task<string> CreateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code = _confirmation.GenerateConfirmationCode();
            if (!await _db.Reservations.AsNoTracking().AnyAsync(x =>
                    x.ConfirmationCode == code, cancellationToken)) return code;
        }
        throw new ConflictException("A unique confirmation code could not be generated.");
    }

    private static IReadOnlyCollection<ReservationStatus> AllowedTransitions(
        ReservationStatus status) => status switch
    {
        ReservationStatus.Pending => [ReservationStatus.Confirmed, ReservationStatus.Cancelled],
        ReservationStatus.Confirmed => [ReservationStatus.Seated,
            ReservationStatus.Cancelled, ReservationStatus.NoShow],
        ReservationStatus.Seated => [ReservationStatus.Completed, ReservationStatus.Cancelled],
        _ => []
    };

    private static void EnsureWithinWorkingHours(
        Branch branch, DateTime startUtc, int durationMinutes)
    {
        var utc = startUtc.Kind == DateTimeKind.Utc
            ? startUtc : DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById(branch.TimeZoneId); }
        catch (TimeZoneNotFoundException)
        {
            throw new ConflictException("The branch time zone is not configured correctly.");
        }
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, zone);
        if (!ReservationSchedule.IsWithinWorkingHours(branch.WorkingHours,
                DateOnly.FromDateTime(local), TimeOnly.FromDateTime(local), durationMinutes))
            throw new ConflictException("The selected time is outside branch working hours.");
    }

    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
