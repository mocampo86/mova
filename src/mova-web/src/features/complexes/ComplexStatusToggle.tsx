import { useState } from 'react';
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  FormControlLabel,
  Switch
} from '@mui/material';
import { useUpdateComplexStatus } from './useUpdateComplexStatus';

interface ComplexStatusToggleProps {
  complexId: string;
  status: string;
}

export default function ComplexStatusToggle({ complexId, status }: ComplexStatusToggleProps) {
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const { mutate, isPending, error } = useUpdateComplexStatus(complexId);

  const isActive = status === 'Active';

  const handleToggle = (_event: React.ChangeEvent<HTMLInputElement>, checked: boolean) => {
    if (!checked) {
      setIsDialogOpen(true);
      return;
    }

    mutate('Active');
  };

  const handleConfirmDeactivate = () => {
    setIsDialogOpen(false);
    mutate('Inactive');
  };

  const handleCancel = () => {
    setIsDialogOpen(false);
  };

  return (
    <>
      <FormControlLabel
        control={
          <Switch
            checked={isActive}
            onChange={handleToggle}
            disabled={isPending}
            inputProps={{ 'aria-label': 'Complex active status' }}
          />
        }
        label={isActive ? 'Active' : 'Inactive'}
      />
      {error && (
        <Alert severity="error" sx={{ mt: 1 }}>
          {error.message}
        </Alert>
      )}
      <Dialog
        open={isDialogOpen}
        onClose={handleCancel}
        aria-labelledby="deactivate-title"
        aria-describedby="deactivate-description"
      >
        <DialogTitle id="deactivate-title">Deactivate complex?</DialogTitle>
        <DialogContent>
          <DialogContentText id="deactivate-description">
            Deactivating the complex will hide it from public listings. You can reactivate it at any time.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCancel} color="inherit" disabled={isPending}>
            Cancel
          </Button>
          <Button
            onClick={handleConfirmDeactivate}
            color="warning"
            variant="contained"
            disabled={isPending}
          >
            Deactivate
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
