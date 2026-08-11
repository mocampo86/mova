namespace Mova.Contracts.Reservations;

public sealed class CreateMyReservationRequest
{
    public Guid CourtId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Notes { get; set; }
}
