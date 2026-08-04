export type UserRole = 'User' | 'ComplexAdmin' | 'SuperAdmin';

export interface UserComplexAssociation {
  complexId: string;
  role: UserRole;
}

export interface AuthUser {
  id: string;
  email: string;
  fullName: string;
  roles: UserRole[];
  complexes?: UserComplexAssociation[];
}

export interface AuthState {
  accessToken: string | null;
  user: AuthUser | null;
  isAuthenticated: boolean;
  requiresProfileCompletion: boolean;
  login: (accessToken: string, requiresProfileCompletion?: boolean) => void;
  logout: () => void;
  completeProfile: () => void;
}
