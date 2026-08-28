namespace Mova.Application.Audit.Queries;

public sealed record GetAuditLogsQuery(
    Guid? SportsComplexId = null,
    Guid? UserId = null,
    string? Action = null,
    string? EntityType = null,
    string? EntityId = null,
    DateTime? From = null,
    DateTime? To = null);
