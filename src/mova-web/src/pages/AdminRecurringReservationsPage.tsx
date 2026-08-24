import { useEffect, useMemo, useState } from 'react';
import { Link as RouterLink, useParams } from 'react-router-dom';
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
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useActiveCourts } from '../features/complexes/complexApi';
import { useCreateRecurringReservationForCustomer } from '../features/reservations/reservationApi';
import { useComplexUsers } from '../features/users/userAdminApi';
import type { UserListFilters } from '../features/users/userAdminTypes';

function todayIso() {
  return new Date().toISOString().split('T')[0];
}

function addWeeksIso(weeks: number) {
  const date = new Date();
  date.setDate(date.getDate() + weeks * 7);
  return date.toISOString().split('T')[0];
}

export default function AdminRecurringReservationsPage() {
  const { t } = useTranslation();
  const { complexId = '' } = useParams();
  const [userFilters] = useState<UserListFilters>({
    page: 0,
    pageSize: 100,
    search: '',
    sort: 'fullName:asc'
  });
  const users = useComplexUsers(complexId, userFilters);
  const courts = useActiveCourts(complexId);
  const createRecurring = useCreateRecurringReservationForCustomer(complexId);

  const [userId, setUserId] = useState('');
  const [courtId, setCourtId] = useState('');
  const [dayOfWeek, setDayOfWeek] = useState(String(new Date().getDay()));
  const [startTime, setStartTime] = useState('18:00');
  const [durationMinutes, setDurationMinutes] = useState('60');
  const [startDate, setStartDate] = useState(todayIso);
  const [endDate, setEndDate] = useState(() => addWeeksIso(8));
  const [notes, setNotes] = useState('');

  useEffect(() => {
    setCourtId('');
  }, [complexId]);

  useEffect(() => {
    setUserId('');
  }, [complexId]);

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

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    await createRecurring.mutateAsync({
      userId,
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
    Boolean(complexId && userId && courtId && startTime && startDate && endDate) &&
    Number(durationMinutes) > 0 &&
    occurrencesCount > 0 &&
    !createRecurring.isPending;

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={3}>
        <Stack spacing={1}>
          <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
            {t('admin.recurringPage.title')}
          </Typography>
          <Typography color="text.secondary">
            {t('admin.recurringPage.subtitle')}
          </Typography>
        </Stack>

        {users.isError && <Alert severity="error">{t('admin.recurringPage.usersError')}</Alert>}
        {courts.isError && <Alert severity="error">{t('dashboard.recurringPage.courtsError')}</Alert>}
        {createRecurring.isError && <Alert severity="error">{createRecurring.error.message}</Alert>}
        {createRecurring.isSuccess && (
          <Alert severity="success">
            {t('admin.recurringPage.success', {
              count: createRecurring.data.occurrences.length
            })}
          </Alert>
        )}

        <Paper component="form" variant="outlined" onSubmit={handleSubmit} sx={{ p: 3 }}>
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <FormControl fullWidth disabled={!complexId || users.isLoading}>
                <InputLabel id="recurring-customer-label">{t('admin.recurringPage.customer')}</InputLabel>
                <Select
                  labelId="recurring-customer-label"
                  label={t('admin.recurringPage.customer')}
                  value={userId}
                  onChange={(event) => setUserId(event.target.value)}
                >
                  <MenuItem value="">{t('admin.recurringPage.selectCustomer')}</MenuItem>
                  {(users.data?.items ?? []).map((user) => (
                    <MenuItem key={user.id} value={user.id}>
                      {user.fullName} ({user.email})
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid size={{ xs: 12, md: 6 }}>
              <FormControl fullWidth disabled={!complexId || courts.isLoading}>
                <InputLabel id="recurring-court-label">{t('common.court')}</InputLabel>
                <Select
                  labelId="recurring-court-label"
                  label={t('common.court')}
                  value={courtId}
                  onChange={(event) => setCourtId(event.target.value)}
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
              <FormControl fullWidth>
                <InputLabel id="recurring-day-label">{t('dashboard.recurringPage.dayOfWeek')}</InputLabel>
                <Select
                  labelId="recurring-day-label"
                  label={t('dashboard.recurringPage.dayOfWeek')}
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
              <Stack direction="row" justifyContent="flex-end" spacing={2}>
                <Button component={RouterLink} to={`/admin/complex/${complexId}/recurring`} variant="outlined">
                  {t('common.cancel')}
                </Button>
                <Button type="submit" variant="contained" disabled={!canSubmit}>
                  {t('admin.recurringPage.create')}
                </Button>
              </Stack>
            </Grid>
          </Grid>
        </Paper>
      </Stack>
    </Container>
  );
}
