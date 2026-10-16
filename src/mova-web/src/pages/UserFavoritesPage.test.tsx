import { cleanup, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import UserFavoritesPage from './UserFavoritesPage';
import { useFavoriteComplexes } from '../features/favorites/favoritesApi';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/favorites/favoritesApi', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../features/favorites/favoritesApi')>();
  return { ...actual, useFavoriteComplexes: vi.fn(), useFavoriteComplexIds: vi.fn(), useAddFavoriteComplex: vi.fn(), useRemoveFavoriteComplex: vi.fn() };
});
vi.mock('../features/favorites/FavoriteToggleButton', () => ({ default: () => <button aria-label="Remove from favorites" /> }));

const mockUseFavoriteComplexes = vi.mocked(useFavoriteComplexes);

const emptyResult = { items: [], page: 1, pageSize: 12, totalItems: 0, totalPages: 0 };

beforeEach(() => {
  cleanup();
  vi.resetAllMocks();
});

describe('UserFavoritesPage', () => {
  it('renders the favorites list with real data', () => {
    mockUseFavoriteComplexes.mockReturnValue({
      data: {
        items: [
          {
            id: 'complex-1',
            name: 'Rocha Courts',
            description: 'Padel club',
            address: 'Ruta 9',
            city: 'Rocha',
            status: 'Active',
            sports: ['Padel', 'Football'],
            createdAt: '2026-10-01T00:00:00Z'
          }
        ],
        page: 1,
        pageSize: 12,
        totalItems: 1,
        totalPages: 1
      },
      isLoading: false,
      isError: false,
      refetch: vi.fn()
    } as unknown as ReturnType<typeof useFavoriteComplexes>);

    renderWithAuth(<UserFavoritesPage />, { authState: { isAuthenticated: true }, initialRoute: '/user/favorites' });

    expect(screen.getByRole('heading', { name: 'My favorites', level: 1 })).toBeTruthy();
    expect(screen.getByRole('heading', { name: 'Rocha Courts' })).toBeTruthy();
    expect(screen.getByText('Rocha · Ruta 9')).toBeTruthy();
    expect(screen.getByText('Padel')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Remove from favorites' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'View courts and hours' }).getAttribute('href')).toBe('/complexes/complex-1');
    expect(document.title).toBe('Mova | My favorites');
  });

  it('shows the empty state with an explore action', () => {
    mockUseFavoriteComplexes.mockReturnValue({
      data: emptyResult,
      isLoading: false,
      isError: false,
      refetch: vi.fn()
    } as unknown as ReturnType<typeof useFavoriteComplexes>);

    renderWithAuth(<UserFavoritesPage />, { authState: { isAuthenticated: true }, initialRoute: '/user/favorites' });

    expect(screen.getByText("You don't have any favorite complexes yet.")).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Explore complexes' }).getAttribute('href')).toBe('/complexes');
  });

  it('shows an error state with retry', () => {
    const refetch = vi.fn();
    mockUseFavoriteComplexes.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      refetch
    } as unknown as ReturnType<typeof useFavoriteComplexes>);

    renderWithAuth(<UserFavoritesPage />, { authState: { isAuthenticated: true }, initialRoute: '/user/favorites' });

    expect(screen.getByText("We couldn't load your favorites. Please try again.")).toBeTruthy();
    screen.getByRole('button', { name: 'Try again' }).click();
    expect(refetch).toHaveBeenCalled();
  });

  it('marks inactive favorites as unavailable and disables booking navigation', () => {
    mockUseFavoriteComplexes.mockReturnValue({
      data: {
        items: [
          {
            id: 'complex-9',
            name: 'Old Club',
            description: '',
            address: 'Street 1',
            city: 'Maldonado',
            status: 'Inactive',
            sports: [],
            createdAt: '2026-10-01T00:00:00Z'
          }
        ],
        page: 1,
        pageSize: 12,
        totalItems: 1,
        totalPages: 1
      },
      isLoading: false,
      isError: false,
      refetch: vi.fn()
    } as unknown as ReturnType<typeof useFavoriteComplexes>);

    renderWithAuth(<UserFavoritesPage />, { authState: { isAuthenticated: true }, initialRoute: '/user/favorites' });

    expect(screen.getByText('Unavailable')).toBeTruthy();
    const cta = screen.getByRole('link', { name: 'View courts and hours' });
    expect(cta.getAttribute('aria-disabled')).toBe('true');
  });
});
