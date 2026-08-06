import { describe, it, expect } from 'vitest';
import { mapJwtToUser, parseJwtPayload } from './jwtParser';

function createToken(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'none', typ: 'JWT' }));
  const body = btoa(JSON.stringify(payload));
  return `${header}.${body}.signature`;
}

describe('parseJwtPayload', () => {
  it('parses a JWT payload with standard claims', () => {
    const payload = {
      sub: 'user-id',
      email: 'user@example.com',
      name: 'Test User',
      roles: ['User']
    };
    const token = createToken(payload);

    const result = parseJwtPayload(token);

    expect(result.sub).toBe('user-id');
    expect(result.email).toBe('user@example.com');
    expect(result.name).toBe('Test User');
    expect(result.roles).toEqual(['User']);
  });

  it('parses a JWT payload with complex associations', () => {
    const payload = {
      sub: 'admin-id',
      email: 'admin@example.com',
      name: 'Admin User',
      roles: ['User', 'ComplexAdmin'],
      complexes: [{ complexId: 'complex-1', role: 'ComplexAdmin' }]
    };
    const token = createToken(payload);

    const result = parseJwtPayload(token);

    expect(result.complexes).toEqual([{ complexId: 'complex-1', role: 'ComplexAdmin' }]);
  });

  it('throws for an invalid token', () => {
    expect(() => parseJwtPayload('invalid-token')).toThrow('Invalid JWT token');
  });
});

describe('mapJwtToUser', () => {
  it('maps a JWT token to an AuthUser', () => {
    const payload = {
      sub: 'user-id',
      email: 'user@example.com',
      name: 'Test User',
      roles: ['SuperAdmin']
    };
    const token = createToken(payload);

    const user = mapJwtToUser(token);

    expect(user.id).toBe('user-id');
    expect(user.email).toBe('user@example.com');
    expect(user.fullName).toBe('Test User');
    expect(user.roles).toEqual(['SuperAdmin']);
  });

  it('maps a JWT token with a single role as a string and complexes as a JSON string', () => {
    const payload = {
      sub: 'user-id',
      email: 'user@example.com',
      name: 'Test User',
      roles: 'User',
      complexes: '[{"complexId":"complex-1","role":"ComplexAdmin"}]'
    };
    const token = createToken(payload);

    const user = mapJwtToUser(token);

    expect(user.id).toBe('user-id');
    expect(user.email).toBe('user@example.com');
    expect(user.fullName).toBe('Test User');
    expect(user.roles).toEqual(['User']);
    expect(user.complexes).toEqual([{ complexId: 'complex-1', role: 'ComplexAdmin' }]);
  });
});
