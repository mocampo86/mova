import { useMemo, useState } from 'react';
import { useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Chip,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
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
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useCourts } from '../features/courts/courtApi';
import type { CourtListFilters } from '../features/courts/courtTypes';
import {
  useCancelReservation,
  useCreateReservation,
  useReservations,
  useUpdateReservationStatus
} from '../features/reservations/reservationApi';
import RecurringReservationCancelDialog from '../features/reservations/RecurringReservationCancelDialog';
import type {
  Reservation,
  ReservationListFilters,
  ReservationStatus
} from '../features/reservations/reservationTypes';
import ReservationCalendar from '../features/reservations/ReservationCalendar';
import { useCourtAvailabilityForCourts } from '../features/reservations/reservationCalendarApi';
import type {
  FreeCalendarSlot,
  ReservationCalendarSlot
} from '../features/reservations/reservationCalendarTypes';
import { buildCalendarColumns } from '../features/reservations/reservationCalendarUtils';
import { getReservationStatusColor } from '../features/reservations/reservationStatus';

function formatDateTimeRange(startAt: string, endAt: string): string {
  const start = new Date(startAt);
  const end = new Date(endAt);
  return `${start.toLocaleString()} - ${end.toLocaleTimeString()}`;
}

function getTodayLocalDateString(): string {
  const today = new Date();
  const year = today.getFullYear();
  const month = String(today.getMonth() + 1).padStart(2, '0');
  const day = String(today.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function toLocalDateTimeInputValue(isoString: string): string {
  const date = new Date(isoString);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

function initialCreateForm(): {
  courtId: string;
  userId: string;
  startAt: string;
  endAt: string;
  notes: string;
} {
  const start = new Date();
  start.setMinutes(0, 0, 0);
  const end = new Date(start.getTime() + 60 * 60 * 1000);
  return {
    courtId: '',
    userId: '',
    startAt: toLocalDateTimeInputValue(start.toISOString()),
    endAt: toLocalDateTimeInputValue(end.toISOString()),
    notes: ''
  };
}

export default function ComplexReservationsPage() {
  const { t } = useTranslation();
  const { complexId = '' } = useParams();
  const [filters, setFilters] = useState<ReservationListFilters>({
    page: 0,
    pageSize: 10,
    courtId: '',
    status: 'All',
    date: getTodayLocalDateString()
  });
  const [view, setView] = useState<'list' | 'calendar'>('list');

  const courtFilters: CourtListFilters = {
    page: 0,
    pageSize: 100,
    status: 'All',
    sportId: '',
    search: ''
  };

  const listReservations = useReservations(complexId, filters);
  const calendarFilters = useMemo<ReservationListFilters>(
    () => ({ ...filters, page: 0, pageSize: 100, sort: 'startAt:asc', status: 'All' }),
    [filters]
  );
  const calendarReservations = useReservations(complexId, calendarFilters, view === 'calendar');

  const courts = useCourts(complexId, courtFilters);
  const createReservation = useCreateReservation(complexId);
  const cancelReservation = useCancelReservation(complexId);
  const updateStatus = useUpdateReservationStatus(complexId);

  const targetCourtIds = useMemo(() => {
    if (!courts.data) {
      return [];
    }
    return filters.courtId ? [filters.courtId] : courts.data.items.map((court) => court.id);
  }, [filters.courtId, courts.data]);

  const availability = useCourtAvailabilityForCourts(
    complexId,
    filters.date,
    targetCourtIds,
    view === 'calendar'
  );

  const calendarColumns = useMemo(() => {
    const targetCourts = filters.courtId
      ? courts.data?.items.filter((court) => court.id === filters.courtId) ?? []
      : courts.data?.items ?? [];
    return buildCalendarColumns(
      targetCourts,
      calendarReservations.data?.items ?? [],
      availability.data
    );
  }, [filters.courtId, courts.data, calendarReservations.data, availability.data]);

  const [createOpen, setCreateOpen] = useState(false);
  const [createForm, setCreateForm] = useState(initialCreateForm);
  const [createFormError, setCreateFormError] = useState<string | null>(null);

  const [cancelDialog, setCancelDialog] = useState<{ open: boolean; reservation: Reservation | null }>({
    open: false,
    reservation: null
  });
  const [cancelReason, setCancelReason] = useState('');

  const [recurringCancelDialog, setRecurringCancelDialog] = useState<{
    open: boolean;
    recurringReservationId: string;
    description: string;
  }>({ open: false, recurringReservationId: '', description: '' });

  const [statusDialog, setStatusDialog] = useState<{ open: boolean; reservation: Reservation | null }>({
    open: false,
    reservation: null
  });
  const [selectedStatus, setSelectedStatus] = useState<'Completed' | 'NoShow'>('Completed');

  const [selectedFreeSlot, setSelectedFreeSlot] = useState<FreeCalendarSlot | null>(null);
  const [selectedReservation, setSelectedReservation] = useState<Reservation | null>(null);

  const statusOptions: { value: ReservationStatus | 'All'; label: string }[] = [
    { value: 'All', label: t('status.all') },
    { value: 'Pending', label: t('status.pending') },
    { value: 'Confirmed', label: t('status.confirmed') },
    { value: 'CancelledByUser', label: t('status.cancelledByUser') },
    { value: 'CancelledByAdmin', label: t('status.cancelledByAdmin') },
    { value: 'Completed', label: t('status.completed') },
    { value: 'NoShow', label: t('status.noShow') }
  ];

  const updateStatusOptions: { value: 'Completed' | 'NoShow'; label: string }[] = [
    { value: 'Completed', label: t('status.completed') },
    { value: 'NoShow', label: t('status.noShow') }
  ];

  const handleChangePage = (_event: unknown, newPage: number) => {
    setFilters((prev) => ({ ...prev, page: newPage }));
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilters((prev) => ({ ...prev, pageSize: parseInt(event.target.value, 10), page: 0 }));
  };

  const handleCreateSubmit = async () => {
    setCreateFormError(null);
    const startAt = new Date(createForm.startAt).toISOString();
    const endAt = new Date(createForm.endAt).toISOString();

    if (new Date(endAt) <= new Date(startAt)) {
      setCreateFormError(t('admin.reservations.endTimeError'));
      return;
    }

    try {
      await createReservation.mutateAsync({
        courtId: createForm.courtId,
        userId: createForm.userId,
        startAt,
        endAt,
        notes: createForm.notes || undefined
      });
      setCreateOpen(false);
      setCreateForm(initialCreateForm());
    } catch {
      // Error is surfaced via mutation state
    }
  };

  const handleCancelSubmit = async () => {
    if (!cancelDialog.reservation) return;

    await cancelReservation.mutateAsync({
      reservationId: cancelDialog.reservation.id,
      request: { reason: cancelReason || undefined }
    });
    setCancelDialog({ open: false, reservation: null });
    setCancelReason('');
  };

  const handleStatusSubmit = async () => {
    if (!statusDialog.reservation) return;

    await updateStatus.mutateAsync({
      reservationId: statusDialog.reservation.id,
      request: { status: selectedStatus }
    });
    setStatusDialog({ open: false, reservation: null });
  };

  const emptyState =
    !listReservations.isLoading &&
    !listReservations.isError &&
    listReservations.data?.items?.length === 0;
  const isActionsDisabled = (status: ReservationStatus) =>
    status === 'CancelledByUser' ||
    status === 'CancelledByAdmin' ||
    status === 'Completed' ||
    status === 'NoShow';

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
            {t('admin.reservations.title')}
          </Typography>
          <Stack direction="row" spacing={2} alignItems="center">
            <ToggleButtonGroup
              value={view}
              exclusive
              onChange={(_, value) => value && setView(value)}
              aria-label={t('admin.reservations.viewLabel')}
            >
              <ToggleButton value="list">{t('admin.reservations.listView')}</ToggleButton>
              <ToggleButton value="calendar">{t('admin.reservations.calendarView')}</ToggleButton>
            </ToggleButtonGroup>
            <Button variant="contained" onClick={() => setCreateOpen(true)}>
              {t('admin.reservations.create')}
            </Button>
          </Stack>
        </Stack>

        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            label={t('admin.reservations.dateFilter')}
            type="date"
            value={filters.date}
            onChange={(event) =>
              setFilters((prev) => ({ ...prev, date: event.target.value, page: 0 }))
            }
            fullWidth
            InputLabelProps={{ shrink: true }}
          />
          <FormControl sx={{ minWidth: 180 }}>
            <InputLabel id="reservation-court-filter-label">{t('admin.reservations.courtFilter')}</InputLabel>
            <Select
              labelId="reservation-court-filter-label"
              value={filters.courtId}
              label={t('admin.reservations.courtFilter')}
              onChange={(event) =>
                setFilters((prev) => ({ ...prev, courtId: event.target.value, page: 0 }))
              }
              disabled={courts.isLoading || courts.isError}
            >
              <MenuItem value="">{t('admin.reservations.allCourts')}</MenuItem>
              {courts.data?.items.map((court) => (
                <MenuItem key={court.id} value={court.id}>
                  {court.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          {view === 'list' && (
            <FormControl sx={{ minWidth: 200 }}>
              <InputLabel id="reservation-status-filter-label">{t('admin.reservations.statusFilter')}</InputLabel>
              <Select
                labelId="reservation-status-filter-label"
                value={filters.status}
                label={t('admin.reservations.statusFilter')}
                onChange={(event) =>
                  setFilters((prev) => ({
                    ...prev,
                    status: event.target.value as ReservationStatus | 'All',
                    page: 0
                  }))
                }
              >
                {statusOptions.map((option) => (
                  <MenuItem key={option.value} value={option.value}>
                    {option.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          )}
        </Stack>

        {courts.isError && (
          <Alert severity="warning">{t('admin.reservations.courtsError')}</Alert>
        )}

        {view === 'list' ? (
          listReservations.isLoading ? (
            <Skeleton variant="rectangular" height={200} />
          ) : listReservations.isError ? (
            <Alert severity="error">{t('admin.reservations.error')}</Alert>
          ) : emptyState ? (
            <Alert severity="info">{t('admin.reservations.empty')}</Alert>
          ) : (
            <>
              <TableContainer
                component={Box}
                sx={{ border: 1, borderColor: 'divider', borderRadius: 2 }}
              >
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>{t('admin.reservations.dateTimeHeader')}</TableCell>
                      <TableCell>{t('admin.reservations.courtHeader')}</TableCell>
                      <TableCell>{t('admin.reservations.userHeader')}</TableCell>
                      <TableCell>{t('admin.reservations.statusHeader')}</TableCell>
                      <TableCell align="right">{t('admin.reservations.actionsHeader')}</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {listReservations.data?.items.map((reservation) => (
                      <TableRow key={reservation.id} hover>
                        <TableCell>
                          <Typography fontWeight={700}>
                            {new Date(reservation.startAt).toLocaleDateString()}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {formatDateTimeRange(reservation.startAt, reservation.endAt)}
                          </Typography>
                        </TableCell>
                        <TableCell>{reservation.courtName}</TableCell>
                        <TableCell>{reservation.userName}</TableCell>
                        <TableCell>
                          <Chip
                            label={statusOptions.find((s) => s.value === reservation.status)?.label ?? reservation.status}
                            color={getReservationStatusColor(reservation.status)}
                            size="small"
                          />
                        </TableCell>
                        <TableCell align="right">
                          <Stack direction="row" spacing={1} justifyContent="flex-end">
                            {reservation.recurringReservationId && reservation.status !== 'CancelledByUser' && reservation.status !== 'CancelledByAdmin' && (
                              <Button
                                size="small"
                                variant="outlined"
                                color="error"
                                disabled={cancelReservation.isPending}
                                onClick={() =>
                                  setRecurringCancelDialog({
                                    open: true,
                                    recurringReservationId: reservation.recurringReservationId!,
                                    description: t('admin.recurringList.cancelDialogInline', {
                                      court: reservation.courtName,
                                      user: reservation.userName,
                                      startAt: new Date(reservation.startAt).toLocaleString()
                                    })
                                  })
                                }
                              >
                                {t('admin.recurringList.cancelSeries')}
                              </Button>
                            )}
                            <Button
                              size="small"
                              variant="outlined"
                              disabled={isActionsDisabled(reservation.status) || cancelReservation.isPending}
                              onClick={() => setCancelDialog({ open: true, reservation })}
                            >
                              {t('admin.reservations.cancel')}
                            </Button>
                            <Button
                              size="small"
                              variant="outlined"
                              disabled={isActionsDisabled(reservation.status) || updateStatus.isPending}
                              onClick={() => {
                                setStatusDialog({ open: true, reservation });
                                setSelectedStatus('Completed');
                              }}
                            >
                              {t('admin.reservations.markStatus')}
                            </Button>
                          </Stack>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
              <TablePagination
                component="div"
                count={listReservations.data?.totalItems ?? 0}
                page={filters.page}
                onPageChange={handleChangePage}
                rowsPerPage={filters.pageSize}
                onRowsPerPageChange={handleChangeRowsPerPage}
                rowsPerPageOptions={[5, 10, 25]}
              />
            </>
          )
        ) : (
          <ReservationCalendar
            columns={calendarColumns}
            isLoading={calendarReservations.isLoading || availability.isLoading || courts.isLoading}
            isError={calendarReservations.isError || availability.isError || courts.isError}
            onFreeSlotClick={setSelectedFreeSlot}
            onReservationClick={(slot: ReservationCalendarSlot) => setSelectedReservation(slot.reservation)}
          />
        )}
      </Stack>

      <Dialog open={createOpen} onClose={() => setCreateOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>{t('admin.reservations.createDialogTitle')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            {createFormError && <Alert severity="error">{createFormError}</Alert>}
            <FormControl fullWidth>
              <InputLabel id="create-court-label">{t('admin.reservations.courtFilter')}</InputLabel>
              <Select
                labelId="create-court-label"
                value={createForm.courtId}
                label={t('admin.reservations.courtFilter')}
                onChange={(event) =>
                  setCreateForm((prev) => ({ ...prev, courtId: event.target.value }))
                }
                disabled={courts.isLoading || courts.isError}
              >
                {courts.data?.items.map((court) => (
                  <MenuItem key={court.id} value={court.id}>
                    {court.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label={t('admin.reservations.userIdLabel')}
              value={createForm.userId}
              onChange={(event) =>
                setCreateForm((prev) => ({ ...prev, userId: event.target.value }))
              }
              fullWidth
            />
            <TextField
              label={t('admin.reservations.startLabel')}
              type="datetime-local"
              value={createForm.startAt}
              onChange={(event) =>
                setCreateForm((prev) => ({ ...prev, startAt: event.target.value }))
              }
              fullWidth
              InputLabelProps={{ shrink: true }}
            />
            <TextField
              label={t('admin.reservations.endLabel')}
              type="datetime-local"
              value={createForm.endAt}
              onChange={(event) =>
                setCreateForm((prev) => ({ ...prev, endAt: event.target.value }))
              }
              fullWidth
              InputLabelProps={{ shrink: true }}
            />
            <TextField
              label={t('common.notes')}
              value={createForm.notes}
              onChange={(event) =>
                setCreateForm((prev) => ({ ...prev, notes: event.target.value }))
              }
              fullWidth
              multiline
              rows={2}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreateOpen(false)}>{t('common.cancel')}</Button>
          <Button
            variant="contained"
            onClick={handleCreateSubmit}
            disabled={
              createReservation.isPending ||
              !createForm.courtId ||
              !createForm.userId ||
              !createForm.startAt ||
              !createForm.endAt
            }
          >
            {t('admin.reservations.createButton')}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={cancelDialog.open} onClose={() => setCancelDialog({ open: false, reservation: null })} fullWidth maxWidth="sm">
        <DialogTitle>{t('admin.reservations.cancelDialogTitle')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <Typography variant="body1">
              {t('admin.reservations.cancelConfirm')}
            </Typography>
            <TextField
              label={t('admin.reservations.reasonLabel')}
              placeholder={t('admin.reservations.reasonPlaceholder')}
              value={cancelReason}
              onChange={(event) => setCancelReason(event.target.value)}
              fullWidth
              multiline
              rows={2}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCancelDialog({ open: false, reservation: null })}>{t('common.cancel')}</Button>
          <Button variant="contained" color="error" onClick={handleCancelSubmit} disabled={cancelReservation.isPending}>
            {t('common.confirm')}
          </Button>
        </DialogActions>
      </Dialog>

      {recurringCancelDialog.open && (
        <RecurringReservationCancelDialog
          open={recurringCancelDialog.open}
          onClose={() => setRecurringCancelDialog({ open: false, recurringReservationId: '', description: '' })}
          complexId={complexId}
          recurringReservationId={recurringCancelDialog.recurringReservationId}
          description={recurringCancelDialog.description}
        />
      )}

      <Dialog open={statusDialog.open} onClose={() => setStatusDialog({ open: false, reservation: null })} fullWidth maxWidth="sm">
        <DialogTitle>{t('admin.reservations.updateDialogTitle')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <FormControl fullWidth>
              <InputLabel id="update-status-label">{t('admin.reservations.statusFilter')}</InputLabel>
              <Select
                labelId="update-status-label"
                value={selectedStatus}
                label={t('admin.reservations.statusFilter')}
                onChange={(event) =>
                  setSelectedStatus(event.target.value as 'Completed' | 'NoShow')
                }
              >
                {updateStatusOptions.map((option) => (
                  <MenuItem key={option.value} value={option.value}>
                    {option.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setStatusDialog({ open: false, reservation: null })}>{t('common.cancel')}</Button>
          <Button variant="contained" onClick={handleStatusSubmit} disabled={updateStatus.isPending}>
            {t('admin.reservations.updateButton')}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={Boolean(selectedFreeSlot)} onClose={() => setSelectedFreeSlot(null)} fullWidth maxWidth="sm">
        <DialogTitle>{t('admin.reservations.slotDetailsTitle')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <Typography>
              {t('admin.reservations.courtHeader')}: {selectedFreeSlot?.courtName}
            </Typography>
            <Typography>
              {t('admin.reservations.dateTimeHeader')}:{' '}
              {selectedFreeSlot && formatDateTimeRange(selectedFreeSlot.startAt, selectedFreeSlot.endAt)}
            </Typography>
            <Chip label={t('admin.reservations.legendFree')} color="success" size="small" />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSelectedFreeSlot(null)}>{t('common.close')}</Button>
          <Button
            variant="contained"
            onClick={() => {
              if (!selectedFreeSlot) return;
              setCreateForm({
                courtId: selectedFreeSlot.courtId,
                userId: '',
                startAt: toLocalDateTimeInputValue(selectedFreeSlot.startAt),
                endAt: toLocalDateTimeInputValue(selectedFreeSlot.endAt),
                notes: ''
              });
              setSelectedFreeSlot(null);
              setCreateOpen(true);
            }}
          >
            {t('admin.reservations.reserveSlot')}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={Boolean(selectedReservation)} onClose={() => setSelectedReservation(null)} fullWidth maxWidth="sm">
        <DialogTitle>{t('admin.reservations.reservationDetailsTitle')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <Typography>
              {t('admin.reservations.courtHeader')}: {selectedReservation?.courtName}
            </Typography>
            <Typography>
              {t('admin.reservations.userHeader')}: {selectedReservation?.userName}
            </Typography>
            <Typography>
              {t('admin.reservations.dateTimeHeader')}:{' '}
              {selectedReservation && formatDateTimeRange(selectedReservation.startAt, selectedReservation.endAt)}
            </Typography>
            {selectedReservation && (
              <Chip
                label={statusOptions.find((s) => s.value === selectedReservation.status)?.label ?? selectedReservation.status}
                color={getReservationStatusColor(selectedReservation.status)}
                size="small"
              />
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSelectedReservation(null)}>{t('common.close')}</Button>
          {selectedReservation && (
            <>
              <Button
                variant="outlined"
                disabled={isActionsDisabled(selectedReservation.status) || cancelReservation.isPending}
                onClick={() => {
                  setSelectedReservation(null);
                  setCancelDialog({ open: true, reservation: selectedReservation });
                }}
              >
                {t('admin.reservations.cancel')}
              </Button>
              <Button
                variant="outlined"
                disabled={isActionsDisabled(selectedReservation.status) || updateStatus.isPending}
                onClick={() => {
                  setSelectedReservation(null);
                  setSelectedStatus('Completed');
                  setStatusDialog({ open: true, reservation: selectedReservation });
                }}
              >
                {t('admin.reservations.markStatus')}
              </Button>
            </>
          )}
        </DialogActions>
      </Dialog>
    </Container>
  );
}
