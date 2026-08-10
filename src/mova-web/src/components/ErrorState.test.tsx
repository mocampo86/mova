import type { ReactNode } from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { I18nextProvider } from 'react-i18next';
import ErrorState from './ErrorState';
import i18n from '../i18n';

function renderWithI18n(ui: ReactNode) {
  return render(<I18nextProvider i18n={i18n}>{ui}</I18nextProvider>);
}

describe('ErrorState', () => {
  it('renders a generic error message', () => {
    renderWithI18n(<ErrorState />);

    expect(screen.getByText('Something went wrong')).toBeDefined();
    expect(screen.getByText('An unexpected error occurred. Please try again later.')).toBeDefined();
  });

  it('renders custom title and message', () => {
    renderWithI18n(<ErrorState title="API Error" message="Could not load courts." />);

    expect(screen.getByText('API Error')).toBeDefined();
    expect(screen.getByText('Could not load courts.')).toBeDefined();
  });
});
