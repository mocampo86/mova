import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithAuth } from '../../test-utils';
import type { Court, CourtAvailabilitySlot } from '../complexes/complexTypes';
import type { Reservation } from './reservationTypes';
import ReservationCalendar from './ReservationCalendar';
import { buildCalendarColumns } from './reservationCalendarUtils';

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
  }
];

const reservation: Reservation = {
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
  cancellationReason: null
};

const columns = buildCalendarColumns([court], [reservation], { 'court-1': freeSlots });

describe('ReservationCalendar', () => {
  const onFreeSlotClick = vi.fn();
  const onReservationClick = vi.fn();

  beforeEach(() => {
    cleanup();
    onFreeSlotClick.mockClear();
    onReservationClick.mockClear();
  });

  function renderCalendar(props: Partial<React.ComponentProps<typeof ReservationCalendar>> = {}) {
    return renderWithAuth(
      <ReservationCalendar
        columns={columns}
        isLoading={false}
        isError={false}
        onFreeSlotClick={onFreeSlotClick}
        onReservationClick={onReservationClick}
        {...props}
      />
    );
  }

  it('renders a skeleton while loading', () => {
    renderCalendar({ isLoading: true });
    expect(screen.getByTestId('calendar-skeleton')).toBeTruthy();
  });

  it('renders an error alert when loading fails', () => {
    renderCalendar({ isError: true });
    expect(screen.getByRole('alert').textContent).toContain('could not be loaded');
  });

  it('renders an empty message when there are no slots', () => {
    renderCalendar({ columns: [] });
    expect(screen.getByRole('alert').textContent).toContain('No slots to display');
  });

  it('renders the calendar legend and court columns', async () => {
    renderCalendar();

    await waitFor(() => {
      expect(screen.getByText('Legend:')).toBeTruthy();
    });

    expect(screen.getByText('Court One')).toBeTruthy();
    expect(screen.getAllByText('Free').length).toBeGreaterThan(0);
    expect(screen.getByText('Test User')).toBeTruthy();
  });

  it('calls onFreeSlotClick when a free slot is selected', async () => {
    renderCalendar();

    await waitFor(() => {
      expect(screen.getByText('Legend:')).toBeTruthy();
    });

    const freeSlotElements = screen.getAllByText('Free');
    fireEvent.click(freeSlotElements[freeSlotElements.length - 1]);

    expect(onFreeSlotClick).toHaveBeenCalledTimes(1);
    expect(onReservationClick).not.toHaveBeenCalled();
  });

  it('calls onReservationClick when a reservation slot is selected', async () => {
    renderCalendar();

    await waitFor(() => {
      expect(screen.getByText('Test User')).toBeTruthy();
    });

    fireEvent.click(screen.getByText('Test User'));

    expect(onReservationClick).toHaveBeenCalledTimes(1);
    expect(onFreeSlotClick).not.toHaveBeenCalled();
  });
});
