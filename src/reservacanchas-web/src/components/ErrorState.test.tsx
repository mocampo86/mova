import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import ErrorState from './ErrorState';

describe('ErrorState', () => {
  it('renders a generic error message', () => {
    render(<ErrorState />);

    expect(screen.getByText('Something went wrong')).toBeDefined();
    expect(screen.getByText('An unexpected error occurred. Please try again later.')).toBeDefined();
  });

  it('renders custom title and message', () => {
    render(<ErrorState title="API Error" message="Could not load courts." />);

    expect(screen.getByText('API Error')).toBeDefined();
    expect(screen.getByText('Could not load courts.')).toBeDefined();
  });
});
