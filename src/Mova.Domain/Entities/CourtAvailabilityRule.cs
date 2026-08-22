namespace Mova.Domain.Entities;

public sealed class CourtAvailabilityRule
{
    public Guid Id { get; private set; }
    public Guid CourtId { get; private set; }
    public Court? Court { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public int SlotDurationMinutes { get; private set; }
    public bool IsActive { get; private set; }

    private CourtAvailabilityRule() { }

    private static readonly TimeSpan Day = TimeSpan.FromHours(24);

    public static CourtAvailabilityRule Create(Guid courtId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes, bool isActive)
    {
        if (courtId == Guid.Empty) throw new ArgumentException("CourtId cannot be empty.", nameof(courtId));
        ValidateTimeOfDay(startTime, nameof(startTime));
        ValidateTimeOfDay(endTime, nameof(endTime));
        if (startTime == endTime)
            throw new ArgumentException("Start and end times cannot be the same.", nameof(startTime));
        if (slotDurationMinutes <= 0)
            throw new ArgumentException("Slot duration must be greater than zero.", nameof(slotDurationMinutes));
        if (!FitsSlotDuration(startTime, endTime, slotDurationMinutes))
            throw new ArgumentException("The time range must be evenly divisible by the slot duration.", nameof(slotDurationMinutes));

        return new CourtAvailabilityRule
        {
            Id = Guid.NewGuid(),
            CourtId = courtId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            SlotDurationMinutes = slotDurationMinutes,
            IsActive = isActive
        };
    }

    public void Update(TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes, bool isActive)
    {
        ValidateTimeOfDay(startTime, nameof(startTime));
        ValidateTimeOfDay(endTime, nameof(endTime));
        if (startTime == endTime)
            throw new ArgumentException("Start and end times cannot be the same.", nameof(startTime));
        if (slotDurationMinutes <= 0)
            throw new ArgumentException("Slot duration must be greater than zero.", nameof(slotDurationMinutes));
        if (!FitsSlotDuration(startTime, endTime, slotDurationMinutes))
            throw new ArgumentException("The time range must be evenly divisible by the slot duration.", nameof(slotDurationMinutes));

        StartTime = startTime;
        EndTime = endTime;
        SlotDurationMinutes = slotDurationMinutes;
        IsActive = isActive;
    }

    public static bool FitsSlotDuration(TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes)
    {
        if (slotDurationMinutes <= 0)
            return false;

        var duration = endTime - startTime;
        if (duration <= TimeSpan.Zero)
            duration += Day;

        return duration.TotalMinutes % slotDurationMinutes == 0;
    }

    private static void ValidateTimeOfDay(TimeSpan time, string paramName)
    {
        if (time < TimeSpan.Zero || time >= Day)
            throw new ArgumentException("Time must be within a 24-hour day.", paramName);
    }
}
