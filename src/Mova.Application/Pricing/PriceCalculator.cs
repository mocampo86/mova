using Mova.Domain.Entities;
using Mova.Domain.Exceptions;

namespace Mova.Application.Pricing;

public sealed record CalculatedPriceLine(
    Guid CourtRateId,
    DateTime StartAt,
    DateTime EndAt,
    decimal HourlyRate,
    decimal Amount);

public sealed record PriceCalculation(
    decimal Total,
    IReadOnlyList<CalculatedPriceLine> Lines,
    IReadOnlyList<PriceGap> Gaps)
{
    public bool IsFullyCovered => Gaps.Count == 0;
}

public static class PriceCalculator
{
    private static readonly TimeSpan Day = TimeSpan.FromHours(24);

    public static PriceCalculation Calculate(
        IReadOnlyCollection<CourtRate> rates,
        TimeZoneInfo timeZone,
        DateTime startAt,
        DateTime endAt)
    {
        ArgumentNullException.ThrowIfNull(rates);
        ArgumentNullException.ThrowIfNull(timeZone);

        if (startAt >= endAt)
        {
            throw new ArgumentException("StartAt must be earlier than EndAt.", nameof(endAt));
        }

        var startUtc = DateTime.SpecifyKind(startAt, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(endAt, DateTimeKind.Utc);

        var localStart = TimeZoneInfo.ConvertTimeFromUtc(startUtc, timeZone);
        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(endUtc, timeZone);

        var coveredIntervals = new List<(CourtRate Rate, DateTime StartUtc, DateTime EndUtc)>();

        for (var date = DateOnly.FromDateTime(localStart); date <= DateOnly.FromDateTime(localEnd); date = date.AddDays(1))
        {
            foreach (var rate in rates)
            {
                if (!rate.IsActive || !rate.AppliesOn(date.DayOfWeek))
                {
                    continue;
                }

                var dayStartLocal = date.ToDateTime(TimeOnly.MinValue);
                var bandStartUtc = ConvertBandBoundToUtc(dayStartLocal.Add(rate.StartTime), timeZone);
                var bandEndUtc = ConvertBandBoundToUtc(dayStartLocal.Add(rate.EndTime), timeZone);

                var segmentStart = bandStartUtc > startUtc ? bandStartUtc : startUtc;
                var segmentEnd = bandEndUtc < endUtc ? bandEndUtc : endUtc;

                if (segmentStart < segmentEnd)
                {
                    coveredIntervals.Add((rate, segmentStart, segmentEnd));
                }
            }
        }

        coveredIntervals.Sort((a, b) => a.StartUtc != b.StartUtc ? a.StartUtc.CompareTo(b.StartUtc) : a.EndUtc.CompareTo(b.EndUtc));

        var lines = new List<CalculatedPriceLine>();
        var gaps = new List<PriceGap>();
        var cursor = startUtc;

        foreach (var (rate, intervalStart, intervalEnd) in coveredIntervals)
        {
            if (intervalEnd <= cursor)
            {
                continue;
            }

            var segmentStart = intervalStart > cursor ? intervalStart : cursor;

            if (segmentStart > cursor)
            {
                gaps.Add(new PriceGap(cursor, segmentStart));
            }

            var last = lines.Count > 0 ? lines[^1] : null;
            if (last is not null && last.CourtRateId == rate.Id && last.HourlyRate == rate.AmountPerHour && last.EndAt == segmentStart)
            {
                lines[^1] = last with { EndAt = intervalEnd };
            }
            else
            {
                lines.Add(new CalculatedPriceLine(rate.Id, segmentStart, intervalEnd, rate.AmountPerHour, 0m));
            }

            cursor = intervalEnd;
        }

        if (cursor < endUtc)
        {
            gaps.Add(new PriceGap(cursor, endUtc));
        }

        var pricedLines = lines
            .Select(line => line with
            {
                Amount = RoundMoney(line.HourlyRate * (decimal)(line.EndAt - line.StartAt).TotalMinutes / 60m)
            })
            .ToArray();

        return new PriceCalculation(pricedLines.Sum(l => l.Amount), pricedLines, gaps);
    }

    public static decimal RoundMoney(decimal amount) =>
        decimal.Round(amount, 2, MidpointRounding.AwayFromZero);

    // Local times that fall inside a DST spring-forward gap do not exist; the band
    // bound is moved forward to the first valid minute (same policy as
    // TimeZoneConverter.GetDayStartUtc). Ambiguous fall-back times resolve to the
    // standard-time occurrence via ConvertTimeToUtc.
    private static DateTime ConvertBandBoundToUtc(DateTime local, TimeZoneInfo timeZone)
    {
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        var adjusted = unspecified;

        while (timeZone.IsInvalidTime(adjusted) && (adjusted - unspecified) < TimeSpan.FromHours(12))
        {
            adjusted = adjusted.AddMinutes(1);
        }

        if (timeZone.IsInvalidTime(adjusted))
        {
            throw new ArgumentException("The rate band cannot be resolved in the complex time zone.");
        }

        return TimeZoneInfo.ConvertTimeToUtc(adjusted, timeZone);
    }
}
