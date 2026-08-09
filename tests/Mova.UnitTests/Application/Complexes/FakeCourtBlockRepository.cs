using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Complexes;

public sealed class FakeCourtBlockRepository : ICourtBlockRepository
{
    private readonly List<CourtBlock> _courtBlocks = [];

    public Task<IReadOnlyCollection<CourtBlock>> GetForCourtAsync(
        Guid courtId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        var result = _courtBlocks
            .Where(b => b.CourtId == courtId)
            .Where(b => b.StartAt < end && b.EndAt > start)
            .ToList();

        return Task.FromResult<IReadOnlyCollection<CourtBlock>>(result);
    }

    public void Add(CourtBlock courtBlock) => _courtBlocks.Add(courtBlock);
}
