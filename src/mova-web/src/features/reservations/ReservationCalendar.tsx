import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Alert, Box, Skeleton, Stack, Typography } from '@mui/material';
import type { CalendarCourtColumn, CalendarSlot, FreeCalendarSlot, ReservationCalendarSlot } from './reservationCalendarTypes';
import { formatTimeRange, getCalendarTimeRange } from './reservationCalendarUtils';

const PIXELS_PER_MINUTE = 1;
const TIME_AXIS_WIDTH = 58;
const COURT_MIN_WIDTH = 220;
const HEADER_HEIGHT = 48;

interface ReservationCalendarProps {
  columns: CalendarCourtColumn[];
  isLoading: boolean;
  isError: boolean;
  onFreeSlotClick: (slot: FreeCalendarSlot) => void;
  onReservationClick: (slot: ReservationCalendarSlot) => void;
  timeZoneId?: string;
}

function reservationColors(slot: ReservationCalendarSlot) {
  switch (slot.reservation.status) {
    case 'Confirmed': return { background: '#DBEAFE', border: '#2563EB', text: '#1E3A8A' };
    case 'Pending': return { background: '#FEF3C7', border: '#F59E0B', text: '#92400E' };
    case 'Completed': return { background: '#DDF4FF', border: '#0284C7', text: '#075985' };
    case 'NoShow': return { background: '#E2E8F0', border: '#64748B', text: '#334155' };
    default: return { background: '#FEE2E2', border: '#DC2626', text: '#991B1B' };
  }
}

function CalendarLegend({ t }: { t: (key: string) => string }) {
  const items = [
    { label: t('admin.reservations.legendFree'), background: '#F8FAFC', border: '#86EFAC' },
    { label: t('status.confirmed'), background: '#DBEAFE', border: '#2563EB' },
    { label: t('status.pending'), background: '#FEF3C7', border: '#F59E0B' },
    { label: t('status.completed'), background: '#DDF4FF', border: '#0284C7' }
  ];
  return <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap sx={{ mb: 2 }}>
    {items.map((item) => <Stack key={item.label} direction="row" spacing={0.75} alignItems="center">
      <Box sx={{ width: 12, height: 12, borderRadius: 0.75, bgcolor: item.background, borderLeft: '3px solid', borderColor: item.border }} />
      <Typography variant="caption" color="text.secondary">{item.label}</Typography>
    </Stack>)}
  </Stack>;
}

function CalendarSlotBlock({ slot, dayStart, t, onClick, timeZoneId }: { slot: CalendarSlot; dayStart: Date; t: (key: string) => string; onClick: () => void; timeZoneId?: string }) {
  const top = (new Date(slot.startAt).getTime() - dayStart.getTime()) / 60000 * PIXELS_PER_MINUTE;
  const height = (new Date(slot.endAt).getTime() - new Date(slot.startAt).getTime()) / 60000 * PIXELS_PER_MINUTE;
  const free = slot.type === 'free';
  const colors = free ? null : reservationColors(slot);

  return <Box
    component="button"
    type="button"
    onClick={onClick}
    aria-label={free
      ? `${t('admin.reservations.legendFree')}: ${formatTimeRange(slot.startAt, slot.endAt, timeZoneId)}`
      : `${slot.reservation.userName}: ${formatTimeRange(slot.startAt, slot.endAt, timeZoneId)}`}
    sx={{
      position: 'absolute', top, left: 3, right: 3, height: Math.max(height, 24), zIndex: 1,
      border: free ? '1px solid transparent' : '1px solid',
      borderLeft: free ? '1px solid transparent' : '4px solid',
      borderColor: free ? 'transparent' : colors!.border,
      borderRadius: 1, p: free ? 0 : 0.75, cursor: 'pointer', textAlign: 'left', font: 'inherit',
      overflow: 'hidden', color: free ? 'success.dark' : colors!.text, bgcolor: free ? 'transparent' : colors!.background,
      display: 'flex', flexDirection: 'column', justifyContent: 'center',
      '&:hover': free
        ? { bgcolor: '#F0FDF4', borderColor: '#86EFAC', '& .calendar-add': { opacity: 1 } }
        : { filter: 'brightness(0.97)', boxShadow: '0 2px 6px rgba(23,43,77,0.14)' },
      '&:focus-visible': { outline: '2px solid', outlineColor: free ? 'success.main' : colors!.border, outlineOffset: '-2px' }
    }}
  >
    {free ? <Typography className="calendar-add" aria-hidden="true" sx={{ opacity: 0, textAlign: 'center', fontSize: 18, lineHeight: 1 }}>+</Typography> : <>
      <Typography variant="caption" fontWeight={800} lineHeight={1.2} noWrap>{slot.reservation.userName}</Typography>
      <Typography variant="caption" lineHeight={1.2} noWrap>{formatTimeRange(slot.startAt, slot.endAt, timeZoneId)}</Typography>
      {height >= 48 && <Typography variant="caption" lineHeight={1.2} sx={{ opacity: 0.8 }} noWrap>{slot.reservation.status}</Typography>}
    </>}
  </Box>;
}

function isToday(dayStart: Date, timeZoneId?: string) {
  try {
    const formatter = new Intl.DateTimeFormat('en-CA', { timeZone: timeZoneId, year: 'numeric', month: '2-digit', day: '2-digit' });
    return formatter.format(dayStart) === formatter.format(new Date());
  } catch { return false; }
}

export default function ReservationCalendar({ columns, isLoading, isError, onFreeSlotClick, onReservationClick, timeZoneId }: ReservationCalendarProps) {
  const { t } = useTranslation();
  const { dayStart, totalMinutes } = useMemo(() => getCalendarTimeRange(columns), [columns]);
  const hasData = columns.length > 0 && dayStart !== null;
  const hourLabels = useMemo(() => {
    if (!dayStart) return [];
    return Array.from({ length: Math.ceil(totalMinutes / 60) + 1 }, (_, index) => ({
      top: index * 60,
      label: new Intl.DateTimeFormat(undefined, { hour: '2-digit', minute: '2-digit', timeZone: timeZoneId }).format(new Date(dayStart.getTime() + index * 3600000))
    }));
  }, [dayStart, timeZoneId, totalMinutes]);
  const nowTop = dayStart && isToday(dayStart, timeZoneId)
    ? (Date.now() - dayStart.getTime()) / 60000 * PIXELS_PER_MINUTE
    : null;

  if (isLoading) return <Skeleton variant="rectangular" height={400} data-testid="calendar-skeleton" />;
  if (isError) return <Alert severity="error">{t('admin.reservations.error')}</Alert>;
  if (!hasData) return <Alert severity="info">{t('admin.reservations.calendarEmpty')}</Alert>;

  const gridColumns = ` ${TIME_AXIS_WIDTH}px repeat(${columns.length}, minmax(${COURT_MIN_WIDTH}px, 1fr))`;
  const minimumWidth = TIME_AXIS_WIDTH + columns.length * COURT_MIN_WIDTH;

  return <Box>
    <CalendarLegend t={t} />
    <Box sx={{ overflow: 'auto', maxHeight: 640, border: 1, borderColor: 'divider', borderRadius: 2 }}>
      <Box sx={{ display: 'grid', gridTemplateColumns: gridColumns, minWidth: columns.length === 1 ? 420 : minimumWidth, maxWidth: columns.length === 1 ? 560 : 'none' }}>
      <Box sx={{ height: HEADER_HEIGHT, position: 'sticky', top: 0, zIndex: 4, bgcolor: 'background.paper', borderBottom: 1, borderColor: 'divider' }} />
      {columns.map((column) => <Box key={column.court.id} sx={{ height: HEADER_HEIGHT, position: 'sticky', top: 0, zIndex: 4, display: 'flex', alignItems: 'center', px: 1.5, bgcolor: 'background.paper', borderBottom: 1, borderLeft: 1, borderColor: 'divider' }}>
        <Typography variant="subtitle2" fontWeight={800} noWrap>{column.court.name}</Typography>
      </Box>)}

      <Box sx={{ position: 'relative', height: totalMinutes * PIXELS_PER_MINUTE, bgcolor: 'background.paper', borderRight: 1, borderColor: 'divider' }}>
        {hourLabels.map((item) => <Typography key={item.top} variant="caption" color="text.secondary" sx={{ position: 'absolute', top: item.top - 8, right: 8, fontVariantNumeric: 'tabular-nums' }}>{item.label}</Typography>)}
      </Box>
      {columns.map((column) => <Box key={column.court.id} sx={{
        position: 'relative', height: totalMinutes * PIXELS_PER_MINUTE, bgcolor: 'background.paper', borderLeft: 1, borderColor: 'divider',
        backgroundImage: 'repeating-linear-gradient(to bottom, transparent 0, transparent 59px, #E2E8F0 60px)'
      }}>
        {column.slots.map((slot, index) => <CalendarSlotBlock key={`${slot.type}-${slot.startAt}-${index}`} slot={slot} dayStart={dayStart} t={t} timeZoneId={timeZoneId} onClick={() => slot.type === 'free' ? onFreeSlotClick(slot) : onReservationClick(slot)} />)}
        {nowTop !== null && nowTop >= 0 && nowTop <= totalMinutes && <Box aria-label={t('admin.dashboard.currentTime')} sx={{ position: 'absolute', top: nowTop, left: 0, right: 0, zIndex: 3, borderTop: '2px solid', borderColor: 'error.main', pointerEvents: 'none' }} />}
      </Box>)}
      </Box>
    </Box>
  </Box>;
}
