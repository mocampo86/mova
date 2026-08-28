import { useState } from 'react';
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import ApiErrorMessage from '../../components/ApiErrorMessage';
import { useCancelRecurringReservation } from './reservationApi';
import type { RecurringReservationListItem } from './reservationTypes';

interface RecurringReservationCancelDialogProps {
  open: boolean;
  onClose: () => void;
  complexId: string;
  recurringReservationId: string;
  recurringReservation?: RecurringReservationListItem;
  description?: string;
}

export default function RecurringReservationCancelDialog({
  open,
  onClose,
  complexId,
  recurringReservationId,
  recurringReservation,
  description
}: RecurringReservationCancelDialogProps) {
  const { t } = useTranslation();
  const cancelRecurringReservation = useCancelRecurringReservation(complexId);
  const [reason, setReason] = useState('');

  const handleClose = () => {
    cancelRecurringReservation.reset();
    setReason('');
    onClose();
  };

  const handleSubmit = async () => {
    await cancelRecurringReservation.mutateAsync({
      recurringReservationId,
      request: { reason: reason.trim() || undefined }
    });
    handleClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="sm">
      <DialogTitle>{t('admin.recurringList.cancelDialogTitle')}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          {cancelRecurringReservation.isError && (
            <Alert severity="error"><ApiErrorMessage error={cancelRecurringReservation.error} /></Alert>
          )}
          {recurringReservation ? (
            <Typography variant="body2" color="text.secondary">
              {t('admin.recurringList.cancelDialogDescription', {
                court: recurringReservation.courtName,
                user: recurringReservation.userName,
                day: t(`days.${recurringReservation.dayOfWeek}`),
                startTime: recurringReservation.startTime.slice(0, 5),
                duration: recurringReservation.durationMinutes,
                startDate: new Date(recurringReservation.startDate).toLocaleDateString(),
                endDate: new Date(recurringReservation.endDate).toLocaleDateString()
              })}
            </Typography>
          ) : (
            <Typography variant="body2" color="text.secondary">
              {description ?? t('admin.recurringList.cancelDialogFallback')}
            </Typography>
          )}
          <TextField
            label={t('admin.reservations.reasonLabel')}
            value={reason}
            onChange={(event) => setReason(event.target.value)}
            fullWidth
            multiline
            rows={2}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>{t('common.cancel')}</Button>
        <Button
          variant="contained"
          color="error"
          onClick={handleSubmit}
          disabled={cancelRecurringReservation.isPending}
        >
          {t('admin.recurringList.cancelSeries')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
