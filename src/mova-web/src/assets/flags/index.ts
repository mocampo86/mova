import en from './en.svg?url';
import es from './es.svg?url';
import pt from './pt.svg?url';

export const LANGUAGE_FLAGS: Record<string, string> = {
  en,
  es,
  pt
};

export function getLanguageFlag(languageCode: string): string {
  return LANGUAGE_FLAGS[languageCode] ?? '';
}
