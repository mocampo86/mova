using Mova.Application.Abstractions.Persistence;
using Mova.Application.Complexes.Queries;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public sealed class GetComplexByIdForAdminHandler : IGetComplexByIdForAdminHandler
{
    private readonly ISportsComplexRepository _sportsComplexRepository;

    public GetComplexByIdForAdminHandler(ISportsComplexRepository sportsComplexRepository)
    {
        _sportsComplexRepository = sportsComplexRepository;
    }

    public async Task<SportsComplexInfo?> HandleAsync(GetComplexByIdForAdminQuery query, CancellationToken cancellationToken = default)
    {
        var sportsComplex = await _sportsComplexRepository.GetByIdAsync(query.ComplexId, cancellationToken);

        if (sportsComplex is null)
        {
            return null;
        }

        return SportsComplexInfoMapper.ToInfo(sportsComplex);
    }
}
