namespace Mova.Contracts.CourtRates;

public sealed class CourtRateInfo
{
    public Guid Id { get; set; }
    public Guid CourtId { get; set; }
    public IReadOnlyCollection<int> DaysOfWeek { get; set; } = [];
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal AmountPerHour { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
