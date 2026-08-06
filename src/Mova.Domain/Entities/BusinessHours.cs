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
        if (!isClosed && openingTime >= closingTime)
            throw new ArgumentException("Opening time must be earlier than closing time.", nameof(openingTime));

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
        if (!isClosed && openingTime >= closingTime)
            throw new ArgumentException("Opening time must be earlier than closing time.", nameof(openingTime));

        OpeningTime = openingTime;
        ClosingTime = closingTime;
        IsClosed = isClosed;
    }
}
