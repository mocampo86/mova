import { Navigate, useParams } from 'react-router-dom';
import { useAuth } from './useAuth';

interface RequireComplexAdminProps {
  children: React.ReactNode;
}

export function RequireComplexAdmin({ children }: RequireComplexAdminProps) {
  const { user, isAuthenticated } = useAuth();
  const { complexId } = useParams<{ complexId: string }>();

  if (!isAuthenticated || !user) {
    return <Navigate to="/login" replace />;
  }

  if (user.roles.includes('SuperAdmin')) {
    return <>{children}</>;
  }

  if (!user.roles.includes('ComplexAdmin') || !complexId) {
    return <Navigate to="/unauthorized" replace />;
  }

  const isAdminOfComplex = user.complexes?.some(
    (association) => association.complexId === complexId && association.role === 'ComplexAdmin'
  );

  if (!isAdminOfComplex) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <>{children}</>;
}
