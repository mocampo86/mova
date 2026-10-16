import { useEffect, useMemo, useState } from 'react';
import { Link as RouterLink, useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import ApiErrorMessage from '../components/ApiErrorMessage';
import Seo from '../components/Seo';
import UserContentContainer from '../layouts/UserContentContainer';
import FavoriteToggleButton from '../features/favorites/FavoriteToggleButton';
import {
  useActiveComplex,
  useActiveCourts,
  useCourtAvailability,
  useSports
} from '../features/complexes/complexApi';
import type { CourtAvailabilitySlot } from '../features/complexes/complexTypes';
import { useAuth } from '../features/auth/useAuth';
import { useMyBlockStatus } from '../features/users/useMyBlockStatus';
import { usePriceQuote } from '../features/rates/rateApi';
import type { PriceQuote } from '../features/rates/rateTypes';
import { formatMoney } from '../features/rates/rateUtils';
import { useCreateMyReservation } from '../features/reservations/reservationApi';
import { ApiError } from '../shared/utils/apiError';
import { todayInTimeZone } from '../utils/timezones';

function formatLocalTime(isoString: string) {
  const date = new Date(isoString);
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function formatLocalDate(isoString: string) {
  const date = new Date(isoString);
  return date.toLocaleDateString();
}

function formatLocalDateTime(isoString: string) {
  const date = new Date(isoString);
  return `${date.toLocaleDateString()} ${date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
}

function addDays(date: string, days: number): string {
  const result = new Date(`${date}T12:00:00`);
  result.setDate(result.getDate() + days);
  return `${result.getFullYear()}-${String(result.getMonth() + 1).padStart(2, '0')}-${String(result.getDate()).padStart(2, '0')}`;
}



interface BookingDialogProps {
  slot: CourtAvailabilitySlot;
  courtName: string;
  notes: string;
  onNotesChange: (notes: string) => void;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: Error | null;
  quote?: PriceQuote;
  quoteLoading: boolean;
  quoteError: boolean;
  onConfirm: () => Promise<void>;
  onCancel: () => void;
  t: (key: string, options?: Record<string, unknown>) => string;
}

function BookingDialog({
  slot,
  courtName,
  notes,
  onNotesChange,
  isAuthenticated,
  isLoading,
  error,
  quote,
  quoteLoading,
  quoteError,
  onConfirm,
  onCancel,
  t
}: BookingDialogProps) {
  const priceBlocked = !quote || !quote.isFullyCovered;

  return (
    <Dialog open onClose={onCancel} maxWidth="sm" fullWidth>
      <DialogTitle>{t('complexDetail.booking.title')}</DialogTitle>
      <DialogContent>
        <Stack spacing={2}>
          {!isAuthenticated && (
            <Alert severity="info">{t('complexDetail.booking.loginPrompt')}</Alert>
          )}
          {error && <Alert severity="error"><ApiErrorMessage error={error} /></Alert>}
          <Typography>
            {t('complexDetail.booking.message', {
              court: courtName,
              date: formatLocalDate(slot.startAt),
              start: formatLocalTime(slot.startAt),
              end: formatLocalTime(slot.endAt)
            })}
          </Typography>
          {quoteLoading && <Typography color="text.secondary">{t('complexDetail.booking.priceLoading')}</Typography>}
          {quoteError && <Alert severity="error">{t('complexDetail.booking.priceError')}</Alert>}
          {quote && !quote.isFullyCovered && (
            <Alert severity="warning">{t('complexDetail.booking.priceNotConfigured')}</Alert>
          )}
          {quote && quote.isFullyCovered && (
            <Stack spacing={0.5}>
              {quote.lines.map((line, index) => (
                <Typography key={index} variant="body2" color="text.secondary">
                  {formatLocalTime(line.startAt)} – {formatLocalTime(line.endAt)}:{' '}
                  {formatMoney(line.hourlyRate, quote.currency)}/h → {formatMoney(line.amount, quote.currency)}
                </Typography>
              ))}
              <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                {t('complexDetail.booking.total', { amount: formatMoney(quote.totalAmount, quote.currency) })}
              </Typography>
            </Stack>
          )}
          {isAuthenticated && (
            <TextField
              label={t('complexDetail.booking.notesLabel')}
              value={notes}
              onChange={(event) => onNotesChange(event.target.value)}
              multiline
              rows={2}
              fullWidth
            />
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel} disabled={isLoading}>
          {t('common.cancel')}
        </Button>
        {isAuthenticated && (
          <Button onClick={onConfirm} variant="contained" disabled={isLoading || quoteLoading || priceBlocked}>
            {t('complexDetail.booking.confirm')}
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
}

export default function ComplexDetailPage() {
  const { t } = useTranslation();
  const { complexId = '' } = useParams();
  const complex = useActiveComplex(complexId);

  const [selectedSportId, setSelectedSportId] = useState('');
  const [selectedCourtId, setSelectedCourtId] = useState('');
  const [selectedDate, setSelectedDate] = useState(todayInTimeZone());
  const [selectedSlot, setSelectedSlot] = useState<CourtAvailabilitySlot | null>(null);
  const [bookingNotes, setBookingNotes] = useState('');

  const { isAuthenticated } = useAuth();
  const myBlockStatus = useMyBlockStatus(complexId);
  const courts = useActiveCourts(complexId, selectedSportId || undefined);
  const sports = useSports();
  const createMyReservation = useCreateMyReservation(complexId);

  useEffect(() => {
    if (complex.data) {
      setSelectedDate(todayInTimeZone(complex.data.timeZoneId));
    }
  }, [complex.data]);

  const availability = useCourtAvailability(complexId, selectedCourtId, selectedDate);
  const dateOptions = useMemo(() => Array.from({ length: 7 }, (_, index) => addDays(selectedDate, index)), [selectedDate]);

  const priceQuote = usePriceQuote(
    complexId,
    selectedSlot?.courtId ?? '',
    selectedSlot?.startAt,
    selectedSlot?.endAt
  );

  useEffect(() => {
    setSelectedCourtId('');
  }, [selectedSportId]);

  if (complex.isLoading || courts.isLoading) {
    return (
      <UserContentContainer width="form" sx={{ py: 6 }}>
        <Seo
          title={`${t('common.appName')} | ${t('complexDetail.loading')}`}
          description={t('common.noDescription')}
        />
        <Typography>{t('complexDetail.loading')}</Typography>
      </UserContentContainer>
    );
  }

  if (complex.isError || !complex.data) {
    return (
      <UserContentContainer width="form" sx={{ py: 6 }}>
        <Seo
          title={`${t('common.appName')} | ${t('notFound.title')}`}
          description={t('seo.notFoundDescription')}
        />
        <Alert severity="error">{t('complexDetail.notFound')}</Alert>
      </UserContentContainer>
    );
  }

  return (
    <UserContentContainer width="form" sx={{ py: { xs: 2, sm: 5 } }}>
      <Seo
        title={`${t('common.appName')} | ${complex.data.name}`}
        description={complex.data.description || t('common.noDescription')}
      />
      <Stack spacing={4}>
        <Button component={RouterLink} to="/complexes" sx={{ alignSelf: 'flex-start' }}>
          {t('common.back')}
        </Button>

        <Box>
          <Stack direction="row" spacing={1.5} alignItems="flex-start" sx={{ minWidth: 0 }}>
            <Typography component="h1" variant="h3" sx={{ fontWeight: 800, minWidth: 0, overflowWrap: 'anywhere', flexGrow: 1 }}>
              {complex.data.name}
            </Typography>
            <FavoriteToggleButton complexId={complex.data.id} />
          </Stack>
          <Typography color="text.secondary">
            {complex.data.city}{t('common.formatSeparator')}{complex.data.address}
          </Typography>
          <Typography sx={{ mt: 2 }}>{complex.data.description}</Typography>
          {isAuthenticated && myBlockStatus.data?.isBlocked && (
            <Alert severity="warning" variant="outlined" sx={{ mt: 2 }}>
              {myBlockStatus.data.blockedUntil
                ? t('complexDetail.blockedMessageWithExpiration', {
                    complex: myBlockStatus.data.complexName ?? complex.data.name,
                    reason: myBlockStatus.data.reason ?? t('dashboard.noReason'),
                    expiresAt: formatLocalDateTime(myBlockStatus.data.blockedUntil)
                  })
                : t('complexDetail.blockedMessage', {
                    complex: myBlockStatus.data.complexName ?? complex.data.name,
                    reason: myBlockStatus.data.reason ?? t('dashboard.noReason')
                  })}
            </Alert>
          )}
          {(complex.data.phoneNumber || complex.data.email) && (
            <Typography color="text.secondary" sx={{ mt: 1 }}>
              {complex.data.phoneNumber && <>{t('complexDetail.phoneWithValue', { value: complex.data.phoneNumber })}</>}
              {complex.data.phoneNumber && complex.data.email && t('common.formatSeparator')}
              {complex.data.email && <>{t('complexDetail.emailWithValue', { value: complex.data.email })}</>}
            </Typography>
          )}
        </Box>

        <Box>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems="center" justifyContent="space-between">
            <Typography component="h2" variant="h5" sx={{ fontWeight: 700 }}>
              {t('complexDetail.activeCourts')}
            </Typography>
            <FormControl sx={{ minWidth: 200 }}>
              <InputLabel id="sport-filter-label">{t('complexDetail.filterBySport')}</InputLabel>
              <Select
                labelId="sport-filter-label"
                value={selectedSportId}
                label={t('complexDetail.filterBySport')}
                onChange={(event) => setSelectedSportId(event.target.value)}
                disabled={sports.isLoading || sports.isError}
              >
                <MenuItem value="">{t('complexDetail.allSports')}</MenuItem>
                {sports.data?.map((sport) => (
                  <MenuItem key={sport.id} value={sport.id}>
                    {sport.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Stack>

          {sports.isError && <Alert severity="warning" sx={{ mt: 2 }}>{t('complexDetail.sportsError')}</Alert>}
          {courts.isError && <Alert severity="error" sx={{ mt: 2 }}>{t('complexDetail.courtsError')}</Alert>}
          {!courts.isError && (courts.data?.items ?? []).length === 0 && (
            <Alert severity="info" sx={{ mt: 2 }}>
              {t('complexDetail.noCourts')}
            </Alert>
          )}

          <Grid container spacing={{ xs: 1.5, sm: 3 }} sx={{ mt: 1 }}>
            {(courts.data?.items ?? []).map((court) => (
              <Grid key={court.id} size={{ xs: 12, sm: 6, md: 4 }}>
                <Card
                  variant="outlined"
                  component="button"
                  type="button"
                  onClick={() => setSelectedCourtId(court.id)}
                  aria-pressed={selectedCourtId === court.id}
                  sx={{
                    width: '100%',
                    textAlign: 'left',
                    font: 'inherit',
                    borderRadius: 3,
                    cursor: 'pointer',
                    borderColor: selectedCourtId === court.id ? 'primary.main' : 'divider',
                    '&:hover': { borderColor: 'primary.main' }
                  }}
                >
                  <CardContent>
                    <Typography component="h3" variant="h6" sx={{ fontWeight: 700 }}>
                      {court.name}
                    </Typography>
                    <Typography color="text.secondary">
                      {court.indoor ? t('common.indoor') : t('common.outdoor')}
                      {court.surfaceType ? `${t('common.formatSeparator')}${court.surfaceType}` : ''}
                    </Typography>
                    {court.description && <Typography sx={{ mt: 1 }}>{court.description}</Typography>}
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        </Box>

        <Box>
          <Typography component="h2" variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
            {t('complexDetail.checkAvailability')}
          </Typography>

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mb: 2 }}>
            <FormControl sx={{ minWidth: 240 }}>
              <InputLabel id="court-select-label">{t('complexDetail.courtLabel')}</InputLabel>
              <Select
                labelId="court-select-label"
                value={selectedCourtId}
                label={t('complexDetail.courtLabel')}
                onChange={(event) => setSelectedCourtId(event.target.value)}
              >
                <MenuItem value="">{t('complexDetail.selectCourt')}</MenuItem>
                {(courts.data?.items ?? []).map((court) => (
                  <MenuItem key={court.id} value={court.id}>
                    {court.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label={t('complexDetail.dateLabel')}
              type="date"
              value={selectedDate}
              onChange={(event) => setSelectedDate(event.target.value)}
              InputLabelProps={{ shrink: true }}
              inputProps={{ min: todayInTimeZone(complex.data?.timeZoneId) }}
              sx={{ minWidth: 160 }}
            />
          </Stack>
          <Stack direction="row" spacing={1} sx={{ overflowX: 'auto', pb: 1.25, mb: 1 }}>
            {dateOptions.map((date) => {
              const selected = date === selectedDate;
              return <Button key={date} variant={selected ? 'contained' : 'outlined'} onClick={() => setSelectedDate(date)} aria-pressed={selected} sx={{ flex: '0 0 auto', minWidth: 72, minHeight: 52, display: 'flex', flexDirection: 'column', lineHeight: 1.15 }}>
                <span>{new Intl.DateTimeFormat(undefined, { weekday: 'short' }).format(new Date(`${date}T12:00:00`))}</span>
                <strong>{new Intl.DateTimeFormat(undefined, { day: 'numeric', month: 'short' }).format(new Date(`${date}T12:00:00`))}</strong>
              </Button>;
            })}
          </Stack>

          {!selectedCourtId && (
            <Alert severity="info">{t('complexDetail.selectPrompt')}</Alert>
          )}

          {selectedCourtId && availability.isLoading && <Typography>{t('complexDetail.loadingAvailability')}</Typography>}
          {selectedCourtId && availability.isError && (
            <Alert severity="error">{t('complexDetail.availabilityError')}</Alert>
          )}
          {selectedCourtId && availability.data && availability.data.length === 0 && (
            <Alert severity="info">{t('complexDetail.noAvailability')}</Alert>
          )}

          {availability.data && availability.data.length > 0 && (
            <Grid container spacing={2}>
              {availability.data.map((slot, index) => (
                <Grid key={index} size={{ xs: 6, sm: 4, md: 3, lg: 2 }}>
                  <Card
                    variant="outlined"
                    component="button"
                    type="button"
                    onClick={() => setSelectedSlot(slot)}
                    aria-pressed={selectedSlot?.startAt === slot.startAt && selectedSlot.courtId === slot.courtId}
                    sx={{
                      width: '100%',
                      font: 'inherit',
                      textAlign: 'center',
                      borderRadius: 2,
                      cursor: 'pointer',
                      '&:hover': { borderColor: 'primary.main' }
                    }}
                  >
                    <CardContent>
                      <Typography variant="h6" sx={{ fontWeight: 700 }}>
                        {formatLocalTime(slot.startAt)}
                      </Typography>
                      <Typography color="text.secondary">
                        {t('complexDetail.availability.to', { time: formatLocalTime(slot.endAt) })}
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>
          )}

          {selectedSlot && (
            <BookingDialog
              slot={selectedSlot}
              courtName={courts.data?.items?.find((court) => court.id === selectedCourtId)?.name ?? ''}
              notes={bookingNotes}
              onNotesChange={setBookingNotes}
              isAuthenticated={isAuthenticated}
              isLoading={createMyReservation.isPending}
              error={createMyReservation.error}
              quote={priceQuote.data}
              quoteLoading={priceQuote.isLoading}
              quoteError={priceQuote.isError}
              onConfirm={async () => {
                const quote = priceQuote.data;
                if (!quote?.isFullyCovered) return;

                try {
                  await createMyReservation.mutateAsync({
                    courtId: selectedSlot.courtId,
                    startAt: selectedSlot.startAt,
                    endAt: selectedSlot.endAt,
                    notes: bookingNotes.trim() || undefined,
                    expectedTotalPrice: quote.totalAmount,
                    expectedCurrency: quote.currency
                  });
                  setSelectedSlot(null);
                  setBookingNotes('');
                } catch (err) {
                  if (err instanceof ApiError && err.code === 'PRICE_CHANGED') {
                    priceQuote.refetch();
                  }
                  // Error is surfaced through createMyReservation.error
                }
              }}
              onCancel={() => {
                setSelectedSlot(null);
                setBookingNotes('');
              }}
              t={t}
            />
          )}
        </Box>
      </Stack>
    </UserContentContainer>
  );
}
