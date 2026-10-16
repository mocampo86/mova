import { screen, waitFor, within } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { useActiveComplexes } from '../features/complexes/complexApi';
import HomePage from './HomePage';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/complexes/complexApi');

const mockUseActiveComplexes = vi.mocked(useActiveComplexes);
const emptyResults = { data: { items: [], page: 1, pageSize: 12, totalItems: 0, totalPages: 0 }, isLoading: false, isError: false } as unknown as ReturnType<typeof useActiveComplexes>;

describe('HomePage', () => {
  it('presents a player-first journey with featured complexes and an owner entry point', async () => {
    mockUseActiveComplexes.mockReturnValue(emptyResults);
    renderWithAuth(<HomePage />);

    expect(screen.getByRole('heading', { name: 'Find your next game.' })).toBeTruthy();
    expect(screen.getByText(/Discover nearby sports complexes/)).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Browse complexes' }).getAttribute('href')).toBe('/complexes');
    expect(screen.getByRole('heading', { name: 'How to find and book a court' })).toBeTruthy();
    expect(screen.getByRole('heading', { name: 'For complex owners' })).toBeTruthy();
    expect(document.title).toBe('Mova | Find your next game');

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Featured complexes' })).toBeTruthy());
  });

  it('keeps a single primary player action in the hero', () => {
    mockUseActiveComplexes.mockReturnValue(emptyResults);
    renderWithAuth(<HomePage />);

    const hero = screen.getByTestId('hero');
    expect(within(hero).getByRole('link', { name: 'Browse complexes' }).getAttribute('href')).toBe('/complexes');
    expect(within(hero).getByTestId('hero-visual')).toBeTruthy();
  });

  it('displays active public complexes as featured links', async () => {
    mockUseActiveComplexes.mockReturnValue({
      data: { items: [{ id: 'complex-1', name: 'Central Padel', description: 'Padel for everyone', address: '123 Main St', city: 'Montevideo', phoneNumber: '', email: '', allowUserRecurringReservations: true, timeZoneId: 'America/Montevideo' }], page: 1, pageSize: 3, totalItems: 1, totalPages: 1 },
      isLoading: false, isError: false
    } as unknown as ReturnType<typeof useActiveComplexes>);
    renderWithAuth(<HomePage />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Central Padel' })).toBeTruthy();
      expect(screen.getByRole('link', { name: 'View complex' }).getAttribute('href')).toBe('/complexes/complex-1');
    });
  });
});
