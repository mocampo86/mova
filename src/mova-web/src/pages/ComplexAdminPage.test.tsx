import { screen } from '@testing-library/react';
import { Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ComplexAdminPage from './ComplexAdminPage';
import { useAdminComplex, useComplexDashboard } from '../features/complexes/complexApi';
import { useCourts } from '../features/courts/courtApi';
import { useReservations } from '../features/reservations/reservationApi';
import { useCourtAvailabilityForCourts } from '../features/reservations/reservationCalendarApi';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/complexes/complexApi');
vi.mock('../features/courts/courtApi');
vi.mock('../features/reservations/reservationApi');
vi.mock('../features/reservations/reservationCalendarApi');

const mockComplex = { id: 'complex-id', name: 'Test Complex', status: 'Active', timeZoneId: 'UTC' };
const mockDashboard = { complex: { id: 'complex-id', name: 'Test Complex', status: 'Active' }, courts: { active: 2, inactive: 1 }, reservationsToday: { confirmed: 5, cancelled: 1, completed: 3 }, blockedUsers: 0 };

function renderPage() {
  return renderWithAuth(<Routes><Route path="/admin/complex/:complexId" element={<ComplexAdminPage />} /></Routes>, { initialRoute: '/admin/complex/complex-id' });
}

describe('ComplexAdminPage', () => {
  beforeEach(() => {
    vi.resetAllMocks();
    vi.mocked(useAdminComplex).mockReturnValue({ data: mockComplex, isLoading: false, isError: false } as never);
    vi.mocked(useComplexDashboard).mockReturnValue({ data: mockDashboard, isLoading: false, isError: false } as never);
    vi.mocked(useCourts).mockReturnValue({ data: { items: [], totalItems: 0 }, isLoading: false, isError: false } as never);
    vi.mocked(useReservations).mockReturnValue({ data: { items: [], totalItems: 0 }, isLoading: false, isError: false } as never);
    vi.mocked(useCourtAvailabilityForCourts).mockReturnValue({ data: {}, isLoading: false, isError: false } as never);
  });

  it('renders real dashboard metrics and the daily agenda', () => {
    renderPage();
    expect(screen.getByRole('heading', { name: 'Test Complex' })).toBeTruthy();
    expect(screen.getByText('Bookings today')).toBeTruthy();
    expect(screen.getByText('Reservation agenda')).toBeTruthy();
    expect(screen.getByText('5 Confirmed')).toBeTruthy();
  });

  it('renders loading placeholders while dashboard data is loading', () => {
    vi.mocked(useComplexDashboard).mockReturnValue({ data: null, isLoading: true, isError: false } as never);
    renderPage();
    expect(document.querySelectorAll('.MuiSkeleton-root').length).toBeGreaterThan(0);
  });

  it('renders an error state when dashboard loading fails', () => {
    vi.mocked(useComplexDashboard).mockReturnValue({ data: null, isLoading: false, isError: true } as never);
    renderPage();
    expect(screen.getByRole('alert').textContent).toContain('dashboard could not be loaded');
  });
});
