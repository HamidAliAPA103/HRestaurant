using System.Data;
using HRestaurant.Configuration;
using HRestaurant.Data;
using HRestaurant.DTOS.Public;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRestaurant.Services.Implementations;

public sealed class PublicReservationService
    : IPublicReservationService
{
    private const string GenericLookupMessage =
        "Reservation information could not be verified.";
    private static readonly ReservationStatus[] CancellableStatuses =
    [
        ReservationStatus.Pending,
        ReservationStatus.Confirmed
    ];

    private readonly AppDbContext _dbContext;
    private readonly ITableAvailabilityService _availabilityService;
    private readonly IReservationConfirmationService _confirmationService;
    private readonly IReservationEmailQueue _emailQueue;
    private readonly IPublicRequestChallengeValidator _challengeValidator;
    private readonly PublicReservationSettings _settings;
    private readonly TimeProvider _timeProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PublicReservationService> _logger;

    public PublicReservationService(
        AppDbContext dbContext,
        ITableAvailabilityService availabilityService,
        IReservationConfirmationService confirmationService,
        IReservationEmailQueue emailQueue,
        IPublicRequestChallengeValidator challengeValidator,
        PublicReservationSettings settings,
        TimeProvider timeProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PublicReservationService> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(availabilityService);
        ArgumentNullException.ThrowIfNull(confirmationService);
        ArgumentNullException.ThrowIfNull(emailQueue);
        ArgumentNullException.ThrowIfNull(challengeValidator);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        ArgumentNullException.ThrowIfNull(logger);

        _dbContext = dbContext;
        _availabilityService = availabilityService;
        _confirmationService = confirmationService;
        _emailQueue = emailQueue;
        _challengeValidator = challengeValidator;
        _settings = settings;
        _timeProvider = timeProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<ApiResponse<PublicReservationCreatedDto>> CreateAsync(
        PublicCreateReservationDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await _challengeValidator.EnsureValidAsync(
            dto.CaptchaToken,
            "reservation-create",
            cancellationToken);

        Reservation reservation;
        Branch branch;
        Table table;
        string trackingToken;

        try
        {
            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            await AcquireTableLockAsync(
                dto.TableId,
                cancellationToken);

            branch = await GetActiveBranchAsync(
                dto.BranchId,
                cancellationToken);
            var (startUtc, endUtc) = ValidateAndConvertInterval(
                branch,
                dto.ReservationDate,
                dto.StartTime,
                dto.DurationMinutes);

            table = await _dbContext.Tables
                .FirstOrDefaultAsync(
                    entity =>
                        entity.ID == dto.TableId
                        && entity.BranchId == dto.BranchId
                        && entity.IsActive
                        && !entity.IsDeleted,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "The selected table was not found.");

            var availability = await _availabilityService.CheckAsync(
                dto.BranchId,
                dto.TableId,
                startUtc,
                endUtc,
                dto.GuestCount,
                cancellationToken);

            if (!availability.IsAvailable)
            {
                throw new ConflictException(
                    "The selected table is no longer available. "
                    + "Please choose another table.");
            }

            trackingToken =
                _confirmationService.GenerateTrackingToken();
            reservation = new Reservation
            {
                BranchId = branch.ID,
                TableId = table.ID,
                CustomerId = null,
                ReservationTime = startUtc,
                EndTime = endUtc,
                DurationMinutes = dto.DurationMinutes,
                GuestCount = dto.GuestCount,
                FullName = PublicInputSanitizer.SanitizeRequired(
                    dto.FullName,
                    100),
                PhoneNormalized =
                    PublicInputSanitizer.NormalizePhone(dto.Phone),
                Email = NormalizeEmail(dto.Email),
                SpecialNotes = PublicInputSanitizer.Sanitize(
                    dto.SpecialNotes,
                    500),
                ConfirmationCode =
                    await CreateUniqueConfirmationCodeAsync(
                        cancellationToken),
                PublicTrackingTokenHash =
                    _confirmationService.HashTrackingToken(
                        trackingToken),
                Status = _settings.InitialStatus,
                CreatAt = UtcNow
            };

            reservation.AuditLogs.Add(new ReservationAuditLog
            {
                Action = "Created",
                IpAddressHash = GetIpAddressHash(),
                CreatAt = UtcNow
            });

            await _dbContext.Reservations.AddAsync(
                reservation,
                cancellationToken);
            await _dbContext.InventoryNotifications.AddAsync(
                SystemNotificationFactory.ReservationCreated(
                    reservation.ID,
                    branch.RestaurantId,
                    branch.ID,
                    reservation.FullName,
                    reservation.ConfirmationCode,
                    reservation.ReservationTime,
                    UtcNow),
                cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (SqlException exception)
            when (exception.Number is 1205 or 1222 or 51000)
        {
            _logger.LogWarning(
                exception,
                "Concurrent reservation creation was rejected for "
                + "table {TableId}.",
                dto.TableId);
            throw CreateTableConflict();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogWarning(
                exception,
                "Reservation persistence conflict for table {TableId}.",
                dto.TableId);
            throw CreateTableConflict();
        }

        var localStart = ReservationSchedule.ToLocal(
            reservation.ReservationTime,
            branch.TimeZoneId);
        var localEnd = ReservationSchedule.ToLocal(
            reservation.EndTime,
            branch.TimeZoneId);
        var emailQueued = await QueueEmailAsync(
            reservation,
            branch,
            table,
            trackingToken,
            localStart,
            localEnd);

        return ApiResponse.Created(
            new PublicReservationCreatedDto
            {
                ReservationId = reservation.ID,
                ConfirmationCode = reservation.ConfirmationCode,
                TrackingToken = trackingToken,
                Status = reservation.Status.ToString(),
                RestaurantName = branch.Restaurant.Name,
                BranchName = branch.Name,
                TableNumber = table.TableNumber,
                ReservationDate =
                    DateOnly.FromDateTime(localStart),
                StartTime = TimeOnly.FromDateTime(localStart),
                EndTime = TimeOnly.FromDateTime(localEnd),
                EmailDeliveryQueued = emailQueued
            },
            "Reservation created successfully.");
    }

    public async Task<ApiResponse<PublicReservationDetailsDto>> LookupAsync(
        PublicReservationLookupRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await _challengeValidator.EnsureValidAsync(
            dto.CaptchaToken,
            "reservation-lookup",
            cancellationToken);

        var query = PublicReservationQuery();

        if (!string.IsNullOrWhiteSpace(dto.TrackingToken))
        {
            var tokenHash =
                _confirmationService.HashTrackingToken(
                    dto.TrackingToken);
            query = query.Where(reservation =>
                reservation.PublicTrackingTokenHash == tokenHash);
        }
        else
        {
            var code = NormalizeConfirmationCode(
                dto.ConfirmationCode);
            var phone = PublicInputSanitizer.NormalizePhone(
                dto.Phone ?? string.Empty);
            query = query.Where(reservation =>
                reservation.ConfirmationCode == code
                && reservation.PhoneNormalized == phone);
        }

        var reservation =
            await query.FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(GenericLookupMessage);

        return ApiResponse.Ok(
            MapDetails(reservation),
            "Reservation retrieved successfully.");
    }

    public async Task<ApiResponse<PublicReservationDetailsDto>> TrackAsync(
        string trackingToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trackingToken)
            || trackingToken.Length != 64)
        {
            throw new NotFoundException(GenericLookupMessage);
        }

        var tokenHash = _confirmationService.HashTrackingToken(
            trackingToken);
        var reservation = await PublicReservationQuery()
            .FirstOrDefaultAsync(
                entity =>
                    entity.PublicTrackingTokenHash == tokenHash,
                cancellationToken)
            ?? throw new NotFoundException(GenericLookupMessage);

        return ApiResponse.Ok(
            MapDetails(reservation),
            "Reservation retrieved successfully.");
    }

    public async Task<ApiResponse<object?>> CancelAsync(
        string confirmationCode,
        PublicCancelReservationDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await _challengeValidator.EnsureValidAsync(
            dto.CaptchaToken,
            "reservation-cancel",
            cancellationToken);

        var normalizedCode =
            NormalizeConfirmationCode(confirmationCode);
        var query = _dbContext.Reservations
            .AsNoTracking()
            .Where(reservation =>
                reservation.ConfirmationCode == normalizedCode
                && !reservation.IsDeleted);

        if (!string.IsNullOrWhiteSpace(dto.TrackingToken))
        {
            var tokenHash =
                _confirmationService.HashTrackingToken(
                    dto.TrackingToken);
            query = query.Where(reservation =>
                reservation.PublicTrackingTokenHash == tokenHash);
        }
        else
        {
            var phone = PublicInputSanitizer.NormalizePhone(
                dto.Phone ?? string.Empty);
            query = query.Where(reservation =>
                reservation.PhoneNormalized == phone);
        }

        var candidate = await query
            .Select(reservation => new
            {
                reservation.ID,
                reservation.TableId
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(GenericLookupMessage);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        await AcquireTableLockAsync(
            candidate.TableId,
            cancellationToken);

        var reservation = await _dbContext.Reservations
            .FirstOrDefaultAsync(
                entity =>
                    entity.ID == candidate.ID
                    && !entity.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException(GenericLookupMessage);

        if (!CancellableStatuses.Contains(reservation.Status))
        {
            throw new ConflictException(
                "This reservation can no longer be cancelled.");
        }

        if (reservation.ReservationTime
                .AddMinutes(-_settings.CancellationCutoffMinutes)
            <= UtcNow)
        {
            throw new ConflictException(
                "The cancellation deadline has passed.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = UtcNow;
        reservation.CancellationReason =
            PublicInputSanitizer.Sanitize(dto.Reason, 300);
        reservation.UpdateAt = UtcNow;
        reservation.AuditLogs.Add(new ReservationAuditLog
        {
            Action = "Cancelled",
            Reason = reservation.CancellationReason,
            IpAddressHash = GetIpAddressHash(),
            CreatAt = UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ApiResponse.Success(
            "Reservation cancelled successfully.");
    }

    private DateTime UtcNow =>
        _timeProvider.GetUtcNow().UtcDateTime;

    private IQueryable<Reservation> PublicReservationQuery()
    {
        return _dbContext.Reservations
            .AsNoTracking()
            .Include(reservation => reservation.Table)
            .Include(reservation => reservation.Branch)
                .ThenInclude(branch => branch.Restaurant)
            .Where(reservation => !reservation.IsDeleted);
    }

    private async Task<Branch> GetActiveBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Branches
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
            DateOnly date,
            TimeOnly startTime,
            int durationMinutes)
    {
        if (!ReservationSchedule.IsWithinWorkingHours(
                branch.WorkingHours,
                date,
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
            date,
            startTime,
            branch.TimeZoneId);

        if (startUtc <= UtcNow)
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

        return (
            startUtc,
            startUtc.AddMinutes(durationMinutes));
    }

    private async Task<string> CreateUniqueConfirmationCodeAsync(
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code =
                _confirmationService.GenerateConfirmationCode();
            var exists = await _dbContext.Reservations
                .AsNoTracking()
                .AnyAsync(
                    reservation =>
                        reservation.ConfirmationCode == code,
                    cancellationToken);

            if (!exists)
            {
                return code;
            }
        }

        throw new InvalidOperationException(
            "A unique confirmation code could not be generated.");
    }

    private async Task AcquireTableLockAsync(
        Guid tableId,
        CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsSqlServer())
        {
            return;
        }

        var resource = $"reservation-table:{tableId:N}";

        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
             DECLARE @lockResult int;
             EXEC @lockResult = sys.sp_getapplock
                 @Resource = {resource},
                 @LockMode = 'Exclusive',
                 @LockOwner = 'Transaction',
                 @LockTimeout = 10000;
             IF @lockResult < 0
                 THROW 51000, 'Reservation lock acquisition failed.', 1;
             """,
            cancellationToken);
    }

    private PublicReservationDetailsDto MapDetails(
        Reservation reservation)
    {
        var localStart = ReservationSchedule.ToLocal(
            reservation.ReservationTime,
            reservation.Branch.TimeZoneId);
        var localEnd = ReservationSchedule.ToLocal(
            reservation.EndTime,
            reservation.Branch.TimeZoneId);

        return new PublicReservationDetailsDto
        {
            ConfirmationCode = reservation.ConfirmationCode,
            Status = reservation.Status.ToString(),
            RestaurantName = reservation.Branch.Restaurant.Name,
            BranchName = reservation.Branch.Name,
            BranchAddress = reservation.Branch.Address,
            ReservationDate = DateOnly.FromDateTime(localStart),
            StartTime = TimeOnly.FromDateTime(localStart),
            EndTime = TimeOnly.FromDateTime(localEnd),
            GuestCount = reservation.GuestCount,
            TableNumber = reservation.Table.TableNumber,
            FullName = reservation.FullName,
            MaskedPhone = MaskPhone(reservation.PhoneNormalized),
            MaskedEmail = MaskEmail(reservation.Email),
            SpecialNotes = reservation.SpecialNotes,
            CanCancel = CanCancel(reservation),
            CancelledAt = reservation.CancelledAt
        };
    }

    private bool CanCancel(Reservation reservation)
    {
        return CancellableStatuses.Contains(reservation.Status)
            && reservation.ReservationTime
                .AddMinutes(-_settings.CancellationCutoffMinutes)
                > UtcNow;
    }

    private async Task<bool> QueueEmailAsync(
        Reservation reservation,
        Branch branch,
        Table table,
        string trackingToken,
        DateTime localStart,
        DateTime localEnd)
    {
        if (string.IsNullOrWhiteSpace(reservation.Email))
        {
            return false;
        }

        var publicBaseUrl = _settings.PublicBaseUrl.TrimEnd('/');
        var encodedToken = Uri.EscapeDataString(trackingToken);
        var trackingUrl =
            $"{publicBaseUrl}/reservation/track?token={encodedToken}";
        var cancellationUrl =
            $"{trackingUrl}&action=cancel";

        try
        {
            await _emailQueue.QueueAsync(
                new ReservationEmailMessage(
                    reservation.Email,
                    reservation.FullName,
                    reservation.ConfirmationCode,
                    branch.Restaurant.Name,
                    branch.Name,
                    branch.Address,
                    DateOnly.FromDateTime(localStart),
                    TimeOnly.FromDateTime(localStart),
                    TimeOnly.FromDateTime(localEnd),
                    reservation.GuestCount,
                    table.TableNumber,
                    trackingUrl,
                    cancellationUrl),
                CancellationToken.None);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Reservation was created, but email queueing failed. "
                + "ReservationId: {ReservationId}",
                reservation.ID);
            return false;
        }
    }

    private string? GetIpAddressHash()
    {
        var address = _httpContextAccessor
            .HttpContext?
            .Connection
            .RemoteIpAddress?
            .ToString();

        return string.IsNullOrWhiteSpace(address)
            ? null
            : _confirmationService.HashTrackingToken(address);
    }

    private static string NormalizeConfirmationCode(string? value)
    {
        return value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim().ToLowerInvariant();
    }

    private static string MaskPhone(string phone)
    {
        if (phone.Length <= 4)
        {
            return new string('*', phone.Length);
        }

        var prefixLength = phone.StartsWith('+') ? 4 : 3;
        prefixLength = Math.Min(prefixLength, phone.Length - 2);

        return string.Concat(
            phone.AsSpan(0, prefixLength),
            new string('*', phone.Length - prefixLength - 2),
            phone.AsSpan(phone.Length - 2));
    }

    private static string? MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var atIndex = email.IndexOf('@');

        if (atIndex <= 0)
        {
            return "***";
        }

        return $"{email[0]}***{email[atIndex..]}";
    }

    private static ConflictException CreateTableConflict()
    {
        return new ConflictException(
            "The selected table is no longer available. "
            + "Please refresh availability and choose another table.");
    }
}
