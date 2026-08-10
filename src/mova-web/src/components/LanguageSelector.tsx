import type { SelectChangeEvent } from '@mui/material/Select';
import { FormControl, InputLabel, MenuItem, Select } from '@mui/material';
import { useTranslation } from 'react-i18next';

const LANGUAGES = [
  { code: 'en', label: 'English' },
  { code: 'es', label: 'Español' },
  { code: 'pt', label: 'Português' }
];

export default function LanguageSelector() {
  const { i18n, t } = useTranslation();

  const handleChange = (event: SelectChangeEvent<string>) => {
    const language = event.target.value;
    if (LANGUAGES.some((lang) => lang.code === language)) {
      i18n.changeLanguage(language);
    }
  };

  return (
    <FormControl size="small" sx={{ minWidth: 130 }}>
      <InputLabel id="language-selector-label">{t('language.label')}</InputLabel>
      <Select
        labelId="language-selector-label"
        value={i18n.language}
        label={t('language.label')}
        onChange={handleChange}
      >
        {LANGUAGES.map((language) => (
          <MenuItem key={language.code} value={language.code}>
            {language.label}
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  );
}
