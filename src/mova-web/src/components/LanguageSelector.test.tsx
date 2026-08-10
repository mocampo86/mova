import type { ReactNode } from 'react';
import { cleanup, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, beforeEach, afterEach, vi } from 'vitest';
import { I18nextProvider } from 'react-i18next';
import { BrowserRouter } from 'react-router-dom';
import i18n from '../i18n';
import HomePage from '../pages/HomePage';
import LanguageSelector from './LanguageSelector';

function renderWithI18n(ui: ReactNode) {
  return render(
    <I18nextProvider i18n={i18n}>
      <BrowserRouter>{ui}</BrowserRouter>
    </I18nextProvider>
  );
}

describe('LanguageSelector', () => {
  beforeEach(async () => {
    await i18n.changeLanguage('en');
  });

  afterEach(() => {
    cleanup();
  });

  it('renders a language selector on the public screen', () => {
    renderWithI18n(<LanguageSelector />);

    const selector = screen.getByRole('combobox', { name: 'Language' });
    expect(selector).toBeTruthy();
    expect(screen.getByText('English')).toBeTruthy();
  });

  it('lists supported languages', async () => {
    const user = userEvent.setup();
    renderWithI18n(<LanguageSelector />);

    await user.click(screen.getByRole('combobox', { name: 'Language' }));

    expect(screen.getByRole('option', { name: 'English' })).toBeTruthy();
    expect(screen.getByRole('option', { name: 'Español' })).toBeTruthy();
    expect(screen.getByRole('option', { name: 'Português' })).toBeTruthy();
  });

  it('calls i18n.changeLanguage when a new language is selected', async () => {
    const user = userEvent.setup();
    const changeLanguageSpy = vi.spyOn(i18n, 'changeLanguage');

    renderWithI18n(<LanguageSelector />);

    await user.click(screen.getByRole('combobox', { name: 'Language' }));
    await user.click(screen.getByRole('option', { name: 'Español' }));

    await waitFor(() => {
      expect(changeLanguageSpy).toHaveBeenCalledWith('es');
    });

    changeLanguageSpy.mockRestore();
  });

  it('changes the application language across the rendered page', async () => {
    const user = userEvent.setup();

    renderWithI18n(
      <>
        <LanguageSelector />
        <HomePage />
      </>
    );

    expect(screen.getByText('Find your next game.')).toBeTruthy();

    await user.click(screen.getByRole('combobox', { name: 'Language' }));
    await user.click(screen.getByRole('option', { name: 'Español' }));

    await waitFor(() => {
      expect(screen.getByText('Encuentra tu próximo juego.')).toBeTruthy();
    });
  });
});
