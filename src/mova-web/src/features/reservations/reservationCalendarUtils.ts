import type { Court, CourtAvailabilitySlot } from '../complexes/complexTypes';
import type { Reservation } from './reservationTypes';
import { getReservationStatusColor } from './reservationStatus';
import type { CalendarCourtColumn, CalendarSlot, FreeCalendarSlot, ReservationCalendarSlot } from './reservationCalendarTypes';

export { getReservationStatusColor };

const MINUTES_IN_MS = 60 * 1000;

export function formatTimeRange(startAt: string, endAt: string): string {
  const start = new Date(startAt);
  const end = new Date(endAt);
  const options: Intl.DateTimeFormatOptions = { hour: '2-digit', minute: '2-digit' };
  return `${start.toLocaleTimeString([], options)} - ${end.toLocaleTimeString([], options)}`;
}

export function getSlotBackgroundColor(slot: CalendarSlot): string {
  if (slot.type === 'free') {
    return 'success.main';
  }
  const color = getReservationStatusColor(slot.reservation.status);
  return color === 'default' ? 'grey.300' : `${color}.main`;
}

export function getSlotTextColor(slot: CalendarSlot): string {
  if (slot.type === 'free') {
    return 'common.white';
  }
  const color = getReservationStatusColor(slot.reservation.status);
  return color === 'default' ? 'text.primary' : 'common.white';
}

function toFreeSlot(court: Court, slot: CourtAvailabilitySlot): FreeCalendarSlot {
  return {
    type: 'free',
    courtId: court.id,
    courtName: court.name,
    startAt: slot.startAt,
    endAt: slot.endAt
  };
}

function toReservationSlot(court: Court, reservation: Reservation): ReservationCalendarSlot {
  return {
    type: 'reservation',
    courtId: court.id,
    courtName: court.name,
    startAt: reservation.startAt,
    endAt: reservation.endAt,
    reservation,
    status: reservation.status
  };
}

function subtractIntervals(
  freeSlots: CourtAvailabilitySlot[],
  reservations: Reservation[]
): CourtAvailabilitySlot[] {
  let remaining: CourtAvailabilitySlot[] = freeSlots;

  for (const reservation of reservations) {
    const reservationStart = new Date(reservation.startAt).getTime();
    const reservationEnd = new Date(reservation.endAt).getTime();
    const next: CourtAvailabilitySlot[] = [];

    for (const interval of remaining) {
      const intervalStart = new Date(interval.startAt).getTime();
      const intervalEnd = new Date(interval.endAt).getTime();

      if (reservationEnd <= intervalStart || reservationStart >= intervalEnd) {
        next.push(interval);
        continue;
      }

      if (reservationStart > intervalStart) {
        next.push({
          courtId: interval.courtId,
          startAt: interval.startAt,
          endAt: reservation.startAt
        });
      }

      if (reservationEnd < intervalEnd) {
        next.push({
          courtId: interval.courtId,
          startAt: reservation.endAt,
          endAt: interval.endAt
        });
      }
    }

    remaining = next;
  }

  return remaining;
}

function sortByStart(a: CalendarSlot, b: CalendarSlot): number {
  const aStart = new Date(a.startAt).getTime();
  const bStart = new Date(b.startAt).getTime();

  if (aStart === bStart) {
    const aEnd = new Date(a.endAt).getTime();
    const bEnd = new Date(b.endAt).getTime();
    return aEnd - bEnd;
  }

  return aStart - bStart;
}

export function buildCourtCalendarColumn(
  court: Court,
  freeSlots: CourtAvailabilitySlot[],
  reservations: Reservation[]
): CalendarCourtColumn {
  const reservedSlots = reservations.map((reservation) => toReservationSlot(court, reservation));
  const availableSlots = subtractIntervals(freeSlots, reservations).map((slot) => toFreeSlot(court, slot));

  return {
    court,
    slots: [...reservedSlots, ...availableSlots].sort(sortByStart)
  };
}

export function buildCalendarColumns(
  courts: Court[],
  reservations: Reservation[],
  availability: Record<string, CourtAvailabilitySlot[]>
): CalendarCourtColumn[] {
  const reservationsByCourt = reservations.reduce<Record<string, Reservation[]>>((acc, reservation) => {
    if (!acc[reservation.courtId]) {
      acc[reservation.courtId] = [];
    }
    acc[reservation.courtId].push(reservation);
    return acc;
  }, {});

  return courts.map((court) =>
    buildCourtCalendarColumn(court, availability[court.id] ?? [], reservationsByCourt[court.id] ?? [])
  );
}

export function getCalendarTimeRange(columns: CalendarCourtColumn[]): {
  dayStart: Date | null;
  totalMinutes: number;
} {
  const allSlots = columns.flatMap((column) => column.slots);

  if (allSlots.length === 0) {
    return { dayStart: null, totalMinutes: 0 };
  }

  const startTimes = allSlots.map((slot) => new Date(slot.startAt).getTime());
  const endTimes = allSlots.map((slot) => new Date(slot.endAt).getTime());
  const minStart = Math.min(...startTimes);
  const maxEnd = Math.max(...endTimes);

  return {
    dayStart: new Date(minStart),
    totalMinutes: Math.max(0, (maxEnd - minStart) / MINUTES_IN_MS)
  };
}
