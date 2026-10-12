import { useState } from 'react';
import { Link as RouterLink, useParams } from 'react-router-dom';
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
  OutlinedInput,
  Select,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import ApiErrorMessage from '../components/ApiErrorMessage';
import { useAdminComplex } from '../features/complexes/complexApi';
import { useCourt } from '../features/courts/courtApi';
import {
  useCourtRates,
  useCreateCourtRate,
  useUpdateCourtRate,
  useUpdateCourtRateStatus
} from '../features/rates/rateApi';
import type { CourtRate } from '../features/rates/rateTypes';
import { formatMoney, formatTimeOfDay, timeInputToTimeSpan } from '../features/rates/rateUtils';

interface RateFormState {
  daysOfWeek: number[];
  startTime: string;
  endTime: string;
  amountPerHour: string;
}

const emptyForm: RateFormState = {
  daysOfWeek: [],
  startTime: '17:00',
  endTime: '00:00',
  amountPerHour: ''
};

function toFormState(rate: CourtRate): RateFormState {
  return {
    daysOfWeek: [...rate.daysOfWeek],
    startTime: formatTimeOfDay(rate.startTime),
    endTime: formatTimeOfDay(rate.endTime) === '24:00' ? '00:00' : formatTimeOfDay(rate.endTime),
    amountPerHour: String(rate.amountPerHour)
  };
}

export default function CourtRatesPage() {
  const { t } = useTranslation();
  const { complexId = '', courtId = '' } = useParams();

  const court = useCourt(complexId, courtId);
  const complex = useAdminComplex(complexId);
  const currency = complex.data?.currency ?? '';
  const rates = useCourtRates(complexId, courtId);
  const createRate = useCreateCourtRate(complexId, courtId);
  const updateRate = useUpdateCourtRate(complexId, courtId);
  const updateStatus = useUpdateCourtRateStatus(complexId, courtId);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingRate, setEditingRate] = useState<CourtRate | null>(null);
  const [form, setForm] = useState<RateFormState>(emptyForm);

  const openCreateDialog = () => {
    setEditingRate(null);
    setForm(emptyForm);
    setDialogOpen(true);
  };

  const openEditDialog = (rate: CourtRate) => {
    setEditingRate(rate);
    setForm(toFormState(rate));
    setDialogOpen(true);
  };

  const amountValue = Number(form.amountPerHour);
  const formValid =
    form.daysOfWeek.length > 0 &&
    Boolean(form.startTime) &&
    Boolean(form.endTime) &&
    form.amountPerHour !== '' &&
    amountValue > 0;

  const handleSubmit = async () => {
    const request = {
      daysOfWeek: form.daysOfWeek,
      startTime: timeInputToTimeSpan(form.startTime),
      endTime: timeInputToTimeSpan(form.endTime, true),
      amountPerHour: amountValue
    };

    if (editingRate) {
      await updateRate.mutateAsync({ rateId: editingRate.id, request });
    } else {
      await createRate.mutateAsync(request);
    }

    setDialogOpen(false);
  };

  const mutationError = createRate.error ?? updateRate.error ?? updateStatus.error;

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={3}>
        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          spacing={2}
          alignItems={{ sm: 'center' }}
          justifyContent="space-between"
        >
          <Box>
            <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
              {t('admin.rates.title')}
            </Typography>
            <Typography color="text.secondary">
              {court.data?.name ?? ''}
              {court.data ? ` — ${t('admin.rates.subtitle')}` : ''}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1}>
            <Button component={RouterLink} to={`/admin/complex/${complexId}/courts`} variant="outlined">
              {t('common.back')}
            </Button>
            <Button variant="contained" onClick={openCreateDialog}>
              {t('admin.rates.create')}
            </Button>
          </Stack>
        </Stack>

        {mutationError && (
          <Alert severity="error"><ApiErrorMessage error={mutationError} /></Alert>
        )}

        {rates.isLoading || court.isLoading || complex.isLoading ? (
          <Skeleton variant="rectangular" height={200} />
        ) : rates.isError ? (
          <Alert severity="error">{t('admin.rates.error')}</Alert>
        ) : (rates.data?.length ?? 0) === 0 ? (
          <Alert severity="info">{t('admin.rates.empty')}</Alert>
        ) : (
          <TableContainer component={Box} sx={{ border: 1, borderColor: 'divider', borderRadius: 2 }}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>{t('admin.rates.daysHeader')}</TableCell>
                  <TableCell>{t('admin.rates.timeRangeHeader')}</TableCell>
                  <TableCell>{t('admin.rates.amountPerHourHeader')}</TableCell>
                  <TableCell>{t('admin.rates.statusHeader')}</TableCell>
                  <TableCell align="right">{t('admin.courts.actionsHeader')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {(rates.data ?? []).map((rate) => (
                  <TableRow key={rate.id} hover>
                    <TableCell>
                      <Stack direction="row" spacing={0.5} flexWrap="wrap" useFlexGap>
                        {rate.daysOfWeek.map((day) => (
                          <Chip key={day} label={t(`days.${day}`)} size="small" variant="outlined" />
                        ))}
                      </Stack>
                    </TableCell>
                    <TableCell>
                      {formatTimeOfDay(rate.startTime)} – {formatTimeOfDay(rate.endTime)}
                    </TableCell>
                    <TableCell>
                      {formatMoney(rate.amountPerHour, currency)}
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={rate.isActive ? t('status.active') : t('status.inactive')}
                        color={rate.isActive ? 'success' : 'warning'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="right">
                      <Stack direction="row" spacing={1} justifyContent="flex-end">
                        <Button size="small" onClick={() => openEditDialog(rate)}>
                          {t('common.edit')}
                        </Button>
                        <Button
                          size="small"
                          variant={rate.isActive ? 'outlined' : 'contained'}
                          color={rate.isActive ? 'error' : 'success'}
                          disabled={updateStatus.isPending}
                          onClick={() =>
                            updateStatus.mutate({
                              rateId: rate.id,
                              request: { isActive: !rate.isActive }
                            })
                          }
                        >
                          {rate.isActive ? t('admin.courts.deactivate') : t('admin.courts.activate')}
                        </Button>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Stack>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingRate ? t('admin.rates.editTitle') : t('admin.rates.createTitle')}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            <FormControl fullWidth>
              <InputLabel id="rate-days-label">{t('admin.rates.days')}</InputLabel>
              <Select
                labelId="rate-days-label"
                multiple
                value={form.daysOfWeek}
                input={<OutlinedInput label={t('admin.rates.days')} />}
                onChange={(event) => {
                  const value = event.target.value;
                  setForm((prev) => ({
                    ...prev,
                    daysOfWeek: (typeof value === 'string' ? [] : value).map(Number).sort((a, b) => a - b)
                  }));
                }}
                renderValue={(selected) => (
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                    {selected.map((day) => (
                      <Chip key={day} label={t(`days.${day}`)} size="small" />
                    ))}
                  </Box>
                )}
              >
                {[0, 1, 2, 3, 4, 5, 6].map((day) => (
                  <MenuItem key={day} value={day}>
                    {t(`days.${day}`)}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField
                label={t('admin.rates.startTime')}
                type="time"
                value={form.startTime}
                onChange={(event) => setForm((prev) => ({ ...prev, startTime: event.target.value }))}
                fullWidth
                InputLabelProps={{ shrink: true }}
              />
              <TextField
                label={t('admin.rates.endTime')}
                type="time"
                value={form.endTime}
                onChange={(event) => setForm((prev) => ({ ...prev, endTime: event.target.value }))}
                fullWidth
                InputLabelProps={{ shrink: true }}
                helperText={t('admin.rates.endTimeHint')}
              />
            </Stack>

            <TextField
              label={t('admin.rates.amountPerHour')}
              type="number"
              value={form.amountPerHour}
              onChange={(event) => setForm((prev) => ({ ...prev, amountPerHour: event.target.value }))}
              fullWidth
              inputProps={{ min: 0, step: '0.01' }}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)} disabled={createRate.isPending || updateRate.isPending}>
            {t('common.cancel')}
          </Button>
          <Button
            variant="contained"
            onClick={handleSubmit}
            disabled={!formValid || createRate.isPending || updateRate.isPending}
          >
            {t('common.save')}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}
