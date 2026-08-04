import { Routes, Route } from 'react-router-dom';
import { RequireComplexAdmin } from '../features/auth/RequireComplexAdmin';
import { RequireRole } from '../features/auth/RequireRole';
import PublicLayout from '../layouts/PublicLayout';
import ComplexAdminPage from '../pages/ComplexAdminPage';
import HomePage from '../pages/HomePage';
import LoginPage from '../pages/LoginPage';
import NotFoundPage from '../pages/NotFoundPage';
import SuperAdminPage from '../pages/SuperAdminPage';
import UnauthorizedPage from '../pages/UnauthorizedPage';

export default function AppRouter() {
  return (
    <Routes>
      <Route path="/" element={<PublicLayout />}>
        <Route index element={<HomePage />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="unauthorized" element={<UnauthorizedPage />} />
        <Route
          path="admin/super"
          element={
            <RequireRole allowedRoles={['SuperAdmin']}>
              <SuperAdminPage />
            </RequireRole>
          }
        />
        <Route
          path="admin/complex/:complexId"
          element={
            <RequireComplexAdmin>
              <ComplexAdminPage />
            </RequireComplexAdmin>
          }
        />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
