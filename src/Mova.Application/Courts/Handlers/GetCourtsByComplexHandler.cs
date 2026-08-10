using Mova.Application.Abstractions.Persistence;
using Mova.Application.Courts.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public sealed class GetCourtsByComplexHandler(
    ICourtRepository courts) : IGetCourtsByComplexHandler
{
    public async Task<PagedResult<CourtInfo>> HandleAsync(GetCourtsByComplexQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalItems) = await courts.GetCourtsByComplexIdAsync(
            query.SportsComplexId,
            query.Page,
            query.PageSize,
            query.SportId,
            query.Status,
            cancellationToken);

        var mappedItems = items.Select(CourtMapper.ToInfo).ToList();

        return PagedResult<CourtInfo>.Create(mappedItems, query.Page, query.PageSize, totalItems);
    }
}
