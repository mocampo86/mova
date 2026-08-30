import { screen, waitFor, within } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { useActiveComplexes } from '../features/complexes/complexApi';
import HomePage from './HomePage';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/complexes/complexApi');

const mockUseActiveComplexes = vi.mocked(useActiveComplexes);

describe('HomePage', () => {
  it('presents the platform value proposition, audiences, and visitor calls to action', async () => {
    mockUseActiveComplexes.mockReturnValue({ data: { items: [], page: 1, pageSize: 12, totalItems: 0, totalPages: 0 }, isLoading: false, isError: false } as unknown as ReturnType<typeof useActiveComplexes>);

    renderWithAuth(<HomePage />);

    expect(screen.getByRole('heading', { name: 'Find your next game.' })).toBeTruthy();
    expect(screen.getByText(/Discover nearby sports complexes/)).toBeTruthy();
    expect(screen.getByRole('heading', { name: 'How to find and book a court' })).toBeTruthy();
    expect(screen.getByRole('heading', { name: 'Search' })).toBeTruthy();
    expect(screen.getByRole('heading', { name: 'Select' })).toBeTruthy();
    expect(screen.getByRole('heading', { name: 'Book' })).toBeTruthy();
    expect(screen.getAllByRole('link', { name: 'Find a court' }).length).toBe(2);
    expect(screen.getAllByRole('link', { name: 'Find a court' })[0].getAttribute('href')).toBe('/complexes');
    expect(screen.getByRole('heading', { name: 'For players' })).toBeTruthy();
    expect(screen.getByRole('heading', { name: 'For complex owners' })).toBeTruthy();
    expect(document.title).toBe('Mova | Find your next game');
    expect(document.querySelector('meta[name="description"]')?.getAttribute('content')).toBe('Discover nearby sports complexes, check court availability, and reserve the time that works for you.');

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Featured complexes' })).toBeTruthy();
    });
  });

  it('renders a modernized hero with three visible, distinguishable calls to action', () => {
    mockUseActiveComplexes.mockReturnValue({ data: { items: [], page: 1, pageSize: 12, totalItems: 0, totalPages: 0 }, isLoading: false, isError: false } as unknown as ReturnType<typeof useActiveComplexes>);

    renderWithAuth(<HomePage />);

    const hero = screen.getByTestId('hero');
    expect(hero).toBeTruthy();
    expect(within(hero).getByRole('heading', { name: 'Find your next game.' })).toBeTruthy();
    expect(within(hero).getByText(/Discover nearby sports complexes/)).toBeTruthy();
    expect(within(hero).getByRole('link', { name: 'Play / Book a court' }).getAttribute('href')).toBe('/login?intent=user');
    expect(within(hero).getByRole('link', { name: 'Manage your complex' }).getAttribute('href')).toBe('/login?intent=complex');
    expect(within(hero).getByRole('link', { name: 'Browse complexes' }).getAttribute('href')).toBe('/complexes');
    expect(within(hero).getByTestId('hero-visual')).toBeTruthy();
  });

  it('exposes login and registration entry points for visitors', () => {
    mockUseActiveComplexes.mockReturnValue({ data: { items: [], page: 1, pageSize: 12, totalItems: 0, totalPages: 0 }, isLoading: false, isError: false } as unknown as ReturnType<typeof useActiveComplexes>);

    renderWithAuth(<HomePage />);

    expect(screen.getByRole('link', { name: 'Play / Book a court' }).getAttribute('href')).toBe('/login?intent=user');
    expect(screen.getAllByRole('link', { name: 'Manage your complex' }).length).toBe(2);
    expect(screen.getAllByRole('link', { name: 'Manage your complex' })[0].getAttribute('href')).toBe('/login?intent=complex');
    expect(screen.getByRole('link', { name: 'Sign in to play' }).getAttribute('href')).toBe('/login?intent=user');
    expect(screen.getByRole('link', { name: 'Register your complex' }).getAttribute('href')).toBe('/login?intent=complex');
  });

  it('displays active public complexes as featured links', async () => {
    mockUseActiveComplexes.mockReturnValue({
      data: {
        items: [{ id: 'complex-1', name: 'Central Padel', description: 'Padel for everyone', address: '123 Main St', city: 'Montevideo', phoneNumber: '', email: '', allowUserRecurringReservations: true, timeZoneId: 'America/Montevideo' }],
        page: 1,
        pageSize: 3,
        totalItems: 1,
        totalPages: 1
      },
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useActiveComplexes>);

    renderWithAuth(<HomePage />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Central Padel' })).toBeTruthy();
      expect(screen.getByRole('link', { name: 'View complex' }).getAttribute('href')).toBe('/complexes/complex-1');
    });
  });
});
