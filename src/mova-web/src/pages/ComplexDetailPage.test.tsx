import { cleanup, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import ComplexDetailPage from './ComplexDetailPage';
import {
  useActiveComplex,
  useActiveCourts,
  useCourtAvailability,
  useSports
} from '../features/complexes/complexApi';
import { useCreateMyReservation } from '../features/reservations/reservationApi';
import { renderWithAuth } from '../test-utils';
import { ApiError } from '../shared/utils/apiError';

vi.mock('../features/complexes/complexApi');
vi.mock('../features/reservations/reservationApi');

const mockComplex = {
  id: 'complex-id',
  name: 'Test Complex',
  description: 'A test complex',
  address: 'Test Address',
  city: 'Test City',
  phoneNumber: '+1234567890',
  email: 'test@complex.com',
  allowUserRecurringReservations: false,
  timeZoneId: 'America/Montevideo'
};

const mockCourts = {
  items: [
    {
      id: 'court-1',
      sportsComplexId: 'complex-id',
      name: 'Padel Court',
      description: 'Indoor padel court',
      surfaceType: 'Synthetic',
      indoor: true,
      sportIds: ['sport-1']
    },
    {
      id: 'court-2',
      sportsComplexId: 'complex-id',
      name: 'Tennis Court',
      description: 'Grass tennis court',
      surfaceType: 'Grass',
      indoor: false,
      sportIds: ['sport-2']
    }
  ],
  page: 1,
  pageSize: 100,
  totalItems: 2,
  totalPages: 1
};

const mockSports = [
  { id: 'sport-1', name: 'Padel' },
  { id: 'sport-2', name: 'Tennis' }
];

const mockSlots = [
  { courtId: 'court-1', startAt: '2026-08-10T08:00:00Z', endAt: '2026-08-10T09:00:00Z' },
  { courtId: 'court-1', startAt: '2026-08-10T09:00:00Z', endAt: '2026-08-10T10:00:00Z' }
];

function mockCourtQuery(sportId?: string) {
  const items = sportId
    ? mockCourts.items.filter((court) => court.sportIds.includes(sportId))
    : mockCourts.items;

  return {
    data: { ...mockCourts, items, totalItems: items.length },
    isLoading: false,
    isError: false
  } as unknown as ReturnType<typeof useActiveCourts>;
}

describe('ComplexDetailPage', () => {
  beforeEach(() => {
    cleanup();
    vi.resetAllMocks();

    vi.mocked(useActiveComplex).mockReturnValue({
      data: mockComplex,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useActiveComplex>);

    vi.mocked(useActiveCourts).mockImplementation((_, sportId) => mockCourtQuery(sportId));

    vi.mocked(useSports).mockReturnValue({
      data: mockSports,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useSports>);

    vi.mocked(useCourtAvailability).mockReturnValue({
      data: mockSlots,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useCourtAvailability>);

    vi.mocked(useCreateMyReservation).mockReturnValue({
      isPending: false,
      isError: false,
      error: null,
      mutateAsync: vi.fn()
    } as unknown as ReturnType<typeof useCreateMyReservation>);
  });

  it('renders complex details and courts', async () => {
    renderWithAuth(<ComplexDetailPage />, { initialRoute: '/complexes/complex-id' });

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Complex' })).toBeTruthy();
    });

    expect(screen.getByText(/Test City/)).toBeTruthy();
    expect(screen.getByText(/test@complex.com/)).toBeTruthy();
    expect(screen.getByText('Padel Court')).toBeTruthy();
    expect(screen.getByText('Tennis Court')).toBeTruthy();
    expect(document.title).toBe('Mova | Test Complex');
    expect(document.querySelector('meta[name="description"]')?.getAttribute('content')).toBe('A test complex');
  });

  it('sets SEO metadata while the complex is loading', () => {
    vi.mocked(useActiveComplex).mockReturnValue({
      data: null,
      isLoading: true,
      isError: false
    } as unknown as ReturnType<typeof useActiveComplex>);

    renderWithAuth(<ComplexDetailPage />, { initialRoute: '/complexes/complex-id' });

    expect(screen.getByText(/Loading complex/i)).toBeTruthy();
    expect(document.title).toBe('Mova | Loading complex…');
    expect(document.querySelector('meta[name="description"]')?.getAttribute('content')).toBe('Explore courts and availability at this complex.');
  });

  it('sets SEO metadata when the complex cannot be found', () => {
    vi.mocked(useActiveComplex).mockReturnValue({
      data: null,
      isLoading: false,
      isError: true
    } as unknown as ReturnType<typeof useActiveComplex>);

    renderWithAuth(<ComplexDetailPage />, { initialRoute: '/complexes/unknown-id' });

    expect(screen.getByText(/This active complex could not be found/i)).toBeTruthy();
    expect(document.title).toBe('Mova | 404 - Page not found');
    expect(document.querySelector('meta[name="description"]')?.getAttribute('content')).toBe('The page you are looking for does not exist.');
  });

  it('renders an empty state when the complex has no courts', async () => {
    vi.mocked(useActiveCourts).mockReturnValue({
      data: { items: [], page: 1, pageSize: 100, totalItems: 0, totalPages: 0 },
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useActiveCourts>);

    renderWithAuth(<ComplexDetailPage />, { initialRoute: '/complexes/complex-id' });

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Complex' })).toBeTruthy();
    });

    expect(screen.getByText(/No active courts match the selected filter/i)).toBeTruthy();
  });

  it('renders an error state when the courts query fails', async () => {
    vi.mocked(useActiveCourts).mockReturnValue({
      data: null,
      isLoading: false,
      isError: true
    } as unknown as ReturnType<typeof useActiveCourts>);

    renderWithAuth(<ComplexDetailPage />, { initialRoute: '/complexes/complex-id' });

    await waitFor(() => {
      expect(screen.getByText(/Courts could not be loaded/i)).toBeTruthy();
    });
  });

  it('filters courts by sport using the sport filter', async () => {
    const user = userEvent.setup();

    renderWithAuth(<ComplexDetailPage />, { initialRoute: '/complexes/complex-id' });

    await waitFor(() => {
      expect(screen.getByText('Padel Court')).toBeTruthy();
    });

    const sportFilter = screen.getByRole('combobox', { name: 'Filter by sport' });
    await user.click(sportFilter);

    const tennisOption = screen.getByRole('option', { name: 'Tennis' });
    await user.click(tennisOption);

    await waitFor(() => {
      expect(screen.getByText('Tennis Court')).toBeTruthy();
    });

    expect(screen.queryByText('Padel Court')).toBeNull();
  });

  it('renders a translated error in the booking dialog when the reservation fails', async () => {
    const user = userEvent.setup();

    vi.mocked(useCreateMyReservation).mockReturnValue({
      isPending: false,
      isError: true,
      error: new ApiError(409, 'Slot is taken', 'RESERVATION_CONFLICT'),
      mutateAsync: vi.fn()
    } as unknown as ReturnType<typeof useCreateMyReservation>);

    renderWithAuth(<ComplexDetailPage />, { initialRoute: '/complexes/complex-id' });

    await waitFor(() => {
      expect(screen.getAllByText(/^\d{1,2}:00/).length).toBeGreaterThan(0);
    });

    await user.click(screen.getAllByText(/^\d{1,2}:00/)[0]);

    await waitFor(() => {
      expect(
        screen.getByText('The requested action conflicts with existing data.')
      ).toBeTruthy();
    });
  });

});
