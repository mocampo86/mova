import { useMemo, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { Box, IconButton, Snackbar, Alert } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../auth/useAuth';
import {
  useAddFavoriteComplex,
  useFavoriteComplexIds,
  useRemoveFavoriteComplex
} from './favoritesApi';

interface FavoriteToggleButtonProps {
  complexId: string;
}

export default function FavoriteToggleButton({ complexId }: FavoriteToggleButtonProps) {
  const { t } = useTranslation();
  const { accessToken } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const idsQuery = useFavoriteComplexIds();
  const addMutation = useAddFavoriteComplex();
  const removeMutation = useRemoveFavoriteComplex();
  const [snackbar, setSnackbar] = useState<{ severity: 'success' | 'error'; message: string } | null>(null);

  const isFavorite = useMemo(
    () => Boolean(idsQuery.data?.items.includes(complexId)),
    [idsQuery.data, complexId]
  );

  const isPending =
    (addMutation.isPending && addMutation.variables === complexId) ||
    (removeMutation.isPending && removeMutation.variables === complexId);

  const handleToggle = () => {
    if (!accessToken) {
      const returnTo = `${location.pathname}${location.search}`;
      navigate(`/login?intent=user&returnTo=${encodeURIComponent(returnTo)}`);
      return;
    }

    const onSuccess = (favorite: boolean) =>
      setSnackbar({
        severity: 'success',
        message: favorite ? t('favorites.added') : t('favorites.removed')
      });
    const onError = () => setSnackbar({ severity: 'error', message: t('favorites.error') });

    if (isFavorite) {
      removeMutation.mutate(complexId, {
        onSuccess: () => onSuccess(false),
        onError
      });
    } else {
      addMutation.mutate(complexId, {
        onSuccess: () => onSuccess(true),
        onError
      });
    }
  };

  return (
    <>
      <IconButton
        onClick={handleToggle}
        disabled={isPending}
        aria-label={isFavorite ? t('favorites.remove') : t('favorites.add')}
        aria-pressed={isFavorite}
        sx={{
          width: 40,
          height: 40,
          color: isFavorite ? 'error.main' : 'text.secondary',
          border: 1,
          borderColor: 'divider',
          bgcolor: 'background.paper',
          '&:hover': { color: 'error.main', borderColor: 'error.light' },
          '&:focus-visible': { outline: '2px solid', outlineColor: 'primary.main', outlineOffset: 2 }
        }}
      >
        <Box
          component="svg"
          viewBox="0 0 24 24"
          aria-hidden="true"
          sx={{
            width: 20,
            height: 20,
            fill: isFavorite ? 'currentColor' : 'none',
            stroke: 'currentColor',
            strokeWidth: 1.8,
            strokeLinecap: 'round',
            strokeLinejoin: 'round',
            opacity: isPending ? 0.5 : 1
          }}
        >
          <path d="M12 20.5 4.7 13a4.9 4.9 0 0 1 0-6.8 4.7 4.7 0 0 1 6.7 0l.6.6.6-.6a4.7 4.7 0 0 1 6.7 0 4.9 4.9 0 0 1 0 6.8z" />
        </Box>
      </IconButton>
      <Snackbar
        open={Boolean(snackbar)}
        autoHideDuration={3000}
        onClose={() => setSnackbar(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        {snackbar ? (
          <Alert severity={snackbar.severity} onClose={() => setSnackbar(null)} variant="filled" sx={{ width: '100%' }}>
            {snackbar.message}
          </Alert>
        ) : undefined}
      </Snackbar>
    </>
  );
}
