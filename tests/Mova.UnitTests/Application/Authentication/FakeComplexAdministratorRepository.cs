using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Authentication;

public sealed class FakeComplexAdministratorRepository : IComplexAdministratorRepository
{
    private readonly List<ComplexAdministrator> _complexAdministrators = [];

    public Task<IReadOnlyCollection<ComplexAdministrator>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<ComplexAdministrator>>(
            _complexAdministrators.Where(ca => ca.UserId == userId).ToList());
    }

    public Task<ComplexAdministrator?> GetByUserAndComplexAsync(Guid userId, Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_complexAdministrators.FirstOrDefault(
            ca => ca.UserId == userId && ca.SportsComplexId == sportsComplexId));
    }

    public Task AddAsync(ComplexAdministrator complexAdministrator, CancellationToken cancellationToken = default)
    {
        _complexAdministrators.Add(complexAdministrator);
        return Task.CompletedTask;
    }
}
