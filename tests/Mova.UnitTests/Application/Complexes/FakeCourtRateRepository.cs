using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Complexes;

public sealed class FakeCourtRateRepository : ICourtRateRepository
{
    private readonly List<CourtRate> _rates = [];

    public IReadOnlyCollection<CourtRate> Rates => _rates;

    public void Clear() => _rates.Clear();

    public Task<IReadOnlyCollection<CourtRate>> GetByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<CourtRate> result = _rates
            .Where(r => r.CourtId == courtId)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<CourtRate>> GetActiveByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<CourtRate> result = _rates
            .Where(r => r.CourtId == courtId && r.IsActive)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<CourtRate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_rates.FirstOrDefault(r => r.Id == id));
    }

    public Task AddAsync(CourtRate rate, CancellationToken cancellationToken = default)
    {
        _rates.Add(rate);
        return Task.CompletedTask;
    }
}
