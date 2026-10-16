import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { Route, Routes } from 'react-router-dom';
import AdminRecurringReservationsPage from './AdminRecurringReservationsPage';
import { useActiveCourts, useAdminComplex } from '../features/complexes/complexApi';
import { useComplexUsers } from '../features/users/userAdminApi';
import { useCreateRecurringReservationForCustomer } from '../features/reservations/reservationApi';
import { usePriceQuote } from '../features/rates/rateApi';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/complexes/complexApi');
vi.mock('../features/users/userAdminApi');
vi.mock('../features/reservations/reservationApi');
vi.mock('../features/rates/rateApi');

const createMutation = {
  mutateAsync: vi.fn(),
  isPending: false,
  isError: false,
  isSuccess: false,
  error: null,
  data: null
};

function renderPage() {
  return renderWithAuth(
    <Routes>
      <Route
        path="/admin/complex/:complexId/recurring/new"
        element={<AdminRecurringReservationsPage />}
      />
    </Routes>,
    { initialRoute: '/admin/complex/complex-1/recurring/new' }
  );
}

function setSelectValue(name: string, value: string) {
  const combobox = screen.getByRole('combobox', { name });
  const input = combobox.parentElement?.querySelector(
    'input.MuiSelect-nativeInput'
  ) as HTMLInputElement;
  fireEvent.change(input, { target: { value } });
}

function setupBaseMocks(quoteOverrides?: { isFullyCovered?: boolean }) {
  vi.mocked(useAdminComplex).mockReturnValue({
    data: { id: 'complex-1', name: 'Mova Club', timeZoneId: 'America/Montevideo' },
    isLoading: false,
    isError: false
  } as unknown as ReturnType<typeof useAdminComplex>);

  vi.mocked(useActiveCourts).mockReturnValue({
    data: { items: [{ id: 'court-1', name: 'Court One' }] },
    isLoading: false,
    isError: false
  } as unknown as ReturnType<typeof useActiveCourts>);

  vi.mocked(useComplexUsers).mockReturnValue({
    data: {
      items: [{ id: 'user-1', fullName: 'Test User', email: 'user@example.com' }]
    },
    isLoading: false,
    isError: false
  } as unknown as ReturnType<typeof useComplexUsers>);

  vi.mocked(useCreateRecurringReservationForCustomer).mockReturnValue(
    createMutation as unknown as ReturnType<typeof useCreateRecurringReservationForCustomer>
  );

  vi.mocked(usePriceQuote).mockReturnValue({
    data: {
      totalAmount: 1200,
      currency: 'UYU',
      isFullyCovered: quoteOverrides?.isFullyCovered ?? true,
      lines: [],
      gaps: quoteOverrides?.isFullyCovered === false ? [{ startAt: '', endAt: '' }] : []
    },
    isLoading: false,
    isError: false,
    refetch: vi.fn()
  } as unknown as ReturnType<typeof usePriceQuote>);
}

describe('AdminRecurringReservationsPage', () => {
  beforeEach(() => {
    vi.resetAllMocks();
    createMutation.mutateAsync.mockClear();
    createMutation.mutateAsync.mockResolvedValue({ occurrences: [] });
  });

  afterEach(() => {
    cleanup();
  });

  it('renders the form organized in sections with a recurrence summary', () => {
    setupBaseMocks();
    renderPage();

    expect(screen.getByRole('heading', { name: 'Recurring bookings' })).toBeTruthy();
    expect(screen.getByText('Customer and court')).toBeTruthy();
    expect(screen.getByText('Schedule settings')).toBeTruthy();
    expect(screen.getByText('Additional notes')).toBeTruthy();
    expect(screen.getByText('Recurrence summary')).toBeTruthy();
    expect(screen.getByText('Estimated occurrences')).toBeTruthy();
    expect(screen.getByText('Estimated total')).toBeTruthy();
  });

  it('keeps the submit disabled until customer, court and a covered rate are set', () => {
    setupBaseMocks();
    renderPage();

    const submit = screen.getByRole('button', { name: 'Create recurring booking' }) as HTMLButtonElement;
    expect(submit.disabled).toBe(true);

    setSelectValue('Customer', 'user-1');
    setSelectValue('Court', 'court-1');

    expect(submit.disabled).toBe(false);
  });

  it('blocks submission when the slot has no configured rate', () => {
    setupBaseMocks({ isFullyCovered: false });
    renderPage();

    setSelectValue('Customer', 'user-1');
    setSelectValue('Court', 'court-1');

    expect(
      screen.getByText('This time slot has no configured rate for all occurrences.')
    ).toBeTruthy();
    const submit = screen.getByRole('button', { name: 'Create recurring booking' }) as HTMLButtonElement;
    expect(submit.disabled).toBe(true);
  });

  it('submits the recurring reservation with the quoted series total', async () => {
    setupBaseMocks();
    renderPage();

    setSelectValue('Customer', 'user-1');
    setSelectValue('Court', 'court-1');

    fireEvent.click(screen.getByRole('button', { name: 'Create recurring booking' }));

    await waitFor(() => {
      expect(createMutation.mutateAsync).toHaveBeenCalledWith(
        expect.objectContaining({
          userId: 'user-1',
          courtId: 'court-1',
          expectedCurrency: 'UYU',
          expectedTotalPrice: expect.any(Number)
        })
      );
    });
  });
});
