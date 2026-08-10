import {
  FALLBACK_LANGUAGE,
  normalizeLanguageCode,
  SUPPORTED_LANGUAGES,
  STORAGE_KEY
} from './languageStorage';
import en from './locales/en.json';
import es from './locales/es.json';
import pt from './locales/pt.json';

export const resources = {
  en: { translation: en },
  es: { translation: es },
  pt: { translation: pt }
};

export const i18nConfig = {
  resources,
  fallbackLng: FALLBACK_LANGUAGE,
  supportedLngs: [...SUPPORTED_LANGUAGES],
  detection: {
    order: ['localStorage', 'navigator', 'querystring', 'htmlTag'],
    caches: ['localStorage'],
    lookupLocalStorage: STORAGE_KEY,
    lookupQuerystring: 'lng',
    convertDetectedLanguage: normalizeLanguageCode
  },
  interpolation: {
    escapeValue: false
  },
  react: {
    useSuspense: false
  }
};
