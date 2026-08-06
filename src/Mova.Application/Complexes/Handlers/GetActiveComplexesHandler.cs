using Mova.Application.Abstractions.Persistence;
using Mova.Application.Complexes.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public sealed class GetActiveComplexesHandler : IGetActiveComplexesHandler
{
    private readonly ISportsComplexRepository _sportsComplexRepository;

    public GetActiveComplexesHandler(ISportsComplexRepository sportsComplexRepository)
    {
        _sportsComplexRepository = sportsComplexRepository;
    }

    public async Task<PagedResult<SportsComplexInfo>> HandleAsync(GetActiveComplexesQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalItems) = await _sportsComplexRepository.GetActiveComplexesAsync(
            query.Page,
            query.PageSize,
            query.Search,
            cancellationToken);

        var mappedItems = items.Select(SportsComplexInfoMapper.ToInfo).ToList();

        return PagedResult<SportsComplexInfo>.Create(mappedItems, query.Page, query.PageSize, totalItems);
    }
}
