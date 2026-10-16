using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.CourtRates.Queries;
using Mova.Application.Pricing;
using Mova.Contracts.CourtRates;
using Mova.Domain.Exceptions;
using Mova.Domain.Helpers;

namespace Mova.Application.CourtRates.Handlers;

public sealed class GetPriceQuoteHandler(
    ISportsComplexRepository sportsComplexes,
    ICourtRepository courts,
    ICourtRateRepository rates) : IGetPriceQuoteHandler
{
    public async Task<PriceQuoteInfo> HandleAsync(GetPriceQuoteQuery query, CancellationToken cancellationToken = default)
    {
        var complex = await sportsComplexes.GetByIdAsync(query.SportsComplexId, cancellationToken)
            ?? throw new NotFoundException("Sports complex not found.");

        var court = await courts.GetByIdAsync(query.CourtId, cancellationToken);
        if (court is null || court.SportsComplexId != query.SportsComplexId)
        {
            throw new NotFoundException("Court not found.");
        }

        if (!TimeZoneConverter.TryGetTimeZone(complex.TimeZoneId, out var timeZone))
        {
            throw new UnresolvedTimeZoneException();
        }

        var activeRates = await rates.GetActiveByCourtIdAsync(query.CourtId, cancellationToken);
        var calculation = PriceCalculator.Calculate(activeRates, timeZone, query.StartAt, query.EndAt);

        return new PriceQuoteInfo
        {
            TotalAmount = calculation.Total,
            Currency = complex.Currency,
            IsFullyCovered = calculation.IsFullyCovered,
            Lines = calculation.Lines
                .Select(l => new PriceQuoteLineInfo
                {
                    StartAt = l.StartAt,
                    EndAt = l.EndAt,
                    HourlyRate = l.HourlyRate,
                    Amount = l.Amount
                })
                .ToArray(),
            Gaps = calculation.Gaps
                .Select(g => new PriceGapInfo { StartAt = g.StartAt, EndAt = g.EndAt })
                .ToArray()
        };
    }
}
