namespace Mova.Contracts.Courts;

public sealed class UpdateBusinessHoursRequest
{
    public IReadOnlyCollection<BusinessHoursRequest> Hours { get; set; } = [];
}
