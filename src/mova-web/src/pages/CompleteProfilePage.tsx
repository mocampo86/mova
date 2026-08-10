import { Box, Button, TextField, Typography } from '@mui/material';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { useCompleteProfile } from '../features/users/useCompleteProfile';

const phoneNumberPattern = /^\+[0-9](?:\s*[0-9]){6,14}$/;

const schema = z.object({
  phoneNumber: z
    .string()
    .min(8, 'Phone number must be at least 8 characters.')
    .max(50, 'Phone number must not exceed 50 characters.')
    .regex(
      phoneNumberPattern,
      "Phone number must be in international format starting with '+' followed by digits."
    )
});

type FormValues = z.infer<typeof schema>;

export default function CompleteProfilePage() {
  const { t } = useTranslation();
  const { mutate, isPending, error } = useCompleteProfile();
  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm<FormValues>({
    resolver: zodResolver(schema)
  });

  const onSubmit = (data: FormValues) => {
    mutate(data);
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 3, mt: 8 }}>
      <Typography variant="h4">{t('completeProfile.title')}</Typography>
      <Typography variant="body1">{t('completeProfile.subtitle')}</Typography>

      <Box
        component="form"
        onSubmit={handleSubmit(onSubmit)}
        sx={{ display: 'flex', flexDirection: 'column', gap: 2, width: '100%', maxWidth: 400 }}
      >
        <TextField
          {...register('phoneNumber')}
          label={t('completeProfile.phoneLabel')}
          placeholder={t('completeProfile.phonePlaceholder')}
          fullWidth
          error={Boolean(errors.phoneNumber)}
          helperText={errors.phoneNumber?.message}
        />

        <Button type="submit" variant="contained" disabled={isPending}>
          {t('completeProfile.continue')}
        </Button>

        {error && (
          <Typography color="error" variant="body2">
            {error.message}
          </Typography>
        )}
      </Box>
    </Box>
  );
}
