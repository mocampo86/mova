import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import FlagIcon from './FlagIcon';

describe('FlagIcon', () => {
  it('renders an image when a flag asset exists', () => {
    const { container } = render(<FlagIcon code="es" />);
    expect(container.querySelector('img')).toBeTruthy();
  });

  it('falls back to the uppercase language code when no flag asset exists', () => {
    render(<FlagIcon code="fr" />);
    expect(screen.getByText('FR')).toBeTruthy();
  });
});
