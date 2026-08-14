using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Reservations;

public sealed class FakeRecurringReservationRepository : IRecurringReservationRepository
{
    private readonly List<RecurringReservation> _recurringReservations = [];

    public Task<RecurringReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_recurringReservations.FirstOrDefault(r => r.Id == id));
    }

    public Task AddAsync(RecurringReservation recurringReservation, CancellationToken cancellationToken = default)
    {
        _recurringReservations.Add(recurringReservation);
        return Task.CompletedTask;
    }
}
