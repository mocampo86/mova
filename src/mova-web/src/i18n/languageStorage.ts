export const STORAGE_KEY = 'mova-language';
export const SUPPORTED_LANGUAGES = ['en', 'es', 'pt'] as const;
export const FALLBACK_LANGUAGE = 'en';

export type SupportedLanguage = (typeof SUPPORTED_LANGUAGES)[number];

export function isSupportedLanguage(code: string): code is SupportedLanguage {
  return SUPPORTED_LANGUAGES.includes(code as SupportedLanguage);
}

export function normalizeLanguageCode(code: string): string {
  return typeof code === 'string' ? code.trim().toLowerCase().split('-')[0] : '';
}

export function sanitizeLanguageCode(code: string): SupportedLanguage {
  const normalized = normalizeLanguageCode(code);
  return isSupportedLanguage(normalized) ? normalized : FALLBACK_LANGUAGE;
}

function getStorage(): Storage | null {
  if (typeof window === 'undefined' || !window.localStorage) return null;
  return window.localStorage;
}

export function getPersistedLanguage(): string | null {
  try {
    return getStorage()?.getItem(STORAGE_KEY) ?? null;
  } catch {
    return null;
  }
}

export function persistLanguage(code: string): void {
  const sanitized = sanitizeLanguageCode(code);
  try {
    getStorage()?.setItem(STORAGE_KEY, sanitized);
  } catch {
    // Ignore storage errors (e.g., private mode, quota exceeded)
  }
}

export function clearPersistedLanguage(): void {
  try {
    getStorage()?.removeItem(STORAGE_KEY);
  } catch {
    // Ignore storage errors
  }
}
