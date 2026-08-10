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
import { useTranslation } from 'react-i18next';
import { useSports } from '../features/complexes/complexApi';
import { useCourts, useUpdateCourtStatus } from '../features/courts/courtApi';
import type { CourtListFilters, CourtStatus } from '../features/courts/courtTypes';

function statusColor(status: string): 'success' | 'default' | 'warning' {
  if (status === 'Active') return 'success';
  if (status === 'Inactive') return 'warning';
  return 'default';
}

export default function ComplexCourtsPage() {
  const { t } = useTranslation();
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

  const statusOptions: { value: CourtStatus | 'All'; label: string }[] = [
    { value: 'All', label: t('status.all') },
    { value: 'Active', label: t('status.active') },
    { value: 'Inactive', label: t('status.inactive') }
  ];

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
            {t('admin.courts.title')}
          </Typography>
          <Button
            component={RouterLink}
            to={`/admin/complex/${complexId}/courts/new`}
            variant="contained"
          >
            {t('admin.courts.create')}
          </Button>
        </Stack>

        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            label={t('admin.courts.searchPlaceholder')}
            placeholder={t('admin.courts.searchBy')}
            value={filters.search}
            onChange={(event) =>
              setFilters((prev) => ({ ...prev, search: event.target.value, page: 0 }))
            }
            fullWidth
          />
          <FormControl sx={{ minWidth: 160 }}>
            <InputLabel id="court-status-filter-label">{t('admin.courts.statusFilter')}</InputLabel>
            <Select
              labelId="court-status-filter-label"
              value={filters.status}
              label={t('admin.courts.statusFilter')}
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
            <InputLabel id="court-sport-filter-label">{t('admin.courts.sportFilter')}</InputLabel>
            <Select
              labelId="court-sport-filter-label"
              value={filters.sportId}
              label={t('admin.courts.sportFilter')}
              onChange={(event) =>
                setFilters((prev) => ({ ...prev, sportId: event.target.value, page: 0 }))
              }
              disabled={sports.isLoading || sports.isError}
            >
              <MenuItem value="">{t('admin.courts.allSports')}</MenuItem>
              {sports.data?.map((sport) => (
                <MenuItem key={sport.id} value={sport.id}>
                  {sport.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Stack>

        {sports.isError && (
          <Alert severity="warning">{t('admin.courts.sportsError')}</Alert>
        )}

        {updateStatus.error && (
          <Alert severity="error">{updateStatus.error.message}</Alert>
        )}

        {isLoading ? (
          <Skeleton variant="rectangular" height={200} />
        ) : isError ? (
          <Alert severity="error">{t('admin.courts.error')}</Alert>
        ) : emptyState ? (
          <Alert severity="info">{t('admin.courts.empty')}</Alert>
        ) : (
          <>
            <TableContainer
              component={Box}
              sx={{ border: 1, borderColor: 'divider', borderRadius: 2 }}
            >
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>{t('admin.courts.nameHeader')}</TableCell>
                    <TableCell>{t('admin.courts.surfaceHeader')}</TableCell>
                    <TableCell>{t('admin.courts.indoorOutdoorHeader')}</TableCell>
                    <TableCell>{t('admin.courts.statusHeader')}</TableCell>
                    <TableCell>{t('admin.courts.sportsHeader')}</TableCell>
                    <TableCell align="right">{t('admin.courts.actionsHeader')}</TableCell>
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
                        <TableCell>{court.indoor ? t('common.indoor') : t('common.outdoor')}</TableCell>
                        <TableCell>
                          <Chip
                            label={court.status === 'Active' ? t('status.active') : t('status.inactive')}
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
                              {t('common.edit')}
                            </Button>
                            <Button
                              component={RouterLink}
                              to={`/admin/complex/${complexId}/courts/${court.id}/edit`}
                              size="small"
                            >
                              {t('admin.courts.configure')}
                            </Button>
                            <Button
                              size="small"
                              variant={court.status === 'Active' ? 'outlined' : 'contained'}
                              color={court.status === 'Active' ? 'error' : 'success'}
                              onClick={() =>
                                updateStatus.mutate({
                                  courtId: court.id,
                                  request: { status: court.status === 'Active' ? 'Inactive' : 'Active' }
                                })
                              }
                              disabled={updateStatus.isPending}
                            >
                              {court.status === 'Active' ? t('admin.courts.deactivate') : t('admin.courts.activate')}
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
