import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { Routes, Route } from 'react-router-dom';
import EditCourtPage from './EditCourtPage';
import {
  useAssignCourtSports,
  useCourt,
  useCourtAvailabilityRules,
  useUpdateCourt,
  useUpdateCourtAvailability
} from '../features/courts/courtApi';
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

const mockRules = [
  {
    id: 'rule-1',
    courtId: 'court-1',
    dayOfWeek: 1,
    startTime: '08:00:00',
    endTime: '12:00:00',
    slotDurationMinutes: 60,
    isActive: true
  }
];

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
  const updateCourtMutateAsync = vi.fn().mockResolvedValue(undefined);
  const assignSportsMutateAsync = vi.fn().mockResolvedValue(undefined);
  const updateAvailabilityMutateAsync = vi.fn().mockResolvedValue(undefined);

  beforeEach(() => {
    cleanup();
    vi.resetAllMocks();
    updateCourtMutateAsync.mockClear();
    assignSportsMutateAsync.mockClear();
    updateAvailabilityMutateAsync.mockClear();
    window.history.pushState({}, '', '/');
  });

  function setupMocks(
    courtOverrides: Partial<ReturnType<typeof useCourt>> = {},
    sportsOverrides: Partial<ReturnType<typeof useSports>> = {},
    availabilityOverrides: Partial<ReturnType<typeof useCourtAvailabilityRules>> = {},
    updateCourtOverrides: Partial<ReturnType<typeof useUpdateCourt>> = {},
    assignSportsOverrides: Partial<ReturnType<typeof useAssignCourtSports>> = {},
    updateAvailabilityOverrides: Partial<ReturnType<typeof useUpdateCourtAvailability>> = {}
  ) {
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

    vi.mocked(useCourtAvailabilityRules).mockReturnValue({
      data: mockRules,
      isLoading: false,
      isError: false,
      error: null,
      ...availabilityOverrides
    } as unknown as ReturnType<typeof useCourtAvailabilityRules>);

    vi.mocked(useUpdateCourt).mockReturnValue({
      mutateAsync: updateCourtMutateAsync,
      isPending: false,
      error: null,
      ...updateCourtOverrides
    } as unknown as ReturnType<typeof useUpdateCourt>);

    vi.mocked(useAssignCourtSports).mockReturnValue({
      mutateAsync: assignSportsMutateAsync,
      isPending: false,
      error: null,
      ...assignSportsOverrides
    } as unknown as ReturnType<typeof useAssignCourtSports>);

    vi.mocked(useUpdateCourtAvailability).mockReturnValue({
      mutateAsync: updateAvailabilityMutateAsync,
      isPending: false,
      error: null,
      ...updateAvailabilityOverrides
    } as unknown as ReturnType<typeof useUpdateCourtAvailability>);
  }

  it('renders the configure court form with pre-populated data', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Configure court' })).toBeTruthy();
    });

    await waitFor(() => {
      expect((screen.getByLabelText(/Court name/i) as HTMLInputElement).value).toBe('Court One');
    });
    expect((screen.getByLabelText(/Description/i) as HTMLInputElement).value).toBe(
      'Original description'
    );
    expect((screen.getByLabelText(/Surface type/i) as HTMLInputElement).value).toBe('Synthetic');
    expect((screen.getByLabelText(/Indoor court/i) as HTMLInputElement).checked).toBe(false);
    expect((screen.getByLabelText('Football') as HTMLInputElement).checked).toBe(true);
    expect((screen.getByLabelText('Padel') as HTMLInputElement).checked).toBe(false);

    const startTimeInputs = screen.getAllByLabelText('Start time');
    expect((startTimeInputs[0] as HTMLInputElement).value).toBe('08:00');

    const endTimeInputs = screen.getAllByLabelText('End time');
    expect((endTimeInputs[0] as HTMLInputElement).value).toBe('12:00');

    expect((screen.getByRole('switch', { name: 'Monday active' }) as HTMLInputElement).checked).toBe(true);
    expect(screen.getByRole('button', { name: 'Update court' })).toBeTruthy();
  }, 10000);

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

  it('displays a validation error when no sports are selected', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getByLabelText('Football')).toBeTruthy();
    });

    fireEvent.click(screen.getByLabelText('Football'));
    fireEvent.click(screen.getByRole('button', { name: 'Update court' }));

    await waitFor(() => {
      expect(screen.getByText(/At least one sport must be assigned/i)).toBeTruthy();
    });
  });

  it('displays availability validation errors for invalid time ranges', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getAllByLabelText('Start time').length).toBeGreaterThan(0);
    });

    const startTimeInputs = screen.getAllByLabelText('Start time');
    const endTimeInputs = screen.getAllByLabelText('End time');
    const slotDurationInputs = screen.getAllByLabelText('Slot duration');

    fireEvent.change(startTimeInputs[0], { target: { value: '10:00' } });
    fireEvent.change(endTimeInputs[0], { target: { value: '10:00' } });
    fireEvent.change(slotDurationInputs[0], { target: { value: '60' } });

    fireEvent.click(screen.getByRole('button', { name: 'Update court' }));

    await waitFor(() => {
      expect(screen.getByText(/Start and end times must be different/i)).toBeTruthy();
    });
  });

  it('displays a validation error when slot duration does not fit the range', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getAllByLabelText('Start time').length).toBeGreaterThan(0);
    });

    const startTimeInputs = screen.getAllByLabelText('Start time');
    const endTimeInputs = screen.getAllByLabelText('End time');
    const slotDurationInputs = screen.getAllByLabelText('Slot duration');

    fireEvent.change(startTimeInputs[0], { target: { value: '08:00' } });
    fireEvent.change(endTimeInputs[0], { target: { value: '09:30' } });
    fireEvent.change(slotDurationInputs[0], { target: { value: '60' } });

    fireEvent.click(screen.getByRole('button', { name: 'Update court' }));

    await waitFor(() => {
      expect(
        screen.getByText(/The time range must be evenly divisible by the slot duration/i)
      ).toBeTruthy();
    });
  });

  it('submits the form with valid values and calls the three mutations', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getByLabelText(/Court name/i)).toBeTruthy();
    });

    fireEvent.change(screen.getByLabelText(/Court name/i), { target: { value: 'Updated Court' } });
    fireEvent.change(screen.getByLabelText(/Description/i), {
      target: { value: 'Updated description' }
    });
    fireEvent.change(screen.getByLabelText(/Surface type/i), { target: { value: 'Grass' } });
    fireEvent.click(screen.getByLabelText(/Indoor court/i));
    fireEvent.click(screen.getByLabelText('Padel'));

    const startTimeInputs = screen.getAllByLabelText('Start time');
    fireEvent.change(startTimeInputs[0], { target: { value: '09:00' } });

    fireEvent.click(screen.getByRole('button', { name: 'Update court' }));

    await waitFor(() => {
      expect(updateCourtMutateAsync).toHaveBeenCalledOnce();
      expect(updateCourtMutateAsync).toHaveBeenCalledWith({
        name: 'Updated Court',
        description: 'Updated description',
        surfaceType: 'Grass',
        indoor: true
      });

      expect(assignSportsMutateAsync).toHaveBeenCalledOnce();
      expect(assignSportsMutateAsync).toHaveBeenCalledWith({
        sportIds: ['sport-1', 'sport-2']
      });

      expect(updateAvailabilityMutateAsync).toHaveBeenCalledOnce();
      const availabilityCall = updateAvailabilityMutateAsync.mock.calls[0][0];
      expect(availabilityCall.rules.length).toBe(7);

      const mondayRule = availabilityCall.rules.find((rule: { dayOfWeek: number }) => rule.dayOfWeek === 1);
      expect(mondayRule).toEqual({
        dayOfWeek: 1,
        startTime: '09:00:00',
        endTime: '12:00:00',
        slotDurationMinutes: 60,
        isActive: true
      });
    });
  }, 10000);

  it('submits an overnight availability rule successfully', async () => {
    setupMocks();
    renderPage();

    await waitFor(() => {
      expect(screen.getAllByLabelText('Start time').length).toBeGreaterThan(0);
    });

    const startTimeInputs = screen.getAllByLabelText('Start time');
    const endTimeInputs = screen.getAllByLabelText('End time');

    fireEvent.change(startTimeInputs[0], { target: { value: '22:00' } });
    fireEvent.change(endTimeInputs[0], { target: { value: '02:00' } });

    fireEvent.click(screen.getByRole('button', { name: 'Update court' }));

    await waitFor(() => {
      expect(updateAvailabilityMutateAsync).toHaveBeenCalledOnce();
      const availabilityCall = updateAvailabilityMutateAsync.mock.calls[0][0];
      const mondayRule = availabilityCall.rules.find((rule: { dayOfWeek: number }) => rule.dayOfWeek === 1);
      expect(mondayRule).toEqual({
        dayOfWeek: 1,
        startTime: '22:00:00',
        endTime: '02:00:00',
        slotDurationMinutes: 60,
        isActive: true
      });
    });
  }, 10000);

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
    setupMocks(
      {},
      {},
      {},
      {
        mutateAsync: vi.fn().mockRejectedValue(new Error('Court update failed')),
        error: new Error('Court update failed')
      }
    );
    renderPage();

    await waitFor(() => {
      expect(screen.getByLabelText(/Court name/i)).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Update court' }));

    await waitFor(() => {
      expect(screen.queryByText('Court update failed')).toBeTruthy();
    });
  });

  it('shows a warning when sports cannot be loaded', async () => {
    setupMocks({}, { data: undefined, isLoading: false, isError: true });
    renderPage();

    await waitFor(() => {
      expect(screen.getByText(/Available sports could not be loaded/i)).toBeTruthy();
    });
  });

  it('shows a warning and disables availability when availability rules cannot be loaded', async () => {
    setupMocks(
      {},
      {},
      { data: undefined, isLoading: false, isError: true, error: new Error('Rules not found') }
    );
    renderPage();

    await waitFor(() => {
      expect(screen.getByText(/Availability rules could not be loaded/i)).toBeTruthy();
    });

    const startTimeInputs = screen.getAllByLabelText('Start time');
    expect((startTimeInputs[0] as HTMLInputElement).disabled).toBe(true);
  });

  it('skips availability update when availability rules could not be loaded', async () => {
    setupMocks(
      {},
      {},
      { data: undefined, isLoading: false, isError: true, error: new Error('Rules not found') }
    );
    renderPage();

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Update court' })).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Update court' }));

    await waitFor(() => {
      expect(updateCourtMutateAsync).toHaveBeenCalledOnce();
      expect(assignSportsMutateAsync).toHaveBeenCalledOnce();
      expect(updateAvailabilityMutateAsync).not.toHaveBeenCalled();
      expect(window.location.pathname).toBe('/admin/complex/complex-1/courts');
    });
  });

  it('shows a skeleton while court is loading', async () => {
    setupMocks({ data: undefined, isLoading: true, isError: false });
    renderPage();

    await waitFor(() => {
      expect(document.querySelectorAll('.MuiSkeleton-root').length).toBeGreaterThan(0);
    });
  });

  it('shows an error when the court cannot be loaded', async () => {
    setupMocks({ data: undefined, isLoading: false, isError: true, error: new Error('Court not found') });
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Court not found')).toBeTruthy();
    });
  });
});
