import { useState } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Paper,
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
import { useSearchUsers } from './userAdminApi';
import type { ComplexUser, UserListFilters } from './userAdminTypes';

export interface UserSearchDialogProps {
  complexId: string;
  open: boolean;
  onClose: () => void;
  onSelect: (user: ComplexUser | null) => void;
}

export default function UserSearchDialog({
  complexId,
  open,
  onClose,
  onSelect
}: UserSearchDialogProps) {
  const { t } = useTranslation();
  const [query, setQuery] = useState('');
  const [filters, setFilters] = useState<UserListFilters>({
    page: 0,
    pageSize: 10,
    search: '',
    sort: 'fullName:asc'
  });

  const search = useSearchUsers(complexId, filters, open && Boolean(complexId));

  const handleSearch = () => {
    setFilters((prev) => ({ ...prev, page: 0, search: query }));
  };

  const handleChangePage = (_event: unknown, newPage: number) => {
    setFilters((prev) => ({ ...prev, page: newPage }));
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilters((prev) => ({
      ...prev,
      pageSize: parseInt(event.target.value, 10),
      page: 0
    }));
  };

  const handleKeyDown = (event: React.KeyboardEvent) => {
    if (event.key === 'Enter') {
      event.preventDefault();
      handleSearch();
    }
  };

  const handleClose = () => {
    onClose();
    setQuery('');
    setFilters((prev) => ({ ...prev, page: 0, search: '' }));
  };

  const handleSelect = (user: ComplexUser) => {
    onSelect(user);
    handleClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="md">
      <DialogTitle>{t('admin.users.title')}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <Stack direction="row" spacing={1}>
            <TextField
              label={t('admin.users.searchBy')}
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              onKeyDown={handleKeyDown}
              fullWidth
            />
            <Button variant="contained" onClick={handleSearch}>
              {t('common.search')}
            </Button>
          </Stack>

          {search.isError && (
            <Typography color="error">{search.error?.message}</Typography>
          )}

          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>{t('admin.users.nameHeader')}</TableCell>
                  <TableCell>{t('common.email')}</TableCell>
                  <TableCell>{t('common.phone')}</TableCell>
                  <TableCell align="right">{t('common.actions')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {search.isLoading && (
                  <TableRow>
                    <TableCell colSpan={4} align="center">
                      {t('common.loading')}
                    </TableCell>
                  </TableRow>
                )}
                {!search.isLoading && search.data?.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={4} align="center">
                      {t('admin.users.empty')}
                    </TableCell>
                  </TableRow>
                )}
                {search.data?.items.map((user) => (
                  <TableRow
                    key={user.id}
                    hover
                    sx={{ opacity: user.isBlocked ? 0.5 : 1 }}
                  >
                    <TableCell>{user.fullName}</TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>{user.phoneNumber ?? '—'}</TableCell>
                    <TableCell align="right">
                      <Button
                        variant="outlined"
                        size="small"
                        disabled={user.isBlocked}
                        onClick={() => handleSelect(user)}
                      >
                        {t('common.select')}
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          <Box display="flex" justifyContent="flex-end">
            <TablePagination
              component="div"
              count={search.data?.totalItems ?? 0}
              page={filters.page}
              onPageChange={handleChangePage}
              rowsPerPage={filters.pageSize}
              onRowsPerPageChange={handleChangeRowsPerPage}
              rowsPerPageOptions={[5, 10, 25]}
            />
          </Box>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>{t('common.cancel')}</Button>
      </DialogActions>
    </Dialog>
  );
}
