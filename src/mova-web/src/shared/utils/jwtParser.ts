import type { AuthUser, UserComplexAssociation, UserRole } from '../../features/auth/authTypes';

interface JwtPayload {
  sub: string;
  email: string;
  name: string;
  roles?: UserRole | UserRole[];
  complexes?: string | UserComplexAssociation[];
}

export function parseJwtPayload(token: string): JwtPayload {
  const base64Url = token.split('.')[1];
  if (!base64Url) {
    throw new Error('Invalid JWT token');
  }

  const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
  const jsonPayload = decodeURIComponent(
    atob(base64)
      .split('')
      .map((char) => `%${`00${char.charCodeAt(0).toString(16)}`.slice(-2)}`)
      .join('')
  );

  return JSON.parse(jsonPayload) as JwtPayload;
}

function normalizeRoles(roles: UserRole | UserRole[] | undefined): UserRole[] {
  if (roles === undefined || roles === null) {
    return [];
  }

  return Array.isArray(roles) ? roles : [roles];
}

function normalizeComplexes(
  complexes: string | UserComplexAssociation[] | undefined
): UserComplexAssociation[] | undefined {
  if (complexes === undefined || complexes === null) {
    return undefined;
  }

  if (Array.isArray(complexes)) {
    return complexes;
  }

  if (typeof complexes === 'string') {
    try {
      return JSON.parse(complexes) as UserComplexAssociation[];
    } catch {
      return undefined;
    }
  }

  return [complexes as UserComplexAssociation];
}

export function mapJwtToUser(token: string): AuthUser {
  const payload = parseJwtPayload(token);

  return {
    id: payload.sub,
    email: payload.email,
    fullName: payload.name,
    roles: normalizeRoles(payload.roles),
    complexes: normalizeComplexes(payload.complexes)
  };
}
