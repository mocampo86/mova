import type { ReactNode } from 'react';
import { cleanup, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, beforeEach, afterEach, vi } from 'vitest';
import { I18nextProvider } from 'react-i18next';
import { BrowserRouter } from 'react-router-dom';
import i18n from '../i18n';
import { STORAGE_KEY } from '../i18n/languageStorage';
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
    const { container } = renderWithI18n(<LanguageSelector />);

    const selector = screen.getByRole('combobox', { name: 'Language' });
    expect(selector).toBeTruthy();
    expect(screen.getByText('English')).toBeTruthy();
    expect(container.querySelector('img')).toBeTruthy();
  });

  it('lists supported languages with a flag icon', async () => {
    const user = userEvent.setup();
    renderWithI18n(<LanguageSelector />);

    await user.click(screen.getByRole('combobox', { name: 'Language' }));

    const options = screen.getAllByRole('option');
    expect(options).toHaveLength(3);
    expect(options[0].querySelector('img')).toBeTruthy();
    expect(options[0].textContent).toContain('English');
    expect(options[1].querySelector('img')).toBeTruthy();
    expect(options[1].textContent).toContain('Español');
    expect(options[2].querySelector('img')).toBeTruthy();
    expect(options[2].textContent).toContain('Português');
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

  it('highlights the selected language in the dropdown', async () => {
    const user = userEvent.setup();
    renderWithI18n(<LanguageSelector />);

    await user.click(screen.getByRole('combobox', { name: 'Language' }));

    const selectedOption = screen.getByRole('option', { name: 'English' });
    expect(selectedOption.getAttribute('aria-selected')).toBe('true');
  });

  it('persists the selected language to localStorage', async () => {
    const user = userEvent.setup();
    window.localStorage.removeItem(STORAGE_KEY);

    renderWithI18n(<LanguageSelector />);

    await user.click(screen.getByRole('combobox', { name: 'Language' }));
    await user.click(screen.getByRole('option', { name: 'Español' }));

    await waitFor(() => {
      expect(window.localStorage.getItem(STORAGE_KEY)).toBe('es');
    });
  });
});
