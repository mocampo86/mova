using Mova.Contracts.Courts;
using Mova.Domain.Entities;

namespace Mova.Application.Courts;

public static class AvailabilitySlotGenerator
{
    public static IReadOnlyCollection<CourtAvailabilitySlotInfo> GenerateSlots(
        Guid courtId,
        DateOnly date,
        CourtAvailabilityRule rule,
        BusinessHours businessHours,
        IEnumerable<Reservation> reservations,
        IEnumerable<CourtBlock> blocks)
    {
        if (rule.DayOfWeek != date.DayOfWeek || businessHours.DayOfWeek != date.DayOfWeek || businessHours.IsClosed)
        {
            return [];
        }

        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var ruleStart = dayStart.Add(rule.StartTime);
        var ruleEnd = dayStart.Add(rule.EndTime);
        var businessStart = dayStart.Add(businessHours.OpeningTime);
        var businessEnd = dayStart.Add(businessHours.ClosingTime);

        var intervalStart = ruleStart > businessStart ? ruleStart : businessStart;
        var intervalEnd = ruleEnd < businessEnd ? ruleEnd : businessEnd;

        if (intervalStart >= intervalEnd)
        {
            return [];
        }

        var slotDuration = TimeSpan.FromMinutes(rule.SlotDurationMinutes);
        var activeReservations = reservations.Where(r => r.IsActiveForAvailability()).ToArray();
        var activeBlocks = blocks.ToArray();
        var slots = new List<CourtAvailabilitySlotInfo>();

        for (var slotStart = intervalStart; slotStart.Add(slotDuration) <= intervalEnd; slotStart = slotStart.Add(slotDuration))
        {
            var slotEnd = slotStart.Add(slotDuration);

            if (IsOverlapping(slotStart, slotEnd, activeReservations, activeBlocks))
            {
                continue;
            }

            slots.Add(new CourtAvailabilitySlotInfo
            {
                CourtId = courtId,
                StartAt = slotStart,
                EndAt = slotEnd
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
