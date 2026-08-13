import { useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Paper,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useCancelMyReservation, useMyReservations } from '../features/reservations/reservationApi';
import { getReservationStatusKey } from '../features/reservations/reservationStatus';
import type { Reservation, UserReservationsFilters } from '../features/reservations/reservationTypes';

function formatLocalDateTime(isoString: string): string {
  const date = new Date(isoString);
  return `${date.toLocaleDateString()} ${date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
}

function canCancel(status: Reservation['status']): boolean {
  return status === 'Pending' || status === 'Confirmed';
}

export default function UserReservationsPage() {
  const { t } = useTranslation();
  const [filters, setFilters] = useState<UserReservationsFilters>({ page: 0, pageSize: 10 });
  const { data, isLoading, isError } = useMyReservations(filters);
  const cancelMyReservation = useCancelMyReservation();

  const [cancelDialog, setCancelDialog] = useState<{ open: boolean; reservation: Reservation | null }>({
    open: false,
    reservation: null
  });
  const [cancelReason, setCancelReason] = useState('');

  const handleChangePage = (_event: unknown, newPage: number) => {
    setFilters((prev) => ({ ...prev, page: newPage }));
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilters({ page: 0, pageSize: parseInt(event.target.value, 10) });
  };

  const handleCancelDialogClose = () => {
    cancelMyReservation.reset();
    setCancelDialog({ open: false, reservation: null });
    setCancelReason('');
  };

  const handleCancelSubmit = async () => {
    if (!cancelDialog.reservation) return;

    try {
      await cancelMyReservation.mutateAsync({
        reservationId: cancelDialog.reservation.id,
        request: { reason: cancelReason || undefined }
      });
      handleCancelDialogClose();
    } catch {
      // The mutation exposes the error state rendered in the dialog.
    }
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
          <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
            {t('dashboard.upcomingTitle')}
          </Typography>
          <Button component={RouterLink} to="/complexes" variant="contained">
            {t('dashboard.newReservation')}
          </Button>
        </Stack>

        {isError && <Alert severity="error">{t('dashboard.reservationsError')}</Alert>}

        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>{t('common.court')}</TableCell>
                <TableCell>{t('common.start')}</TableCell>
                <TableCell>{t('common.end')}</TableCell>
                <TableCell>{t('common.status')}</TableCell>
                <TableCell align="right">{t('common.actions')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {isLoading && (
                <TableRow>
                  <TableCell colSpan={5}>
                    <Skeleton variant="rectangular" height={120} />
                  </TableCell>
                </TableRow>
              )}
              {!isLoading && !isError && data?.items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5}>
                    <Alert severity="info">{t('dashboard.noUpcomingReservations')}</Alert>
                  </TableCell>
                </TableRow>
              )}
              {!isLoading &&
                !isError &&
                data?.items.map((reservation) => (
                  <TableRow key={reservation.id}>
                    <TableCell>{reservation.courtName}</TableCell>
                    <TableCell>{formatLocalDateTime(reservation.startAt)}</TableCell>
                    <TableCell>{formatLocalDateTime(reservation.endAt)}</TableCell>
                    <TableCell>{t(`status.${getReservationStatusKey(reservation.status)}`)}</TableCell>
                    <TableCell align="right">
                      <Button
                        size="small"
                        variant="outlined"
                        disabled={!canCancel(reservation.status) || cancelMyReservation.isPending}
                        onClick={() => {
                          cancelMyReservation.reset();
                          setCancelDialog({ open: true, reservation });
                        }}
                      >
                        {t('common.cancel')}
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
            </TableBody>
          </Table>
        </TableContainer>

        {!isLoading && !isError && data && data.totalItems > 0 && (
          <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
            <TablePagination
              component="div"
              count={data.totalItems}
              page={filters.page}
              onPageChange={handleChangePage}
              rowsPerPage={filters.pageSize}
              onRowsPerPageChange={handleChangeRowsPerPage}
              rowsPerPageOptions={[5, 10, 25]}
            />
          </Box>
        )}
      </Stack>

      <Dialog open={cancelDialog.open} onClose={handleCancelDialogClose} fullWidth maxWidth="sm">
        <DialogTitle>{t('admin.reservations.cancelDialogTitle')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            {cancelMyReservation.isError && (
              <Alert severity="error">{cancelMyReservation.error.message}</Alert>
            )}
            <Typography variant="body1">{t('admin.reservations.cancelConfirm')}</Typography>
            <TextField
              label={t('admin.reservations.reasonLabel')}
              value={cancelReason}
              onChange={(event) => setCancelReason(event.target.value)}
              fullWidth
              multiline
              rows={2}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCancelDialogClose}>{t('common.cancel')}</Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleCancelSubmit}
            disabled={cancelMyReservation.isPending}
          >
            {t('common.confirm')}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}
