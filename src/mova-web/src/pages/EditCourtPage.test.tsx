import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { Routes, Route } from 'react-router-dom';
import EditCourtPage from './EditCourtPage';
import { useCourt, useUpdateCourt } from '../features/courts/courtApi';
import { useSports } from '../features/complexes/complexApi';
import { renderWithAuth } from '../test-utils';
import type { Court } from '../features/complexes/complexTypes';

vi.mock('../features/courts/courtApi');
vi.mock('../features/complexes/complexApi');

const mockSports = [
  { id: 'sport-1', name: 'Football' },
  { id: 'sport-2', name: 'Padel' }
];

const mockCourt: Court = {
  id: 'court-1',
  sportsComplexId: 'complex-1',
  name: 'Court One',
  description: 'Original description',
  surfaceType: 'Synthetic',
  indoor: false,
  status: 'Active',
  sportIds: ['sport-1'],
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: null
};

function renderPage() {
  return renderWithAuth(
    <Routes>
      <Route path="/admin/complex/:complexId/courts/:courtId/edit" element={<EditCourtPage />} />
      <Route path="/admin/complex/:complexId/courts" element={<div>Courts list</div>} />
    </Routes>,
    { initialRoute: '/admin/complex/complex-1/courts/court-1/edit' }
  );
}

describe('EditCourtPage', () => {
  const mutate = vi.fn((_variables, options) => {
    options?.onSuccess?.();
  });

  beforeEach(() => {
    cleanup();
    vi.resetAllMocks();
    mutate.mockClear();
    window.history.pushState({}, '', '/');
  });

  function setupMocks(
    overrides: Partial<ReturnType<typeof useUpdateCourt>> = {},
    courtOverrides: Partial<ReturnType<typeof useCourt>> = {},
    sportsOverrides: Partial<ReturnType<typeof useSports>> = {}
  ) {
    vi.mocked(useUpdateCourt).mockReturnValue({
      mutate,
      isPending: false,
      error: null,
      ...overrides
    } as unknown as ReturnType<typeof useUpdateCourt>);

    vi.mocked(useCourt).mockReturnValue({
      data: mockCourt,
      isLoading: false,
      isError: false,
      error: null,
      ...courtOverrides
    } as unknown as ReturnType<typeof useCourt>);

    vi.mocked(useSports).mockReturnValue({
      data: mockSports,
      isLoading: false,
      isError: false,
      ...sportsOverrides
    } as unknown as ReturnType<typeof useSports>);
  }

  it('renders the edit court form with pre-populated data', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Edit court' })).toBeTruthy();
    });

    expect((screen.getByLabelText(/Court name/i) as HTMLInputElement).value).toBe('Court One');
    expect((screen.getByLabelText(/Description/i) as HTMLInputElement).value).toBe('Original description');
    expect((screen.getByLabelText(/Surface type/i) as HTMLInputElement).value).toBe('Synthetic');
    expect((screen.getByLabelText(/Indoor court/i) as HTMLInputElement).checked).toBe(false);
    expect((screen.getByLabelText('Football') as HTMLInputElement).checked).toBe(true);
    expect((screen.getByLabelText('Padel') as HTMLInputElement).checked).toBe(false);
    expect(screen.getByRole('button', { name: 'Update court' })).toBeTruthy();
  });

  it('displays validation errors for missing required fields', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Update court' })).toBeTruthy();
    });

    fireEvent.change(screen.getByLabelText(/Court name/i), { target: { value: '' } });
    fireEvent.change(screen.getByLabelText(/Description/i), { target: { value: '' } });
    fireEvent.change(screen.getByLabelText(/Surface type/i), { target: { value: '' } });

    fireEvent.click(screen.getByRole('button', { name: 'Update court' }));

    await waitFor(() => {
      expect(screen.getByText(/Name is required/i)).toBeTruthy();
      expect(screen.getByText(/Description is required/i)).toBeTruthy();
      expect(screen.getByText(/Surface type is required/i)).toBeTruthy();
    });
  });

  it('submits the form with valid values and calls mutate', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getByLabelText(/Court name/i)).toBeTruthy();
    });

    fireEvent.change(screen.getByLabelText(/Court name/i), { target: { value: 'Updated Court' } });
    fireEvent.change(screen.getByLabelText(/Description/i), { target: { value: 'Updated description' } });
    fireEvent.change(screen.getByLabelText(/Surface type/i), { target: { value: 'Grass' } });
    fireEvent.click(screen.getByLabelText(/Indoor court/i));

    fireEvent.click(screen.getByRole('button', { name: 'Update court' }));

    await waitFor(() => {
      expect(mutate).toHaveBeenCalledOnce();
      expect(mutate).toHaveBeenCalledWith(
        {
          name: 'Updated Court',
          description: 'Updated description',
          surfaceType: 'Grass',
          indoor: true,
          sportIds: ['sport-1']
        },
        expect.any(Object)
      );
    });
  });

  it('navigates to the courts list after successful update', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getByLabelText(/Court name/i)).toBeTruthy();
    });

    fireEvent.change(screen.getByLabelText(/Court name/i), { target: { value: 'Updated Court' } });

    fireEvent.click(screen.getByRole('button', { name: 'Update court' }));

    await waitFor(() => {
      expect(window.location.pathname).toBe('/admin/complex/complex-1/courts');
    });
  });

  it('displays the mutation error message when update fails', async () => {
    setupMocks({ error: new Error('Court update failed') });
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Court update failed')).toBeTruthy();
    });
  });

  it('shows a warning when sports cannot be loaded', async () => {
    setupMocks({}, {}, { data: undefined, isLoading: false, isError: true });
    renderPage();

    await waitFor(() => {
      expect(screen.getByText(/Available sports could not be loaded/i)).toBeTruthy();
    });
  });

  it('shows a skeleton while court is loading', async () => {
    setupMocks({}, { data: undefined, isLoading: true, isError: false });
    renderPage();

    await waitFor(() => {
      expect(document.querySelectorAll('.MuiSkeleton-root').length).toBeGreaterThan(0);
    });
  });

  it('shows an error when the court cannot be loaded', async () => {
    setupMocks({}, { data: undefined, isLoading: false, isError: true, error: new Error('Court not found') });
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Court not found')).toBeTruthy();
    });
  });
});
