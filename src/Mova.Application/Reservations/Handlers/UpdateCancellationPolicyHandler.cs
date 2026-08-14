using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;
using Mova.Domain.Entities;

namespace Mova.Application.Reservations.Handlers;

public sealed class UpdateCancellationPolicyHandler(
    ISportsComplexRepository sportsComplexes,
    ICancellationPolicyRepository repository,
    IUnitOfWork unitOfWork) : IUpdateCancellationPolicyHandler
{
    public async Task<CancellationPolicyInfo> HandleAsync(UpdateCancellationPolicyCommand command, CancellationToken cancellationToken = default)
    {
        var complex = await sportsComplexes.GetByIdAsync(command.SportsComplexId, cancellationToken);

        if (complex is null)
        {
            throw new NotFoundException("Sports complex not found.");
        }

        var existing = await repository.GetBySportsComplexIdAsync(command.SportsComplexId, cancellationToken);

        if (existing is null)
        {
            existing = CancellationPolicy.Create(command.SportsComplexId, command.MinimumHours, command.AllowUserCancellation);
            await repository.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.Update(command.MinimumHours, command.AllowUserCancellation);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CancellationPolicyInfo
        {
            SportsComplexId = existing.SportsComplexId,
            MinimumHours = existing.MinimumHours,
            AllowUserCancellation = existing.AllowUserCancellation
        };
    }
}
