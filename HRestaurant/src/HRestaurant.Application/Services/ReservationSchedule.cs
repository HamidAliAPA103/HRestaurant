using HRestaurant.Models;

namespace HRestaurant.Services;

public static class ReservationSchedule
{
    public static DateTime ToUtc(
        DateOnly date,
        TimeOnly time,
        string timeZoneId)
    {
        var local = DateTime.SpecifyKind(
            date.ToDateTime(time),
            DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(
            local,
            ResolveTimeZone(timeZoneId));
    }

    public static DateTime ToLocal(
        DateTime utc,
        string timeZoneId)
    {
        var normalized = utc.Kind == DateTimeKind.Utc
            ? utc
            : DateTime.SpecifyKind(utc, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(
            normalized,
            ResolveTimeZone(timeZoneId));
    }

    public static bool IsWithinWorkingHours(
        IEnumerable<BranchWorkingHour> workingHours,
        DateOnly date,
        TimeOnly startTime,
        int durationMinutes)
    {
        if (durationMinutes <= 0)
        {
            return false;
        }

        var requestedStart = date.ToDateTime(startTime);
        var requestedEnd = requestedStart.AddMinutes(durationMinutes);
        var hoursByDay = workingHours
            .Where(entry =>
                !entry.IsDeleted
                && !entry.IsClosed
                && entry.OpensAt.HasValue
                && entry.ClosesAt.HasValue)
            .ToDictionary(entry => entry.DayOfWeek);

        return IsInsideSchedule(
                hoursByDay,
                date,
                requestedStart,
                requestedEnd)
            || IsInsideSchedule(
                hoursByDay,
                date.AddDays(-1),
                requestedStart,
                requestedEnd);
    }

    public static bool IsOpenNow(
        IEnumerable<BranchWorkingHour> workingHours,
        DateTime utcNow,
        string timeZoneId)
    {
        var localNow = ToLocal(utcNow, timeZoneId);

        return IsWithinWorkingHours(
            workingHours,
            DateOnly.FromDateTime(localNow),
            TimeOnly.FromDateTime(localNow),
            1);
    }

    public static bool IsOpenNow(
        IEnumerable<RestaurantWorkingHour> workingHours,
        DateTime utcNow,
        string timeZoneId)
    {
        var branchHours = workingHours.Select(entry =>
            new BranchWorkingHour
            {
                DayOfWeek = entry.DayOfWeek,
                OpensAt = entry.OpensAt,
                ClosesAt = entry.ClosesAt,
                IsClosed = entry.IsClosed,
                IsDeleted = entry.IsDeleted
            });

        return IsOpenNow(branchHours, utcNow, timeZoneId);
    }

    private static bool IsInsideSchedule(
        IReadOnlyDictionary<DayOfWeek, BranchWorkingHour> hoursByDay,
        DateOnly scheduleDate,
        DateTime requestedStart,
        DateTime requestedEnd)
    {
        if (!hoursByDay.TryGetValue(
                scheduleDate.DayOfWeek,
                out var schedule))
        {
            return false;
        }

        var opensAt = scheduleDate.ToDateTime(
            schedule.OpensAt!.Value);
        var closesAt = scheduleDate.ToDateTime(
            schedule.ClosesAt!.Value);

        if (closesAt <= opensAt)
        {
            closesAt = closesAt.AddDays(1);
        }

        return requestedStart >= opensAt
            && requestedEnd <= closesAt;
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
            when (string.Equals(
                timeZoneId,
                "Asia/Baku",
                StringComparison.OrdinalIgnoreCase))
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                "Azerbaijan Standard Time");
        }
    }
}
