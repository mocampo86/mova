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

    public static CourtAvailabilityRule Create(Guid courtId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes, bool isActive)
    {
        if (courtId == Guid.Empty) throw new ArgumentException("CourtId cannot be empty.", nameof(courtId));
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be earlier than end time.", nameof(startTime));
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
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be earlier than end time.", nameof(startTime));
        if (slotDurationMinutes <= 0)
            throw new ArgumentException("Slot duration must be greater than zero.", nameof(slotDurationMinutes));
        if (!FitsSlotDuration(startTime, endTime, slotDurationMinutes))
            throw new ArgumentException("The time range must be evenly divisible by the slot duration.", nameof(slotDurationMinutes));

        StartTime = startTime;
        EndTime = endTime;
        SlotDurationMinutes = slotDurationMinutes;
        IsActive = isActive;
    }

    private static bool FitsSlotDuration(TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes)
    {
        var duration = endTime - startTime;
        return duration.TotalMinutes % slotDurationMinutes == 0;
    }
}
