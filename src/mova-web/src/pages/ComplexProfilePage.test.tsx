import { cleanup, screen, waitFor } from '@testing-library/react';
import { Routes, Route } from 'react-router-dom';
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import ComplexProfilePage from './ComplexProfilePage';
import { useComplexDashboard } from '../features/complexes/complexApi';
import { useUpdateComplexStatus } from '../features/complexes/useUpdateComplexStatus';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/complexes/complexApi');
vi.mock('../features/complexes/useUpdateComplexStatus');

const createMockDashboard = (status = 'Active') => ({
  complex: {
    id: 'complex-id',
    name: 'Test Complex',
    status,
    lastUpdatedAt: '2026-08-06T12:00:00Z'
  },
  courts: { active: 2, inactive: 1 },
  reservationsToday: { confirmed: 5, cancelled: 1, completed: 3 },
  blockedUsers: 2
});

function renderPage(dashboardState: {
  data?: ReturnType<typeof useComplexDashboard>['data'];
  isLoading: boolean;
  isError: boolean;
}) {
  vi.mocked(useComplexDashboard).mockReturnValue(
    dashboardState as unknown as ReturnType<typeof useComplexDashboard>
  );

  vi.mocked(useUpdateComplexStatus).mockReturnValue({
    mutate: vi.fn(),
    isPending: false,
    error: null,
    reset: vi.fn()
  } as unknown as ReturnType<typeof useUpdateComplexStatus>);

  return renderWithAuth(
    <Routes>
      <Route path="/admin/complex/:complexId" element={<ComplexProfilePage />} />
    </Routes>,
    { initialRoute: '/admin/complex/complex-id' }
  );
}

describe('ComplexProfilePage', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  afterEach(cleanup);

  it('renders complex name, last updated date, and the status toggle', async () => {
    renderPage({ data: createMockDashboard(), isLoading: false, isError: false });

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Complex' })).toBeTruthy();
    });

    expect(screen.getByText(/Last updated/i)).toBeTruthy();
    expect(screen.getByText(/This complex is visible to the public/i)).toBeTruthy();
    expect(screen.getByLabelText('Active')).toBeTruthy();
  });

  it('reflects an inactive complex status', async () => {
    renderPage({ data: createMockDashboard('Inactive'), isLoading: false, isError: false });

    await waitFor(() => {
      expect(screen.getByText(/This complex is hidden from public listings/i)).toBeTruthy();
    });

    expect(screen.getByLabelText('Inactive')).toBeTruthy();
  });

  it('renders loading skeletons while the dashboard is loading', () => {
    renderPage({ data: undefined, isLoading: true, isError: false });

    expect(document.querySelectorAll('.MuiSkeleton-root').length).toBeGreaterThan(0);
  });

  it('renders an error state when the dashboard cannot be loaded', () => {
    renderPage({ data: undefined, isLoading: false, isError: true });

    expect(screen.getByText(/profile could not be loaded/i)).toBeTruthy();
  });
});
