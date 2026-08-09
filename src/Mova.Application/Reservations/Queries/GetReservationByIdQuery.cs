namespace Mova.Application.Reservations.Queries;

public sealed class GetReservationByIdQuery
{
    public GetReservationByIdQuery(Guid sportsComplexId, Guid reservationId)
    {
        SportsComplexId = sportsComplexId;
        ReservationId = reservationId;
    }

    public Guid SportsComplexId { get; }
    public Guid ReservationId { get; }
}
