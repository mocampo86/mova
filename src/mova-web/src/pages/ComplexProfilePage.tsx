import { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { zodResolver } from '@hookform/resolvers/zod';
import { Controller, useForm } from 'react-hook-form';
import { z } from 'zod';
import {
  Alert,
  Box,
  Button,
  Container,
  Grid,
  Skeleton,
  Stack,
  TextField,
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useAdminComplex, useUpdateComplex } from '../features/complexes/complexApi';

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
  name: z.string().min(1, 'Complex name is required.').max(255, 'Complex name must not exceed 255 characters.'),
  description: z
    .string()
    .min(1, 'Description is required.')
    .max(2000, 'Description must not exceed 2000 characters.'),
  address: z.string().min(1, 'Address is required.').max(255, 'Address must not exceed 255 characters.'),
  city: z.string().min(1, 'City is required.').max(255, 'City must not exceed 255 characters.'),
  latitude: nullableCoordinate(-90, 90, 'Latitude must be between -90 and 90.'),
  longitude: nullableCoordinate(-180, 180, 'Longitude must be between -180 and 180.'),
  phoneNumber: z
    .string()
    .min(8, 'Phone number must be at least 8 characters.')
    .max(50, 'Phone number must not exceed 50 characters.')
    .regex(phoneNumberPattern, "Phone number must be in international format starting with '+' followed by digits."),
  email: z
    .string()
    .min(1, 'Email is required.')
    .email('Email is not valid.')
    .max(255, 'Email must not exceed 255 characters.')
});

type FormValues = z.infer<typeof schema>;

function formatUpdatedAt(t: (key: string, options?: Record<string, unknown>) => string, isoString?: string | null) {
  if (!isoString) return t('common.notUpdatedYet');
  const date = new Date(isoString);
  return t('common.lastUpdated', { date: `${date.toLocaleDateString()} ${date.toLocaleTimeString()}` });
}

export default function ComplexProfilePage() {
  const { t } = useTranslation();
  const { complexId = '' } = useParams();
  const { data, isLoading, isError } = useAdminComplex(complexId);
  const { mutate, isPending, error, isSuccess } = useUpdateComplex(complexId);

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors }
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: '',
      description: '',
      address: '',
      city: '',
      latitude: null,
      longitude: null,
      phoneNumber: '',
      email: ''
    }
  });

  useEffect(() => {
    if (data) {
      reset({
        name: data.name,
        description: data.description,
        address: data.address,
        city: data.city,
        latitude: data.latitude ?? null,
        longitude: data.longitude ?? null,
        phoneNumber: data.phoneNumber,
        email: data.email
      });
    }
  }, [data, reset]);

  const onSubmit = (values: FormValues) => {
    mutate(values);
  };

  if (isLoading) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Skeleton variant="text" width="40%" height={48} />
        <Skeleton variant="text" width="30%" />
        <Stack spacing={2} sx={{ mt: 3 }}>
          <Skeleton variant="rectangular" height={56} />
          <Skeleton variant="rectangular" height={120} />
          <Skeleton variant="rectangular" height={56} />
          <Skeleton variant="rectangular" height={56} />
          <Skeleton variant="rectangular" height={56} />
        </Stack>
      </Container>
    );
  }

  if (isError || !data) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error">{t('admin.profile.loadError')}</Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        {t('admin.profile.title')}
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        {formatUpdatedAt(t, data.updatedAt)}
      </Typography>

      {isSuccess && (
        <Alert severity="success" sx={{ mb: 3 }}>
          {t('admin.profile.success')}
        </Alert>
      )}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error.message}
        </Alert>
      )}

      <Box
        component="form"
        onSubmit={handleSubmit(onSubmit)}
        sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}
      >
        <TextField
          {...register('name')}
          label={t('admin.profile.name')}
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

        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Controller
              name="latitude"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label={t('admin.profile.latitude')}
                  type="number"
                  fullWidth
                  value={field.value ?? ''}
                  onChange={(event) =>
                    field.onChange(event.target.value === '' ? null : Number(event.target.value))
                  }
                  error={Boolean(errors.latitude)}
                  helperText={errors.latitude?.message}
                />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Controller
              name="longitude"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label={t('admin.profile.longitude')}
                  type="number"
                  fullWidth
                  value={field.value ?? ''}
                  onChange={(event) =>
                    field.onChange(event.target.value === '' ? null : Number(event.target.value))
                  }
                  error={Boolean(errors.longitude)}
                  helperText={errors.longitude?.message}
                />
              )}
            />
          </Grid>
        </Grid>

        <TextField
          {...register('phoneNumber')}
          label={t('admin.profile.phone')}
          placeholder={t('completeProfile.phonePlaceholder')}
          fullWidth
          error={Boolean(errors.phoneNumber)}
          helperText={errors.phoneNumber?.message}
        />

        <TextField
          {...register('email')}
          label={t('common.email')}
          type="email"
          fullWidth
          error={Boolean(errors.email)}
          helperText={errors.email?.message}
        />

        <TextField
          label={t('common.status')}
          value={data.status ?? ''}
          fullWidth
          InputProps={{ readOnly: true }}
        />

        <Button type="submit" variant="contained" disabled={isPending} sx={{ alignSelf: 'flex-start' }}>
          {t('admin.profile.save')}
        </Button>
      </Box>
    </Container>
  );
}
