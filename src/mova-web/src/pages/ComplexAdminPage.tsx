import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Alert, Box, Button, Card, CardActionArea, CardContent, Chip, Container, Grid, Paper, Skeleton, Stack, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useAdminComplex, useComplexDashboard } from '../features/complexes/complexApi';
import type { CourtAvailabilitySlot } from '../features/complexes/complexTypes';
import { useCourts } from '../features/courts/courtApi';
import type { CourtListFilters } from '../features/courts/courtTypes';
import ReservationCalendar from '../features/reservations/ReservationCalendar';
import { useReservations } from '../features/reservations/reservationApi';
import { useCourtAvailabilityForCourts } from '../features/reservations/reservationCalendarApi';
import type { FreeCalendarSlot, ReservationCalendarSlot } from '../features/reservations/reservationCalendarTypes';
import { buildCalendarColumns } from '../features/reservations/reservationCalendarUtils';
import { getReservationStatusColor, isCancelledStatus } from '../features/reservations/reservationStatus';
import type { Reservation, ReservationListFilters } from '../features/reservations/reservationTypes';
import { todayInTimeZone } from '../utils/timezones';

const courtFilters: CourtListFilters = { page: 0, pageSize: 100, status: 'All', sportId: '', search: '' };
const minutes = (reservation: Reservation) => Math.max(0, (new Date(reservation.endAt).getTime() - new Date(reservation.startAt).getTime()) / 60000);

function MetricCard({ title, value, detail, loading }: { title: string; value: string | number; detail: string; loading: boolean }) {
  return <Card variant="outlined" sx={{ height: '100%', borderColor: 'divider' }}><CardActionArea sx={{ height: '100%', p: 0.5 }}>
    <CardContent><Typography color="text.secondary" variant="body2" fontWeight={700}>{title}</Typography>
      {loading ? <Skeleton width="48%" height={48} /> : <Typography variant="h4" sx={{ mt: 1 }}>{value}</Typography>}
      <Typography color="text.secondary" variant="caption">{detail}</Typography>
    </CardContent>
  </CardActionArea></Card>;
}

function ArrowIcon({ direction }: { direction: 'left' | 'right' }) {
  return <Box component="svg" viewBox="0 0 24 24" aria-hidden="true" sx={{ width: 18, height: 18, display: 'block', fill: 'none', stroke: 'currentColor', strokeWidth: 2.25 }}>
    <path d={direction === 'left' ? 'm14 5-7 7 7 7' : 'm10 5 7 7-7 7'} />
  </Box>;
}

export default function ComplexAdminPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { complexId = '' } = useParams();
  const dashboard = useComplexDashboard(complexId);
  const complex = useAdminComplex(complexId);
  const [date, setDate] = useState(() => todayInTimeZone(complex.data?.timeZoneId));
  const reservations = useReservations(complexId, { page: 0, pageSize: 100, courtId: '', status: 'All', date, sort: 'startAt:asc' } satisfies ReservationListFilters);
  const courts = useCourts(complexId, courtFilters);
  const activeCourts = useMemo(() => courts.data?.items.filter((court) => court.status === 'Active') ?? [], [courts.data]);
  const availability = useCourtAvailabilityForCourts(complexId, date, activeCourts.map((court) => court.id), Boolean(complex.data));

  useEffect(() => { if (complex.data?.timeZoneId) setDate(todayInTimeZone(complex.data.timeZoneId)); }, [complex.data?.timeZoneId]);

  const dailyReservations = useMemo(() => reservations.data?.items ?? [], [reservations.data?.items]);
  const columns = useMemo(() => buildCalendarColumns(activeCourts, dailyReservations, availability.data), [activeCourts, availability.data, dailyReservations]);
  const activeReservations = dailyReservations.filter((item) => !isCancelledStatus(item.status));
  const bookedMinutes = activeReservations.reduce((total, item) => total + minutes(item), 0);
  const freeMinutes = Object.values(availability.data as Record<string, CourtAvailabilitySlot[]>).flat().reduce((total, slot) => total + (new Date(slot.endAt).getTime() - new Date(slot.startAt).getTime()) / 60000, 0);
  const occupancy = freeMinutes + bookedMinutes > 0 ? Math.round(bookedMinutes * 100 / (freeMinutes + bookedMinutes)) : null;
  const totals = dashboard.data?.reservationsToday;
  const changeDate = (days: number) => {
    const next = new Date(`${date}T12:00:00Z`);
    next.setUTCDate(next.getUTCDate() + days);
    setDate(next.toISOString().slice(0, 10));
  };
  const openFreeSlot = (slot: FreeCalendarSlot) => navigate(`/admin/complex/${complexId}/reservations`, { state: { selectedFreeSlot: slot } });
  const openReservation = (slot: ReservationCalendarSlot) => navigate(`/admin/complex/${complexId}/reservations`, { state: { selectedReservation: slot.reservation } });
  const upcoming = activeReservations.filter((item) => new Date(item.endAt) >= new Date()).slice(0, 5);

  if (dashboard.isError || complex.isError) return <Container maxWidth="xl" sx={{ py: 3 }}><Alert severity="error">{t('admin.dashboard.error')}</Alert></Container>;

  return <Container maxWidth="xl" sx={{ py: { xs: 2, md: 3 } }}><Stack spacing={3}>
    <Box><Typography component="h1" variant="h4">{dashboard.data?.complex.name ?? <Skeleton width={230} />}</Typography><Typography color="text.secondary" sx={{ mt: 0.5 }}>{t('admin.dashboard.overview')}</Typography></Box>
    <Grid container spacing={2}>
      <Grid size={{ xs: 12, sm: 6, lg: 3 }}><MetricCard title={t('admin.dashboard.bookingsToday')} value={dailyReservations.length} detail={t('admin.dashboard.bookingsDetail')} loading={reservations.isLoading} /></Grid>
      <Grid size={{ xs: 12, sm: 6, lg: 3 }}><MetricCard title={t('admin.dashboard.activeCourts')} value={`${dashboard.data?.courts.active ?? 0}/${(dashboard.data?.courts.active ?? 0) + (dashboard.data?.courts.inactive ?? 0)}`} detail={t('admin.dashboard.courtsDetail')} loading={dashboard.isLoading} /></Grid>
      <Grid size={{ xs: 12, sm: 6, lg: 3 }}><MetricCard title={t('admin.dashboard.occupancyToday')} value={occupancy === null ? '-' : `${occupancy}%`} detail={t('admin.dashboard.occupancyDetail')} loading={availability.isLoading || reservations.isLoading} /></Grid>
      <Grid size={{ xs: 12, sm: 6, lg: 3 }}><MetricCard title={t('admin.dashboard.bookedHours')} value={`${(bookedMinutes / 60).toFixed(1)} h`} detail={t('admin.dashboard.bookedHoursDetail')} loading={reservations.isLoading} /></Grid>
    </Grid>
    <Paper component="section" variant="outlined" sx={{ p: { xs: 1.5, sm: 2.5 }, borderColor: 'divider' }}>
      <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ sm: 'center' }} spacing={1.5} sx={{ mb: 2 }}>
        <Box><Typography component="h2" variant="h5">{t('admin.dashboard.agenda')}</Typography><Typography color="text.secondary" variant="body2">{new Intl.DateTimeFormat(undefined, { weekday: 'long', month: 'long', day: 'numeric', timeZone: complex.data?.timeZoneId }).format(new Date(`${date}T12:00:00Z`))}</Typography></Box>
        <Stack direction="row" spacing={1}><Button variant="outlined" onClick={() => changeDate(-1)} aria-label={t('admin.dashboard.previousDay')} sx={{ minWidth: 40, px: 1 }}><ArrowIcon direction="left" /></Button><Button variant="outlined" onClick={() => setDate(todayInTimeZone(complex.data?.timeZoneId))}>{t('admin.dashboard.today')}</Button><Button variant="outlined" onClick={() => changeDate(1)} aria-label={t('admin.dashboard.nextDay')} sx={{ minWidth: 40, px: 1 }}><ArrowIcon direction="right" /></Button></Stack>
      </Stack>
      <ReservationCalendar columns={columns} isLoading={courts.isLoading || reservations.isLoading || availability.isLoading} isError={courts.isError || reservations.isError || availability.isError} onFreeSlotClick={openFreeSlot} onReservationClick={openReservation} timeZoneId={complex.data?.timeZoneId} />
    </Paper>
    <Grid container spacing={2}>
      <Grid size={{ xs: 12, lg: 5 }}><Paper component="section" variant="outlined" sx={{ p: 2.5, height: '100%', borderColor: 'divider' }}><Typography component="h2" variant="h6" sx={{ mb: 2 }}>{t('admin.dashboard.activity')}</Typography><Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap><Chip label={`${totals?.confirmed ?? 0} ${t('status.confirmed')}`} color="primary" /><Chip label={`${totals?.completed ?? 0} ${t('status.completed')}`} color="info" /><Chip label={`${totals?.cancelled ?? 0} ${t('admin.dashboard.cancelled')}`} color="error" /></Stack></Paper></Grid>
      <Grid size={{ xs: 12, lg: 7 }}><Paper component="section" variant="outlined" sx={{ p: 2.5, borderColor: 'divider' }}><Typography component="h2" variant="h6" sx={{ mb: 1 }}>{t('admin.dashboard.upcoming')}</Typography>{upcoming.length === 0 ? <Typography color="text.secondary">{t('admin.dashboard.noUpcoming')}</Typography> : <Stack divider={<Box sx={{ borderBottom: 1, borderColor: 'divider' }} />}>{upcoming.map((item) => <Button key={item.id} color="inherit" onClick={() => openReservation({ type: 'reservation', courtId: item.courtId, courtName: item.courtName, startAt: item.startAt, endAt: item.endAt, reservation: item, status: item.status })} sx={{ justifyContent: 'space-between', py: 1.25, textAlign: 'left' }}><Box><Typography fontWeight={700}>{new Date(item.startAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', timeZone: complex.data?.timeZoneId })} - {item.courtName}</Typography><Typography variant="body2" color="text.secondary">{item.userName} · {minutes(item)} min</Typography></Box><Chip label={t(`status.${item.status.charAt(0).toLowerCase()}${item.status.slice(1)}`)} color={getReservationStatusColor(item.status)} size="small" /></Button>)}</Stack>}</Paper></Grid>
    </Grid>
  </Stack></Container>;
}
