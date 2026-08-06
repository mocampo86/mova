namespace Mova.Contracts.Courts;

public sealed class UpdateCourtAvailabilityRulesRequest
{
    public IReadOnlyCollection<CreateCourtAvailabilityRuleRequest> Rules { get; set; } = [];
}
