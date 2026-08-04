using Mova.Application.Abstractions.Persistence;

namespace Mova.UnitTests.Application.Authentication;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
