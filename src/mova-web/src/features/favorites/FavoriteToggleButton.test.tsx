import { cleanup, fireEvent, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import FavoriteToggleButton from './FavoriteToggleButton';
import {
  useAddFavoriteComplex,
  useFavoriteComplexIds,
  useRemoveFavoriteComplex
} from './favoritesApi';
import { renderWithAuth } from '../../test-utils';

vi.mock('./favoritesApi', () => ({
  useFavoriteComplexIds: vi.fn(),
  useAddFavoriteComplex: vi.fn(),
  useRemoveFavoriteComplex: vi.fn()
}));

const mockIds = vi.mocked(useFavoriteComplexIds);
const mockAdd = vi.mocked(useAddFavoriteComplex);
const mockRemove = vi.mocked(useRemoveFavoriteComplex);

function mockMutations() {
  const addMutate = vi.fn();
  const removeMutate = vi.fn();
  mockAdd.mockReturnValue({ mutate: addMutate, isPending: false } as unknown as ReturnType<typeof useAddFavoriteComplex>);
  mockRemove.mockReturnValue({ mutate: removeMutate, isPending: false } as unknown as ReturnType<typeof useRemoveFavoriteComplex>);
  return { addMutate, removeMutate };
}

beforeEach(() => {
  cleanup();
  vi.resetAllMocks();
});

describe('FavoriteToggleButton', () => {
  it('shows the add label when the complex is not a favorite', () => {
    mockIds.mockReturnValue({ data: { items: [] } } as unknown as ReturnType<typeof useFavoriteComplexIds>);
    mockMutations();

    renderWithAuth(<FavoriteToggleButton complexId="c1" />, { authState: { isAuthenticated: true, accessToken: 'token' } });

    const button = screen.getByRole('button', { name: 'Add to favorites' });
    expect(button.getAttribute('aria-pressed')).toBe('false');
  });

  it('shows the remove label when the complex is a favorite and calls remove on click', () => {
    mockIds.mockReturnValue({ data: { items: ['c1'] } } as unknown as ReturnType<typeof useFavoriteComplexIds>);
    const { removeMutate } = mockMutations();

    renderWithAuth(<FavoriteToggleButton complexId="c1" />, { authState: { isAuthenticated: true, accessToken: 'token' } });

    const button = screen.getByRole('button', { name: 'Remove from favorites' });
    expect(button.getAttribute('aria-pressed')).toBe('true');
    fireEvent.click(button);
    expect(removeMutate).toHaveBeenCalledWith('c1', expect.anything());
  });

  it('calls add on click when not favorite', () => {
    mockIds.mockReturnValue({ data: { items: [] } } as unknown as ReturnType<typeof useFavoriteComplexIds>);
    const { addMutate } = mockMutations();

    renderWithAuth(<FavoriteToggleButton complexId="c1" />, { authState: { isAuthenticated: true, accessToken: 'token' } });

    fireEvent.click(screen.getByRole('button', { name: 'Add to favorites' }));
    expect(addMutate).toHaveBeenCalledWith('c1', expect.anything());
  });

  it('redirects anonymous visitors to login preserving the current path', () => {
    mockIds.mockReturnValue({ data: undefined } as unknown as ReturnType<typeof useFavoriteComplexIds>);
    mockMutations();

    renderWithAuth(<FavoriteToggleButton complexId="c1" />, { initialRoute: '/complexes?x=1' });

    fireEvent.click(screen.getByRole('button', { name: 'Add to favorites' }));
    expect(window.location.pathname).toBe('/login');
    expect(window.location.search).toContain('intent=user');
    expect(decodeURIComponent(window.location.search)).toContain('returnTo=/complexes?x=1');
  });
});
