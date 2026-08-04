import { useCallback, useMemo, useState, type ReactNode } from 'react';
import { AuthContext } from './AuthContext';
import type { AuthState } from './authTypes';
import { mapJwtToUser } from '../../shared/utils/jwtParser';

interface AuthProviderProps {
  children: ReactNode;
}

export default function AuthProvider({ children }: AuthProviderProps) {
  const [accessToken, setAccessToken] = useState<string | null>(null);

  const login = useCallback((token: string) => {
    setAccessToken(token);
  }, []);

  const logout = useCallback(() => {
    setAccessToken(null);
  }, []);

  const user = useMemo(() => (accessToken ? mapJwtToUser(accessToken) : null), [accessToken]);

  const value: AuthState = useMemo(
    () => ({
      accessToken,
      user,
      isAuthenticated: user !== null,
      login,
      logout
    }),
    [accessToken, user, login, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
