import { useState } from 'react';
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
import {
  useBlockUser,
  useComplexUsers,
  useUnblockUser,
  useUserReservations
} from '../features/users/userAdminApi';
import type {
  ComplexUser,
  UserListFilters,
  UserReservationFilters
} from '../features/users/userAdminTypes';

const sortOptions: { value: string; label: string }[] = [
  { value: 'fullName:asc', label: 'Name (A-Z)' },
  { value: 'fullName:desc', label: 'Name (Z-A)' },
  { value: 'email:asc', label: 'Email (A-Z)' },
  { value: 'createdAt:desc', label: 'Newest users' }
];

function formatDateTime(isoString?: string | null): string {
  if (!isoString) return '—';
  return new Date(isoString).toLocaleString();
}

export default function ComplexUsersPage() {
  const { complexId = '' } = useParams();
  const [filters, setFilters] = useState<UserListFilters>({
    page: 0,
    pageSize: 10,
    search: '',
    sort: 'fullName:asc'
  });

  const { data, isLoading, isError } = useComplexUsers(complexId, filters);
  const blockUser = useBlockUser(complexId);
  const unblockUser = useUnblockUser(complexId);

  const [blockDialog, setBlockDialog] = useState<{ open: boolean; user: ComplexUser | null }>({
    open: false,
    user: null
  });
  const [blockForm, setBlockForm] = useState({ reason: '', blockedUntil: '' });
  const [blockFormError, setBlockFormError] = useState<string | null>(null);

  const [reservationsDialog, setReservationsDialog] = useState<{
    open: boolean;
    user: ComplexUser | null;
  }>({ open: false, user: null });
  const [reservationFilters, setReservationFilters] = useState<UserReservationFilters>({
    page: 0,
    pageSize: 10,
    sort: 'startAt:desc'
  });

  const handleChangePage = (_event: unknown, newPage: number) => {
    setFilters((prev) => ({ ...prev, page: newPage }));
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilters((prev) => ({ ...prev, pageSize: parseInt(event.target.value, 10), page: 0 }));
  };

  const handleReservationChangePage = (_event: unknown, newPage: number) => {
    setReservationFilters((prev) => ({ ...prev, page: newPage }));
  };

  const handleReservationChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setReservationFilters((prev) => ({
      ...prev,
      pageSize: parseInt(event.target.value, 10),
      page: 0
    }));
  };

  const openBlockDialog = (user: ComplexUser) => {
    setBlockDialog({ open: true, user });
    setBlockForm({ reason: '', blockedUntil: '' });
    setBlockFormError(null);
  };

  const closeBlockDialog = () => {
    setBlockDialog({ open: false, user: null });
    setBlockFormError(null);
  };

  const handleBlockSubmit = async () => {
    if (!blockDialog.user) return;

    setBlockFormError(null);

    const blockedUntil = blockForm.blockedUntil ? new Date(blockForm.blockedUntil).toISOString() : undefined;

    try {
      await blockUser.mutateAsync({
        userId: blockDialog.user.id,
        reason: blockForm.reason || undefined,
        blockedUntil
      });
      closeBlockDialog();
    } catch {
      setBlockFormError('Failed to block the user. Please try again.');
    }
  };

  const handleUnblock = async (user: ComplexUser) => {
    if (!user.blockId) return;

    await unblockUser.mutateAsync(user.blockId);
  };

  const openReservationsDialog = (user: ComplexUser) => {
    setReservationsDialog({ open: true, user });
    setReservationFilters({ page: 0, pageSize: 10, sort: 'startAt:desc' });
  };

  const closeReservationsDialog = () => {
    setReservationsDialog({ open: false, user: null });
  };

  const emptyState = !isLoading && !isError && data?.items?.length === 0;

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={3}>
        <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
          Users
        </Typography>

        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            label="Search users"
            placeholder="Search by name, email or phone"
            value={filters.search}
            onChange={(event) =>
              setFilters((prev) => ({ ...prev, search: event.target.value, page: 0 }))
            }
            fullWidth
          />
          <TextField
            select
            label="Sort"
            value={filters.sort}
            onChange={(event) =>
              setFilters((prev) => ({ ...prev, sort: event.target.value, page: 0 }))
            }
            sx={{ minWidth: 180 }}
            slotProps={{ select: { native: true } }}
          >
            {sortOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </TextField>
        </Stack>

        {(blockUser.error || unblockUser.error) && (
          <Alert severity="error">
            {blockUser.error?.message || unblockUser.error?.message}
          </Alert>
        )}

        {isLoading ? (
          <Skeleton variant="rectangular" height={200} />
        ) : isError ? (
          <Alert severity="error">The users could not be loaded. Please try again later.</Alert>
        ) : emptyState ? (
          <Alert severity="info">No users found for this complex.</Alert>
        ) : (
          <>
            <TableContainer
              component={Box}
              sx={{ border: 1, borderColor: 'divider', borderRadius: 2 }}
            >
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Contact</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data?.items.map((user) => (
                    <TableRow key={user.id} hover>
                      <TableCell>
                        <Typography fontWeight={700}>{user.fullName}</Typography>
                        <Typography variant="body2" color="text.secondary">
                          {user.email}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">{user.phoneNumber || '—'}</Typography>
                      </TableCell>
                      <TableCell>
                        {user.isBlocked ? (
                          <Stack spacing={0.5} alignItems="flex-start">
                            <Chip label="Blocked" color="error" size="small" />
                            {user.blockReason && (
                              <Typography variant="caption" color="text.secondary">
                                {user.blockReason}
                              </Typography>
                            )}
                            {user.blockedUntil && (
                              <Typography variant="caption" color="text.secondary">
                                Until {formatDateTime(user.blockedUntil)}
                              </Typography>
                            )}
                          </Stack>
                        ) : (
                          <Chip label="Active" color="success" size="small" />
                        )}
                      </TableCell>
                      <TableCell align="right">
                        <Stack direction="row" spacing={1} justifyContent="flex-end">
                          <Button
                            size="small"
                            variant="outlined"
                            onClick={() => openReservationsDialog(user)}
                          >
                            History
                          </Button>
                          {user.isBlocked ? (
                            <Button
                              size="small"
                              variant="contained"
                              color="success"
                              onClick={() => handleUnblock(user)}
                              disabled={unblockUser.isPending}
                            >
                              Unblock
                            </Button>
                          ) : (
                            <Button
                              size="small"
                              variant="outlined"
                              color="error"
                              onClick={() => openBlockDialog(user)}
                            >
                              Block
                            </Button>
                          )}
                        </Stack>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
            <TablePagination
              component="div"
              count={data?.totalItems ?? 0}
              page={filters.page}
              onPageChange={handleChangePage}
              rowsPerPage={filters.pageSize}
              onRowsPerPageChange={handleChangeRowsPerPage}
              rowsPerPageOptions={[5, 10, 25]}
            />
          </>
        )}
      </Stack>

      <Dialog open={blockDialog.open} onClose={closeBlockDialog} fullWidth maxWidth="sm">
        <DialogTitle>
          Block {blockDialog.user?.fullName ?? 'user'}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            {blockFormError && <Alert severity="error">{blockFormError}</Alert>}
            <TextField
              label="Reason"
              placeholder="Reason for blocking"
              value={blockForm.reason}
              onChange={(event) =>
                setBlockForm((prev) => ({ ...prev, reason: event.target.value }))
              }
              fullWidth
              multiline
              rows={2}
            />
            <TextField
              label="Expiration (optional)"
              type="datetime-local"
              value={blockForm.blockedUntil}
              onChange={(event) =>
                setBlockForm((prev) => ({ ...prev, blockedUntil: event.target.value }))
              }
              fullWidth
              InputLabelProps={{ shrink: true }}
              helperText="Leave empty to block indefinitely"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeBlockDialog}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleBlockSubmit}
            disabled={blockUser.isPending}
          >
            Block
          </Button>
        </DialogActions>
      </Dialog>

      {reservationsDialog.user && (
        <UserReservationsDialog
          complexId={complexId}
          user={reservationsDialog.user}
          open={reservationsDialog.open}
          onClose={closeReservationsDialog}
          filters={reservationFilters}
          onPageChange={handleReservationChangePage}
          onRowsPerPageChange={handleReservationChangeRowsPerPage}
        />
      )}
    </Container>
  );
}

interface UserReservationsDialogProps {
  complexId: string;
  user: ComplexUser;
  open: boolean;
  onClose: () => void;
  filters: UserReservationFilters;
  onPageChange: (event: unknown, page: number) => void;
  onRowsPerPageChange: (event: React.ChangeEvent<HTMLInputElement>) => void;
}

function UserReservationsDialog({
  complexId,
  user,
  open,
  onClose,
  filters,
  onPageChange,
  onRowsPerPageChange
}: UserReservationsDialogProps) {
  const { data, isLoading, isError } = useUserReservations(complexId, user.id, filters);
  const emptyState = !isLoading && !isError && data?.items?.length === 0;

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="lg">
      <DialogTitle>Reservation history: {user.fullName}</DialogTitle>
      <DialogContent>
        {isLoading ? (
          <Skeleton variant="rectangular" height={200} />
        ) : isError ? (
          <Alert severity="error">The reservation history could not be loaded.</Alert>
        ) : emptyState ? (
          <Alert severity="info">No reservations found for this user.</Alert>
        ) : (
          <>
            <TableContainer component={Box} sx={{ border: 1, borderColor: 'divider', borderRadius: 2 }}>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Court</TableCell>
                    <TableCell>Start</TableCell>
                    <TableCell>End</TableCell>
                    <TableCell>Status</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data?.items.map((reservation) => (
                    <TableRow key={reservation.id} hover>
                      <TableCell>{reservation.courtName}</TableCell>
                      <TableCell>{formatDateTime(reservation.startAt)}</TableCell>
                      <TableCell>{formatDateTime(reservation.endAt)}</TableCell>
                      <TableCell>
                        <Chip label={reservation.status} size="small" />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
            <TablePagination
              component="div"
              count={data?.totalItems ?? 0}
              page={filters.page}
              onPageChange={onPageChange}
              rowsPerPage={filters.pageSize}
              onRowsPerPageChange={onRowsPerPageChange}
              rowsPerPageOptions={[5, 10, 25]}
            />
          </>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}
