namespace Mova.Domain.Entities;

public sealed class BusinessHours
{
    public Guid Id { get; private set; }
    public Guid SportsComplexId { get; private set; }
    public SportsComplex? SportsComplex { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan OpeningTime { get; private set; }
    public TimeSpan ClosingTime { get; private set; }
    public bool IsClosed { get; private set; }

    private BusinessHours() { }

    public static BusinessHours Create(Guid sportsComplexId, DayOfWeek dayOfWeek, TimeSpan openingTime, TimeSpan closingTime, bool isClosed)
    {
        if (sportsComplexId == Guid.Empty) throw new ArgumentException("SportsComplexId cannot be empty.", nameof(sportsComplexId));
        ValidateDayOfWeek(dayOfWeek);
        ValidateTimeOfDay(openingTime, nameof(openingTime));
        ValidateTimeOfDay(closingTime, nameof(closingTime));
        if (!isClosed && openingTime == closingTime)
            throw new ArgumentException("Opening and closing times cannot be the same.", nameof(openingTime));

        return new BusinessHours
        {
            Id = Guid.NewGuid(),
            SportsComplexId = sportsComplexId,
            DayOfWeek = dayOfWeek,
            OpeningTime = openingTime,
            ClosingTime = closingTime,
            IsClosed = isClosed
        };
    }

    public void Update(TimeSpan openingTime, TimeSpan closingTime, bool isClosed)
    {
        ValidateTimeOfDay(openingTime, nameof(openingTime));
        ValidateTimeOfDay(closingTime, nameof(closingTime));
        if (!isClosed && openingTime == closingTime)
            throw new ArgumentException("Opening and closing times cannot be the same.", nameof(openingTime));

        OpeningTime = openingTime;
        ClosingTime = closingTime;
        IsClosed = isClosed;
    }

    private static void ValidateDayOfWeek(DayOfWeek dayOfWeek)
    {
        var value = (int)dayOfWeek;
        if (value < 0 || value > 6)
            throw new ArgumentException("DayOfWeek must be between 0 and 6.", nameof(dayOfWeek));
    }

    private static void ValidateTimeOfDay(TimeSpan time, string paramName)
    {
        if (time < TimeSpan.Zero || time >= TimeSpan.FromHours(24))
            throw new ArgumentException("Time must be within a 24-hour day.", paramName);
    }
}
