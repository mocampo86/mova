import { cleanup, fireEvent, screen, waitFor, within } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { Routes, Route } from 'react-router-dom';
import ComplexReservationsPage from './ComplexReservationsPage';
import { useCourts } from '../features/courts/courtApi';
import {
  useCancelReservation,
  useCreateReservation,
  useReservations,
  useUpdateReservationStatus
} from '../features/reservations/reservationApi';
import { useCourtAvailabilityForCourts } from '../features/reservations/reservationCalendarApi';
import { useAdminComplex } from '../features/complexes/complexApi';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/reservations/reservationApi');
vi.mock('../features/courts/courtApi');
vi.mock('../features/complexes/complexApi');
vi.mock('../features/reservations/reservationCalendarApi');

const mockCourts = {
  items: [
    {
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
    }
  ],
  page: 1,
  pageSize: 100,
  totalItems: 1,
  totalPages: 1
};

const mockReservations = {
  items: [
    {
      id: 'reservation-1',
      complexId: 'complex-1',
      courtId: 'court-1',
      courtName: 'Court One',
      userId: 'user-1',
      userName: 'Test User',
      startAt: '2026-08-10T14:00:00Z',
      endAt: '2026-08-10T15:00:00Z',
      status: 'Confirmed',
      source: 'Web',
      notes: null,
      createdAt: '2026-08-01T12:00:00Z',
      cancelledAt: null,
      cancellationReason: null
    }
  ],
  page: 1,
  pageSize: 10,
  totalItems: 1,
  totalPages: 1
};

describe('ComplexReservationsPage', () => {
  const createMutate = vi.fn();
  const cancelMutate = vi.fn();
  const updateStatusMutate = vi.fn();

  beforeEach(() => {
    cleanup();
    vi.resetAllMocks();
    createMutate.mockClear();
    cancelMutate.mockClear();
    updateStatusMutate.mockClear();
  });

  function setupMocks(
    overrides: Partial<ReturnType<typeof useReservations>> = {}
  ) {
    vi.mocked(useReservations).mockReturnValue({
      data: mockReservations,
      isLoading: false,
      isError: false,
      ...overrides
    } as unknown as ReturnType<typeof useReservations>);

    vi.mocked(useCourts).mockReturnValue({
      data: mockCourts,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useCourts>);

    vi.mocked(useCreateReservation).mockReturnValue({
      mutateAsync: createMutate,
      isPending: false,
      error: null
    } as unknown as ReturnType<typeof useCreateReservation>);

    vi.mocked(useCancelReservation).mockReturnValue({
      mutateAsync: cancelMutate,
      isPending: false,
      error: null
    } as unknown as ReturnType<typeof useCancelReservation>);

    vi.mocked(useUpdateReservationStatus).mockReturnValue({
      mutateAsync: updateStatusMutate,
      isPending: false,
      error: null
    } as unknown as ReturnType<typeof useUpdateReservationStatus>);

    vi.mocked(useCourtAvailabilityForCourts).mockReturnValue({
      data: {
        'court-1': [
          {
            courtId: 'court-1',
            startAt: '2026-08-10T12:00:00Z',
            endAt: '2026-08-10T13:00:00Z'
          }
        ]
      },
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useCourtAvailabilityForCourts>);

    vi.mocked(useAdminComplex).mockReturnValue({
      data: {
        id: 'complex-1',
        name: 'Complex One',
        timeZoneId: 'America/Montevideo'
      },
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useAdminComplex>);
  }

  it('renders the reservations list with details and actions', async () => {
    setupMocks();

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/reservations" element={<ComplexReservationsPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/reservations' }
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Reservations' })).toBeTruthy();
    });

    expect(screen.getByRole('button', { name: 'Create reservation' })).toBeTruthy();
    expect(screen.getByText('Court One')).toBeTruthy();
    expect(screen.getByText('Test User')).toBeTruthy();
    expect(screen.getByText('Confirmed')).toBeTruthy();
    expect(screen.getAllByRole('button', { name: 'Cancel' }).length).toBeGreaterThan(0);
    expect(screen.getAllByRole('button', { name: 'Mark status' }).length).toBeGreaterThan(0);
  });

  it('renders an empty state when no reservations exist', async () => {
    setupMocks({
      data: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 }
    } as unknown as ReturnType<typeof useReservations>);

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/reservations" element={<ComplexReservationsPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/reservations' }
    );

    await waitFor(() => {
      expect(screen.getByText('No reservations found for this complex.')).toBeTruthy();
    });
  });

  it('opens the create reservation dialog and calls the mutation on submit', async () => {
    setupMocks();

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/reservations" element={<ComplexReservationsPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/reservations' }
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Reservations' })).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Create reservation' }));

    await waitFor(() => {
      expect(screen.getByRole('dialog', { name: 'Create manual reservation' })).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Create' }));
    expect(createMutate).not.toHaveBeenCalled();
  });

  it('switches to the calendar view and preserves the date and court filter', async () => {
    setupMocks();

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/reservations" element={<ComplexReservationsPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/reservations' }
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Reservations' })).toBeTruthy();
    });

    const dateInputBefore = screen.getByLabelText('Date') as HTMLInputElement;
    const previousDate = dateInputBefore.value;

    fireEvent.click(screen.getByRole('button', { name: 'Calendar' }));

    await waitFor(() => {
      expect(screen.getByText('Legend:')).toBeTruthy();
    });

    expect(screen.getByText('Court One')).toBeTruthy();
    expect(screen.getByText('Test User')).toBeTruthy();

    const dateInputAfter = screen.getByLabelText('Date') as HTMLInputElement;
    expect(dateInputAfter.value).toBe(previousDate);
    expect(screen.getByLabelText('Court')).toBeTruthy();
  });

  it('opens the create dialog prefilled from a free calendar slot', async () => {
    setupMocks();

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/reservations" element={<ComplexReservationsPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/reservations' }
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Reservations' })).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Calendar' }));

    await waitFor(() => {
      expect(screen.getByText('Legend:')).toBeTruthy();
    });

    const freeSlots = screen.getAllByText('Free');
    fireEvent.click(freeSlots[freeSlots.length - 1]);

    await waitFor(() => {
      expect(screen.getByText('Slot details')).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Reserve slot' }));

    await waitFor(() => {
      expect(screen.getByRole('dialog', { name: 'Create manual reservation' })).toBeTruthy();
    });
  });

  it('resets the create reservation form when the dialog is cancelled', async () => {
    setupMocks();

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/reservations" element={<ComplexReservationsPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/reservations' }
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Reservations' })).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Create reservation' }));

    await waitFor(() => {
      expect(screen.getByRole('dialog', { name: 'Create manual reservation' })).toBeTruthy();
    });

    const dialog = screen.getByRole('dialog', { name: 'Create manual reservation' });
    const notesInput = within(dialog).getByLabelText('Notes') as HTMLInputElement;
    fireEvent.change(notesInput, { target: { value: 'cancelled notes' } });

    fireEvent.click(within(dialog).getByRole('button', { name: 'Cancel' }));

    await waitFor(() => {
      expect(screen.queryByRole('dialog', { name: 'Create manual reservation' })).toBeNull();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Create reservation' }));

    await waitFor(() => {
      expect(screen.getByRole('dialog', { name: 'Create manual reservation' })).toBeTruthy();
    });

    const reopenedDialog = screen.getByRole('dialog', { name: 'Create manual reservation' });
    const reopenedNotesInput = within(reopenedDialog).getByLabelText('Notes') as HTMLInputElement;
    expect(reopenedNotesInput.value).toBe('');
  });
});
