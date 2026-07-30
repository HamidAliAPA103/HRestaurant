using AutoMapper;
using HRestaurant.Configuration;
using HRestaurant.Data;
using HRestaurant.DTOS.Public;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class TableAvailabilityService
    : ITableAvailabilityService
{
    private static readonly ReservationStatus[] BlockingStatuses =
    [
        ReservationStatus.Pending,
        ReservationStatus.Confirmed,
        ReservationStatus.Seated
    ];

    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly PublicReservationSettings _settings;
    private readonly TimeProvider _timeProvider;

    public TableAvailabilityService(
        AppDbContext dbContext,
        IMapper mapper,
        PublicReservationSettings settings,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dbContext = dbContext;
        _mapper = mapper;
        _settings = settings;
        _timeProvider = timeProvider;
    }

    public async Task<
        ApiResponse<IReadOnlyCollection<PublicRestaurantTableDto>>>
        GetTablesAsync(
            Guid branchId,
            TableAvailabilityRequestDto request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var branch = await GetActiveBranchAsync(
            branchId,
            cancellationToken);
        var (startUtc, endUtc) = ValidateAndConvertInterval(
            branch,
            request.ReservationDate,
            request.StartTime,
            request.DurationMinutes);

        var tables = await _dbContext.Tables
            .AsNoTracking()
            .Where(table =>
                table.BranchId == branchId
                && table.IsActive
                && !table.IsDeleted)
            .OrderBy(table => table.TableNumber)
            .ToArrayAsync(cancellationToken);

        var reservedTableIds = await GetReservedTableIdsAsync(
            branchId,
            startUtc,
            endUtc,
            cancellationToken);

        var response = tables
            .Select(table => MapTable(
                table,
                request.GuestCount,
                reservedTableIds.Contains(table.ID)))
            .ToArray();

        return ApiResponse.Ok<IReadOnlyCollection<
            PublicRestaurantTableDto>>(
            response,
            "Table availability retrieved successfully.");
    }

    public async Task<TableAvailabilityCheckResult> CheckAsync(
        Guid branchId,
        Guid tableId,
        DateTime startUtc,
        DateTime endUtc,
        int guestCount,
        CancellationToken cancellationToken = default)
    {
        var table = await _dbContext.Tables
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity =>
                    entity.ID == tableId
                    && entity.BranchId == branchId
                    && entity.IsActive
                    && !entity.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException(
                "The selected table was not found.");

        var statusReason = GetStatusReason(table, guestCount);

        if (statusReason is not null)
        {
            return new TableAvailabilityCheckResult(
                false,
                statusReason);
        }

        var isReserved = await HasOverlappingReservationAsync(
            branchId,
            tableId,
            startUtc,
            endUtc,
            cancellationToken);

        return isReserved
            ? new TableAvailabilityCheckResult(false, "Reserved")
            : new TableAvailabilityCheckResult(true, null);
    }

    private async Task<Branch> GetActiveBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Branches
                .AsNoTracking()
                .Include(branch => branch.WorkingHours)
                .Include(branch => branch.Restaurant)
                .FirstOrDefaultAsync(
                    branch =>
                        branch.ID == branchId
                        && branch.IsActive
                        && !branch.IsDeleted
                        && branch.Restaurant.IsActive
                        && !branch.Restaurant.IsDeleted,
                    cancellationToken)
            ?? throw new NotFoundException(
                "The selected branch was not found.");
    }

    private (DateTime StartUtc, DateTime EndUtc)
        ValidateAndConvertInterval(
            Branch branch,
            DateOnly reservationDate,
            TimeOnly startTime,
            int durationMinutes)
    {
        if (!ReservationSchedule.IsWithinWorkingHours(
                branch.WorkingHours,
                reservationDate,
                startTime,
                durationMinutes))
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["startTime"] =
                    [
                        "The selected time is outside branch working hours."
                    ]
                });
        }

        var startUtc = ReservationSchedule.ToUtc(
            reservationDate,
            startTime,
            branch.TimeZoneId);
        var endUtc = startUtc.AddMinutes(durationMinutes);

        if (startUtc <= _timeProvider.GetUtcNow().UtcDateTime)
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["startTime"] =
                    [
                        "The reservation start time must be in the future."
                    ]
                });
        }

        return (startUtc, endUtc);
    }

    private async Task<HashSet<Guid>> GetReservedTableIdsAsync(
        Guid branchId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken)
    {
        var bufferedEnd = endUtc.AddMinutes(_settings.BufferMinutes);

        var ids = await _dbContext.Reservations
            .AsNoTracking()
            .Where(reservation =>
                reservation.BranchId == branchId
                && !reservation.IsDeleted
                && BlockingStatuses.Contains(reservation.Status)
                && reservation.ReservationTime < bufferedEnd
                && reservation.EndTime
                    .AddMinutes(_settings.BufferMinutes) > startUtc)
            .Select(reservation => reservation.TableId)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        return ids.ToHashSet();
    }

    private Task<bool> HasOverlappingReservationAsync(
        Guid branchId,
        Guid tableId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken)
    {
        var bufferedEnd = endUtc.AddMinutes(_settings.BufferMinutes);

        return _dbContext.Reservations
            .AsNoTracking()
            .AnyAsync(
                reservation =>
                    reservation.BranchId == branchId
                    && reservation.TableId == tableId
                    && !reservation.IsDeleted
                    && BlockingStatuses.Contains(reservation.Status)
                    && reservation.ReservationTime < bufferedEnd
                    && reservation.EndTime
                        .AddMinutes(_settings.BufferMinutes) > startUtc,
                cancellationToken);
    }

    private PublicRestaurantTableDto MapTable(
        Table table,
        int guestCount,
        bool hasReservation)
    {
        var dto = _mapper.Map<PublicRestaurantTableDto>(table);
        var reason = GetStatusReason(table, guestCount)
            ?? (hasReservation ? "Reserved" : null);

        dto.IsAvailable = reason is null;
        dto.UnavailableReason = reason;
        dto.Status = reason ?? "Available";

        return dto;
    }

    private static string? GetStatusReason(
        Table table,
        int guestCount)
    {
        if (table.Status == TableStatus.Disabled)
        {
            return "Disabled";
        }

        if (table.Status == TableStatus.Occupied)
        {
            return "Occupied";
        }

        if (table.Status == TableStatus.Reserved)
        {
            return "Reserved";
        }

        return table.Tutum < guestCount
            ? "CapacityNotSuitable"
            : null;
    }
}
