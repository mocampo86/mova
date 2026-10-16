import { useAuth } from '../features/auth/useAuth';
import PublicLayout from './PublicLayout';
import UserLayout from './UserLayout';

// Complex discovery remains public, while signed-in players keep their portal chrome.
export default function PlayerComplexLayout() {
  const { isAuthenticated } = useAuth();

  if (isAuthenticated) {
    return <UserLayout />;
  }

  return <PublicLayout />;
}
