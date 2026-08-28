import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Container,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  TextField,
  Tooltip,
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import ApiErrorMessage from '../components/ApiErrorMessage';
import { useActiveComplexes, useActiveCourts, useRecurringReservationSettings } from '../features/complexes/complexApi';
import { useCreateMyRecurringReservation } from '../features/reservations/reservationApi';

function todayIso() {
  return new Date().toISOString().split('T')[0];
}

function addWeeksIso(weeks: number) {
  const date = new Date();
  date.setDate(date.getDate() + weeks * 7);
  return date.toISOString().split('T')[0];
}

export default function UserRecurringReservationsPage() {
  const { t } = useTranslation();
  const complexes = useActiveComplexes('', 1);
  const [complexId, setComplexId] = useState('');
  const [courtId, setCourtId] = useState('');
  const [dayOfWeek, setDayOfWeek] = useState(String(new Date().getDay()));
  const [startTime, setStartTime] = useState('18:00');
  const [durationMinutes, setDurationMinutes] = useState('60');
  const [startDate, setStartDate] = useState(todayIso);
  const [endDate, setEndDate] = useState(() => addWeeksIso(8));
  const [notes, setNotes] = useState('');

  const courts = useActiveCourts(complexId);
  const settings = useRecurringReservationSettings(complexId);
  const createRecurring = useCreateMyRecurringReservation(complexId);

  const userRecurringReservationsEnabled =
    complexId !== '' &&
    !settings.isLoading &&
    !settings.isError &&
    settings.data?.allowUserRecurringReservations !== false;

  const occurrencesCount = useMemo(() => {
    if (!startDate || !endDate) return 0;

    const start = new Date(`${startDate}T00:00:00`);
    const end = new Date(`${endDate}T00:00:00`);
    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime()) || start > end) return 0;

    const targetDay = Number(dayOfWeek);
    const first = new Date(start);
    first.setDate(start.getDate() + ((targetDay - start.getDay() + 7) % 7));
    if (first > end) return 0;

    return Math.floor((end.getTime() - first.getTime()) / (7 * 24 * 60 * 60 * 1000)) + 1;
  }, [dayOfWeek, endDate, startDate]);

  useEffect(() => {
    setCourtId('');
  }, [complexId]);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!userRecurringReservationsEnabled) {
      return;
    }

    await createRecurring.mutateAsync({
      courtId,
      dayOfWeek: Number(dayOfWeek),
      startTime: `${startTime}:00`,
      durationMinutes: Number(durationMinutes),
      startDate,
      endDate,
      notes: notes.trim() || undefined
    });
  };

  const canSubmit =
    userRecurringReservationsEnabled &&
    Boolean(complexId && courtId && startTime && startDate && endDate) &&
    Number(durationMinutes) > 0 &&
    occurrencesCount > 0 &&
    !createRecurring.isPending;

  const formDisabled = !userRecurringReservationsEnabled || courts.isLoading;

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={3}>
        <Stack spacing={1}>
          <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
            {t('dashboard.recurringPage.title')}
          </Typography>
          <Typography color="text.secondary">
            {t('dashboard.recurringPage.subtitle')}
          </Typography>
        </Stack>

        {complexes.isError && <Alert severity="error">{t('dashboard.recurringPage.complexesError')}</Alert>}
        {courts.isError && <Alert severity="error">{t('dashboard.recurringPage.courtsError')}</Alert>}
        {settings.isError && <Alert severity="error">{t('dashboard.recurringPage.settingsError')}</Alert>}
        {createRecurring.isError && <Alert severity="error"><ApiErrorMessage error={createRecurring.error} /></Alert>}
        {createRecurring.isSuccess && (
          <Alert severity="success">
            {t('dashboard.recurringPage.success', {
              count: createRecurring.data.occurrences.length
            })}
          </Alert>
        )}

        {complexId !== '' && !settings.isLoading && settings.data?.allowUserRecurringReservations === false && (
          <Alert severity="info" variant="outlined">
            {t('dashboard.recurringPage.disabledForComplex')}
          </Alert>
        )}

        <Paper component="form" variant="outlined" onSubmit={handleSubmit} sx={{ p: 3 }}>
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <FormControl fullWidth>
                <InputLabel id="recurring-complex-label">{t('dashboard.recurringPage.complex')}</InputLabel>
                <Select
                  labelId="recurring-complex-label"
                  label={t('dashboard.recurringPage.complex')}
                  name="complexId"
                  value={complexId}
                  onChange={(event) => setComplexId(event.target.value)}
                  disabled={complexes.isLoading}
                  data-testid="recurring-complex-select"
                >
                  <MenuItem value="">{t('dashboard.recurringPage.selectComplex')}</MenuItem>
                  {(complexes.data?.items ?? []).map((complex) => (
                    <MenuItem key={complex.id} value={complex.id}>
                      {complex.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid size={{ xs: 12, md: 6 }}>
              <FormControl fullWidth disabled={formDisabled}>
                <InputLabel id="recurring-court-label">{t('common.court')}</InputLabel>
                <Select
                  labelId="recurring-court-label"
                  label={t('common.court')}
                  name="courtId"
                  value={courtId}
                  onChange={(event) => setCourtId(event.target.value)}
                  data-testid="recurring-court-select"
                >
                  <MenuItem value="">{t('dashboard.recurringPage.selectCourt')}</MenuItem>
                  {(courts.data?.items ?? []).map((court) => (
                    <MenuItem key={court.id} value={court.id}>
                      {court.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <FormControl fullWidth disabled={formDisabled}>
                <InputLabel id="recurring-day-label">{t('dashboard.recurringPage.dayOfWeek')}</InputLabel>
                <Select
                  labelId="recurring-day-label"
                  label={t('dashboard.recurringPage.dayOfWeek')}
                  name="dayOfWeek"
                  data-testid="recurring-day-select"
                  value={dayOfWeek}
                  onChange={(event) => setDayOfWeek(event.target.value)}
                >
                  {[0, 1, 2, 3, 4, 5, 6].map((day) => (
                    <MenuItem key={day} value={String(day)}>
                      {t(`days.${day}`)}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                label={t('dashboard.recurringPage.startTime')}
                type="time"
                value={startTime}
                onChange={(event) => setStartTime(event.target.value)}
                fullWidth
                disabled={formDisabled}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                label={t('dashboard.recurringPage.duration')}
                type="number"
                value={durationMinutes}
                onChange={(event) => setDurationMinutes(event.target.value)}
                fullWidth
                disabled={formDisabled}
                inputProps={{ min: 1, max: 1440 }}
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                label={t('dashboard.recurringPage.startDate')}
                type="date"
                value={startDate}
                onChange={(event) => setStartDate(event.target.value)}
                fullWidth
                disabled={formDisabled}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                label={t('dashboard.recurringPage.endDate')}
                type="date"
                value={endDate}
                onChange={(event) => setEndDate(event.target.value)}
                fullWidth
                disabled={formDisabled}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>

            <Grid size={{ xs: 12, md: 9 }}>
              <TextField
                label={t('common.notes')}
                value={notes}
                onChange={(event) => setNotes(event.target.value)}
                fullWidth
                multiline
                disabled={formDisabled}
                rows={2}
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              {occurrencesCount > 0 ? (
                <Alert severity="info">
                  {t('dashboard.recurringPage.preview', { count: occurrencesCount })}
                </Alert>
              ) : (
                <Alert severity="warning">{t('dashboard.recurringPage.noOccurrences')}</Alert>
              )}
            </Grid>

            <Grid size={{ xs: 12 }}>
              <Stack direction="row" justifyContent="flex-end">
                <Tooltip
                  title={
                    !userRecurringReservationsEnabled && complexId !== ''
                      ? t('dashboard.recurringPage.disabledTooltip')
                      : ''
                  }
                >
                  <span>
                    <Button type="submit" variant="contained" disabled={!canSubmit}>
                      {t('dashboard.recurringPage.create')}
                    </Button>
                  </span>
                </Tooltip>
              </Stack>
            </Grid>
          </Grid>
        </Paper>
      </Stack>
    </Container>
  );
}
