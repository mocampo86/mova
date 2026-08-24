using Mova.Contracts.Courts;
using Mova.Domain.Entities;
using Mova.Domain.Helpers;

namespace Mova.Application.Courts;

public static class AvailabilitySlotGenerator
{
    public static IReadOnlyCollection<CourtAvailabilitySlotInfo> GenerateSlots(
        Guid courtId,
        DateOnly date,
        CourtAvailabilityRule rule,
        BusinessHours businessHours,
        IEnumerable<Reservation> reservations,
        IEnumerable<CourtBlock> blocks,
        TimeZoneInfo timeZone,
        DateTime? referenceTime = null)
    {
        if (rule.DayOfWeek != date.DayOfWeek || businessHours.DayOfWeek != date.DayOfWeek || businessHours.IsClosed)
        {
            return [];
        }

        var ruleStartLocal = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.FromTimeSpan(rule.StartTime)), DateTimeKind.Unspecified);
        var ruleEndDate = date.AddDays(rule.StartTime > rule.EndTime ? 1 : 0);
        var ruleEndLocal = DateTime.SpecifyKind(ruleEndDate.ToDateTime(TimeOnly.FromTimeSpan(rule.EndTime)), DateTimeKind.Unspecified);

        var businessStartLocal = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.FromTimeSpan(businessHours.OpeningTime)), DateTimeKind.Unspecified);
        var businessEndDate = date.AddDays(businessHours.OpeningTime > businessHours.ClosingTime ? 1 : 0);
        var businessEndLocal = DateTime.SpecifyKind(businessEndDate.ToDateTime(TimeOnly.FromTimeSpan(businessHours.ClosingTime)), DateTimeKind.Unspecified);

        if (!TimeZoneConverter.TryGetUtc(ruleStartLocal, timeZone, out var ruleStartUtc) ||
            !TimeZoneConverter.TryGetUtc(ruleEndLocal, timeZone, out var ruleEndUtc) ||
            !TimeZoneConverter.TryGetUtc(businessStartLocal, timeZone, out var businessStartUtc) ||
            !TimeZoneConverter.TryGetUtc(businessEndLocal, timeZone, out var businessEndUtc))
        {
            return [];
        }

        var intervalStartUtc = ruleStartUtc > businessStartUtc ? ruleStartUtc : businessStartUtc;
        var intervalEndUtc = ruleEndUtc < businessEndUtc ? ruleEndUtc : businessEndUtc;

        if (intervalStartUtc >= intervalEndUtc)
        {
            return [];
        }

        var intervalStartLocal = ruleStartLocal > businessStartLocal ? ruleStartLocal : businessStartLocal;
        var intervalEndLocal = ruleEndLocal < businessEndLocal ? ruleEndLocal : businessEndLocal;

        var slotDuration = TimeSpan.FromMinutes(rule.SlotDurationMinutes);
        var activeReservations = reservations.Where(r => r.IsActiveForAvailability()).ToArray();
        var activeBlocks = blocks.ToArray();
        var slots = new List<CourtAvailabilitySlotInfo>();
        var now = referenceTime ?? DateTime.MinValue;

        for (var i = 0; ; i++)
        {
            var slotStartLocal = intervalStartLocal.Add(TimeSpan.FromMinutes(i * rule.SlotDurationMinutes));
            var slotEndLocal = slotStartLocal.Add(slotDuration);

            if (slotStartLocal >= intervalEndLocal || slotEndLocal > intervalEndLocal)
            {
                break;
            }

            if (!TimeZoneConverter.TryGetUtc(slotStartLocal, timeZone, out var slotStartUtc) ||
                !TimeZoneConverter.TryGetUtc(slotEndLocal, timeZone, out var slotEndUtc))
            {
                continue;
            }

            if (slotStartUtc < now)
            {
                continue;
            }

            if (IsOverlapping(slotStartUtc, slotEndUtc, activeReservations, activeBlocks))
            {
                continue;
            }

            slots.Add(new CourtAvailabilitySlotInfo
            {
                CourtId = courtId,
                StartAt = slotStartUtc,
                EndAt = slotEndUtc
            });
        }

        return slots;
    }

    private static bool IsOverlapping(
        DateTime slotStart,
        DateTime slotEnd,
        IEnumerable<Reservation> reservations,
        IEnumerable<CourtBlock> blocks)
    {
        return reservations.Any(r => r.StartAt < slotEnd && r.EndAt > slotStart)
            || blocks.Any(b => b.StartAt < slotEnd && b.EndAt > slotStart);
    }
}
