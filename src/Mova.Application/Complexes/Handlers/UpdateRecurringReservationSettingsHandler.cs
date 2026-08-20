using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Complexes.Commands;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public sealed class UpdateRecurringReservationSettingsHandler(
    ISportsComplexRepository sportsComplexRepository,
    IUnitOfWork unitOfWork) : IUpdateRecurringReservationSettingsHandler
{
    public async Task<SportsComplexInfo> HandleAsync(UpdateRecurringReservationSettingsCommand command, CancellationToken cancellationToken = default)
    {
        var complex = await sportsComplexRepository.GetByIdAsync(command.ComplexId, cancellationToken)
            ?? throw new NotFoundException("Sports complex not found.");

        complex.UpdateRecurringReservationSettings(command.AllowUserRecurringReservations);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return SportsComplexInfoMapper.ToInfo(complex);
    }
}
