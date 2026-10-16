using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Exceptions;

namespace Mova.Application.Pricing;

public static class ReservationPricer
{
    public static async Task<decimal> ApplyAsync(
        ICourtRateRepository courtRates,
        IReadOnlyList<Reservation> occurrences,
        string currency,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken)
    {
        var activeRates = await courtRates.GetActiveByCourtIdAsync(
            occurrences[0].CourtId, cancellationToken);
        var gaps = new List<PriceGap>();
        var seriesTotal = 0m;

        foreach (var occurrence in occurrences)
        {
            var calculation = PriceCalculator.Calculate(activeRates, timeZone, occurrence.StartAt, occurrence.EndAt);

            if (!calculation.IsFullyCovered)
            {
                gaps.AddRange(calculation.Gaps);
                continue;
            }

            ApplyCalculation(occurrence, calculation, currency);
            seriesTotal += calculation.Total;
        }

        if (gaps.Count > 0)
        {
            throw new PriceNotConfiguredException(gaps);
        }

        return seriesTotal;
    }

    public static void ApplyCalculation(Reservation reservation, PriceCalculation calculation, string currency)
    {
        reservation.ApplyPrice(
            calculation.Total,
            currency,
            calculation.Lines.Select(line => ReservationPriceLine.Create(
                reservation.Id,
                line.CourtRateId,
                line.StartAt,
                line.EndAt,
                line.HourlyRate,
                line.Amount)));
    }
}
