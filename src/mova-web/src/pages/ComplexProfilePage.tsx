import { useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Card,
  CardContent,
  Container,
  Skeleton,
  Stack,
  Typography
} from '@mui/material';
import { useComplexDashboard } from '../features/complexes/complexApi';
import ComplexStatusToggle from '../features/complexes/ComplexStatusToggle';

function formatLastUpdated(isoString?: string | null) {
  if (!isoString) return 'Not updated yet';
  const date = new Date(isoString);
  return `Last updated ${date.toLocaleDateString()} ${date.toLocaleTimeString()}`;
}

export default function ComplexProfilePage() {
  const { complexId = '' } = useParams();
  const { data, isLoading, isError } = useComplexDashboard(complexId);

  if (isError) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error">The profile could not be loaded. Please try again later.</Alert>
      </Container>
    );
  }

  const complex = data?.complex;

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Stack spacing={4}>
        {isLoading || !complex ? (
          <>
            <Skeleton variant="text" width="60%" height={48} />
            <Skeleton variant="text" width="40%" />
            <Card variant="outlined">
              <CardContent>
                <Skeleton variant="text" width="40%" />
                <Skeleton variant="text" width="60%" />
              </CardContent>
            </Card>
          </>
        ) : (
          <>
            <Box>
              <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
                {complex.name}
              </Typography>
              <Typography color="text.secondary" sx={{ mt: 1 }}>
                {formatLastUpdated(complex.lastUpdatedAt)}
              </Typography>
            </Box>

            <Card variant="outlined">
              <CardContent>
                <Stack
                  direction={{ xs: 'column', sm: 'row' }}
                  spacing={2}
                  alignItems="center"
                  justifyContent="space-between"
                  flexWrap="wrap"
                >
                  <Box>
                    <Typography component="h2" variant="h6">
                      Public visibility
                    </Typography>
                    <Typography color="text.secondary" variant="body2">
                      {complex.status === 'Active'
                        ? 'This complex is visible to the public and can be found in listings.'
                        : 'This complex is hidden from public listings.'}
                    </Typography>
                  </Box>
                  <ComplexStatusToggle complexId={complexId} status={complex.status} />
                </Stack>
              </CardContent>
            </Card>
          </>
        )}
      </Stack>
    </Container>
  );
}
