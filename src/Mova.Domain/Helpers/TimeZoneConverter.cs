namespace Mova.Domain.Helpers;

public static class TimeZoneConverter
{
    public const int MaxTimeZoneIdLength = 100;

    public static bool IsValidTimeZoneId(string? timeZoneId)
    {
        return !string.IsNullOrWhiteSpace(timeZoneId)
            && timeZoneId.Length <= MaxTimeZoneIdLength
            && TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId.Trim(), out _);
    }

    public static TimeZoneInfo GetTimeZone(string? timeZoneId)
    {
        if (!IsValidTimeZoneId(timeZoneId))
        {
            throw new ArgumentException("The time zone identifier is not supported by the runtime.", nameof(timeZoneId));
        }

        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId!.Trim());
    }

    public static bool TryGetTimeZone(string? timeZoneId, out TimeZoneInfo timeZone)
    {
        if (!IsValidTimeZoneId(timeZoneId))
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }

        timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId!.Trim());
        return true;
    }

    public static DateTime GetDayStartUtc(DateOnly date, TimeZoneInfo timeZone)
    {
        var localMidnight = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);

        if (timeZone.IsInvalidTime(localMidnight))
        {
            var local = localMidnight;
            do
            {
                local = local.AddMinutes(1);
            }
            while (timeZone.IsInvalidTime(local) && (local - localMidnight) < TimeSpan.FromHours(12));

            return ConvertTimeToUtc(local, timeZone);
        }

        return ConvertTimeToUtc(localMidnight, timeZone);
    }

    public static DateTime GetDayEndUtc(DateOnly date, TimeZoneInfo timeZone)
    {
        var nextDate = date.AddDays(1);
        var localMidnight = DateTime.SpecifyKind(nextDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);

        if (timeZone.IsInvalidTime(localMidnight))
        {
            var local = localMidnight;
            do
            {
                local = local.AddMinutes(1);
            }
            while (timeZone.IsInvalidTime(local) && (local - localMidnight) < TimeSpan.FromHours(12));

            return ConvertTimeToUtc(local, timeZone);
        }

        return ConvertTimeToUtc(localMidnight, timeZone);
    }

    public static DateTime GetLocalToUtc(DateTime localDateTime, TimeZoneInfo timeZone)
    {
        var local = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        return ConvertTimeToUtc(local, timeZone);
    }

    public static bool TryGetUtc(DateTime localDateTime, TimeZoneInfo timeZone, out DateTime utc)
    {
        var local = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);

        if (timeZone.IsInvalidTime(local))
        {
            utc = default;
            return false;
        }

        utc = ConvertTimeToUtc(local, timeZone);
        return true;
    }

    private static DateTime ConvertTimeToUtc(DateTime local, TimeZoneInfo timeZone)
    {
        // For ambiguous local times (fall-back), TimeZoneInfo resolves to the standard-time occurrence.
        return TimeZoneInfo.ConvertTimeToUtc(local, timeZone);
    }
}
