import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Alert,
  Box,
  Paper,
  Skeleton,
  Stack,
  Typography
} from '@mui/material';
import type { CalendarCourtColumn, CalendarSlot, FreeCalendarSlot, ReservationCalendarSlot } from './reservationCalendarTypes';
import {
  formatTimeRange,
  getCalendarTimeRange,
  getSlotBackgroundColor,
  getSlotTextColor
} from './reservationCalendarUtils';

const PIXELS_PER_MINUTE = 1;
const HOUR_LABEL_HEIGHT = 28;

interface ReservationCalendarProps {
  columns: CalendarCourtColumn[];
  isLoading: boolean;
  isError: boolean;
  onFreeSlotClick: (slot: FreeCalendarSlot) => void;
  onReservationClick: (slot: ReservationCalendarSlot) => void;
}

function CalendarLegend({ t }: { t: (key: string) => string }) {
  const items = [
    { label: t('admin.reservations.legendFree'), color: 'success.main' },
    { label: t('status.confirmed'), color: 'primary.main' },
    { label: t('status.pending'), color: 'warning.main' },
    { label: t('status.completed'), color: 'info.main' },
    { label: t('status.noShow'), color: 'grey.500' }
  ];

  return (
    <Stack
      direction={{ xs: 'column', sm: 'row' }}
      spacing={{ xs: 1, sm: 2 }}
      alignItems={{ sm: 'center' }}
      flexWrap="wrap"
      sx={{ mb: 2 }}
    >
      <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
        {t('admin.reservations.legendTitle')}:
      </Typography>
      {items.map((item) => (
        <Stack key={item.label} direction="row" spacing={1} alignItems="center">
          <Box
            sx={{
              width: 16,
              height: 16,
              borderRadius: 1,
              bgcolor: item.color
            }}
          />
          <Typography variant="body2">{item.label}</Typography>
        </Stack>
      ))}
    </Stack>
  );
}

function CalendarSlotCard({
  slot,
  dayStart,
  t,
  onClick
}: {
  slot: CalendarSlot;
  dayStart: Date;
  t: (key: string) => string;
  onClick: () => void;
}) {
  const startMinutes = (new Date(slot.startAt).getTime() - dayStart.getTime()) / 60 / 1000;
  const durationMinutes = (new Date(slot.endAt).getTime() - new Date(slot.startAt).getTime()) / 60 / 1000;
  const top = startMinutes * PIXELS_PER_MINUTE;
  const height = durationMinutes * PIXELS_PER_MINUTE;

  return (
    <Paper
      elevation={0}
      onClick={onClick}
      sx={{
        position: 'absolute',
        top,
        left: 0,
        right: 0,
        height,
        minHeight: 24,
        bgcolor: getSlotBackgroundColor(slot),
        color: getSlotTextColor(slot),
        borderRadius: 1,
        border: 1,
        borderColor: 'background.paper',
        boxSizing: 'border-box',
        p: 0.5,
        cursor: 'pointer',
        overflow: 'hidden',
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        '&:hover': {
          filter: 'brightness(0.95)'
        }
      }}
    >
      <Typography variant="caption" fontWeight={700} lineHeight={1.2} noWrap>
        {slot.type === 'free'
          ? t('admin.reservations.legendFree')
          : slot.reservation.userName}
      </Typography>
      <Typography variant="caption" lineHeight={1.2} sx={{ opacity: 0.9 }} noWrap>
        {formatTimeRange(slot.startAt, slot.endAt)}
      </Typography>
    </Paper>
  );
}

function CourtColumn({
  column,
  dayStart,
  totalMinutes,
  t,
  onFreeSlotClick,
  onReservationClick
}: {
  column: CalendarCourtColumn;
  dayStart: Date | null;
  totalMinutes: number;
  t: (key: string) => string;
  onFreeSlotClick: (slot: FreeCalendarSlot) => void;
  onReservationClick: (slot: ReservationCalendarSlot) => void;
}) {
  return (
    <Box
      sx={{
        flex: '0 0 auto',
        minWidth: 200,
        width: { xs: '100%', sm: 220 }
      }}
    >
      <Typography
        variant="subtitle2"
        sx={{
          fontWeight: 700,
          height: HOUR_LABEL_HEIGHT,
          pb: 1,
          boxSizing: 'border-box',
          display: 'flex',
          alignItems: 'flex-end'
        }}
      >
        {column.court.name}
      </Typography>
      <Box
        sx={{
          position: 'relative',
          height: totalMinutes * PIXELS_PER_MINUTE,
          bgcolor: 'background.paper',
          border: 1,
          borderColor: 'divider',
          borderRadius: 1
        }}
      >
        {dayStart &&
          column.slots.map((slot, index) => (
            <CalendarSlotCard
              key={index}
              slot={slot}
              dayStart={dayStart}
              t={t}
              onClick={() =>
                slot.type === 'free'
                  ? onFreeSlotClick(slot)
                  : onReservationClick(slot)
              }
            />
          ))}
      </Box>
    </Box>
  );
}

export default function ReservationCalendar({
  columns,
  isLoading,
  isError,
  onFreeSlotClick,
  onReservationClick
}: ReservationCalendarProps) {
  const { t } = useTranslation();

  const { dayStart, totalMinutes } = useMemo(() => getCalendarTimeRange(columns), [columns]);
  const hasData = columns.length > 0 && columns.some((column) => column.slots.length > 0);

  if (isLoading) {
    return <Skeleton variant="rectangular" height={400} data-testid="calendar-skeleton" />;
  }

  if (isError) {
    return <Alert severity="error">{t('admin.reservations.error')}</Alert>;
  }

  if (!hasData) {
    return <Alert severity="info">{t('admin.reservations.calendarEmpty')}</Alert>;
  }

  return (
    <Box>
      <CalendarLegend t={t} />
      <Box
        sx={{
          display: 'flex',
          gap: 2,
          overflowX: 'auto',
          pb: 2
        }}
      >
        {columns.map((column) => (
          <CourtColumn
            key={column.court.id}
            column={column}
            dayStart={dayStart}
            totalMinutes={totalMinutes}
            t={t}
            onFreeSlotClick={onFreeSlotClick}
            onReservationClick={onReservationClick}
          />
        ))}
      </Box>
    </Box>
  );
}
