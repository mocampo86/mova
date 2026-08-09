import { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { zodResolver } from '@hookform/resolvers/zod';
import { Controller, useForm } from 'react-hook-form';
import { z } from 'zod';
import {
  Alert,
  Box,
  Button,
  Checkbox,
  Container,
  FormControl,
  FormControlLabel,
  FormGroup,
  FormHelperText,
  Skeleton,
  Switch,
  TextField,
  Typography
} from '@mui/material';
import { useCourt, useUpdateCourt } from '../features/courts/courtApi';
import { useSports } from '../features/complexes/complexApi';

const schema = z.object({
  name: z.string().min(1, 'Name is required.').max(255, 'Name must not exceed 255 characters.'),
  description: z
    .string()
    .min(1, 'Description is required.')
    .max(2000, 'Description must not exceed 2000 characters.'),
  surfaceType: z
    .string()
    .min(1, 'Surface type is required.')
    .max(100, 'Surface type must not exceed 100 characters.'),
  indoor: z.boolean(),
  sportIds: z.array(z.string()).default([])
});

type FormValues = z.infer<typeof schema>;

export default function EditCourtPage() {
  const { complexId = '', courtId = '' } = useParams();
  const navigate = useNavigate();
  const { data: court, isLoading: isCourtLoading, isError: isCourtError, error: courtError } = useCourt(complexId, courtId);
  const { mutate, isPending, error } = useUpdateCourt(complexId, courtId);
  const sports = useSports();

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors }
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: '',
      description: '',
      surfaceType: '',
      indoor: false,
      sportIds: []
    }
  });

  useEffect(() => {
    if (court) {
      reset({
        name: court.name,
        description: court.description,
        surfaceType: court.surfaceType,
        indoor: court.indoor,
        sportIds: court.sportIds ?? []
      });
    }
  }, [court, reset]);

  const onSubmit = (values: FormValues) => {
    mutate(values, {
      onSuccess: () => {
        navigate(`/admin/complex/${complexId}/courts`);
      }
    });
  };

  if (!complexId || !courtId) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error">The complex or court identifier is missing.</Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Edit court
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        Update the court information for your sports complex.
      </Typography>

      {isCourtError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {courtError?.message ?? 'The court could not be loaded. Please try again later.'}
        </Alert>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error.message}
        </Alert>
      )}

      {sports.isError && (
        <Alert severity="warning" sx={{ mb: 3 }}>
          Available sports could not be loaded. You can still update the court without sports.
        </Alert>
      )}

      {isCourtLoading ? (
        <Skeleton variant="rectangular" height={400} />
      ) : (
        <Box
          component="form"
          onSubmit={handleSubmit(onSubmit)}
          sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}
        >
          <TextField
            {...register('name')}
            label="Court name"
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
            {...register('surfaceType')}
            label="Surface type"
            placeholder="e.g., Synthetic, Grass, Concrete"
            fullWidth
            error={Boolean(errors.surfaceType)}
            helperText={errors.surfaceType?.message}
          />

          <Controller
            name="indoor"
            control={control}
            render={({ field }) => (
              <FormControl error={Boolean(errors.indoor)}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={field.value}
                      onChange={(event) => field.onChange(event.target.checked)}
                    />
                  }
                  label="Indoor court"
                />
                {errors.indoor && <FormHelperText>{errors.indoor.message}</FormHelperText>}
              </FormControl>
            )}
          />

          <FormControl
            component="fieldset"
            error={Boolean(errors.sportIds)}
            disabled={sports.isLoading || sports.isError}
          >
            <Typography component="legend" variant="subtitle2" sx={{ mb: 1 }}>
              Sports (optional)
            </Typography>

            {sports.isLoading ? (
              <Skeleton variant="rectangular" height={56} />
            ) : (
              <Controller
                name="sportIds"
                control={control}
                render={({ field }) => (
                  <FormGroup>
                    {sports.data?.map((sport) => (
                      <FormControlLabel
                        key={sport.id}
                        control={
                          <Checkbox
                            checked={field.value.includes(sport.id)}
                            onChange={(event) => {
                              const value = event.target.checked
                                ? [...field.value, sport.id]
                                : field.value.filter((id) => id !== sport.id);
                              field.onChange(value);
                            }}
                          />
                        }
                        label={sport.name}
                      />
                    ))}
                  </FormGroup>
                )}
              />
            )}

            {errors.sportIds && <FormHelperText>{errors.sportIds.message}</FormHelperText>}
          </FormControl>

          <Button
            type="submit"
            variant="contained"
            disabled={isPending || isCourtLoading || sports.isLoading}
            sx={{ alignSelf: 'flex-start' }}
          >
            Update court
          </Button>
        </Box>
      )}
    </Container>
  );
}
