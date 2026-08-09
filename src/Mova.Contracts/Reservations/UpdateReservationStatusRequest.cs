namespace Mova.Contracts.Reservations;

public sealed class UpdateReservationStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
