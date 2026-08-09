import { useMemo, useState } from 'react';
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
  Typography
} from '@mui/material';
import { useSports } from '../features/complexes/complexApi';
import { useCourts, useUpdateCourtStatus } from '../features/courts/courtApi';
import type { CourtListFilters, CourtStatus } from '../features/courts/courtTypes';

const statusOptions: { value: CourtStatus | 'All'; label: string }[] = [
  { value: 'All', label: 'All' },
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' }
];

function statusColor(status: string): 'success' | 'default' | 'warning' {
  if (status === 'Active') return 'success';
  if (status === 'Inactive') return 'warning';
  return 'default';
}

export default function ComplexCourtsPage() {
  const { complexId = '' } = useParams();
  const [filters, setFilters] = useState<CourtListFilters>({
    page: 0,
    pageSize: 10,
    status: 'All',
    sportId: '',
    search: ''
  });

  const { data, isLoading, isError } = useCourts(complexId, filters);
  const sports = useSports();
  const updateStatus = useUpdateCourtStatus(complexId);

  const filteredItems = useMemo(() => {
    if (!data?.items) return [];
    if (!filters.search.trim()) return data.items;
    const term = filters.search.trim().toLowerCase();
    return data.items.filter(
      (court) =>
        court.name.toLowerCase().includes(term) ||
        court.surfaceType.toLowerCase().includes(term)
    );
  }, [data, filters.search]);

  const paginationCount = filters.search.trim() ? filteredItems.length : (data?.totalItems ?? 0);

  const handleChangePage = (_event: unknown, newPage: number) => {
    setFilters((prev) => ({ ...prev, page: newPage }));
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilters((prev) => ({ ...prev, pageSize: parseInt(event.target.value, 10), page: 0 }));
  };

  const emptyState = !isLoading && !isError && data?.items?.length === 0;

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
            Courts
          </Typography>
          <Button
            component={RouterLink}
            to={`/admin/complex/${complexId}/courts/new`}
            variant="contained"
          >
            Create court
          </Button>
        </Stack>

        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            label="Search courts"
            placeholder="Search by name or surface"
            value={filters.search}
            onChange={(event) =>
              setFilters((prev) => ({ ...prev, search: event.target.value, page: 0 }))
            }
            fullWidth
          />
          <FormControl sx={{ minWidth: 160 }}>
            <InputLabel id="court-status-filter-label">Status</InputLabel>
            <Select
              labelId="court-status-filter-label"
              value={filters.status}
              label="Status"
              onChange={(event) =>
                setFilters((prev) => ({
                  ...prev,
                  status: event.target.value as CourtStatus | 'All',
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
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel id="court-sport-filter-label">Sport</InputLabel>
            <Select
              labelId="court-sport-filter-label"
              value={filters.sportId}
              label="Sport"
              onChange={(event) =>
                setFilters((prev) => ({ ...prev, sportId: event.target.value, page: 0 }))
              }
              disabled={sports.isLoading || sports.isError}
            >
              <MenuItem value="">All sports</MenuItem>
              {sports.data?.map((sport) => (
                <MenuItem key={sport.id} value={sport.id}>
                  {sport.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Stack>

        {sports.isError && (
          <Alert severity="warning">Sports could not be loaded. Court sports will not be shown.</Alert>
        )}

        {updateStatus.error && (
          <Alert severity="error">{updateStatus.error.message}</Alert>
        )}

        {isLoading ? (
          <Skeleton variant="rectangular" height={200} />
        ) : isError ? (
          <Alert severity="error">The courts could not be loaded. Please try again later.</Alert>
        ) : emptyState ? (
          <Alert severity="info">No courts found for this complex.</Alert>
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
                    <TableCell>Surface</TableCell>
                    <TableCell>Indoor / Outdoor</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Sports</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {filteredItems.map((court) => {
                    const sportsNames =
                      court.sportIds
                        .map((id) => sports.data?.find((sport) => sport.id === id)?.name)
                        .filter(Boolean)
                        .join(', ') || '—';

                    return (
                      <TableRow key={court.id} hover>
                        <TableCell>
                          <Typography fontWeight={700}>{court.name}</Typography>
                          {court.description && (
                            <Typography variant="body2" color="text.secondary">
                              {court.description}
                            </Typography>
                          )}
                        </TableCell>
                        <TableCell>{court.surfaceType}</TableCell>
                        <TableCell>{court.indoor ? 'Indoor' : 'Outdoor'}</TableCell>
                        <TableCell>
                          <Chip
                            label={court.status}
                            color={statusColor(court.status)}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>{sportsNames}</TableCell>
                        <TableCell align="right">
                          <Stack direction="row" spacing={1} justifyContent="flex-end">
                            <Button
                              component={RouterLink}
                              to={`/admin/complex/${complexId}/courts/${court.id}/edit`}
                              size="small"
                            >
                              Edit
                            </Button>
                            <Button
                              component={RouterLink}
                              to={`/admin/complex/${complexId}/courts/${court.id}/availability`}
                              size="small"
                            >
                              Configure
                            </Button>
                            <Button
                              size="small"
                              variant={court.status === 'Active' ? 'outlined' : 'contained'}
                              color={court.status === 'Active' ? 'warning' : 'success'}
                              onClick={() =>
                                updateStatus.mutate({
                                  courtId: court.id,
                                  request: {
                                    status: court.status === 'Active' ? 'Inactive' : 'Active'
                                  }
                                })
                              }
                              disabled={updateStatus.isPending}
                            >
                              {court.status === 'Active' ? 'Deactivate' : 'Activate'}
                            </Button>
                          </Stack>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </TableContainer>
            <TablePagination
              component="div"
              count={paginationCount}
              page={filters.page}
              onPageChange={handleChangePage}
              rowsPerPage={filters.pageSize}
              onRowsPerPageChange={handleChangeRowsPerPage}
              rowsPerPageOptions={[5, 10, 25]}
            />
          </>
        )}
      </Stack>
    </Container>
  );
}
