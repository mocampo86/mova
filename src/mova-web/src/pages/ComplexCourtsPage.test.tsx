import { cleanup, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { Routes, Route } from 'react-router-dom';
import ComplexCourtsPage from './ComplexCourtsPage';
import { useCourts, useUpdateCourtStatus } from '../features/courts/courtApi';
import { useSports } from '../features/complexes/complexApi';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/courts/courtApi');
vi.mock('../features/complexes/complexApi');

const mockCourts = {
  items: [
    {
      id: 'court-1',
      sportsComplexId: 'complex-1',
      name: 'Court One',
      description: 'Main court',
      surfaceType: 'Synthetic',
      indoor: true,
      status: 'Active',
      sportIds: ['sport-1'],
      createdAt: '2026-08-01T12:00:00Z',
      updatedAt: '2026-08-02T12:00:00Z'
    },
    {
      id: 'court-2',
      sportsComplexId: 'complex-1',
      name: 'Court Two',
      description: '',
      surfaceType: 'Grass',
      indoor: false,
      status: 'Inactive',
      sportIds: ['sport-1', 'sport-2'],
      createdAt: '2026-08-03T12:00:00Z',
      updatedAt: null
    }
  ],
  page: 1,
  pageSize: 10,
  totalItems: 2,
  totalPages: 1
};

const mockSports = [
  { id: 'sport-1', name: 'Football' },
  { id: 'sport-2', name: 'Padel' }
];

describe('ComplexCourtsPage', () => {
  const mutate = vi.fn();

  beforeEach(() => {
    cleanup();
    vi.resetAllMocks();
    mutate.mockClear();
  });

  function setupMocks(
    overrides: Partial<Omit<ReturnType<typeof useCourts>, 'data'>> & { data?: unknown } = {}
  ) {
    vi.mocked(useCourts).mockReturnValue({
      data: mockCourts,
      isLoading: false,
      isError: false,
      ...overrides
    } as unknown as ReturnType<typeof useCourts>);

    vi.mocked(useSports).mockReturnValue({
      data: mockSports,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useSports>);

    vi.mocked(useUpdateCourtStatus).mockReturnValue({
      mutate,
      isPending: false,
      error: null
    } as unknown as ReturnType<typeof useUpdateCourtStatus>);
  }

  it('renders the courts list with details and actions', async () => {
    setupMocks();

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/courts" element={<ComplexCourtsPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/courts' }
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Courts' })).toBeTruthy();
    });

    expect(screen.getByRole('link', { name: 'Create court' })).toBeTruthy();

    expect(screen.getByText('Court One')).toBeTruthy();
    expect(screen.getByText('Court Two')).toBeTruthy();
    expect(screen.getByText('Synthetic')).toBeTruthy();
    expect(screen.getByText('Grass')).toBeTruthy();
    expect(screen.getByText('Indoor')).toBeTruthy();
    expect(screen.getByText('Outdoor')).toBeTruthy();

    expect(screen.getAllByText('Active').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Inactive').length).toBeGreaterThan(0);

    expect(screen.getByText('Football')).toBeTruthy();
    expect(screen.getByText('Football, Padel')).toBeTruthy();

    expect(screen.getAllByRole('link', { name: 'Edit' }).length).toBe(2);
    expect(screen.getAllByRole('link', { name: 'Configure' }).length).toBe(2);
    expect(screen.getByRole('button', { name: 'Deactivate' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Activate' })).toBeTruthy();
  });

  it('renders an empty state when no courts exist', async () => {
    setupMocks({
      data: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 }
    });

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/courts" element={<ComplexCourtsPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/courts' }
    );

    await waitFor(() => {
      expect(screen.getByText(/No courts found for this complex/i)).toBeTruthy();
    });
  });

  it('renders an error state when the query fails', async () => {
    setupMocks({ data: null, isLoading: false, isError: true });

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/courts" element={<ComplexCourtsPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/courts' }
    );

    await waitFor(() => {
      expect(screen.getByText(/The courts could not be loaded/i)).toBeTruthy();
    });
  });

  it('renders loading skeletons while loading', () => {
    setupMocks({ data: null, isLoading: true, isError: false });

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/courts" element={<ComplexCourtsPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/courts' }
    );

    expect(document.querySelectorAll('.MuiSkeleton-root').length).toBeGreaterThan(0);
  });

  it('calls mutate to deactivate an active court', async () => {
    setupMocks();

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/courts" element={<ComplexCourtsPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/courts' }
    );

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Deactivate' })).toBeTruthy();
    });

    screen.getByRole('button', { name: 'Deactivate' }).click();

    expect(mutate).toHaveBeenCalledWith({
      courtId: 'court-1',
      request: { status: 'Inactive' }
    });
  });

  it('calls mutate to activate an inactive court', async () => {
    setupMocks();

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/courts" element={<ComplexCourtsPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/courts' }
    );

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Activate' })).toBeTruthy();
    });

    screen.getByRole('button', { name: 'Activate' }).click();

    expect(mutate).toHaveBeenCalledWith({
      courtId: 'court-2',
      request: { status: 'Active' }
    });
  });
});
