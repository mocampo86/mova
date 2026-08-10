import { beforeEach, describe, expect, it } from 'vitest';
import {
  clearPersistedLanguage,
  FALLBACK_LANGUAGE,
  getPersistedLanguage,
  isSupportedLanguage,
  normalizeLanguageCode,
  persistLanguage,
  sanitizeLanguageCode,
  SUPPORTED_LANGUAGES,
  STORAGE_KEY
} from './languageStorage';

describe('languageStorage', () => {
  beforeEach(() => {
    clearPersistedLanguage();
  });

  it('defines supported languages and a fallback', () => {
    expect(SUPPORTED_LANGUAGES).toEqual(['en', 'es', 'pt']);
    expect(FALLBACK_LANGUAGE).toBe('en');
  });

  it('validates supported language codes', () => {
    expect(isSupportedLanguage('en')).toBe(true);
    expect(isSupportedLanguage('es')).toBe(true);
    expect(isSupportedLanguage('pt')).toBe(true);
    expect(isSupportedLanguage('fr')).toBe(false);
    expect(isSupportedLanguage('')).toBe(false);
    expect(isSupportedLanguage('en-US')).toBe(false);
  });

  it('normalizes language codes by stripping region and lowercasing', () => {
    expect(normalizeLanguageCode('es-ES')).toBe('es');
    expect(normalizeLanguageCode('EN-us')).toBe('en');
    expect(normalizeLanguageCode('pt-BR')).toBe('pt');
    expect(normalizeLanguageCode('  PT  ')).toBe('pt');
    expect(normalizeLanguageCode('')).toBe('');
  });

  it('sanitizes language codes to supported locales', () => {
    expect(sanitizeLanguageCode('es-ES')).toBe('es');
    expect(sanitizeLanguageCode('EN-us')).toBe('en');
    expect(sanitizeLanguageCode('pt-BR')).toBe('pt');
    expect(sanitizeLanguageCode('fr-FR')).toBe(FALLBACK_LANGUAGE);
    expect(sanitizeLanguageCode('invalid')).toBe(FALLBACK_LANGUAGE);
    expect(sanitizeLanguageCode('')).toBe(FALLBACK_LANGUAGE);
    expect(sanitizeLanguageCode('  PT  ')).toBe('pt');
  });

  it('returns the persisted language from localStorage', () => {
    window.localStorage.setItem(STORAGE_KEY, 'es');
    expect(getPersistedLanguage()).toBe('es');
  });

  it('returns null when there is no persisted language', () => {
    expect(getPersistedLanguage()).toBeNull();
  });

  it('persists a sanitized language code', () => {
    persistLanguage('es');
    expect(window.localStorage.getItem(STORAGE_KEY)).toBe('es');
  });

  it('sanitizes invalid values before persisting', () => {
    persistLanguage('fr-FR');
    expect(window.localStorage.getItem(STORAGE_KEY)).toBe(FALLBACK_LANGUAGE);
  });

  it('clears the persisted language', () => {
    persistLanguage('pt');
    clearPersistedLanguage();
    expect(getPersistedLanguage()).toBeNull();
    expect(window.localStorage.getItem(STORAGE_KEY)).toBeNull();
  });
});
