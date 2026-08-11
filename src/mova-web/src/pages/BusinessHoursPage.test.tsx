import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { Route, Routes } from 'react-router-dom';
import BusinessHoursPage from './BusinessHoursPage';
import { useBusinessHours, useUpdateBusinessHours } from '../features/complexes/complexApi';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/complexes/complexApi');

const mockBusinessHours = [
  {
    id: 'bh-1',
    sportsComplexId: 'complex-1',
    dayOfWeek: 1,
    openingTime: '09:00:00',
    closingTime: '21:00:00',
    isClosed: false
  },
  {
    id: 'bh-2',
    sportsComplexId: 'complex-1',
    dayOfWeek: 2,
    openingTime: '08:00:00',
    closingTime: '22:00:00',
    isClosed: true
  }
];

function renderPage() {
  return renderWithAuth(
    <Routes>
      <Route path="/admin/complex/:complexId/business-hours" element={<BusinessHoursPage />} />
    </Routes>,
    { initialRoute: '/admin/complex/complex-1/business-hours' }
  );
}

describe('BusinessHoursPage', () => {
  const mutateAsync = vi.fn().mockResolvedValue(undefined);

  beforeEach(() => {
    cleanup();
    vi.resetAllMocks();
    mutateAsync.mockClear();
    window.history.pushState({}, '', '/');
  });

  function setupMocks(
    hoursOverrides: Partial<ReturnType<typeof useBusinessHours>> = {},
    updateOverrides: Partial<ReturnType<typeof useUpdateBusinessHours>> = {}
  ) {
    vi.mocked(useBusinessHours).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: false,
      error: null,
      ...hoursOverrides
    } as unknown as ReturnType<typeof useBusinessHours>);

    vi.mocked(useUpdateBusinessHours).mockReturnValue({
      mutateAsync,
      isPending: false,
      isSuccess: false,
      isError: false,
      error: null,
      ...updateOverrides
    } as unknown as ReturnType<typeof useUpdateBusinessHours>);
  }

  it('renders business hours form with default values', () => {
    setupMocks();
    renderPage();

    expect(screen.getByRole('heading', { name: 'Business hours' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Save hours' })).toBeTruthy();

    const openingTimeInputs = screen.getAllByLabelText('Opening time') as HTMLInputElement[];
    const closingTimeInputs = screen.getAllByLabelText('Closing time') as HTMLInputElement[];

    expect(openingTimeInputs).toHaveLength(7);
    expect(closingTimeInputs).toHaveLength(7);
    expect(openingTimeInputs[0].value).toBe('08:00');
    expect(closingTimeInputs[0].value).toBe('22:00');
  });

  it('merges existing business hours into the form', () => {
    setupMocks({ data: mockBusinessHours });
    renderPage();

    const openingTimeInputs = screen.getAllByLabelText('Opening time') as HTMLInputElement[];
    const closingTimeInputs = screen.getAllByLabelText('Closing time') as HTMLInputElement[];

    // Monday (index 0) should reflect the existing 09:00-21:00 values.
    expect(openingTimeInputs[0].value).toBe('09:00');
    expect(closingTimeInputs[0].value).toBe('21:00');
  });

  it('submits business hours mapped to API format', async () => {
    setupMocks();
    renderPage();

    const openingTimeInputs = screen.getAllByLabelText('Opening time') as HTMLInputElement[];
    const closingTimeInputs = screen.getAllByLabelText('Closing time') as HTMLInputElement[];

    fireEvent.change(openingTimeInputs[0], { target: { value: '10:00' } });
    fireEvent.change(closingTimeInputs[0], { target: { value: '20:00' } });

    fireEvent.click(screen.getByRole('button', { name: 'Save hours' }));

    await waitFor(() => {
      expect(mutateAsync).toHaveBeenCalled();
    });

    const request = mutateAsync.mock.calls[0][0];
    expect(request.hours).toHaveLength(7);
    expect(request.hours[0]).toEqual({
      dayOfWeek: 1,
      openingTime: '10:00:00',
      closingTime: '20:00:00',
      isClosed: false
    });
  });

  it('displays an error when business hours fail to load', () => {
    setupMocks({ isError: true, error: new Error('Load failed') });
    renderPage();

    expect(screen.getByText('Load failed')).toBeTruthy();
  });
});
