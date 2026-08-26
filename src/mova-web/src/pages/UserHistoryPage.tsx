import { useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Container,
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
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useMyReservationHistory } from '../features/reservations/reservationApi';
import { getReservationStatusKey, isCancelledStatus } from '../features/reservations/reservationStatus';
import type { UserReservationsFilters } from '../features/reservations/reservationTypes';

function formatLocalDateTime(isoString: string): string {
  const date = new Date(isoString);
  return `${date.toLocaleDateString()} ${date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
}

export default function UserHistoryPage() {
  const { t } = useTranslation();
  const [filters, setFilters] = useState<UserReservationsFilters>({ page: 0, pageSize: 10 });
  const { data, isLoading, isError } = useMyReservationHistory(filters);

  const handleChangePage = (_event: unknown, newPage: number) => {
    setFilters((prev) => ({ ...prev, page: newPage }));
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilters({ page: 0, pageSize: parseInt(event.target.value, 10) });
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={3}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
          <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
            {t('dashboard.historyTitle')}
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
                <TableCell>{t('dashboard.historyDetailsHeader')}</TableCell>
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
                    <Alert severity="info">{t('dashboard.noHistory')}</Alert>
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
                    <TableCell>
                      {isCancelledStatus(reservation.status) && (
                        <Stack spacing={0.5}>
                          <Typography variant="body2">
                            {t('dashboard.cancelledBy', {
                              name: reservation.cancelledByUserName ?? t('common.emptyValue')
                            })}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {t('dashboard.cancellationReason', {
                              reason: reservation.cancellationReason ?? t('dashboard.noCancellationReason')
                            })}
                          </Typography>
                        </Stack>
                      )}
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
    </Container>
  );
}
