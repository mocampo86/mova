import { useEffect, useState } from 'react';
import { Link as RouterLink, useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Container,
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
import {
  useActiveComplex,
  useActiveCourts,
  useCourtAvailability,
  useSports
} from '../features/complexes/complexApi';
import type { CourtAvailabilitySlot } from '../features/complexes/complexTypes';
import { useAuth } from '../features/auth/useAuth';
import { useCreateMyReservation } from '../features/reservations/reservationApi';
import { todayInTimeZone } from '../utils/timezones';

function formatLocalTime(isoString: string) {
  const date = new Date(isoString);
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function formatLocalDate(isoString: string) {
  const date = new Date(isoString);
  return date.toLocaleDateString();
}



interface BookingDialogProps {
  slot: CourtAvailabilitySlot;
  courtName: string;
  notes: string;
  onNotesChange: (notes: string) => void;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: Error | null;
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
  onConfirm,
  onCancel,
  t
}: BookingDialogProps) {
  return (
    <Dialog open onClose={onCancel} maxWidth="sm" fullWidth>
      <DialogTitle>{t('complexDetail.booking.title')}</DialogTitle>
      <DialogContent>
        <Stack spacing={2}>
          {!isAuthenticated && (
            <Alert severity="info">{t('complexDetail.booking.loginPrompt')}</Alert>
          )}
          {error && <Alert severity="error">{error.message}</Alert>}
          <Typography>
            {t('complexDetail.booking.message', {
              court: courtName,
              date: formatLocalDate(slot.startAt),
              start: formatLocalTime(slot.startAt),
              end: formatLocalTime(slot.endAt)
            })}
          </Typography>
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
          <Button onClick={onConfirm} variant="contained" disabled={isLoading}>
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
  const courts = useActiveCourts(complexId, selectedSportId || undefined);
  const sports = useSports();
  const createMyReservation = useCreateMyReservation(complexId);

  useEffect(() => {
    if (complex.data) {
      setSelectedDate(todayInTimeZone(complex.data.timeZoneId));
    }
  }, [complex.data]);

  const availability = useCourtAvailability(complexId, selectedCourtId, selectedDate);

  useEffect(() => {
    setSelectedCourtId('');
  }, [selectedSportId]);

  if (complex.isLoading || courts.isLoading) {
    return (
      <Container sx={{ py: 6 }}>
        <Typography>{t('complexDetail.loading')}</Typography>
      </Container>
    );
  }

  if (complex.isError || !complex.data) {
    return (
      <Container sx={{ py: 6 }}>
        <Alert severity="error">{t('complexDetail.notFound')}</Alert>
      </Container>
    );
  }

  return (
    <Container component="main" maxWidth="lg" sx={{ py: 6 }}>
      <Stack spacing={4}>
        <Button component={RouterLink} to="/complexes" sx={{ alignSelf: 'flex-start' }}>
          {t('common.back')}
        </Button>

        <Box>
          <Typography component="h1" variant="h3" sx={{ fontWeight: 800 }}>
            {complex.data.name}
          </Typography>
          <Typography color="text.secondary">
            {complex.data.city}{t('common.formatSeparator')}{complex.data.address}
          </Typography>
          <Typography sx={{ mt: 2 }}>{complex.data.description}</Typography>
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

          <Grid container spacing={3} sx={{ mt: 1 }}>
            {(courts.data?.items ?? []).map((court) => (
              <Grid key={court.id} size={{ xs: 12, sm: 6, md: 4 }}>
                <Card
                  variant="outlined"
                  onClick={() => setSelectedCourtId(court.id)}
                  sx={{
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

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mb: 3 }}>
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
                    onClick={() => setSelectedSlot(slot)}
                    sx={{
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
              onConfirm={async () => {
                await createMyReservation.mutateAsync({
                  courtId: selectedSlot.courtId,
                  startAt: selectedSlot.startAt,
                  endAt: selectedSlot.endAt,
                  notes: bookingNotes.trim() || undefined
                });
                setSelectedSlot(null);
                setBookingNotes('');
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
    </Container>
  );
}
