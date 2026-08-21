import { useEffect, useState } from 'react';
import {
  Autocomplete,
  Box,
  Button,
  CircularProgress,
  TextField,
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useSearchUsers } from './userAdminApi';
import type { ComplexUser } from './userAdminTypes';
import UserSearchDialog from './UserSearchDialog';

function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedValue(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);

  return debouncedValue;
}

export interface UserAutocompleteProps {
  complexId: string;
  value: ComplexUser | null;
  onChange: (user: ComplexUser | null) => void;
  label?: string;
  disabled?: boolean;
  error?: boolean;
  helperText?: string;
  required?: boolean;
}

export default function UserAutocomplete({
  complexId,
  value,
  onChange,
  label,
  disabled = false,
  error = false,
  helperText,
  required = false
}: UserAutocompleteProps) {
  const { t } = useTranslation();
  const [inputValue, setInputValue] = useState('');
  const [search, setSearch] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);

  const debouncedSearch = useDebounce(search, 300);
  const minSearchLength = 2;

  const filters = {
    page: 0,
    pageSize: 10,
    search: debouncedSearch,
    sort: 'fullName:asc'
  };

  const query = useSearchUsers(
    complexId,
    filters,
    Boolean(complexId) && debouncedSearch.length >= minSearchLength
  );

  useEffect(() => {
    if (value) {
      setInputValue(`${value.fullName} (${value.email})`);
      setSearch('');
    } else {
      setInputValue('');
    }
  }, [value]);

  const handleInputChange = (_event: unknown, newInputValue: string) => {
    setInputValue(newInputValue);
    setSearch(newInputValue);
  };

  const handleChange = (_event: unknown, newValue: ComplexUser | null) => {
    onChange(newValue);
    if (newValue) {
      setInputValue(`${newValue.fullName} (${newValue.email})`);
      setSearch('');
    }
  };

  const handleSelectFromDialog = (user: ComplexUser | null) => {
    onChange(user);
    setDialogOpen(false);
    if (user) {
      setInputValue(`${user.fullName} (${user.email})`);
      setSearch('');
    }
  };

  return (
    <Box>
      <Autocomplete
        freeSolo={false}
        disabled={disabled}
        options={query.data?.items ?? []}
        getOptionLabel={(option) => `${option.fullName} (${option.email})`}
        renderOption={(props, option) => (
          <Box component="li" {...props}>
            <Box>
              <Typography variant="body1">{option.fullName}</Typography>
              <Typography variant="body2" color="text.secondary">
                {option.email}
                {option.phoneNumber ? ` · ${option.phoneNumber}` : ''}
              </Typography>
            </Box>
          </Box>
        )}
        value={value}
        onChange={handleChange}
        inputValue={inputValue}
        onInputChange={handleInputChange}
        loading={query.isLoading}
        noOptionsText={
          debouncedSearch.length >= minSearchLength
            ? t('admin.users.empty')
            : t('admin.users.searchBy')
        }
        renderInput={(params) => (
          <TextField
            {...params}
            required={required}
            label={label ?? t('admin.reservations.userIdLabel')}
            error={error}
            helperText={helperText}
            InputProps={{
              ...params.InputProps,
              endAdornment: (
                <>
                  {query.isLoading ? (
                    <CircularProgress color="inherit" size={20} />
                  ) : null}
                  {params.InputProps.endAdornment}
                </>
              )
            }}
          />
        )}
      />
      <Button
        variant="outlined"
        size="small"
        onClick={() => setDialogOpen(true)}
        disabled={disabled}
        sx={{ mt: 1 }}
      >
        {t('common.search')}
      </Button>
      <UserSearchDialog
        complexId={complexId}
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        onSelect={handleSelectFromDialog}
      />
    </Box>
  );
}
