import { Typography } from '@mui/material';
import { useParams } from 'react-router-dom';

export default function ComplexAdminPage() {
  const { complexId } = useParams<{ complexId: string }>();

  return <Typography variant="h4">Complex Admin Dashboard - {complexId}</Typography>;
}
