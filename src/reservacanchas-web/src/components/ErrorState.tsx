import Alert from '@mui/material/Alert';
import AlertTitle from '@mui/material/AlertTitle';

export interface ErrorStateProps {
  title?: string;
  message?: string;
}

export default function ErrorState({
  title = 'Something went wrong',
  message = 'An unexpected error occurred. Please try again later.'
}: ErrorStateProps) {
  return (
    <Alert severity="error" sx={{ mt: 2 }}>
      <AlertTitle>{title}</AlertTitle>
      {message}
    </Alert>
  );
}
