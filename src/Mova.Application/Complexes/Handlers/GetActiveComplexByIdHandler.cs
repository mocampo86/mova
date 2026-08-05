using Mova.Application.Abstractions.Persistence;
using Mova.Application.Complexes.Queries;
using Mova.Contracts.Complexes;
using Mova.Domain.Enums;

namespace Mova.Application.Complexes.Handlers;

public sealed class GetActiveComplexByIdHandler : IGetActiveComplexByIdHandler
{
    private readonly ISportsComplexRepository _sportsComplexRepository;

    public GetActiveComplexByIdHandler(ISportsComplexRepository sportsComplexRepository)
    {
        _sportsComplexRepository = sportsComplexRepository;
    }

    public async Task<SportsComplexInfo?> HandleAsync(GetActiveComplexByIdQuery query, CancellationToken cancellationToken = default)
    {
        var sportsComplex = await _sportsComplexRepository.GetByIdAsync(query.ComplexId, cancellationToken);

        if (sportsComplex is null || sportsComplex.Status != ComplexStatus.Active)
        {
            return null;
        }

        return SportsComplexInfoMapper.ToInfo(sportsComplex);
    }
}
