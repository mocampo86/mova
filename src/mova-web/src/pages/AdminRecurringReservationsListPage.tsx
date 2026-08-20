import { useState } from 'react';
import { Link as RouterLink, useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Chip,
  Container,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
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
import { useCourts } from '../features/courts/courtApi';
import { useComplexUsers } from '../features/users/userAdminApi';
import { useRecurringReservations } from '../features/reservations/reservationApi';
import type { RecurringReservationListFilters, RecurringReservationListItem } from '../features/reservations/reservationTypes';
import RecurringReservationCancelDialog from '../features/reservations/RecurringReservationCancelDialog';

function formatDate(date: string): string {
  return new Date(date).toLocaleDateString();
}

function getRecurringStatusColor(status: string): 'success' | 'error' | 'default' {
  if (status === 'Active') return 'success';
  if (status === 'Cancelled') return 'error';
  return 'default';
}

const statusOptions: { value: string; labelKey: string }[] = [
  { value: 'All', labelKey: 'status.all' },
  { value: 'Active', labelKey: 'status.active' },
  { value: 'Cancelled', labelKey: 'status.cancelled' }
];

const sortOptions: { value: string; labelKey: string }[] = [
  { value: 'createdAt:desc', labelKey: 'admin.recurringList.sortCreatedAtDesc' },
  { value: 'createdAt:asc', labelKey: 'admin.recurringList.sortCreatedAtAsc' },
  { value: 'startDate:asc', labelKey: 'admin.recurringList.sortStartDateAsc' },
  { value: 'startDate:desc', labelKey: 'admin.recurringList.sortStartDateDesc' },
  { value: 'courtName:asc', labelKey: 'admin.recurringList.sortCourtNameAsc' },
  { value: 'userName:asc', labelKey: 'admin.recurringList.sortUserNameAsc' }
];

export default function AdminRecurringReservationsListPage() {
  const { t } = useTranslation();
  const { complexId = '' } = useParams();
  const [filters, setFilters] = useState<RecurringReservationListFilters>({
    page: 0,
    pageSize: 10,
    status: 'All',
    sort: 'createdAt:desc',
    courtId: undefined
  });
  const [cancelDialog, setCancelDialog] = useState<RecurringReservationListItem | null>(null);

  const recurringReservations = useRecurringReservations(complexId, filters);
  const courts = useCourts(complexId, { page: 0, pageSize: 100, status: 'All', sportId: '', search: '' });
  const users = useComplexUsers(complexId, { page: 0, pageSize: 100, search: '', sort: 'fullName:asc' });

  const handleChangePage = (_event: unknown, newPage: number) => {
    setFilters((prev) => ({ ...prev, page: newPage }));
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilters({ ...filters, page: 0, pageSize: parseInt(event.target.value, 10) });
  };

  const canCancel = (item: RecurringReservationListItem) => item.status === 'Active';

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={3}>
        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          spacing={2}
          alignItems={{ sm: 'center' }}
          justifyContent="space-between"
        >
          <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
            {t('admin.recurringList.title')}
          </Typography>
          <Button component={RouterLink} to={`/admin/complex/${complexId}/recurring/new`} variant="contained">
            {t('admin.recurringList.newRecurringBooking')}
          </Button>
        </Stack>

        {recurringReservations.isError && <Alert severity="error">{recurringReservations.error.message}</Alert>}

        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel id="recurring-court-filter-label">{t('admin.recurringList.courtFilter')}</InputLabel>
            <Select
              labelId="recurring-court-filter-label"
              value={filters.courtId ?? ''}
              label={t('admin.recurringList.courtFilter')}
              onChange={(event) => setFilters((prev) => ({ ...prev, courtId: event.target.value || undefined, page: 0 }))}
              disabled={courts.isLoading}
            >
              <MenuItem value="">{t('admin.recurringList.allCourts')}</MenuItem>
              {(courts.data?.items ?? []).map((court) => (
                <MenuItem key={court.id} value={court.id}>
                  {court.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel id="recurring-user-filter-label">{t('admin.recurringList.userFilter')}</InputLabel>
            <Select
              labelId="recurring-user-filter-label"
              value={filters.userId ?? ''}
              label={t('admin.recurringList.userFilter')}
              onChange={(event) => setFilters((prev) => ({ ...prev, userId: event.target.value || undefined, page: 0 }))}
              disabled={users.isLoading}
            >
              <MenuItem value="">{t('admin.recurringList.allUsers')}</MenuItem>
              {(users.data?.items ?? []).map((user) => (
                <MenuItem key={user.id} value={user.id}>
                  {user.fullName} ({user.email})
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl sx={{ minWidth: 160 }}>
            <InputLabel id="recurring-status-filter-label">{t('admin.reservations.statusFilter')}</InputLabel>
            <Select
              labelId="recurring-status-filter-label"
              value={filters.status ?? 'All'}
              label={t('admin.reservations.statusFilter')}
              onChange={(event) => setFilters((prev) => ({ ...prev, status: event.target.value, page: 0 }))}
            >
              {statusOptions.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {t(option.labelKey)}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl sx={{ minWidth: 220 }}>
            <InputLabel id="recurring-sort-label">{t('common.sort')}</InputLabel>
            <Select
              labelId="recurring-sort-label"
              value={filters.sort ?? 'createdAt:desc'}
              label={t('common.sort')}
              onChange={(event) => setFilters((prev) => ({ ...prev, sort: event.target.value, page: 0 }))}
            >
              {sortOptions.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {t(option.labelKey)}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Stack>

        {recurringReservations.isLoading ? (
          <Skeleton variant="rectangular" height={200} />
        ) : (
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>{t('admin.recurringList.day')}</TableCell>
                  <TableCell>{t('admin.recurringList.time')}</TableCell>
                  <TableCell>{t('admin.recurringList.duration')}</TableCell>
                  <TableCell>{t('common.court')}</TableCell>
                  <TableCell>{t('common.user')}</TableCell>
                  <TableCell>{t('admin.recurringList.period')}</TableCell>
                  <TableCell>{t('common.status')}</TableCell>
                  <TableCell align="right">{t('common.actions')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {recurringReservations.data?.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={8}>
                      <Alert severity="info">{t('admin.recurringList.empty')}</Alert>
                    </TableCell>
                  </TableRow>
                )}
                {recurringReservations.data?.items.map((item) => (
                  <TableRow key={item.id} hover>
                    <TableCell>{t(`days.${item.dayOfWeek}`)}</TableCell>
                    <TableCell>{item.startTime.slice(0, 5)}</TableCell>
                    <TableCell>{item.durationMinutes} {t('common.minutes')}</TableCell>
                    <TableCell>{item.courtName}</TableCell>
                    <TableCell>{item.userName}</TableCell>
                    <TableCell>{formatDate(item.startDate)} - {formatDate(item.endDate)}</TableCell>
                    <TableCell>
                      <Chip
                        label={t(`status.${item.status.toLowerCase()}`)}
                        color={getRecurringStatusColor(item.status)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="right">
                      <Button
                        size="small"
                        variant="outlined"
                        color="error"
                        disabled={!canCancel(item)}
                        onClick={() => setCancelDialog(item)}
                      >
                        {t('admin.recurringList.cancelSeries')}
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}

        {!recurringReservations.isLoading && !recurringReservations.isError && recurringReservations.data && (
          <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
            <TablePagination
              component="div"
              count={recurringReservations.data.totalItems}
              page={filters.page}
              onPageChange={handleChangePage}
              rowsPerPage={filters.pageSize}
              onRowsPerPageChange={handleChangeRowsPerPage}
              rowsPerPageOptions={[5, 10, 25]}
            />
          </Box>
        )}
      </Stack>

      {cancelDialog && (
        <RecurringReservationCancelDialog
          open
          onClose={() => setCancelDialog(null)}
          complexId={complexId}
          recurringReservationId={cancelDialog.id}
          recurringReservation={cancelDialog}
        />
      )}
    </Container>
  );
}
