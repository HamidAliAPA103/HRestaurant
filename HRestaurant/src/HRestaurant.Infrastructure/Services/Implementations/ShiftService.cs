using AutoMapper;
using System.Data;
using HRestaurant.Data;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Shift;
using HRestaurant.Exceptions;
using HRestaurant.Models;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class ShiftService : IShiftService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public ShiftService(
        AppDbContext db,
        IMapper mapper,
        ICurrentUserContext currentUser,
        TimeProvider timeProvider)
    {
        _db = db;
        _mapper = mapper;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<ApiResponse<Guid>> CreateAsync(
        ShiftCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        EnsureTimeRange(dto.StartTime, dto.EndTime);
        await EnsureBranchAccessAsync(dto.BranchId, dto.RestaurantId, cancellationToken);
        var entity = _mapper.Map<Shift>(dto);
        entity.Name = dto.Name.Trim();
        entity.CreatAt = UtcNow;
        _db.Shifts.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Created(entity.ID, "Shift created successfully.");
    }

    public async Task<ApiResponse<ShiftGetDTO>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await ShiftQuery().FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted && !x.Branch.IsDeleted,
            cancellationToken) ?? throw new NotFoundException("Shift", id);
        await EnsureBranchAccessAsync(entity.BranchId, entity.RestaurantId, cancellationToken);
        return ApiResponse.Ok(_mapper.Map<ShiftGetDTO>(entity));
    }

    public async Task<PagedResponse<ShiftGetDTO>> GetShiftsAsync(
        ShiftListRequest request,
        CancellationToken cancellationToken = default)
    {
        var restaurantId = ResolveRestaurantId(request.RestaurantId);
        var query = ShiftQuery().Where(x => !x.IsDeleted && !x.Branch.IsDeleted);
        if (restaurantId.HasValue) query = query.Where(x => x.RestaurantId == restaurantId.Value);
        if (request.BranchId.HasValue)
        {
            await EnsureBranchAccessByIdAsync(request.BranchId.Value, cancellationToken);
            query = query.Where(x => x.BranchId == request.BranchId.Value);
        }
        if (_currentUser.IsManager)
        {
            var branches = await ManagedBranchIdsAsync(cancellationToken);
            query = query.Where(x => branches.Contains(x.BranchId));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Branch.Name).ThenBy(x => x.StartTime)
            .ThenBy(x => x.Name).Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize).ToListAsync(cancellationToken);
        return PagedResponse<ShiftGetDTO>.Create(
            _mapper.Map<List<ShiftGetDTO>>(items), request.PageNumber, request.PageSize,
            total, "Shifts retrieved successfully.");
    }

    public async Task<ApiResponse<object?>> UpdateAsync(
        Guid id,
        ShiftUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        EnsureTimeRange(dto.StartTime, dto.EndTime);
        var entity = await GetForMutationAsync(id, cancellationToken);
        entity.Name = dto.Name.Trim();
        entity.StartTime = dto.StartTime;
        entity.EndTime = dto.EndTime;
        entity.IsActive = dto.IsActive;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Shift updated successfully.");
    }

    public async Task<ApiResponse<object?>> SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Shifts.Include(x => x.EmployeeShifts)
            .FirstOrDefaultAsync(x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Shift", id);
        await EnsureBranchAccessAsync(entity.BranchId, entity.RestaurantId, cancellationToken);
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var now = UtcNow;
        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.DeletedAt = now;
        entity.UpdateAt = now;
        foreach (var assignment in entity.EmployeeShifts.Where(x => !x.IsDeleted))
        {
            assignment.IsDeleted = true;
            assignment.DeletedAt = now;
            assignment.UpdateAt = now;
        }
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.NoContent("Shift deleted successfully.");
    }

    public async Task<ApiResponse<Guid>> AssignEmployeeAsync(
        EmployeeShiftAssignDTO dto,
        CancellationToken cancellationToken = default)
    {
        var shift = await _db.Shifts.AsNoTracking().FirstOrDefaultAsync(
            x => x.ID == dto.ShiftId && !x.IsDeleted && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Active shift", dto.ShiftId);
        await EnsureBranchAccessAsync(shift.BranchId, shift.RestaurantId, cancellationToken);

        var employee = await _db.BusinessUsers.AsNoTracking().FirstOrDefaultAsync(x =>
            x.ID == dto.EmployeeId && x.RestaurantId != null && !x.IsDeleted && x.IsActive,
            cancellationToken) ?? throw new ConflictException("The employee does not exist or is inactive.");
        if (employee.RestaurantId != shift.RestaurantId || employee.BranchId != shift.BranchId)
            throw new ConflictException("The employee can only be assigned to a shift in their own branch.");

        var start = dto.StartTime ?? shift.StartTime;
        var end = dto.EndTime ?? shift.EndTime;
        EnsureTimeRange(start, end);

        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        if (await _db.EmployeeShifts.AsNoTracking().AnyAsync(x =>
                x.EmployeeId == dto.EmployeeId && x.ShiftId == dto.ShiftId
                && x.WorkDate == dto.WorkDate && !x.IsDeleted, cancellationToken))
            throw new ConflictException("This employee already has the same shift assignment on this date.");

        var overlaps = await _db.EmployeeShifts.AsNoTracking().AnyAsync(x =>
            x.EmployeeId == dto.EmployeeId && x.WorkDate == dto.WorkDate && !x.IsDeleted
            && start < x.EndTime && end > x.StartTime, cancellationToken);
        if (overlaps)
            throw new ConflictException("The shift overlaps an existing employee shift.");

        var entity = new EmployeeShift
        {
            EmployeeId = dto.EmployeeId,
            ShiftId = dto.ShiftId,
            WorkDate = dto.WorkDate,
            StartTime = start,
            EndTime = end,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            Status = dto.Status,
            CreatAt = UtcNow
        };
        _db.EmployeeShifts.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ApiResponse.Created(entity.ID, "Employee assigned to shift successfully.");
    }

    public async Task<ApiResponse<object?>> RemoveEmployeeAsync(
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.EmployeeShifts.Include(x => x.Shift).FirstOrDefaultAsync(
            x => x.ID == assignmentId && !x.IsDeleted && !x.Shift.IsDeleted,
            cancellationToken) ?? throw new NotFoundException("Employee shift assignment", assignmentId);
        await EnsureBranchAccessAsync(entity.Shift.BranchId, entity.Shift.RestaurantId, cancellationToken);
        entity.IsDeleted = true;
        entity.DeletedAt = UtcNow;
        entity.UpdateAt = UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.NoContent("Employee removed from shift successfully.");
    }

    public async Task<PagedResponse<EmployeeShiftGetDTO>> GetAssignmentsAsync(
        ShiftListRequest request,
        CancellationToken cancellationToken = default)
    {
        var restaurantId = ResolveRestaurantId(request.RestaurantId);
        var query = _db.EmployeeShifts.AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.Shift).ThenInclude(x => x.Branch)
            .AsSplitQuery()
            .Where(x => !x.IsDeleted && !x.Shift.IsDeleted && !x.Employee.IsDeleted);

        if (restaurantId.HasValue) query = query.Where(x => x.Shift.RestaurantId == restaurantId.Value);
        if (request.BranchId.HasValue)
        {
            await EnsureBranchAccessByIdAsync(request.BranchId.Value, cancellationToken);
            query = query.Where(x => x.Shift.BranchId == request.BranchId.Value);
        }
        if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
        if (request.FromDate.HasValue) query = query.Where(x => x.WorkDate >= request.FromDate.Value);
        if (request.ToDate.HasValue) query = query.Where(x => x.WorkDate <= request.ToDate.Value);
        if (_currentUser.IsManager)
        {
            var branches = await ManagedBranchIdsAsync(cancellationToken);
            query = query.Where(x => branches.Contains(x.Shift.BranchId));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.WorkDate).ThenBy(x => x.StartTime)
            .ThenBy(x => x.Employee.Name).Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize).ToListAsync(cancellationToken);
        return PagedResponse<EmployeeShiftGetDTO>.Create(
            _mapper.Map<List<EmployeeShiftGetDTO>>(items), request.PageNumber,
            request.PageSize, total, "Employee shifts retrieved successfully.");
    }

    private IQueryable<Shift> ShiftQuery() => _db.Shifts.AsNoTracking()
        .Include(x => x.Branch);

    private async Task<Shift> GetForMutationAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.Shifts.FirstOrDefaultAsync(
            x => x.ID == id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Shift", id);
        await EnsureBranchAccessAsync(entity.BranchId, entity.RestaurantId, cancellationToken);
        return entity;
    }

    private Guid? ResolveRestaurantId(Guid? requested)
    {
        if (_currentUser.IsSuperAdmin) return requested;
        if (requested.HasValue && requested.Value != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant's shifts cannot be accessed.");
        return _currentUser.RestaurantId;
    }

    private async Task EnsureBranchAccessByIdAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(
            x => x.ID == branchId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Branch", branchId);
        await EnsureBranchAccessAsync(branch.ID, branch.RestaurantId, cancellationToken);
    }

    private async Task EnsureBranchAccessAsync(
        Guid branchId, Guid restaurantId, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsSuperAdmin && restaurantId != _currentUser.RestaurantId)
            throw new ForbiddenException("Another restaurant's shifts cannot be accessed or modified.");
        if (!await _db.Branches.AsNoTracking().AnyAsync(x =>
                x.ID == branchId && x.RestaurantId == restaurantId && !x.IsDeleted,
                cancellationToken)) throw new NotFoundException("Branch", branchId);
        if (_currentUser.IsManager)
        {
            var branchIds = await ManagedBranchIdsAsync(cancellationToken);
            if (!branchIds.Contains(branchId))
                throw new ForbiddenException("Managers can manage shifts only in their own branch.");
        }
    }

    private Task<List<Guid>> ManagedBranchIdsAsync(CancellationToken cancellationToken) =>
        _db.Branches.AsNoTracking().Where(x => x.ManagerId == _currentUser.UserId && !x.IsDeleted)
            .Select(x => x.ID).ToListAsync(cancellationToken);

    private static void EnsureTimeRange(TimeOnly start, TimeOnly end)
    {
        if (end <= start)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(end)] = ["EndTime must be after StartTime."]
            });
    }

    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
