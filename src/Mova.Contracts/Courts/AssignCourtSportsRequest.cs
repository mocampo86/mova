namespace Mova.Contracts.Courts;

public sealed class AssignCourtSportsRequest
{
    public IReadOnlyCollection<Guid> SportIds { get; set; } = [];
}
