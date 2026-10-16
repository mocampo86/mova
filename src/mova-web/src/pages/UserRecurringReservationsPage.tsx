import { useEffect, useMemo, useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Collapse,
  Divider,
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
import { usePriceQuote } from '../features/rates/rateApi';
import { formatMoney } from '../features/rates/rateUtils';
import { ApiError } from '../shared/utils/apiError';
import { useCreateMyRecurringReservation } from '../features/reservations/reservationApi';
import { endTimeLabel, localDateTimeToUtc } from '../utils/timezones';
import UserContentContainer from '../layouts/UserContentContainer';

const MAX_VISIBLE_OCCURRENCES = 12;

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

export default function UserRecurringReservationsPage() {
  const { t, i18n } = useTranslation();
  const complexes = useActiveComplexes('', 1);
  const [complexId, setComplexId] = useState('');
  const [courtId, setCourtId] = useState('');
  const [dayOfWeek, setDayOfWeek] = useState(String(new Date().getDay()));
  const [startTime, setStartTime] = useState('18:00');
  const [durationMinutes, setDurationMinutes] = useState('60');
  const [startDate, setStartDate] = useState(todayIso);
  const [endDate, setEndDate] = useState(() => addWeeksIso(8));
  const [notes, setNotes] = useState('');
  const [datesOpen, setDatesOpen] = useState(false);

  const courts = useActiveCourts(complexId);
  const settings = useRecurringReservationSettings(complexId);
  const createRecurring = useCreateMyRecurringReservation(complexId);

  const userRecurringReservationsEnabled =
    complexId !== '' &&
    !settings.isLoading &&
    !settings.isError &&
    settings.data?.allowUserRecurringReservations !== false;

  const occurrenceDates = useMemo(() => {
    if (!startDate || !endDate) return [];

    const start = new Date(`${startDate}T00:00:00`);
    const end = new Date(`${endDate}T00:00:00`);
    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime()) || start > end) return [];

    const targetDay = Number(dayOfWeek);
    const first = new Date(start);
    first.setDate(start.getDate() + ((targetDay - start.getDay() + 7) % 7));
    if (first > end) return [];

    const dates: string[] = [];
    for (let cursor = new Date(first); cursor <= end && dates.length < 260; cursor.setDate(cursor.getDate() + 7)) {
      dates.push(toLocalIsoDate(cursor));
    }
    return dates;
  }, [dayOfWeek, endDate, startDate]);

  const occurrencesCount = occurrenceDates.length;

  const timeZoneId = complexes.data?.items.find((c) => c.id === complexId)?.timeZoneId;

  // Quotes the first occurrence; every occurrence falls on the same weekday/time,
  // so the per-occurrence price is uniform and the expected series total is unit × count.
  const firstOccurrenceUtc = useMemo(() => {
    if (!courtId || occurrenceDates.length === 0 || !startTime || Number(durationMinutes) <= 0) return null;

    const startUtc = localDateTimeToUtc(timeZoneId, `${occurrenceDates[0]}T${startTime}`);
    const endUtc = new Date(new Date(startUtc).getTime() + Number(durationMinutes) * 60000).toISOString();
    return { startUtc, endUtc };
  }, [courtId, occurrenceDates, startTime, durationMinutes, timeZoneId]);

  const occurrenceQuote = usePriceQuote(
    complexId,
    courtId,
    userRecurringReservationsEnabled ? firstOccurrenceUtc?.startUtc : null,
    userRecurringReservationsEnabled ? firstOccurrenceUtc?.endUtc : null
  );

  const quote = occurrenceQuote.data;
  const expectedSeriesTotal =
    quote?.isFullyCovered && occurrencesCount > 0
      ? Math.round(quote.totalAmount * occurrencesCount * 100) / 100
      : undefined;

  const selectedComplex = complexes.data?.items.find((c) => c.id === complexId);
  const selectedCourt = courts.data?.items.find((court) => court.id === courtId);
  const scheduleEndTime = endTimeLabel(startTime, Number(durationMinutes) || 0);

  useEffect(() => {
    setCourtId('');
  }, [complexId]);

  const formatOccurrenceDate = (iso: string) => {
    const formatted = new Intl.DateTimeFormat(i18n.language, {
      weekday: 'long',
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    }).format(new Date(`${iso}T00:00:00`));
    return formatted.charAt(0).toUpperCase() + formatted.slice(1);
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!userRecurringReservationsEnabled) {
      return;
    }

    try {
      await createRecurring.mutateAsync({
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
    userRecurringReservationsEnabled &&
    Boolean(complexId && courtId && startTime && startDate && endDate) &&
    Number(durationMinutes) > 0 &&
    occurrencesCount > 0 &&
    Boolean(quote?.isFullyCovered) &&
    !occurrenceQuote.isLoading &&
    !createRecurring.isPending;

  const formDisabled = !userRecurringReservationsEnabled || courts.isLoading;

  return (
    <UserContentContainer width="form" sx={{ py: { xs: 2, md: 4 } }}>
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
          <Alert
            severity="success"
            action={
              <Button component={RouterLink} to="/user/reservations" color="inherit" size="small">
                {t('dashboard.nav.reservations')}
              </Button>
            }
          >
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

        <Paper component="form" variant="outlined" onSubmit={handleSubmit} sx={{ p: { xs: 2, sm: 3 } }}>
          <Stack spacing={3} divider={<Divider flexItem />}>
            <Box component="fieldset" sx={{ m: 0, p: 0, border: 0, minWidth: 0 }}>
              <Typography component="legend" variant="subtitle1" sx={{ fontWeight: 700, mb: 1.5 }}>
                {t('dashboard.recurringPage.sectionWhere')}
              </Typography>
              <Grid container spacing={2.5}>
                <Grid size={{ xs: 12, md: 6 }}>
                  <FormControl fullWidth required>
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
                  <FormControl fullWidth required disabled={formDisabled}>
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
              </Grid>
            </Box>

            <Box component="fieldset" sx={{ m: 0, p: 0, border: 0, minWidth: 0 }}>
              <Typography component="legend" variant="subtitle1" sx={{ fontWeight: 700, mb: 1.5 }}>
                {t('dashboard.recurringPage.sectionWhen')}
              </Typography>
              <Grid container spacing={2.5}>
                <Grid size={{ xs: 12, sm: 4 }}>
                  <FormControl fullWidth required disabled={formDisabled}>
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
                <Grid size={{ xs: 12, sm: 4 }}>
                  <TextField
                    label={t('dashboard.recurringPage.startTime')}
                    type="time"
                    value={startTime}
                    onChange={(event) => setStartTime(event.target.value)}
                    fullWidth
                    required
                    disabled={formDisabled}
                    InputLabelProps={{ shrink: true }}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                  <TextField
                    label={t('dashboard.recurringPage.duration')}
                    type="number"
                    value={durationMinutes}
                    onChange={(event) => setDurationMinutes(event.target.value)}
                    fullWidth
                    required
                    disabled={formDisabled}
                    inputProps={{ min: 1, max: 1440 }}
                    error={Number(durationMinutes) <= 0 && durationMinutes !== ''}
                  />
                </Grid>
              </Grid>
            </Box>

            <Box component="fieldset" sx={{ m: 0, p: 0, border: 0, minWidth: 0 }}>
              <Typography component="legend" variant="subtitle1" sx={{ fontWeight: 700, mb: 1.5 }}>
                {t('dashboard.recurringPage.sectionPeriod')}
              </Typography>
              <Grid container spacing={2.5}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    label={t('dashboard.recurringPage.startDate')}
                    type="date"
                    value={startDate}
                    onChange={(event) => setStartDate(event.target.value)}
                    fullWidth
                    required
                    disabled={formDisabled}
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
                    disabled={formDisabled}
                    InputLabelProps={{ shrink: true }}
                    error={Boolean(startDate && endDate && startDate > endDate)}
                  />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  {occurrencesCount > 0 ? (
                    <Typography variant="body2" color="text.secondary">
                      {t('dashboard.recurringPage.periodSummary', {
                        count: occurrencesCount,
                        day: t(`days.${dayOfWeek}`).toLowerCase(),
                        start: startTime,
                        end: scheduleEndTime
                      })}
                    </Typography>
                  ) : (
                    <Alert severity="warning">{t('dashboard.recurringPage.noOccurrences')}</Alert>
                  )}
                </Grid>
              </Grid>
            </Box>

            <Box component="fieldset" sx={{ m: 0, p: 0, border: 0, minWidth: 0 }}>
              <Typography component="legend" variant="subtitle1" sx={{ fontWeight: 700, mb: 1.5 }}>
                {t('dashboard.recurringPage.sectionNotes')}
              </Typography>
              <TextField
                label={t('common.notes')}
                value={notes}
                onChange={(event) => setNotes(event.target.value)}
                fullWidth
                multiline
                disabled={formDisabled}
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
                    {t('dashboard.recurringPage.summaryTitle')}
                  </Typography>
                </Stack>

                <Grid container spacing={2}>
                  <Grid size={{ xs: 6, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('dashboard.recurringPage.complex')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {selectedComplex?.name ?? '—'}
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
                      {t('dashboard.recurringPage.dayOfWeek')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {t(`days.${dayOfWeek}`)}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('dashboard.recurringPage.summarySchedule')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {startTime}–{scheduleEndTime}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('dashboard.recurringPage.summaryPeriod')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {startDate} → {endDate}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('dashboard.recurringPage.summaryOccurrences')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {occurrencesCount}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 6, sm: 4 }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                      {t('dashboard.recurringPage.summaryPerOccurrence')}
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
                      {t('dashboard.recurringPage.summaryTotal')}
                    </Typography>
                    <Typography variant="body1" sx={{ fontWeight: 700, color: 'primary.main' }}>
                      {expectedSeriesTotal !== undefined && quote
                        ? formatMoney(expectedSeriesTotal, quote.currency)
                        : '—'}
                    </Typography>
                  </Grid>
                </Grid>

                {occurrencesCount > 0 && (
                  <Box>
                    <Button
                      variant="text"
                      size="small"
                      onClick={() => setDatesOpen((open) => !open)}
                      aria-expanded={datesOpen}
                      endIcon={
                        <Box
                          component="span"
                          aria-hidden="true"
                          sx={{
                            display: 'inline-block',
                            transition: 'transform 150ms ease',
                            transform: datesOpen ? 'rotate(180deg)' : 'none'
                          }}
                        >
                          ▾
                        </Box>
                      }
                    >
                      {t('dashboard.recurringPage.viewDates')}
                    </Button>
                    <Collapse in={datesOpen}>
                      <Stack
                        component="ul"
                        spacing={0.5}
                        sx={{ m: 0, mt: 1, p: 0, listStyle: 'none' }}
                      >
                        {occurrenceDates.slice(0, MAX_VISIBLE_OCCURRENCES).map((iso) => (
                          <Stack
                            component="li"
                            key={iso}
                            direction="row"
                            justifyContent="space-between"
                            alignItems="center"
                            spacing={1}
                            sx={{ py: 0.75, borderBottom: 1, borderColor: 'divider' }}
                          >
                            <Typography variant="body2">{formatOccurrenceDate(iso)}</Typography>
                            <Typography variant="body2" color="text.secondary">
                              {startTime}–{scheduleEndTime}
                            </Typography>
                            <Typography
                              variant="body2"
                              sx={{ fontWeight: 600, color: quote?.isFullyCovered ? 'text.primary' : 'warning.main' }}
                            >
                              {quote?.isFullyCovered
                                ? formatMoney(quote.totalAmount, quote.currency)
                                : t('dashboard.recurringPage.occurrenceNoRate')}
                            </Typography>
                          </Stack>
                        ))}
                        {occurrencesCount > MAX_VISIBLE_OCCURRENCES && (
                          <Typography variant="body2" color="text.secondary" sx={{ pt: 1 }}>
                            {t('dashboard.recurringPage.moreDates', {
                              count: occurrencesCount - MAX_VISIBLE_OCCURRENCES
                            })}
                          </Typography>
                        )}
                      </Stack>
                    </Collapse>
                  </Box>
                )}

                {courtId && occurrenceQuote.isLoading && (
                  <Alert severity="info" variant="outlined">{t('dashboard.recurringPage.priceLoading')}</Alert>
                )}
                {courtId && occurrenceQuote.isError && (
                  <Alert severity="error">{t('dashboard.recurringPage.priceError')}</Alert>
                )}
                {quote && !quote.isFullyCovered && (
                  <Alert severity="warning">{t('dashboard.recurringPage.priceNotConfigured')}</Alert>
                )}
              </Stack>
            </Box>

            <Paper
              elevation={0}
              sx={{
                position: 'sticky',
                bottom: { xs: 'calc(76px + env(safe-area-inset-bottom))', md: 16 },
                zIndex: 1,
                border: 1,
                borderColor: 'divider',
                borderRadius: 2,
                p: 1.5,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                gap: 2,
                bgcolor: 'background.paper'
              }}
            >
              <Box sx={{ minWidth: 0 }}>
                <Typography variant="caption" color="text.secondary" display="block">
                  {t('dashboard.recurringPage.summaryTotal')}
                </Typography>
                <Typography variant="h6" sx={{ fontWeight: 800 }}>
                  {expectedSeriesTotal !== undefined && quote
                    ? formatMoney(expectedSeriesTotal, quote.currency)
                    : '—'}
                </Typography>
              </Box>
              <Tooltip
                title={
                  !userRecurringReservationsEnabled && complexId !== ''
                    ? t('dashboard.recurringPage.disabledTooltip')
                    : ''
                }
              >
                <span>
                  <Button
                    type="submit"
                    variant="contained"
                    size="large"
                    disabled={!canSubmit}
                    sx={{ minHeight: 48, whiteSpace: 'nowrap' }}
                  >
                    {createRecurring.isPending
                      ? t('common.loading')
                      : t('dashboard.recurringPage.confirm')}
                  </Button>
                </span>
              </Tooltip>
            </Paper>
          </Stack>
        </Paper>
      </Stack>
    </UserContentContainer>
  );
}
