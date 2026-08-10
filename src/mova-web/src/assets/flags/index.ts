export const LANGUAGE_FLAGS: Record<string, string> = {
  en: '🇬🇧',
  es: '🇪🇸',
  pt: '🇵🇹'
};

export function getLanguageFlag(languageCode: string): string {
  return LANGUAGE_FLAGS[languageCode] ?? '';
}
