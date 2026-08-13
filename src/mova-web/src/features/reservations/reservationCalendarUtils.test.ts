import { describe, expect, it } from 'vitest';
import type { Court, CourtAvailabilitySlot } from '../complexes/complexTypes';
import type { Reservation } from './reservationTypes';
import {
  buildCalendarColumns,
  buildCourtCalendarColumn,
  formatTimeRange,
  getCalendarTimeRange,
  getReservationStatusColor,
  getSlotBackgroundColor,
  getSlotTextColor
} from './reservationCalendarUtils';

const court: Court = {
  id: 'court-1',
  sportsComplexId: 'complex-1',
  name: 'Court One',
  description: '',
  surfaceType: 'Synthetic',
  indoor: true,
  status: 'Active',
  sportIds: ['sport-1'],
  createdAt: '2026-08-01T12:00:00Z',
  updatedAt: null
};

const freeSlots: CourtAvailabilitySlot[] = [
  {
    courtId: 'court-1',
    startAt: '2026-08-10T12:00:00Z',
    endAt: '2026-08-10T13:00:00Z'
  },
  {
    courtId: 'court-1',
    startAt: '2026-08-10T13:00:00Z',
    endAt: '2026-08-10T14:00:00Z'
  },
  {
    courtId: 'court-1',
    startAt: '2026-08-10T15:00:00Z',
    endAt: '2026-08-10T16:00:00Z'
  }
];

function createReservation(overrides: Partial<Reservation> = {}): Reservation {
  return {
    id: 'reservation-1',
    complexId: 'complex-1',
    courtId: 'court-1',
    courtName: 'Court One',
    userId: 'user-1',
    userName: 'Test User',
    startAt: '2026-08-10T13:00:00Z',
    endAt: '2026-08-10T14:00:00Z',
    status: 'Confirmed',
    source: 'Web',
    notes: null,
    createdAt: '2026-08-01T12:00:00Z',
    cancelledAt: null,
    cancellationReason: null,
    ...overrides
  };
}

describe('getReservationStatusColor', () => {
  it('maps each reservation status to its semantic color', () => {
    expect(getReservationStatusColor('Confirmed')).toBe('primary');
    expect(getReservationStatusColor('Completed')).toBe('info');
    expect(getReservationStatusColor('Pending')).toBe('warning');
    expect(getReservationStatusColor('CancelledByUser')).toBe('error');
    expect(getReservationStatusColor('CancelledByAdmin')).toBe('error');
    expect(getReservationStatusColor('NoShow')).toBe('default');
  });
});

describe('formatTimeRange', () => {
  it('returns a formatted local time range', () => {
    const start = new Date('2026-08-10T12:00:00Z').toLocaleTimeString([], {
      hour: '2-digit',
      minute: '2-digit'
    });
    const end = new Date('2026-08-10T13:00:00Z').toLocaleTimeString([], {
      hour: '2-digit',
      minute: '2-digit'
    });

    const result = formatTimeRange('2026-08-10T12:00:00Z', '2026-08-10T13:00:00Z');

    expect(result).toContain(start);
    expect(result).toContain(end);
  });
});

describe('buildCourtCalendarColumn', () => {
  it('combines free and reservation slots sorted by start time', () => {
    const reservation = createReservation({
      startAt: '2026-08-10T13:00:00Z',
      endAt: '2026-08-10T14:00:00Z',
      status: 'Confirmed'
    });

    const column = buildCourtCalendarColumn(court, freeSlots, [reservation]);

    expect(column.court.id).toBe('court-1');
    expect(column.slots).toHaveLength(3);
    expect(column.slots[0].type).toBe('free');
    expect(column.slots[1].type).toBe('reservation');
    expect(column.slots[2].type).toBe('free');
  });

  it('excludes cancelled reservations so the time slot is shown as free', () => {
    const cancelledReservation = createReservation({
      id: 'reservation-2',
      startAt: '2026-08-10T15:00:00Z',
      endAt: '2026-08-10T16:00:00Z',
      status: 'CancelledByUser'
    });

    const column = buildCourtCalendarColumn(court, freeSlots, [cancelledReservation]);

    const reservedSlot = column.slots.find((slot) => slot.type === 'reservation');
    const freeAtSameTime = column.slots.find(
      (slot) => slot.type === 'free' && slot.startAt === cancelledReservation.startAt
    );

    expect(reservedSlot).toBeFalsy();
    expect(freeAtSameTime).toBeTruthy();
  });

  it('returns an empty column when there are no slots', () => {
    const column = buildCourtCalendarColumn(court, [], []);
    expect(column.slots).toHaveLength(0);
  });
});

describe('buildCalendarColumns', () => {
  it('groups reservations and free slots by court', () => {
    const secondCourt: Court = { ...court, id: 'court-2', name: 'Court Two' };
    const reservation = createReservation({
      courtId: 'court-2',
      courtName: 'Court Two',
      startAt: '2026-08-10T15:00:00Z',
      endAt: '2026-08-10T16:00:00Z'
    });

    const columns = buildCalendarColumns(
      [court, secondCourt],
      [reservation],
      { 'court-1': freeSlots }
    );

    expect(columns).toHaveLength(2);
    expect(columns[0].slots.some((slot) => slot.type === 'free')).toBe(true);
    expect(columns[1].slots.some((slot) => slot.type === 'reservation')).toBe(true);
  });
});

describe('getCalendarTimeRange', () => {
  it('computes the start and total minutes for all calendar slots', () => {
    const columns = buildCalendarColumns([court], [createReservation()], { 'court-1': freeSlots });
    const { dayStart, totalMinutes } = getCalendarTimeRange(columns);

    expect(dayStart).toBeInstanceOf(Date);
    expect(totalMinutes).toBeGreaterThan(0);
  });

  it('returns zero range when there are no slots', () => {
    const { dayStart, totalMinutes } = getCalendarTimeRange([]);

    expect(dayStart).toBeNull();
    expect(totalMinutes).toBe(0);
  });
});

describe('slot colors', () => {
  it('uses success color for free slots', () => {
    const freeSlot = buildCourtCalendarColumn(court, freeSlots, []).slots[0];
    expect(getSlotBackgroundColor(freeSlot)).toBe('success.main');
    expect(getSlotTextColor(freeSlot)).toBe('common.white');
  });

  it('uses status color for reservation slots', () => {
    const reservation = createReservation({ status: 'Pending' });
    const reservationSlot = buildCourtCalendarColumn(court, [], [reservation]).slots[0];

    expect(getSlotBackgroundColor(reservationSlot)).toBe('warning.main');
    expect(getSlotTextColor(reservationSlot)).toBe('common.white');
  });

  it('uses a neutral background for no-show reservations', () => {
    const reservation = createReservation({ status: 'NoShow' });
    const reservationSlot = buildCourtCalendarColumn(court, [], [reservation]).slots[0];

    expect(getSlotBackgroundColor(reservationSlot)).toBe('grey.300');
    expect(getSlotTextColor(reservationSlot)).toBe('text.primary');
  });
});
