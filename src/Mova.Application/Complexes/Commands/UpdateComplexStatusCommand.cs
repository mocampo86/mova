using Mova.Domain.Enums;

namespace Mova.Application.Complexes.Commands;

public sealed record UpdateComplexStatusCommand(
    Guid ComplexId,
    Guid UserId,
    ComplexStatus Status);
