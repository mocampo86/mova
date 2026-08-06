import { screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import HomePage from './HomePage';
import { renderWithAuth } from '../test-utils';

describe('HomePage', () => {
  it('presents the platform value proposition and visitor calls to action', () => {
    renderWithAuth(<HomePage />);

    expect(screen.getByRole('heading', { name: 'Find your next game.' })).toBeTruthy();
    expect(screen.getByText(/Discover nearby sports complexes/)).toBeTruthy();
    expect(screen.getAllByRole('link', { name: /get started|sign in to mova/i })).toHaveLength(2);
    expect(screen.getByRole('heading', { name: 'Everything you need to play' })).toBeTruthy();
  });
});
