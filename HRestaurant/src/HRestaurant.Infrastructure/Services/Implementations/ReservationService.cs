using AutoMapper;
using HRestaurant.Data;
using HRestaurant.DTOS.Reservation;
using HRestaurant.DTOS.Responses;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class ReservationService :
    CrudServiceBase<
        Reservation,
        ReservationCreateDTO,
        ReservationUpdateDTO,
        ReservationGetDTO>,
    IReservationService
{
    private readonly AppDbContext _dbContext;
    private readonly IReservationConfirmationService _confirmationService;
    private readonly TimeProvider _timeProvider;

    public ReservationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        AppDbContext dbContext,
        IReservationConfirmationService confirmationService,
        TimeProvider timeProvider)
        : base(unitOfWork, mapper, "Reservation")
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(confirmationService);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dbContext = dbContext;
        _confirmationService = confirmationService;
        _timeProvider = timeProvider;
    }

    public override async Task<ApiResponse<Guid>> CreateAsync(
        ReservationCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var table = await _dbContext.Tables
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity =>
                    entity.ID == dto.TableId
                    && entity.BranchId == dto.BranchId
                    && !entity.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException(
                "The selected table was not found.");
        var customer = await _dbContext.BusinessUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity =>
                    entity.ID == dto.CustomerId
                    && !entity.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException(
                "The selected customer was not found.");

        var trackingToken =
            _confirmationService.GenerateTrackingToken();
        var reservation = Mapper.Map<Reservation>(dto);
        reservation.FullName = customer.Name;
        reservation.Email = customer.Email;
        reservation.PhoneNormalized = string.Empty;
        reservation.ConfirmationCode =
            await CreateUniqueConfirmationCodeAsync(
                cancellationToken);
        reservation.PublicTrackingTokenHash =
            _confirmationService.HashTrackingToken(trackingToken);
        reservation.CreatAt =
            _timeProvider.GetUtcNow().UtcDateTime;
        reservation.AuditLogs.Add(new ReservationAuditLog
        {
            Action = "CreatedByStaff",
            CreatAt = reservation.CreatAt
        });

        await _dbContext.Reservations.AddAsync(
            reservation,
            cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse.Created(
            reservation.ID,
            "Reservation created successfully.");
    }

    private async Task<string> CreateUniqueConfirmationCodeAsync(
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code =
                _confirmationService.GenerateConfirmationCode();

            if (!await _dbContext.Reservations
                    .AsNoTracking()
                    .AnyAsync(
                        reservation =>
                            reservation.ConfirmationCode == code,
                        cancellationToken))
            {
                return code;
            }
        }

        throw new ConflictException(
            "A unique confirmation code could not be generated.");
    }
}
