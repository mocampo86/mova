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
  useRecurringReservationSettings,
  useUpdateCancellationPolicy,
  useUpdateRecurringReservationSettings
} from '../features/complexes/complexApi';

const cancellationSchema = z.object({
  minimumHours: z.coerce.number().int().min(0),
  allowUserCancellation: z.boolean()
});

const recurringSchema = z.object({
  allowUserRecurringReservations: z.boolean()
});

type CancellationFormValues = z.infer<typeof cancellationSchema>;
type RecurringFormValues = z.infer<typeof recurringSchema>;

const DEFAULT_CANCELLATION_VALUES: CancellationFormValues = {
  minimumHours: 24,
  allowUserCancellation: true
};

const DEFAULT_RECURRING_VALUES: RecurringFormValues = {
  allowUserRecurringReservations: true
};

export default function ConfigurationPage() {
  const { t } = useTranslation();
  const { complexId = '' } = useParams();

  const {
    data: cancellationData,
    isLoading: isLoadingCancellation,
    isError: isCancellationError,
    error: cancellationError
  } = useCancellationPolicy(complexId);
  const updatePolicy = useUpdateCancellationPolicy(complexId);

  const {
    data: recurringData,
    isLoading: isLoadingRecurring,
    isError: isRecurringError,
    error: recurringError
  } = useRecurringReservationSettings(complexId);
  const updateRecurring = useUpdateRecurringReservationSettings(complexId);

  const cancellationForm = useForm<CancellationFormValues>({
    resolver: zodResolver(cancellationSchema),
    defaultValues: DEFAULT_CANCELLATION_VALUES
  });

  const recurringForm = useForm<RecurringFormValues>({
    resolver: zodResolver(recurringSchema),
    defaultValues: DEFAULT_RECURRING_VALUES
  });

  useEffect(() => {
    if (cancellationData) {
      cancellationForm.reset({
        minimumHours: cancellationData.minimumHours,
        allowUserCancellation: cancellationData.allowUserCancellation
      });
    }
  }, [cancellationData, cancellationForm]);

  useEffect(() => {
    if (recurringData) {
      recurringForm.reset({
        allowUserRecurringReservations: recurringData.allowUserRecurringReservations
      });
    }
  }, [recurringData, recurringForm]);

  const onSubmitCancellation = async (values: CancellationFormValues) => {
    try {
      await updatePolicy.mutateAsync(values);
    } catch {
      // Mutation hooks surface error states through their error property.
    }
  };

  const onSubmitRecurring = async (values: RecurringFormValues) => {
    try {
      await updateRecurring.mutateAsync(values);
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

      <Stack spacing={4}>
        <Paper variant="outlined" sx={{ p: 3 }}>
          <Typography variant="h6" component="h2" gutterBottom>
            {t('admin.configuration.cancellationTitle')}
          </Typography>

          {isLoadingCancellation && (
            <Alert severity="info" sx={{ mb: 3 }}>
              {t('admin.configuration.loading')}
            </Alert>
          )}

          {isCancellationError && (
            <Alert severity="error" sx={{ mb: 3 }}>
              {cancellationError?.message ?? t('admin.configuration.loadError')}
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

          <Box component="form" onSubmit={cancellationForm.handleSubmit(onSubmitCancellation)}>
            <Stack spacing={3}>
              <Controller
                name="minimumHours"
                control={cancellationForm.control}
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
                    error={Boolean(cancellationForm.formState.errors.minimumHours)}
                    helperText={
                      cancellationForm.formState.errors.minimumHours?.message ??
                      t('admin.configuration.minimumHoursHelper')
                    }
                    fullWidth
                  />
                )}
              />
              <FormControlLabel
                control={
                  <Controller
                    name="allowUserCancellation"
                    control={cancellationForm.control}
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

        <Paper variant="outlined" sx={{ p: 3 }}>
          <Typography variant="h6" component="h2" gutterBottom>
            {t('admin.configuration.recurringTitle')}
          </Typography>
          <Typography color="text.secondary" sx={{ mb: 3 }}>
            {t('admin.configuration.recurringSubtitle')}
          </Typography>

          {isLoadingRecurring && (
            <Alert severity="info" sx={{ mb: 3 }}>
              {t('admin.configuration.recurringLoading')}
            </Alert>
          )}

          {isRecurringError && (
            <Alert severity="error" sx={{ mb: 3 }}>
              {recurringError?.message ?? t('admin.configuration.recurringLoadError')}
            </Alert>
          )}

          {updateRecurring.isSuccess && (
            <Alert severity="success" sx={{ mb: 3 }}>
              {t('admin.configuration.recurringSuccess')}
            </Alert>
          )}

          {updateRecurring.isError && (
            <Alert severity="error" sx={{ mb: 3 }}>
              {updateRecurring.error?.message ?? t('admin.configuration.recurringSaveError')}
            </Alert>
          )}

          <Box component="form" onSubmit={recurringForm.handleSubmit(onSubmitRecurring)}>
            <Stack spacing={3}>
              <FormControlLabel
                control={
                  <Controller
                    name="allowUserRecurringReservations"
                    control={recurringForm.control}
                    render={({ field }) => (
                      <Switch
                        checked={field.value}
                        onChange={(event) => field.onChange(event.target.checked)}
                      />
                    )}
                  />
                }
                label={t('admin.configuration.allowUserRecurringReservations')}
              />
              <Typography color="text.secondary" variant="body2">
                {t('admin.configuration.allowUserRecurringReservationsHelper')}
              </Typography>
              <Button
                type="submit"
                variant="contained"
                disabled={updateRecurring.isPending}
              >
                {t('admin.configuration.recurringSave')}
              </Button>
            </Stack>
          </Box>
        </Paper>
      </Stack>
    </Container>
  );
}
