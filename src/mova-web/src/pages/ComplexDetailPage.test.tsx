import { screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import ComplexDetailPage from './ComplexDetailPage';
import { renderWithAuth } from '../test-utils';

const mockComplex = {
  id: 'complex-id',
  name: 'Test Complex',
  description: 'A test complex',
  address: 'Test Address',
  city: 'Test City',
  phoneNumber: '+1234567890',
  email: 'test@complex.com'
};

const mockCourts = {
  items: [
    {
      id: 'court-id',
      sportsComplexId: 'complex-id',
      name: 'Court 1',
      description: 'Test court',
      surfaceType: 'Synthetic',
      indoor: true,
      sportIds: ['sport-id']
    }
  ],
  page: 1,
  pageSize: 100,
  totalItems: 1,
  totalPages: 1
};

const mockSports = [
  { id: 'sport-id', name: 'Padel' }
];

const mockSlots = [
  { courtId: 'court-id', startAt: '2026-08-10T08:00:00Z', endAt: '2026-08-10T09:00:00Z' },
  { courtId: 'court-id', startAt: '2026-08-10T09:00:00Z', endAt: '2026-08-10T10:00:00Z' }
];

vi.mock('../features/complexes/complexApi', () => ({
  useActiveComplex: vi.fn(() => ({ isLoading: false, isError: false, data: mockComplex })),
  useActiveCourts: vi.fn(() => ({ isLoading: false, isError: false, data: mockCourts })),
  useSports: vi.fn(() => ({ isLoading: false, isError: false, data: mockSports })),
  useCourtAvailability: vi.fn(() => ({ isLoading: false, isError: false, data: mockSlots }))
}));

describe('ComplexDetailPage', () => {
  it('renders complex details and courts', async () => {
    renderWithAuth(<ComplexDetailPage />, { initialRoute: '/complexes/complex-id' });

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Complex' })).toBeTruthy();
    });

    expect(screen.getByText(/Test City/)).toBeTruthy();
    expect(screen.getByText(/test@complex.com/)).toBeTruthy();
    expect(screen.getByText('Court 1')).toBeTruthy();
  });
});
