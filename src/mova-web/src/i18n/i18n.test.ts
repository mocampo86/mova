import { createInstance } from 'i18next';
import { initReactI18next } from 'react-i18next';
import { describe, expect, it } from 'vitest';
import en from './locales/en.json';
import es from './locales/es.json';
import pt from './locales/pt.json';

async function createTestI18n(language = 'en') {
  const instance = createInstance();
  await instance
    .use(initReactI18next)
    .init({
      resources: {
        en: { translation: en },
        es: { translation: es },
        pt: { translation: pt }
      },
      lng: language,
      fallbackLng: 'en',
      supportedLngs: ['en', 'es', 'pt'],
      interpolation: { escapeValue: false }
    });
  return instance;
}

describe('i18n configuration', () => {
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
