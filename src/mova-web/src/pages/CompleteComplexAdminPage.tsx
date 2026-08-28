import { Box, Button, Stack, TextField, Typography } from '@mui/material';
import { zodResolver } from '@hookform/resolvers/zod';
import { Controller, useForm } from 'react-hook-form';
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import ApiErrorMessage from '../components/ApiErrorMessage';
import { useCompleteComplexAdminProfile } from '../features/users/useCompleteComplexAdminProfile';
import TimezoneSelector from '../components/TimezoneSelector';
import { DEFAULT_TIME_ZONE_ID } from '../utils/timezones';

const phoneNumberPattern = /^\+[0-9](?:\s*[0-9]){6,14}$/;

const nullableCoordinate = <T extends number>(min: T, max: T, message: string) =>
  z.preprocess(
    (value) => {
      if (value === '' || value === null || value === undefined) {
        return null;
      }

      const parsed = Number(value);
      return Number.isNaN(parsed) ? null : parsed;
    },
    z.number({ invalid_type_error: 'Value must be a valid number.' }).min(min, message).max(max, message).nullable().optional()
  );

const schema = z.object({
  phoneNumber: z
    .string()
    .min(8, 'Phone number must be at least 8 characters.')
    .max(50, 'Phone number must not exceed 50 characters.')
    .regex(
      phoneNumberPattern,
      "Phone number must be in international format starting with '+' followed by digits."
    ),
  name: z
    .string()
    .min(1, 'Complex name is required.')
    .max(255, 'Complex name must not exceed 255 characters.'),
  description: z
    .string()
    .min(1, 'Description is required.')
    .max(2000, 'Description must not exceed 2000 characters.'),
  address: z
    .string()
    .min(1, 'Address is required.')
    .max(255, 'Address must not exceed 255 characters.'),
  city: z
    .string()
    .min(1, 'City is required.')
    .max(255, 'City must not exceed 255 characters.'),
  latitude: nullableCoordinate(
    -90,
    90,
    'Latitude must be between -90 and 90.'
  ),
  longitude: nullableCoordinate(
    -180,
    180,
    'Longitude must be between -180 and 180.'
  ),
  complexPhoneNumber: z
    .string()
    .min(8, 'Phone number must be at least 8 characters.')
    .max(50, 'Phone number must not exceed 50 characters.')
    .regex(
      phoneNumberPattern,
      "Phone number must be in international format starting with '+' followed by digits."
    ),
  complexEmail: z
    .string()
    .min(1, 'Email is required.')
    .email('Email is not valid.')
    .max(255, 'Email must not exceed 255 characters.'),
  timeZoneId: z.string().min(1, 'Time zone is required.')
});

type FormValues = z.infer<typeof schema>;

export default function CompleteComplexAdminPage() {
  const { t } = useTranslation();
  const { mutate, isPending, error } = useCompleteComplexAdminProfile();
  const {
    register,
    handleSubmit,
    control,
    formState: { errors }
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      phoneNumber: '',
      name: '',
      description: '',
      address: '',
      city: '',
      latitude: null,
      longitude: null,
      complexPhoneNumber: '',
      complexEmail: '',
      timeZoneId: DEFAULT_TIME_ZONE_ID
    }
  });

  const onSubmit = (data: FormValues) => {
    mutate(data);
  };

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        gap: 3,
        mt: 4,
        mb: 8,
        px: 2
      }}
    >
      <Typography variant="h4">{t('completeComplexAdmin.title')}</Typography>
      <Typography variant="body1" textAlign="center" sx={{ maxWidth: 500 }}>
        {t('completeComplexAdmin.subtitle')}
      </Typography>

      <Box
        component="form"
        onSubmit={handleSubmit(onSubmit)}
        sx={{ display: 'flex', flexDirection: 'column', gap: 2, width: '100%', maxWidth: 600 }}
      >
        <Typography variant="h6">{t('completeComplexAdmin.contactInfo')}</Typography>
        <TextField
          {...register('phoneNumber')}
          label={t('completeComplexAdmin.yourPhone')}
          placeholder={t('completeProfile.phonePlaceholder')}
          fullWidth
          error={Boolean(errors.phoneNumber)}
          helperText={errors.phoneNumber?.message}
        />

        <Typography variant="h6" sx={{ mt: 2 }}>
          {t('completeComplexAdmin.complexInfo')}
        </Typography>
        <TextField
          {...register('name')}
          label={t('completeComplexAdmin.complexName')}
          fullWidth
          error={Boolean(errors.name)}
          helperText={errors.name?.message}
        />
        <TextField
          {...register('description')}
          label={t('common.description')}
          multiline
          rows={3}
          fullWidth
          error={Boolean(errors.description)}
          helperText={errors.description?.message}
        />
        <TextField
          {...register('address')}
          label={t('common.address')}
          fullWidth
          error={Boolean(errors.address)}
          helperText={errors.address?.message}
        />
        <TextField
          {...register('city')}
          label={t('common.city')}
          fullWidth
          error={Boolean(errors.city)}
          helperText={errors.city?.message}
        />
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            {...register('latitude')}
            label={t('completeComplexAdmin.latitude')}
            type="number"
            fullWidth
            error={Boolean(errors.latitude)}
            helperText={errors.latitude?.message}
          />
          <TextField
            {...register('longitude')}
            label={t('completeComplexAdmin.longitude')}
            type="number"
            fullWidth
            error={Boolean(errors.longitude)}
            helperText={errors.longitude?.message}
          />
        </Stack>
        <TextField
          {...register('complexPhoneNumber')}
          label={t('completeComplexAdmin.complexPhone')}
          placeholder={t('completeProfile.phonePlaceholder')}
          fullWidth
          error={Boolean(errors.complexPhoneNumber)}
          helperText={errors.complexPhoneNumber?.message}
        />
        <TextField
          {...register('complexEmail')}
          label={t('completeComplexAdmin.complexEmail')}
          type="email"
          fullWidth
          error={Boolean(errors.complexEmail)}
          helperText={errors.complexEmail?.message}
        />

        <Controller
          name="timeZoneId"
          control={control}
          defaultValue={DEFAULT_TIME_ZONE_ID}
          render={({ field }) => (
            <TimezoneSelector
              value={field.value}
              onChange={field.onChange}
              label={t('completeComplexAdmin.timeZone')}
              helperText={t('completeComplexAdmin.timeZoneHelper')}
              error={Boolean(errors.timeZoneId)}
              required
              fullWidth
            />
          )}
        />

        <Button type="submit" variant="contained" disabled={isPending}>
          {t('completeComplexAdmin.submit')}
        </Button>

        {error && (
          <Typography color="error" variant="body2">
            <ApiErrorMessage error={error} />
          </Typography>
        )}
      </Box>
    </Box>
  );
}
