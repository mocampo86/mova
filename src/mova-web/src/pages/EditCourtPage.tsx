import { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { zodResolver } from '@hookform/resolvers/zod';
import { Controller, useFieldArray, useForm } from 'react-hook-form';
import { z } from 'zod';
import {
  Alert,
  Box,
  Button,
  Checkbox,
  Container,
  FormControl,
  FormControlLabel,
  FormGroup,
  FormHelperText,
  Paper,
  Skeleton,
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
import {
  useAssignCourtSports,
  useCourt,
  useCourtAvailabilityRules,
  useUpdateCourt,
  useUpdateCourtAvailability
} from '../features/courts/courtApi';
import { useSports } from '../features/complexes/complexApi';
import type { CourtAvailabilityRule } from '../features/courts/courtTypes';

const DAY_LABELS: Record<number, string> = {
  0: 'Sunday',
  1: 'Monday',
  2: 'Tuesday',
  3: 'Wednesday',
  4: 'Thursday',
  5: 'Friday',
  6: 'Saturday'
};

const DEFAULT_AVAILABILITY: AvailabilityFormRule[] = [
  { dayOfWeek: 1, startTime: '08:00', endTime: '22:00', slotDurationMinutes: 60, isActive: false },
  { dayOfWeek: 2, startTime: '08:00', endTime: '22:00', slotDurationMinutes: 60, isActive: false },
  { dayOfWeek: 3, startTime: '08:00', endTime: '22:00', slotDurationMinutes: 60, isActive: false },
  { dayOfWeek: 4, startTime: '08:00', endTime: '22:00', slotDurationMinutes: 60, isActive: false },
  { dayOfWeek: 5, startTime: '08:00', endTime: '22:00', slotDurationMinutes: 60, isActive: false },
  { dayOfWeek: 6, startTime: '08:00', endTime: '22:00', slotDurationMinutes: 60, isActive: false },
  { dayOfWeek: 0, startTime: '08:00', endTime: '22:00', slotDurationMinutes: 60, isActive: false }
];

const timePattern = /^([01]\d|2[0-3]):([0-5]\d)$/;

const availabilityRuleSchema = z
  .object({
    dayOfWeek: z.number(),
    startTime: z.string().regex(timePattern, 'Start time is required.'),
    endTime: z.string().regex(timePattern, 'End time is required.'),
    slotDurationMinutes: z.number().min(1, 'Slot duration must be at least 1 minute.'),
    isActive: z.boolean()
  })
  .refine(
    (rule) => {
      const [startHour, startMinute] = rule.startTime.split(':').map(Number);
      const [endHour, endMinute] = rule.endTime.split(':').map(Number);
      const startMinutes = startHour * 60 + startMinute;
      const endMinutes = endHour * 60 + endMinute;
      return endMinutes > startMinutes;
    },
    {
      message: 'End time must be after start time.',
      path: ['endTime']
    }
  )
  .refine(
    (rule) => {
      const [startHour, startMinute] = rule.startTime.split(':').map(Number);
      const [endHour, endMinute] = rule.endTime.split(':').map(Number);
      const startMinutes = startHour * 60 + startMinute;
      const endMinutes = endHour * 60 + endMinute;
      return (endMinutes - startMinutes) % rule.slotDurationMinutes === 0;
    },
    {
      message: 'The time range must be evenly divisible by the slot duration.',
      path: ['slotDurationMinutes']
    }
  );

const schema = z.object({
  name: z.string().min(1, 'Name is required.').max(255, 'Name must not exceed 255 characters.'),
  description: z
    .string()
    .min(1, 'Description is required.')
    .max(2000, 'Description must not exceed 2000 characters.'),
  surfaceType: z
    .string()
    .min(1, 'Surface type is required.')
    .max(100, 'Surface type must not exceed 100 characters.'),
  indoor: z.boolean(),
  sportIds: z.array(z.string()).min(1, 'At least one sport must be assigned to the court.'),
  availability: z.array(availabilityRuleSchema)
});

type AvailabilityFormRule = z.infer<typeof availabilityRuleSchema>;
type FormValues = z.infer<typeof schema>;

function mergeAvailabilityRules(existing: CourtAvailabilityRule[] | undefined): AvailabilityFormRule[] {
  const merged = DEFAULT_AVAILABILITY.map((rule) => ({ ...rule }));

  for (const rule of existing ?? []) {
    const target = merged.find((r) => r.dayOfWeek === rule.dayOfWeek);
    if (target) {
      target.startTime = rule.startTime.slice(0, 5);
      target.endTime = rule.endTime.slice(0, 5);
      target.slotDurationMinutes = rule.slotDurationMinutes;
      target.isActive = rule.isActive;
    }
  }

  return merged;
}

function formatTimeForApi(time: string): string {
  return `${time}:00`;
}

export default function EditCourtPage() {
  const { complexId = '', courtId = '' } = useParams();
  const navigate = useNavigate();
  const {
    data: court,
    isLoading: isCourtLoading,
    isError: isCourtError,
    error: courtError
  } = useCourt(complexId, courtId);
  const availabilityRules = useCourtAvailabilityRules(complexId, courtId);
  const sports = useSports();
  const updateCourt = useUpdateCourt(complexId, courtId);
  const assignSports = useAssignCourtSports(complexId, courtId);
  const updateAvailability = useUpdateCourtAvailability(complexId, courtId);

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
      surfaceType: '',
      indoor: false,
      sportIds: [],
      availability: DEFAULT_AVAILABILITY
    }
  });

  const { fields } = useFieldArray({ control, name: 'availability', keyName: 'fieldId' });

  useEffect(() => {
    if (court) {
      reset({
        name: court.name,
        description: court.description,
        surfaceType: court.surfaceType,
        indoor: court.indoor,
        sportIds: court.sportIds ?? [],
        availability: mergeAvailabilityRules(availabilityRules.data)
      });
    }
  }, [court, availabilityRules.data, reset]);

  const onSubmit = async (values: FormValues) => {
    const { name, description, surfaceType, indoor, sportIds, availability } = values;

    try {
      await updateCourt.mutateAsync({ name, description, surfaceType, indoor });
      await assignSports.mutateAsync({ sportIds });

      if (!availabilityRules.isError) {
        await updateAvailability.mutateAsync({
          rules: availability.map((rule) => ({
            dayOfWeek: rule.dayOfWeek,
            startTime: formatTimeForApi(rule.startTime),
            endTime: formatTimeForApi(rule.endTime),
            slotDurationMinutes: rule.slotDurationMinutes,
            isActive: rule.isActive
          }))
        });
      }

      navigate(`/admin/complex/${complexId}/courts`);
    } catch {
      // Mutation hooks surface error states through their error property.
    }
  };

  const isSaving =
    updateCourt.isPending || assignSports.isPending || updateAvailability.isPending;
  const isLoading = isCourtLoading || availabilityRules.isLoading;

  if (!complexId || !courtId) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error">The complex or court identifier is missing.</Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Configure court
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        Update the court information, sports, and availability rules for your sports complex.
      </Typography>

      {isCourtError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {courtError?.message ?? 'The court could not be loaded. Please try again later.'}
        </Alert>
      )}

      {updateCourt.error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {updateCourt.error.message}
        </Alert>
      )}

      {assignSports.error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {assignSports.error.message}
        </Alert>
      )}

      {updateAvailability.error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {updateAvailability.error.message}
        </Alert>
      )}

      {isLoading ? (
        <Skeleton variant="rectangular" height={600} />
      ) : (
        <Box
          component="form"
          onSubmit={handleSubmit(onSubmit)}
          sx={{ display: 'flex', flexDirection: 'column', gap: 4 }}
        >
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="h6" component="h2" gutterBottom>
              Court details
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
              <TextField
                {...register('name')}
                label="Court name"
                fullWidth
                error={Boolean(errors.name)}
                helperText={errors.name?.message}
              />

              <TextField
                {...register('description')}
                label="Description"
                multiline
                rows={3}
                fullWidth
                error={Boolean(errors.description)}
                helperText={errors.description?.message}
              />

              <TextField
                {...register('surfaceType')}
                label="Surface type"
                placeholder="e.g., Synthetic, Grass, Concrete"
                fullWidth
                error={Boolean(errors.surfaceType)}
                helperText={errors.surfaceType?.message}
              />

              <Controller
                name="indoor"
                control={control}
                render={({ field }) => (
                  <FormControl error={Boolean(errors.indoor)}>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={field.value}
                          onChange={(event) => field.onChange(event.target.checked)}
                        />
                      }
                      label="Indoor court"
                    />
                    {errors.indoor && <FormHelperText>{errors.indoor.message}</FormHelperText>}
                  </FormControl>
                )}
              />
            </Box>
          </Paper>

          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="h6" component="h2" gutterBottom>
              Sports
            </Typography>

            {sports.isError && (
              <Alert severity="warning" sx={{ mb: 2 }}>
                Available sports could not be loaded. Existing assignments will be kept, but you
                cannot change them until sports are loaded.
              </Alert>
            )}

            <FormControl
              component="fieldset"
              error={Boolean(errors.sportIds)}
              disabled={sports.isLoading || sports.isError}
            >
              {sports.isLoading ? (
                <Skeleton variant="rectangular" height={56} />
              ) : (
                <Controller
                  name="sportIds"
                  control={control}
                  render={({ field }) => (
                    <FormGroup>
                      {sports.data?.map((sport) => (
                        <FormControlLabel
                          key={sport.id}
                          control={
                            <Checkbox
                              checked={field.value.includes(sport.id)}
                              onChange={(event) => {
                                const value = event.target.checked
                                  ? [...field.value, sport.id]
                                  : field.value.filter((id) => id !== sport.id);
                                field.onChange(value);
                              }}
                            />
                          }
                          label={sport.name}
                        />
                      ))}
                    </FormGroup>
                  )}
                />
              )}

              {errors.sportIds && <FormHelperText>{errors.sportIds.message}</FormHelperText>}
            </FormControl>
          </Paper>

          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="h6" component="h2" gutterBottom>
              Availability rules
            </Typography>

            {availabilityRules.isError && (
              <Alert severity="warning" sx={{ mb: 2 }}>
                Availability rules could not be loaded. Court details and sports can still be
                updated, but availability will not be saved.
              </Alert>
            )}

            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Day</TableCell>
                    <TableCell>Active</TableCell>
                    <TableCell>Start time</TableCell>
                    <TableCell>End time</TableCell>
                    <TableCell>Slot duration (minutes)</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {fields.map((field, index) => (
                    <TableRow key={field.fieldId}>
                      <TableCell>{DAY_LABELS[field.dayOfWeek]}</TableCell>
                      <TableCell>
                        <input
                          type="hidden"
                          {...register(`availability.${index}.dayOfWeek` as const)}
                        />
                        <Controller
                          name={`availability.${index}.isActive` as const}
                          control={control}
                          render={({ field: controllerField }) => (
                            <Switch
                              checked={controllerField.value}
                              onChange={(_, checked) => controllerField.onChange(checked)}
                              disabled={availabilityRules.isError}
                              slotProps={{
                                input: { 'aria-label': `${DAY_LABELS[field.dayOfWeek]} active` }
                              }}
                            />
                          )}
                        />
                      </TableCell>
                      <TableCell>
                        <Controller
                          name={`availability.${index}.startTime` as const}
                          control={control}
                          render={({ field: controllerField }) => (
                            <TextField
                              {...controllerField}
                              type="time"
                              label="Start time"
                              InputLabelProps={{ shrink: true }}
                              size="small"
                              fullWidth
                              error={Boolean(errors.availability?.[index]?.startTime)}
                              helperText={errors.availability?.[index]?.startTime?.message}
                              disabled={availabilityRules.isError}
                            />
                          )}
                        />
                      </TableCell>
                      <TableCell>
                        <Controller
                          name={`availability.${index}.endTime` as const}
                          control={control}
                          render={({ field: controllerField }) => (
                            <TextField
                              {...controllerField}
                              type="time"
                              label="End time"
                              InputLabelProps={{ shrink: true }}
                              size="small"
                              fullWidth
                              error={Boolean(errors.availability?.[index]?.endTime)}
                              helperText={errors.availability?.[index]?.endTime?.message}
                              disabled={availabilityRules.isError}
                            />
                          )}
                        />
                      </TableCell>
                      <TableCell>
                        <Controller
                          name={`availability.${index}.slotDurationMinutes` as const}
                          control={control}
                          render={({ field: controllerField }) => (
                            <TextField
                              {...controllerField}
                              type="number"
                              label="Slot duration"
                              inputProps={{ min: 1 }}
                              size="small"
                              fullWidth
                              value={controllerField.value}
                              onChange={(event) =>
                                controllerField.onChange(Number(event.target.value))
                              }
                              error={Boolean(errors.availability?.[index]?.slotDurationMinutes)}
                              helperText={errors.availability?.[index]?.slotDurationMinutes?.message}
                              disabled={availabilityRules.isError}
                            />
                          )}
                        />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>

          <Button
            type="submit"
            variant="contained"
            disabled={isSaving || sports.isLoading}
            sx={{ alignSelf: 'flex-start' }}
          >
            Update court
          </Button>
        </Box>
      )}
    </Container>
  );
}
