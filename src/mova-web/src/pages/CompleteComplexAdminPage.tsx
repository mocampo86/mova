import { Box, Button, Stack, TextField, Typography } from '@mui/material';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { useCompleteComplexAdminProfile } from '../features/users/useCompleteComplexAdminProfile';

const phoneNumberPattern = /^\+[0-9](?:\s*[0-9]){6,14}$/;

const nullableCoordinate = <T extends number>(min: T, max: T, message: string) =>
  z.preprocess(
    (value) => {
      if (value === '' || value === null || value === undefined) {
        return null;
      }

      const parsed = Number(value);
      return Number.isNaN(parsed) ? null : parsed;
    },
    z.number({ invalid_type_error: 'Value must be a valid number.' }).min(min, message).max(max, message).nullable().optional()
  );

const schema = z.object({
  phoneNumber: z
    .string()
    .min(8, 'Phone number must be at least 8 characters.')
    .max(50, 'Phone number must not exceed 50 characters.')
    .regex(
      phoneNumberPattern,
      "Phone number must be in international format starting with '+' followed by digits."
    ),
  name: z
    .string()
    .min(1, 'Complex name is required.')
    .max(255, 'Complex name must not exceed 255 characters.'),
  description: z
    .string()
    .min(1, 'Description is required.')
    .max(2000, 'Description must not exceed 2000 characters.'),
  address: z
    .string()
    .min(1, 'Address is required.')
    .max(255, 'Address must not exceed 255 characters.'),
  city: z
    .string()
    .min(1, 'City is required.')
    .max(255, 'City must not exceed 255 characters.'),
  latitude: nullableCoordinate(
    -90,
    90,
    'Latitude must be between -90 and 90.'
  ),
  longitude: nullableCoordinate(
    -180,
    180,
    'Longitude must be between -180 and 180.'
  ),
  complexPhoneNumber: z
    .string()
    .min(8, 'Phone number must be at least 8 characters.')
    .max(50, 'Phone number must not exceed 50 characters.')
    .regex(
      phoneNumberPattern,
      "Phone number must be in international format starting with '+' followed by digits."
    ),
  complexEmail: z
    .string()
    .min(1, 'Email is required.')
    .email('Email is not valid.')
    .max(255, 'Email must not exceed 255 characters.')
});

type FormValues = z.infer<typeof schema>;

export default function CompleteComplexAdminPage() {
  const { mutate, isPending, error } = useCompleteComplexAdminProfile();
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
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        gap: 3,
        mt: 4,
        mb: 8,
        px: 2
      }}
    >
      <Typography variant="h4">Complete your complex profile</Typography>
      <Typography variant="body1" textAlign="center" sx={{ maxWidth: 500 }}>
        We need a few details about you and your sports complex. Your request will be reviewed before the complex goes live.
      </Typography>

      <Box
        component="form"
        onSubmit={handleSubmit(onSubmit)}
        sx={{ display: 'flex', flexDirection: 'column', gap: 2, width: '100%', maxWidth: 600 }}
      >
        <Typography variant="h6">Your contact information</Typography>
        <TextField
          {...register('phoneNumber')}
          label="Your phone number"
          placeholder="+54 11 1234 5678"
          fullWidth
          error={Boolean(errors.phoneNumber)}
          helperText={errors.phoneNumber?.message}
        />

        <Typography variant="h6" sx={{ mt: 2 }}>
          Complex information
        </Typography>
        <TextField
          {...register('name')}
          label="Complex name"
          fullWidth
          error={Boolean(errors.name)}
          helperText={errors.name?.message}
        />
        <TextField
          {...register('description')}
          label="Description"
          multiline
          rows={3}
          fullWidth
          error={Boolean(errors.description)}
          helperText={errors.description?.message}
        />
        <TextField
          {...register('address')}
          label="Address"
          fullWidth
          error={Boolean(errors.address)}
          helperText={errors.address?.message}
        />
        <TextField
          {...register('city')}
          label="City"
          fullWidth
          error={Boolean(errors.city)}
          helperText={errors.city?.message}
        />
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            {...register('latitude')}
            label="Latitude (optional)"
            type="number"
            fullWidth
            error={Boolean(errors.latitude)}
            helperText={errors.latitude?.message}
          />
          <TextField
            {...register('longitude')}
            label="Longitude (optional)"
            type="number"
            fullWidth
            error={Boolean(errors.longitude)}
            helperText={errors.longitude?.message}
          />
        </Stack>
        <TextField
          {...register('complexPhoneNumber')}
          label="Complex phone number"
          placeholder="+54 11 1234 5678"
          fullWidth
          error={Boolean(errors.complexPhoneNumber)}
          helperText={errors.complexPhoneNumber?.message}
        />
        <TextField
          {...register('complexEmail')}
          label="Complex email"
          type="email"
          fullWidth
          error={Boolean(errors.complexEmail)}
          helperText={errors.complexEmail?.message}
        />

        <Button type="submit" variant="contained" disabled={isPending}>
          Submit for review
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
