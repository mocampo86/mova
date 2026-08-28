import { cleanup, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { I18nextProvider } from 'react-i18next';
import i18n from '../i18n';
import ApiErrorMessage from './ApiErrorMessage';
import { ApiError } from '../shared/utils/apiError';

function renderWithI18n(error: unknown) {
  return render(
    <I18nextProvider i18n={i18n}>
      <ApiErrorMessage error={error} />
    </I18nextProvider>
  );
}

describe('ApiErrorMessage', () => {
  beforeEach(async () => {
    await i18n.changeLanguage('en');
  });

  afterEach(() => {
    cleanup();
  });

  it('renders nothing when there is no error', () => {
    const { container } = renderWithI18n(null);
    expect(container.textContent).toBe('');
  });

  it('renders a translated message for a known ApiError code', () => {
    renderWithI18n(new ApiError(404, 'Court not found', 'NOT_FOUND'));
    expect(screen.getByText('The requested resource was not found.')).toBeTruthy();
  });

  it('renders the ApiError message when the code is not mapped', () => {
    renderWithI18n(new ApiError(500, 'Internal failure', 'UNKNOWN'));
    expect(screen.getByText('Internal failure')).toBeTruthy();
  });

  it('renders a generic Error message when the error is not an ApiError', () => {
    renderWithI18n(new Error('Network failure'));
    expect(screen.getByText('Network failure')).toBeTruthy();
  });
});
