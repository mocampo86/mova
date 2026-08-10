import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import FlagIcon from './FlagIcon';

describe('FlagIcon', () => {
  it('renders a flag emoji for a supported language', () => {
    render(<FlagIcon code="es" />);
    expect(screen.getByText('🇪🇸')).toBeTruthy();
  });

  it('falls back to the uppercase language code when no flag asset exists', () => {
    render(<FlagIcon code="fr" />);
    expect(screen.getByText('FR')).toBeTruthy();
  });
});
