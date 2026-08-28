import { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { zodResolver } from '@hookform/resolvers/zod';
import { Controller, useFieldArray, useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import ApiErrorMessage from '../components/ApiErrorMessage';
import { z } from 'zod';
import {
  Alert,
  Box,
  Button,
  Container,
  Paper,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography
} from '@mui/material';
import { useBusinessHours, useUpdateBusinessHours } from '../features/complexes/complexApi';
import type { BusinessHours, BusinessHoursItem } from '../features/complexes/complexTypes';

const timePattern = /^([01]\d|2[0-3]):([0-5]\d)$/;

const businessHoursItemSchema = z
  .object({
    dayOfWeek: z.number(),
    openingTime: z.string().regex(timePattern, 'Opening time is required.'),
    closingTime: z.string().regex(timePattern, 'Closing time is required.'),
    isClosed: z.boolean()
  })
  .refine(
    (data) => {
      if (data.isClosed) return true;
      const [startHour, startMinute] = data.openingTime.split(':').map(Number);
      const [endHour, endMinute] = data.closingTime.split(':').map(Number);
      const startMinutes = startHour * 60 + startMinute;
      const endMinutes = endHour * 60 + endMinute;
      return startMinutes !== endMinutes;
    },
    {
      message: 'Opening and closing times must be different.',
      path: ['closingTime']
    }
  );

const schema = z.object({
  hours: z.array(businessHoursItemSchema).length(7)
});

type FormValues = z.infer<typeof schema>;

const DEFAULT_BUSINESS_HOURS: BusinessHoursItem[] = [
  { dayOfWeek: 1, openingTime: '08:00', closingTime: '22:00', isClosed: false },
  { dayOfWeek: 2, openingTime: '08:00', closingTime: '22:00', isClosed: false },
  { dayOfWeek: 3, openingTime: '08:00', closingTime: '22:00', isClosed: false },
  { dayOfWeek: 4, openingTime: '08:00', closingTime: '22:00', isClosed: false },
  { dayOfWeek: 5, openingTime: '08:00', closingTime: '22:00', isClosed: false },
  { dayOfWeek: 6, openingTime: '08:00', closingTime: '22:00', isClosed: false },
  { dayOfWeek: 0, openingTime: '08:00', closingTime: '22:00', isClosed: false }
];

function formatTimeForApi(time: string): string {
  return `${time}:00`;
}

function getDayLabel(t: (key: string) => string, dayOfWeek: number): string {
  return t(`days.${dayOfWeek}`);
}

function mergeBusinessHours(existing: BusinessHours[] | undefined): BusinessHoursItem[] {
  const merged = DEFAULT_BUSINESS_HOURS.map((hour) => ({ ...hour }));

  for (const hour of existing ?? []) {
    const target = merged.find((h) => h.dayOfWeek === hour.dayOfWeek);
    if (target) {
      target.openingTime = hour.openingTime.slice(0, 5);
      target.closingTime = hour.closingTime.slice(0, 5);
      target.isClosed = hour.isClosed;
    }
  }

  return merged;
}

export default function BusinessHoursPage() {
  const { t } = useTranslation();
  const { complexId = '' } = useParams();
  const businessHours = useBusinessHours(complexId);
  const updateBusinessHours = useUpdateBusinessHours(complexId);

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors }
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      hours: DEFAULT_BUSINESS_HOURS
    }
  });

  const { fields } = useFieldArray({ control, name: 'hours', keyName: 'fieldId' });

  useEffect(() => {
    if (businessHours.data) {
      reset({
        hours: mergeBusinessHours(businessHours.data)
      });
    }
  }, [businessHours.data, reset]);

  const onSubmit = async (values: FormValues) => {
    try {
      await updateBusinessHours.mutateAsync({
        hours: values.hours.map((hour) => ({
          dayOfWeek: hour.dayOfWeek,
          openingTime: formatTimeForApi(hour.openingTime),
          closingTime: formatTimeForApi(hour.closingTime),
          isClosed: hour.isClosed
        }))
      });
    } catch {
      // Mutation hooks surface error states through their error property.
    }
  };

  if (!complexId) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error">{t('admin.businessHours.missingId')}</Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        {t('admin.businessHours.title')}
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        {t('admin.businessHours.subtitle')}
      </Typography>

      {businessHours.isError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {businessHours.error?.message ?? t('admin.businessHours.loadError')}
        </Alert>
      )}

      {updateBusinessHours.isSuccess && (
        <Alert severity="success" sx={{ mb: 3 }}>
          {t('admin.businessHours.success')}
        </Alert>
      )}

      {updateBusinessHours.error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          <ApiErrorMessage error={updateBusinessHours.error} />
        </Alert>
      )}

      <Paper variant="outlined" sx={{ p: 3 }}>
        <Box component="form" onSubmit={handleSubmit(onSubmit)}>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>{t('admin.businessHours.dayHeader')}</TableCell>
                  <TableCell>{t('admin.businessHours.openHeader')}</TableCell>
                  <TableCell>{t('admin.businessHours.openingTimeHeader')}</TableCell>
                  <TableCell>{t('admin.businessHours.closingTimeHeader')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {fields.map((field, index) => (
                  <TableRow key={field.fieldId}>
                    <TableCell>{getDayLabel(t, field.dayOfWeek)}</TableCell>
                    <TableCell>
                      <input type="hidden" {...register(`hours.${index}.dayOfWeek` as const)} />
                      <Controller
                        name={`hours.${index}.isClosed` as const}
                        control={control}
                        render={({ field: controllerField }) => (
                          <Switch
                            checked={!controllerField.value}
                            onChange={(_, checked) => controllerField.onChange(!checked)}
                            slotProps={{
                              input: { 'aria-label': `${getDayLabel(t, field.dayOfWeek)} open` }
                            }}
                          />
                        )}
                      />
                    </TableCell>
                    <TableCell>
                      <Controller
                        name={`hours.${index}.openingTime` as const}
                        control={control}
                        render={({ field: controllerField }) => (
                          <TextField
                            {...controllerField}
                            type="time"
                            label={t('admin.businessHours.openingTimeHeader')}
                            InputLabelProps={{ shrink: true }}
                            size="small"
                            fullWidth
                            error={Boolean(errors.hours?.[index]?.openingTime)}
                            helperText={errors.hours?.[index]?.openingTime?.message}
                          />
                        )}
                      />
                    </TableCell>
                    <TableCell>
                      <Controller
                        name={`hours.${index}.closingTime` as const}
                        control={control}
                        render={({ field: controllerField }) => (
                          <TextField
                            {...controllerField}
                            type="time"
                            label={t('admin.businessHours.closingTimeHeader')}
                            InputLabelProps={{ shrink: true }}
                            size="small"
                            fullWidth
                            error={Boolean(errors.hours?.[index]?.closingTime)}
                            helperText={errors.hours?.[index]?.closingTime?.message}
                          />
                        )}
                      />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          <Stack direction="row" justifyContent="flex-end" sx={{ mt: 3 }}>
            <Button
              type="submit"
              variant="contained"
              disabled={updateBusinessHours.isPending || businessHours.isError}
            >
              {t('admin.businessHours.save')}
            </Button>
          </Stack>
        </Box>
      </Paper>
    </Container>
  );
}
