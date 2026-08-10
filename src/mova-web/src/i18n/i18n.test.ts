import { createInstance } from 'i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import { initReactI18next } from 'react-i18next';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { i18nConfig, resources } from './config';
import { clearPersistedLanguage, STORAGE_KEY } from './languageStorage';

async function createTestI18n(language = 'en') {
  const instance = createInstance();
  await instance
    .use(initReactI18next)
    .init({
      ...i18nConfig,
      resources,
      lng: language
    });
  return instance;
}

async function createDetectingI18n() {
  const instance = createInstance();
  await instance
    .use(LanguageDetector)
    .use(initReactI18next)
    .init({
      ...i18nConfig,
      resources
    });
  return instance;
}

describe('i18n configuration', () => {
  beforeEach(() => {
    clearPersistedLanguage();
  });

  it('returns English text for known en keys', async () => {
    const instance = await createTestI18n('en');
    expect(instance.t('home.hero.title')).toBe('Find your next game.');
  });

  it('returns Spanish text when language is set to es', async () => {
    const instance = await createTestI18n('es');
    expect(instance.t('home.hero.title')).toBe('Encuentra tu próximo juego.');
  });

  it('returns Portuguese text when language is set to pt', async () => {
    const instance = await createTestI18n('pt');
    expect(instance.t('home.hero.title')).toBe('Encontre seu próximo jogo.');
  });

  it('falls back to English for unsupported language codes', async () => {
    const instance = await createTestI18n('fr');
    expect(instance.t('home.hero.title')).toBe('Find your next game.');
  });

  it('interpolates dynamic values', async () => {
    const instance = await createTestI18n('en');
    expect(instance.t('common.lastUpdated', { date: '01/01/2026' })).toBe('Last updated 01/01/2026');
  });
});

describe('language detection and persistence', () => {
  beforeEach(() => {
    clearPersistedLanguage();
  });

  afterEach(() => {
    document.documentElement.removeAttribute('lang');
    vi.unstubAllGlobals();
  });

  it('detects language from localStorage when available', async () => {
    window.localStorage.setItem(STORAGE_KEY, 'pt');
    const instance = await createDetectingI18n();
    expect(instance.resolvedLanguage).toBe('pt');
  });

  it('falls back to English and resets an invalid persisted value', async () => {
    window.localStorage.setItem(STORAGE_KEY, 'invalid');
    const instance = await createDetectingI18n();
    expect(instance.resolvedLanguage).toBe('en');
    expect(window.localStorage.getItem(STORAGE_KEY)).toBe('en');
  });

  it('detects browser language from navigator when no preference is stored', async () => {
    vi.stubGlobal('navigator', { language: 'es-ES' });
    const instance = await createDetectingI18n();
    expect(instance.resolvedLanguage).toBe('es');
  });

  it('falls back to English when browser language is not supported', async () => {
    vi.stubGlobal('navigator', { language: 'de-DE' });
    const instance = await createDetectingI18n();
    expect(instance.resolvedLanguage).toBe('en');
  });

  it('detects language from html tag when no higher-priority source is available', async () => {
    vi.stubGlobal('navigator', {});
    document.documentElement.setAttribute('lang', 'pt-BR');
    const instance = await createDetectingI18n();
    expect(instance.resolvedLanguage).toBe('pt');
  });

  it('persists language changes to localStorage immediately', async () => {
    const instance = await createDetectingI18n();
    await instance.changeLanguage('es');
    expect(window.localStorage.getItem(STORAGE_KEY)).toBe('es');
  });

  it('sanitizes regional browser language codes to supported locales', async () => {
    vi.stubGlobal('navigator', { language: 'EN-us' });
    const instance = await createDetectingI18n();
    expect(instance.resolvedLanguage).toBe('en');
  });
});
