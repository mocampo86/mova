using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Complexes;

public sealed class FakeSportsComplexRepository : ISportsComplexRepository
{
    private readonly List<SportsComplex> _sportsComplexes = [];

    public Task<SportsComplex?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_sportsComplexes.FirstOrDefault(s => s.Id == id));
    }

    public Task AddAsync(SportsComplex sportsComplex, CancellationToken cancellationToken = default)
    {
        _sportsComplexes.Add(sportsComplex);
        return Task.CompletedTask;
    }
}
