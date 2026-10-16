import { useEffect, useMemo, useState } from 'react';
import { Link as RouterLink, useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Container,
  Divider,
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
import ApiErrorMessage from '../components/ApiErrorMessage';
import { useActiveCourts, useAdminComplex } from '../features/complexes/complexApi';
import { usePriceQuote } from '../features/rates/rateApi';
import { formatMoney } from '../features/rates/rateUtils';
import { ApiError } from '../shared/utils/apiError';
import { useCreateRecurringReservationForCustomer } from '../features/reservations/reservationApi';
import { useComplexUsers } from '../features/users/userAdminApi';
import type { UserListFilters } from '../features/users/userAdminTypes';
import { endTimeLabel, localDateTimeToUtc } from '../utils/timezones';

function todayIso() {
  return new Date().toISOString().split('T')[0];
}

function toLocalIsoDate(date: Date) {
  const pad = (value: number) => String(value).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
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
  const complex = useAdminComplex(complexId);
  const timeZoneId = complex.data?.timeZoneId ?? undefined;
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

  // Quotes the first occurrence; every occurrence falls on the same weekday/time,
  // so the per-occurrence price is uniform and the expected series total is unit × count.
  const firstOccurrenceUtc = useMemo(() => {
    if (!courtId || !startDate || !endDate || !startTime || Number(durationMinutes) <= 0) return null;

    const start = new Date(`${startDate}T00:00:00`);
    const end = new Date(`${endDate}T00:00:00`);
    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime()) || start > end) return null;

    const targetDay = Number(dayOfWeek);
    const first = new Date(start);
    first.setDate(start.getDate() + ((targetDay - start.getDay() + 7) % 7));
    if (first > end) return null;

    const startUtc = localDateTimeToUtc(timeZoneId, `${toLocalIsoDate(first)}T${startTime}`);
    const endUtc = new Date(new Date(startUtc).getTime() + Number(durationMinutes) * 60000).toISOString();
    return { startUtc, endUtc };
  }, [courtId, startDate, endDate, startTime, durationMinutes, dayOfWeek, timeZoneId]);

  const occurrenceQuote = usePriceQuote(
    complexId,
    courtId,
    firstOccurrenceUtc?.startUtc,
    firstOccurrenceUtc?.endUtc
  );

  const quote = occurrenceQuote.data;
  const expectedSeriesTotal =
    quote?.isFullyCovered && occurrencesCount > 0
      ? Math.round(quote.totalAmount * occurrencesCount * 100) / 100
      : undefined;
  const selectedCustomer = users.data?.items.find((user) => user.id === userId);
  const selectedCourt = courts.data?.items.find((court) => court.id === courtId);
  const scheduleEndTime = endTimeLabel(startTime, Number(durationMinutes) || 0);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    try {
      await createRecurring.mutateAsync({
        userId,
        courtId,
        dayOfWeek: Number(dayOfWeek),
        startTime: `${startTime}:00`,
        durationMinutes: Number(durationMinutes),
        startDate,
        endDate,
        notes: notes.trim() || undefined,
        expectedTotalPrice: expectedSeriesTotal,
        expectedCurrency: quote?.currency
      });
    } catch (err) {
      if (err instanceof ApiError && err.code === 'PRICE_CHANGED') {
        occurrenceQuote.refetch();
      }
    }
  };

  const canSubmit =
    Boolean(complexId && userId && courtId && startTime && startDate && endDate) &&
    Number(durationMinutes) > 0 &&
    occurrencesCount > 0 &&
    Boolean(quote?.isFullyCovered) &&
    !occurrenceQuote.isLoading &&
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
        {createRecurring.isError && <Alert severity="error"><ApiErrorMessage error={createRecurring.error} /></Alert>}
        {createRecurring.isSuccess && (
          <Alert severity="success">
            {t('admin.recurringPage.success', {
              count: createRecurring.data.occurrences.length
            })}
          </Alert>
        )}

        <Paper
          component="form"
          variant="outlined"
          onSubmit={handleSubmit}
          sx={{ p: { xs: 2, sm: 3 }, maxWidth: 900 }}
        >
          <Stack spacing={3} divider={<Divider flexItem />}>
            <Box component="fieldset" sx={{ m: 0, p: 0, border: 0, minWidth: 0 }}>
              <Typography component="legend" variant="subtitle1" sx={{ fontWeight: 700, mb: 1.5 }}>
                {t('admin.recurringPage.sectionCustomerCourt')}
              </Typography>
              <Grid container spacing={2.5}>
                <Grid size={{ xs: 12, md: 6 }}>
                  <FormControl fullWidth required disabled={!complexId || users.isLoading}>
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
                  <FormControl fullWidth required disabled={!complexId || courts.isLoading}>
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
              </Grid>
            </Box>

            <Box component="fieldset" sx={{ m: 0, p: 0, border: 0, minWidth: 0 }}>
              <Typography component="legend" variant="subtitle1" sx={{ fontWeight: 700, mb: 1.5 }}>
                {t('admin.recurringPage.sectionSchedule')}
              </Typography>
              <Grid container spacing={2.5}>
                <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                  <FormControl fullWidth required>
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
                <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                  <TextField
                    label={t('dashboard.recurringPage.startTime')}
                    type="time"
                    value={startTime}
                    onChange={(event) => setStartTime(event.target.value)}
                    fullWidth
                    required
                    InputLabelProps={{ shrink: true }}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                  <TextField
                    label={t('dashboard.recurringPage.duration')}
                    type="number"
                    value={durationMinutes}
                    onChange={(event) => setDurationMinutes(event.target.value)}
                    fullWidth
                    required
                    inputProps={{ min: 1, max: 1440 }}
                    error={Number(durationMinutes) <= 0 && durationMinutes !== ''}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    label={t('dashboard.recurringPage.startDate')}
                    type="date"
                    value={startDate}
                    onChange={(event) => setStartDate(event.target.value)}
                    fullWidth
                    required
                    InputLabelProps={{ shrink: true }}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    label={t('dashboard.recurringPage.endDate')}
                    type="date"
                    value={endDate}
                    onChange={(event) => setEndDate(event.target.value)}
                    fullWidth
                    required
                    InputLabelProps={{ shrink: true }}
                    error={Boolean(startDate && endDate && startDate > endDate)}
                  />
                </Grid>
              </Grid>
            </Box>

            <Box component="fieldset" sx={{ m: 0, p: 0, border: 0, minWidth: 0 }}>
              <Typography component="legend" variant="subtitle1" sx={{ fontWeight: 700, mb: 1.5 }}>
                {t('admin.recurringPage.sectionNotes')}
              </Typography>
              <TextField
                label={t('common.notes')}
                value={notes}
                onChange={(event) => setNotes(event.target.value)}
                fullWidth
                multiline
                rows={2}
              />
            </Box>

            <Box
              sx={{
                bgcolor: 'background.default',
                border: 1,
                borderColor: 'divider',
                borderRadius: 2,
                p: { xs: 2, sm: 2.5 }
              }}
            >
              <Stack spacing={2}>
                <Stack spacing={0.5}>
                  <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                    {t('admin.recurringPage.summaryTitle')}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {t('admin.recurringPage.summaryDescription')}
                  </Typography>
                </Stack>

                <Grid container spacing={2}>
                  <Grid size={{ xs: 6, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('admin.recurringPage.customer')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {selectedCustomer?.fullName ?? '—'}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('common.court')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {selectedCourt?.name ?? '—'}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('admin.recurringPage.summaryFrequency')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {t('admin.recurringPage.summaryWeekly', { day: t(`days.${dayOfWeek}`) })}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('admin.recurringPage.summarySchedule')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {startTime}–{scheduleEndTime} ({durationMinutes} min)
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('admin.recurringPage.summaryPeriod')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {startDate} → {endDate}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('admin.recurringPage.summaryOccurrences')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {occurrencesCount}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('admin.recurringPage.summaryPerOccurrence')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {quote?.isFullyCovered
                        ? formatMoney(quote.totalAmount, quote.currency)
                        : courtId && occurrenceQuote.isLoading
                          ? t('common.loading')
                          : '—'}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('admin.recurringPage.summaryTotal')}
                    </Typography>
                    <Typography variant="body1" sx={{ fontWeight: 700, color: 'primary.main' }}>
                      {expectedSeriesTotal !== undefined && quote
                        ? formatMoney(expectedSeriesTotal, quote.currency)
                        : '—'}
                    </Typography>
                  </Grid>
                </Grid>

                {occurrencesCount === 0 && startDate && endDate && (
                  <Alert severity="warning">{t('dashboard.recurringPage.noOccurrences')}</Alert>
                )}
                {courtId && occurrenceQuote.isLoading && (
                  <Alert severity="info" variant="outlined">{t('admin.recurringPage.priceLoading')}</Alert>
                )}
                {courtId && occurrenceQuote.isError && (
                  <Alert severity="error">{t('admin.recurringPage.priceError')}</Alert>
                )}
                {quote && !quote.isFullyCovered && (
                  <Alert severity="warning">{t('admin.recurringPage.priceNotConfigured')}</Alert>
                )}
              </Stack>
            </Box>

            <Stack
              direction={{ xs: 'column', sm: 'row' }}
              justifyContent="flex-end"
              spacing={2}
            >
              <Button
                component={RouterLink}
                to={`/admin/complex/${complexId}/recurring`}
                variant="outlined"
                sx={{ width: { xs: '100%', sm: 'auto' } }}
              >
                {t('common.cancel')}
              </Button>
              <Button
                type="submit"
                variant="contained"
                disabled={!canSubmit}
                sx={{ width: { xs: '100%', sm: 'auto' } }}
              >
                {createRecurring.isPending ? t('common.loading') : t('admin.recurringPage.create')}
              </Button>
            </Stack>
          </Stack>
        </Paper>
      </Stack>
    </Container>
  );
}
