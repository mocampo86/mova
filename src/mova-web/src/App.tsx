import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import AppProviders from './app/providers';
import AppRouter from './app/router';

function I18nDocumentLang() {
  const { i18n } = useTranslation();

  useEffect(() => {
    const updateLang = (lng: string) => {
      document.documentElement.lang = lng;
    };

    i18n.on('languageChanged', updateLang);
    if (i18n.resolvedLanguage) {
      updateLang(i18n.resolvedLanguage);
    }

    return () => {
      i18n.off('languageChanged', updateLang);
    };
  }, [i18n]);

  return null;
}

export default function App() {
  return (
    <AppProviders>
      <I18nDocumentLang />
      <AppRouter />
    </AppProviders>
  );
}
