import { useCallback, useMemo, useState, type ReactNode } from 'react';
import { AuthContext } from './AuthContext';
import type { AuthState } from './authTypes';
import { mapJwtToUser } from '../../shared/utils/jwtParser';

interface AuthProviderProps {
  children: ReactNode;
}

export default function AuthProvider({ children }: AuthProviderProps) {
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [requiresProfileCompletion, setRequiresProfileCompletion] = useState(false);

  const login = useCallback((token: string, needsProfileCompletion = false) => {
    setAccessToken(token);
    setRequiresProfileCompletion(needsProfileCompletion);
  }, []);

  const logout = useCallback(() => {
    setAccessToken(null);
    setRequiresProfileCompletion(false);
  }, []);

  const completeProfile = useCallback(() => {
    setRequiresProfileCompletion(false);
  }, []);

  const user = useMemo(() => (accessToken ? mapJwtToUser(accessToken) : null), [accessToken]);

  const value: AuthState = useMemo(
    () => ({
      accessToken,
      user,
      isAuthenticated: user !== null,
      requiresProfileCompletion,
      login,
      logout,
      completeProfile
    }),
    [accessToken, user, requiresProfileCompletion, login, logout, completeProfile]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
