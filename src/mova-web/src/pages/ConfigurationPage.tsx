import { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { zodResolver } from '@hookform/resolvers/zod';
import { Controller, useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { z } from 'zod';
import {
  Alert,
  Box,
  Button,
  Container,
  FormControlLabel,
  Paper,
  Stack,
  Switch,
  TextField,
  Typography
} from '@mui/material';
import {
  useCancellationPolicy,
  useUpdateCancellationPolicy
} from '../features/complexes/complexApi';

const schema = z.object({
  minimumHours: z.coerce.number().int().min(0),
  allowUserCancellation: z.boolean()
});

type FormValues = z.infer<typeof schema>;

const DEFAULT_VALUES: FormValues = {
  minimumHours: 24,
  allowUserCancellation: true
};

export default function ConfigurationPage() {
  const { t } = useTranslation();
  const { complexId = '' } = useParams();
  const { data, isLoading, isError, error } = useCancellationPolicy(complexId);
  const updatePolicy = useUpdateCancellationPolicy(complexId);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors }
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: DEFAULT_VALUES
  });

  useEffect(() => {
    if (data) {
      reset({
        minimumHours: data.minimumHours,
        allowUserCancellation: data.allowUserCancellation
      });
    }
  }, [data, reset]);

  const onSubmit = async (values: FormValues) => {
    try {
      await updatePolicy.mutateAsync(values);
    } catch {
      // Mutation hooks surface error states through their error property.
    }
  };

  if (!complexId) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error">{t('admin.configuration.missingId')}</Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        {t('admin.configuration.title')}
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        {t('admin.configuration.subtitle')}
      </Typography>

      {isLoading && <Alert severity="info">{t('admin.configuration.loading')}</Alert>}

      {isError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error?.message ?? t('admin.configuration.loadError')}
        </Alert>
      )}

      {updatePolicy.isSuccess && (
        <Alert severity="success" sx={{ mb: 3 }}>
          {t('admin.configuration.success')}
        </Alert>
      )}

      {updatePolicy.isError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {updatePolicy.error?.message ?? t('admin.configuration.saveError')}
        </Alert>
      )}

      <Paper variant="outlined" sx={{ p: 3 }}>
        <Box component="form" onSubmit={handleSubmit(onSubmit)}>
          <Stack spacing={3}>
            <Controller
              name="minimumHours"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  value={field.value}
                  onChange={(event) =>
                    field.onChange(
                      event.target.value === '' ? 0 : Number(event.target.value)
                    )
                  }
                  label={t('admin.configuration.minimumHours')}
                  type="number"
                  inputProps={{ min: 0, step: 1 }}
                  error={Boolean(errors.minimumHours)}
                  helperText={errors.minimumHours?.message ?? t('admin.configuration.minimumHoursHelper')}
                  fullWidth
                />
              )}
            />
            <FormControlLabel
              control={
                <Controller
                  name="allowUserCancellation"
                  control={control}
                  render={({ field }) => (
                    <Switch
                      checked={field.value}
                      onChange={(event) => field.onChange(event.target.checked)}
                    />
                  )}
                />
              }
              label={t('admin.configuration.allowUserCancellation')}
            />
            <Button
              type="submit"
              variant="contained"
              disabled={updatePolicy.isPending}
            >
              {t('admin.configuration.save')}
            </Button>
          </Stack>
        </Box>
      </Paper>
    </Container>
  );
}
