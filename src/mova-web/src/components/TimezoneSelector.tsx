import { Autocomplete, TextField } from '@mui/material';
import { useMemo } from 'react';
import { getTimeZoneOptions } from '../utils/timezones';

export interface TimezoneSelectorProps {
  value?: string | null;
  onChange: (timeZoneId: string) => void;
  label?: string;
  helperText?: string;
  error?: boolean;
  required?: boolean;
  disabled?: boolean;
  fullWidth?: boolean;
}

export default function TimezoneSelector({
  value,
  onChange,
  label,
  helperText,
  error = false,
  required = false,
  disabled = false,
  fullWidth = false
}: TimezoneSelectorProps) {
  const options = useMemo(() => getTimeZoneOptions(), []);
  const selected = useMemo(
    () => options.find((option) => option.id === value) ?? null,
    [options, value]
  );

  return (
    <Autocomplete
      options={options}
      value={selected}
      onChange={(_event, newValue) => onChange(newValue?.id ?? '')}
      getOptionLabel={(option) => option?.label ?? ''}
      isOptionEqualToValue={(option, current) => option?.id === current?.id}
      disabled={disabled}
      fullWidth={fullWidth}
      noOptionsText="No time zones found"
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          helperText={helperText}
          error={error}
          required={required}
          fullWidth={fullWidth}
        />
      )}
    />
  );
}
