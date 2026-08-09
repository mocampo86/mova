import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { Routes, Route } from 'react-router-dom';
import CreateCourtPage from './CreateCourtPage';
import { useCreateCourt } from '../features/courts/courtApi';
import { useSports } from '../features/complexes/complexApi';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/courts/courtApi');
vi.mock('../features/complexes/complexApi');

const mockSports = [
  { id: 'sport-1', name: 'Football' },
  { id: 'sport-2', name: 'Padel' }
];

function renderPage() {
  return renderWithAuth(
    <Routes>
      <Route path="/admin/complex/:complexId/courts/new" element={<CreateCourtPage />} />
      <Route path="/admin/complex/:complexId/courts" element={<div>Courts list</div>} />
    </Routes>,
    { initialRoute: '/admin/complex/complex-1/courts/new' }
  );
}

describe('CreateCourtPage', () => {
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
    overrides: Partial<ReturnType<typeof useCreateCourt>> = {},
    sportsOverrides: Partial<ReturnType<typeof useSports>> = {}
  ) {
    vi.mocked(useCreateCourt).mockReturnValue({
      mutate,
      isPending: false,
      error: null,
      ...overrides
    } as unknown as ReturnType<typeof useCreateCourt>);

    vi.mocked(useSports).mockReturnValue({
      data: mockSports,
      isLoading: false,
      isError: false,
      ...sportsOverrides
    } as unknown as ReturnType<typeof useSports>);
  }

  it('renders the create court form', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Create court' })).toBeTruthy();
    });

    expect(screen.getByLabelText(/Court name/i)).toBeTruthy();
    expect(screen.getByLabelText(/Description/i)).toBeTruthy();
    expect(screen.getByLabelText(/Surface type/i)).toBeTruthy();
    expect(screen.getByLabelText(/Indoor court/i)).toBeTruthy();
    expect(screen.getByText(/Sports \(optional\)/i)).toBeTruthy();
    expect(screen.getByLabelText('Football')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Create court' })).toBeTruthy();
  });

  it('displays validation errors for missing required fields', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Create court' })).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Create court' }));

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

    fireEvent.change(screen.getByLabelText(/Court name/i), {
      target: { value: 'New Court' }
    });
    fireEvent.change(screen.getByLabelText(/Description/i), {
      target: { value: 'A new court for the complex' }
    });
    fireEvent.change(screen.getByLabelText(/Surface type/i), {
      target: { value: 'Synthetic' }
    });

    fireEvent.click(screen.getByRole('button', { name: 'Create court' }));

    await waitFor(() => {
      expect(mutate).toHaveBeenCalledOnce();
      expect(mutate).toHaveBeenCalledWith(
        {
          name: 'New Court',
          description: 'A new court for the complex',
          surfaceType: 'Synthetic',
          indoor: false,
          sportIds: []
        },
        expect.any(Object)
      );
    });
  });

  it('includes selected sports in the submission', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getByLabelText(/Court name/i)).toBeTruthy();
    });

    fireEvent.change(screen.getByLabelText(/Court name/i), {
      target: { value: 'Sports Court' }
    });
    fireEvent.change(screen.getByLabelText(/Description/i), {
      target: { value: 'Court with sports' }
    });
    fireEvent.change(screen.getByLabelText(/Surface type/i), {
      target: { value: 'Grass' }
    });

    fireEvent.click(screen.getByLabelText('Football'));
    fireEvent.click(screen.getByLabelText('Padel'));

    fireEvent.click(screen.getByRole('button', { name: 'Create court' }));

    await waitFor(() => {
      expect(mutate).toHaveBeenCalledOnce();
      expect(mutate).toHaveBeenCalledWith(
        {
          name: 'Sports Court',
          description: 'Court with sports',
          surfaceType: 'Grass',
          indoor: false,
          sportIds: ['sport-1', 'sport-2']
        },
        expect.any(Object)
      );
    });
  });

  it('navigates to the courts list after successful creation', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getByLabelText(/Court name/i)).toBeTruthy();
    });

    fireEvent.change(screen.getByLabelText(/Court name/i), {
      target: { value: 'New Court' }
    });
    fireEvent.change(screen.getByLabelText(/Description/i), {
      target: { value: 'A new court' }
    });
    fireEvent.change(screen.getByLabelText(/Surface type/i), {
      target: { value: 'Concrete' }
    });

    fireEvent.click(screen.getByRole('button', { name: 'Create court' }));

    await waitFor(() => {
      expect(window.location.pathname).toBe('/admin/complex/complex-1/courts');
    });
  });

  it('displays the mutation error message when creation fails', async () => {
    setupMocks({
      error: new Error('Court creation failed')
    });
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Court creation failed')).toBeTruthy();
    });
  });

  it('shows a warning when sports cannot be loaded', async () => {
    setupMocks({}, { data: undefined, isLoading: false, isError: true });
    renderPage();

    await waitFor(() => {
      expect(
        screen.getByText(/Available sports could not be loaded/i)
      ).toBeTruthy();
    });
  });

  it('shows a skeleton while sports are loading', async () => {
    setupMocks({}, { data: undefined, isLoading: true, isError: false });
    renderPage();

    await waitFor(() => {
      expect(document.querySelectorAll('.MuiSkeleton-root').length).toBeGreaterThan(0);
    });
  });
});
