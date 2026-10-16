import { screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { Route, Routes } from 'react-router-dom';
import PlayerComplexLayout from './PlayerComplexLayout';
import { renderWithAuth } from '../test-utils';

vi.mock('./UserLayout', () => ({ default: () => <div>Player layout</div> }));
vi.mock('./PublicLayout', () => ({ default: () => <div>Public layout</div> }));

describe('PlayerComplexLayout', () => {
  it('uses the player layout for an authenticated visitor on /complexes', () => {
    renderWithAuth(
      <Routes><Route path="/complexes" element={<PlayerComplexLayout />} /></Routes>,
      { initialRoute: '/complexes', authState: { isAuthenticated: true } }
    );

    expect(screen.getByText('Player layout')).toBeTruthy();
  });

  it('preserves the public layout for anonymous visitors', () => {
    renderWithAuth(
      <Routes><Route path="/complexes" element={<PlayerComplexLayout />} /></Routes>,
      { initialRoute: '/complexes' }
    );

    expect(screen.getByText('Public layout')).toBeTruthy();
  });
});
