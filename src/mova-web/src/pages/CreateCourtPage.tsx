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
import { useTranslation } from 'react-i18next';
import ApiErrorMessage from '../components/ApiErrorMessage';
import { useCreateCourt } from '../features/courts/courtApi';
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

export default function CreateCourtPage() {
  const { t } = useTranslation();
  const { complexId = '' } = useParams();
  const navigate = useNavigate();
  const { mutate, isPending, error } = useCreateCourt(complexId);
  const sports = useSports();

  const {
    register,
    handleSubmit,
    control,
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

  const onSubmit = (values: FormValues) => {
    mutate(values, {
      onSuccess: () => {
        navigate(`/admin/complex/${complexId}/courts`);
      }
    });
  };

  if (!complexId) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error">{t('admin.createCourt.missingId')}</Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        {t('admin.createCourt.title')}
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        {t('admin.createCourt.subtitle')}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          <ApiErrorMessage error={error} />
        </Alert>
      )}

      {sports.isError && (
        <Alert severity="warning" sx={{ mb: 3 }}>
          {t('admin.createCourt.sportsError')}
        </Alert>
      )}

      <Box
        component="form"
        onSubmit={handleSubmit(onSubmit)}
        sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}
      >
        <TextField
          {...register('name')}
          label={t('admin.createCourt.name')}
          fullWidth
          error={Boolean(errors.name)}
          helperText={errors.name?.message}
        />

        <TextField
          {...register('description')}
          label={t('common.description')}
          multiline
          rows={3}
          fullWidth
          error={Boolean(errors.description)}
          helperText={errors.description?.message}
        />

        <TextField
          {...register('surfaceType')}
          label={t('admin.createCourt.surfaceType')}
          placeholder={t('admin.createCourt.surfacePlaceholder')}
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
                label={t('admin.createCourt.indoor')}
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
            {t('admin.createCourt.sports')}
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
          disabled={isPending || sports.isLoading}
          sx={{ alignSelf: 'flex-start' }}
        >
          {t('admin.createCourt.create')}
        </Button>
      </Box>
    </Container>
  );
}
