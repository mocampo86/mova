import type { SelectChangeEvent } from '@mui/material/Select';
import { Box, FormControl, InputLabel, ListItemIcon, ListItemText, MenuItem, Select, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import FlagIcon from '../assets/flags/FlagIcon';

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

  const currentCode = i18n.resolvedLanguage ?? i18n.language;
  const selectedLanguage = LANGUAGES.find((lang) => lang.code === currentCode) ?? LANGUAGES[0];

  const renderSelectedValue = (value: string) => {
    const language = LANGUAGES.find((lang) => lang.code === value);
    if (!language) return value;
    return (
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <Box component="span" sx={{ mr: 1 }}>
          <FlagIcon code={language.code} />
        </Box>
        <Typography>{language.label}</Typography>
      </Box>
    );
  };

  return (
    <FormControl size="small" sx={{ minWidth: { xs: 120, sm: 150 } }}>
      <InputLabel id="language-selector-label">{t('language.label')}</InputLabel>
      <Select
        labelId="language-selector-label"
        value={selectedLanguage.code}
        label={t('language.label')}
        onChange={handleChange}
        renderValue={renderSelectedValue}
      >
        {LANGUAGES.map((language) => (
          <MenuItem key={language.code} value={language.code}>
            <ListItemIcon sx={{ minWidth: 'auto', mr: 1 }}>
              <FlagIcon code={language.code} />
            </ListItemIcon>
            <ListItemText primary={language.label} />
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  );
}
