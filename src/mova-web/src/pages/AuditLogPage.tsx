import { useMemo, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Container,
  FormControl,
  Grid,
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
  TextField,
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import ApiErrorMessage from '../components/ApiErrorMessage';
import { useAuditLogs } from '../features/audit/auditApi';
import type { AuditLogFilters } from '../features/audit/auditTypes';

const actionOptions = [
  '',
  'SportsComplex.Create',
  'SportsComplex.Update',
  'SportsComplex.UpdateStatus',
  'SportsComplex.UpdateRecurringReservationSettings',
  'Court.Create',
  'Court.Update',
  'Court.UpdateStatus',
  'BlockedUser.Block',
  'BlockedUser.Unblock',
  'Reservation.Cancel',
  'Reservation.UpdateStatus'
];

const entityTypeOptions = ['', 'SportsComplex', 'Court', 'BlockedUser', 'Reservation'];

function formatMetadata(metadata: string | null): string {
  if (!metadata) return '';
  try {
    return JSON.stringify(JSON.parse(metadata), null, 2);
  } catch {
    return metadata;
  }
}

export default function AuditLogPage() {
  const { t } = useTranslation();
  const [filters, setFilters] = useState<AuditLogFilters>({
    page: 0,
    pageSize: 25,
    action: '',
    entityType: '',
    entityId: '',
    sportsComplexId: '',
    userId: '',
    from: '',
    to: ''
  });

  const auditLogs = useAuditLogs(filters);

  const handleChangePage = (_event: unknown, newPage: number) => {
    setFilters((prev) => ({ ...prev, page: newPage }));
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilters({ ...filters, page: 0, pageSize: parseInt(event.target.value, 10) });
  };

  const parsedMetadata = useMemo(() => {
    return (auditLogs.data?.items ?? []).map((item) => ({
      ...item,
      formattedMetadata: formatMetadata(item.metadata)
    }));
  }, [auditLogs.data]);

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Stack spacing={3}>
        <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
          {t('superAdmin.auditLog.title')}
        </Typography>

        {auditLogs.isError && <Alert severity="error"><ApiErrorMessage error={auditLogs.error} /></Alert>}

        <Paper variant="outlined" sx={{ p: 2 }}>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <FormControl fullWidth>
                <InputLabel id="audit-action-label">{t('superAdmin.auditLog.actionFilter')}</InputLabel>
                <Select
                  labelId="audit-action-label"
                  value={filters.action}
                  label={t('superAdmin.auditLog.actionFilter')}
                  onChange={(event) =>
                    setFilters((prev) => ({ ...prev, action: event.target.value, page: 0 }))
                  }
                >
                  {actionOptions.map((action) => (
                    <MenuItem key={action || 'all'} value={action}>
                      {action || t('common.all')}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <FormControl fullWidth>
                <InputLabel id="audit-entity-type-label">{t('superAdmin.auditLog.entityTypeFilter')}</InputLabel>
                <Select
                  labelId="audit-entity-type-label"
                  value={filters.entityType}
                  label={t('superAdmin.auditLog.entityTypeFilter')}
                  onChange={(event) =>
                    setFilters((prev) => ({ ...prev, entityType: event.target.value, page: 0 }))
                  }
                >
                  {entityTypeOptions.map((type) => (
                    <MenuItem key={type || 'all'} value={type}>
                      {type || t('common.all')}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                fullWidth
                label={t('superAdmin.auditLog.entityIdFilter')}
                value={filters.entityId}
                onChange={(event) =>
                  setFilters((prev) => ({ ...prev, entityId: event.target.value, page: 0 }))
                }
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                fullWidth
                label={t('superAdmin.auditLog.complexIdFilter')}
                value={filters.sportsComplexId}
                onChange={(event) =>
                  setFilters((prev) => ({ ...prev, sportsComplexId: event.target.value, page: 0 }))
                }
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                fullWidth
                label={t('superAdmin.auditLog.userIdFilter')}
                value={filters.userId}
                onChange={(event) =>
                  setFilters((prev) => ({ ...prev, userId: event.target.value, page: 0 }))
                }
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                fullWidth
                type="datetime-local"
                label={t('superAdmin.auditLog.fromFilter')}
                value={filters.from}
                onChange={(event) =>
                  setFilters((prev) => ({ ...prev, from: event.target.value, page: 0 }))
                }
                slotProps={{ inputLabel: { shrink: true } }}
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                fullWidth
                type="datetime-local"
                label={t('superAdmin.auditLog.toFilter')}
                value={filters.to}
                onChange={(event) =>
                  setFilters((prev) => ({ ...prev, to: event.target.value, page: 0 }))
                }
                slotProps={{ inputLabel: { shrink: true } }}
              />
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }} sx={{ display: 'flex', alignItems: 'center' }}>
              <Button
                variant="outlined"
                onClick={() =>
                  setFilters({
                    page: 0,
                    pageSize: 25,
                    action: '',
                    entityType: '',
                    entityId: '',
                    sportsComplexId: '',
                    userId: '',
                    from: '',
                    to: ''
                  })
                }
              >
                {t('common.clear')}
              </Button>
            </Grid>
          </Grid>
        </Paper>

        {auditLogs.isLoading ? (
          <Skeleton variant="rectangular" height={200} />
        ) : (
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>{t('superAdmin.auditLog.createdAt')}</TableCell>
                  <TableCell>{t('superAdmin.auditLog.action')}</TableCell>
                  <TableCell>{t('superAdmin.auditLog.entityType')}</TableCell>
                  <TableCell>{t('superAdmin.auditLog.entityId')}</TableCell>
                  <TableCell>{t('superAdmin.auditLog.userId')}</TableCell>
                  <TableCell>{t('superAdmin.auditLog.complexId')}</TableCell>
                  <TableCell>{t('superAdmin.auditLog.metadata')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {parsedMetadata.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={7}>
                      <Alert severity="info">{t('superAdmin.auditLog.empty')}</Alert>
                    </TableCell>
                  </TableRow>
                )}
                {parsedMetadata.map((item) => (
                  <TableRow key={item.id} hover>
                    <TableCell>{new Date(item.createdAt).toLocaleString()}</TableCell>
                    <TableCell>{item.action}</TableCell>
                    <TableCell>{item.entityType}</TableCell>
                    <TableCell>{item.entityId}</TableCell>
                    <TableCell>{item.userId ?? '—'}</TableCell>
                    <TableCell>{item.sportsComplexId ?? '—'}</TableCell>
                    <TableCell>
                      <Box
                        component="pre"
                        sx={{
                          m: 0,
                          p: 0,
                          fontFamily: 'monospace',
                          fontSize: '0.75rem',
                          maxWidth: 240,
                          overflow: 'hidden',
                          textOverflow: 'ellipsis'
                        }}
                      >
                        {item.formattedMetadata || '—'}
                      </Box>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}

        {!auditLogs.isLoading && !auditLogs.isError && auditLogs.data && (
          <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
            <TablePagination
              component="div"
              count={auditLogs.data.totalItems}
              page={filters.page}
              onPageChange={handleChangePage}
              rowsPerPage={filters.pageSize}
              onRowsPerPageChange={handleChangeRowsPerPage}
              rowsPerPageOptions={[10, 25, 50, 100]}
            />
          </Box>
        )}
      </Stack>
    </Container>
  );
}
