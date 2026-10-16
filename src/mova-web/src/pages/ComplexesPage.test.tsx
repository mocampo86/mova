import { act, cleanup, fireEvent, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useActiveComplexes } from '../features/complexes/complexApi';
import ComplexesPage from './ComplexesPage';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/complexes/complexApi');

const mockUseActiveComplexes = vi.mocked(useActiveComplexes);

const emptyPagedResult = {
  items: [],
  page: 1,
  pageSize: 12,
  totalItems: 0,
  totalPages: 0
};

beforeEach(() => {
  cleanup();
  vi.resetAllMocks();
});

describe('ComplexesPage', () => {
  it('renders search form and complex list', () => {
    mockUseActiveComplexes.mockReturnValue({
      data: {
        items: [
          {
            id: 'complex-1',
            name: 'Central Padel',
            description: 'Padel for everyone',
            address: '123 Main St',
            city: 'Montevideo',
            phoneNumber: '',
            email: '',
            allowUserRecurringReservations: true,
            timeZoneId: 'America/Montevideo'
          }
        ],
        page: 1,
        pageSize: 12,
        totalItems: 1,
        totalPages: 1
      },
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useActiveComplexes>);

    renderWithAuth(<ComplexesPage />);

    expect(screen.getByRole('heading', { name: 'Find your court' })).toBeTruthy();
    expect(screen.getByRole('searchbox', { name: 'Search complexes' })).toBeTruthy();
    expect(screen.getByRole('heading', { name: 'Available complexes' })).toBeTruthy();
    expect(screen.getByText('1 complex available')).toBeTruthy();
    expect(screen.getByRole('heading', { name: 'Central Padel' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'View courts and times' }).getAttribute('href')).toBe('/complexes/complex-1');
    expect(document.title).toBe('Mova | Find a sports complex');
    expect(document.querySelector('meta[name="description"]')?.getAttribute('content')).toBe('Search active complexes by name, city, or address.');
  });

  it('renders pagination controls when there is more than one page', () => {
    mockUseActiveComplexes.mockReturnValue({
      data: { ...emptyPagedResult, totalItems: 25, totalPages: 3 },
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useActiveComplexes>);

    renderWithAuth(<ComplexesPage />);

    expect(screen.getByRole('button', { name: 'Go to page 2' })).toBeTruthy();
  });

  it('does not render pagination controls when there is only one page', () => {
    mockUseActiveComplexes.mockReturnValue({
      data: emptyPagedResult,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useActiveComplexes>);

    renderWithAuth(<ComplexesPage />);

    expect(screen.queryByRole('button', { name: 'Go to page 2' })).toBeNull();
  });

  it('changes page when a pagination button is clicked', () => {
    mockUseActiveComplexes.mockReturnValue({
      data: { ...emptyPagedResult, totalItems: 25, totalPages: 3 },
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useActiveComplexes>);

    renderWithAuth(<ComplexesPage />);

    fireEvent.click(screen.getByRole('button', { name: 'Go to page 2' }));

    expect(mockUseActiveComplexes).toHaveBeenLastCalledWith('', 2);
  });

  it('searches while typing after a debounce', () => {
    vi.useFakeTimers();
    mockUseActiveComplexes.mockReturnValue({
      data: emptyPagedResult,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useActiveComplexes>);

    renderWithAuth(<ComplexesPage />);

    fireEvent.change(screen.getByRole('searchbox', { name: 'Search complexes' }), { target: { value: 'padel' } });
    act(() => { vi.advanceTimersByTime(400); });

    expect(mockUseActiveComplexes).toHaveBeenLastCalledWith('padel', 1);
    vi.useRealTimers();
  });

  it('renders an empty state message when there are no matches', () => {
    mockUseActiveComplexes.mockReturnValue({
      data: emptyPagedResult,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useActiveComplexes>);

    renderWithAuth(<ComplexesPage />);

    expect(screen.getByText('We could not find complexes matching that search.')).toBeTruthy();
  });
});
