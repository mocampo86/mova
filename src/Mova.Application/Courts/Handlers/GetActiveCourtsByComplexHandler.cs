using Mova.Application.Abstractions.Persistence;
using Mova.Application.Courts.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public sealed class GetActiveCourtsByComplexHandler(
    ICourtRepository courts) : IGetActiveCourtsByComplexHandler
{
    public async Task<PagedResult<CourtInfo>> HandleAsync(GetActiveCourtsByComplexQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalItems) = await courts.GetActiveCourtsByComplexIdAsync(
            query.SportsComplexId,
            query.Page,
            query.PageSize,
            cancellationToken);

        var mappedItems = items.Select(CourtMapper.ToInfo).ToList();

        return PagedResult<CourtInfo>.Create(mappedItems, query.Page, query.PageSize, totalItems);
    }
}
