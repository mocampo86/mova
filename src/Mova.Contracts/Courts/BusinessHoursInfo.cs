namespace Mova.Contracts.Courts;

public sealed class BusinessHoursInfo
{
    public Guid Id { get; set; }
    public Guid SportsComplexId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public bool IsClosed { get; set; }
}
