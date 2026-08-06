namespace Mova.Contracts.Courts;

public sealed class CourtAvailabilityRuleInfo
{
    public Guid Id { get; set; }
    public Guid CourtId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public bool IsActive { get; set; }
}
