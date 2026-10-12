namespace Mova.Domain.Entities;

public sealed class CourtRate
{
    public const int MinDaysOfWeekMask = 1;
    public const int MaxDaysOfWeekMask = 0b1111111;

    private static readonly TimeSpan Day = TimeSpan.FromHours(24);

    public Guid Id { get; private set; }
    public Guid CourtId { get; private set; }
    public int DaysOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public decimal AmountPerHour { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Court? Court { get; private set; }

    private CourtRate()
    {
    }

    public static CourtRate Create(
        Guid courtId,
        int daysOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        decimal amountPerHour,
        bool isActive = true)
    {
        if (courtId == Guid.Empty) throw new ArgumentException("CourtId cannot be empty.", nameof(courtId));
        Validate(daysOfWeek, startTime, endTime, amountPerHour);

        return new CourtRate
        {
            Id = Guid.NewGuid(),
            CourtId = courtId,
            DaysOfWeek = daysOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            AmountPerHour = amountPerHour,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(int daysOfWeek, TimeSpan startTime, TimeSpan endTime, decimal amountPerHour)
    {
        Validate(daysOfWeek, startTime, endTime, amountPerHour);

        DaysOfWeek = daysOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        AmountPerHour = amountPerHour;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool AppliesOn(DayOfWeek dayOfWeek)
    {
        return (DaysOfWeek & (1 << (int)dayOfWeek)) != 0;
    }

    public bool OverlapsWith(CourtRate other)
    {
        if ((DaysOfWeek & other.DaysOfWeek) == 0)
        {
            return false;
        }

        return StartTime < other.EndTime && other.StartTime < EndTime;
    }

    public static bool HasOverlap(IEnumerable<CourtRate> rates)
    {
        var list = rates.Where(r => r.IsActive).ToArray();

        for (var i = 0; i < list.Length; i++)
        {
            for (var j = i + 1; j < list.Length; j++)
            {
                if (list[i].OverlapsWith(list[j]))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void Validate(int daysOfWeek, TimeSpan startTime, TimeSpan endTime, decimal amountPerHour)
    {
        if (daysOfWeek is < MinDaysOfWeekMask or > MaxDaysOfWeekMask)
        {
            throw new ArgumentException("At least one day of week must be selected.", nameof(daysOfWeek));
        }

        if (startTime < TimeSpan.Zero || startTime >= Day)
        {
            throw new ArgumentException("StartTime must be within a 24-hour day.", nameof(startTime));
        }

        if (endTime <= startTime || endTime > Day)
        {
            throw new ArgumentException("EndTime must be greater than StartTime and at most 24:00.", nameof(endTime));
        }

        if (amountPerHour <= 0)
        {
            throw new ArgumentException("Amount per hour must be greater than zero.", nameof(amountPerHour));
        }

        if (decimal.Round(amountPerHour, 2) != amountPerHour)
        {
            throw new ArgumentException("Amount per hour must have at most two decimal places.", nameof(amountPerHour));
        }
    }
}
