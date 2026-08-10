import { Routes, Route } from 'react-router-dom';
import { RequireComplexAdmin } from '../features/auth/RequireComplexAdmin';
import { RequireRole } from '../features/auth/RequireRole';
import ComplexAdminLayout from '../layouts/ComplexAdminLayout';
import PublicLayout from '../layouts/PublicLayout';
import CompleteComplexAdminPage from '../pages/CompleteComplexAdminPage';
import CompleteProfilePage from '../pages/CompleteProfilePage';
import ComplexAdminPage from '../pages/ComplexAdminPage';
import ComplexAdminPlaceholderPage from '../pages/ComplexAdminPlaceholderPage';
import ComplexCourtsPage from '../pages/ComplexCourtsPage';
import ComplexReservationsPage from '../pages/ComplexReservationsPage';
import ComplexUsersPage from '../pages/ComplexUsersPage';
import CreateCourtPage from '../pages/CreateCourtPage';
import EditCourtPage from '../pages/EditCourtPage';
import ComplexDetailPage from '../pages/ComplexDetailPage';
import ComplexProfilePage from '../pages/ComplexProfilePage';
import ComplexesPage from '../pages/ComplexesPage';
import HomePage from '../pages/HomePage';
import LoginPage from '../pages/LoginPage';
import NotFoundPage from '../pages/NotFoundPage';
import SuperAdminPage from '../pages/SuperAdminPage';
import UnauthorizedPage from '../pages/UnauthorizedPage';
import UserHomePage from '../pages/UserHomePage';

export default function AppRouter() {
  return (
    <Routes>
      <Route
        path="/admin/complex/:complexId"
        element={
          <RequireComplexAdmin>
            <ComplexAdminLayout />
          </RequireComplexAdmin>
        }
      >
        <Route index element={<ComplexAdminPage />} />
        <Route path="profile" element={<ComplexProfilePage />} />
        <Route path="courts" element={<ComplexCourtsPage />} />
        <Route path="courts/new" element={<CreateCourtPage />} />
        <Route path="courts/:courtId/edit" element={<EditCourtPage />} />
        <Route path="reservations" element={<ComplexReservationsPage />} />
        <Route path="users" element={<ComplexUsersPage />} />
        <Route path="*" element={<ComplexAdminPlaceholderPage />} />
      </Route>
      <Route path="/" element={<PublicLayout />}>
        <Route index element={<HomePage />} />
        <Route path="complexes" element={<ComplexesPage />} />
        <Route path="complexes/:complexId" element={<ComplexDetailPage />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="unauthorized" element={<UnauthorizedPage />} />
        <Route
          path="complete-profile"
          element={
            <RequireRole allowedRoles={['User', 'ComplexAdmin', 'SuperAdmin']}>
              <CompleteProfilePage />
            </RequireRole>
          }
        />
        <Route
          path="complete-complex-admin"
          element={
            <RequireRole allowedRoles={['User', 'ComplexAdmin', 'SuperAdmin']}>
              <CompleteComplexAdminPage />
            </RequireRole>
          }
        />
        <Route
          path="user"
          element={
            <RequireRole allowedRoles={['User', 'ComplexAdmin', 'SuperAdmin']}>
              <UserHomePage />
            </RequireRole>
          }
        />
        <Route
          path="admin/super"
          element={
            <RequireRole allowedRoles={['SuperAdmin']}>
              <SuperAdminPage />
            </RequireRole>
          }
        />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
