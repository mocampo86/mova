using Mova.Application.Abstractions.Persistence;
using Mova.Application.Complexes.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public sealed class GetAllComplexesHandler : IGetAllComplexesHandler
{
    private readonly ISportsComplexRepository _sportsComplexRepository;

    public GetAllComplexesHandler(ISportsComplexRepository sportsComplexRepository)
    {
        _sportsComplexRepository = sportsComplexRepository;
    }

    public async Task<PagedResult<SportsComplexInfo>> HandleAsync(GetAllComplexesQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalItems) = await _sportsComplexRepository.GetAllComplexesAsync(
            query.Page,
            query.PageSize,
            cancellationToken);

        var mappedItems = items.Select(SportsComplexInfoMapper.ToInfo).ToList();

        return PagedResult<SportsComplexInfo>.Create(mappedItems, query.Page, query.PageSize, totalItems);
    }
}
