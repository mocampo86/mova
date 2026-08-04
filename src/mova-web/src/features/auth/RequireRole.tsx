import { Navigate } from 'react-router-dom';
import type { UserRole } from './authTypes';
import { useAuth } from './useAuth';

interface RequireRoleProps {
  allowedRoles: UserRole[];
  children: React.ReactNode;
}

export function RequireRole({ allowedRoles, children }: RequireRoleProps) {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated || !user) {
    return <Navigate to="/login" replace />;
  }

  const hasAllowedRole = user.roles.some((role) => allowedRoles.includes(role));
  if (!hasAllowedRole) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <>{children}</>;
}
