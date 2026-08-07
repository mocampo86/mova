import { apiClient } from './apiClient';

export interface GoogleLoginRequest {
  idToken: string;
}

export interface GoogleLoginResponse {
  accessToken: string;
  expiresAt: string;
  user: {
    id: string;
    email: string;
    fullName: string;
    phoneNumber: string | null;
    phoneVerified: boolean;
  };
  requiresProfileCompletion: boolean;
}

export interface CompleteComplexAdminRequest {
  phoneNumber: string;
  name: string;
  description: string;
  address: string;
  city: string;
  latitude?: number | null;
  longitude?: number | null;
  complexPhoneNumber: string;
  complexEmail: string;
}

export interface CompleteComplexAdminResponse {
  accessToken: string;
  expiresAt: string;
  user: {
    id: string;
    email: string;
    fullName: string;
    phoneNumber: string | null;
    phoneVerified: boolean;
  };
  complexId: string;
  requiresProfileCompletion: boolean;
}

export async function googleLogin(request: GoogleLoginRequest): Promise<GoogleLoginResponse> {
  return apiClient<GoogleLoginResponse>('/api/v1/auth/google', {
    method: 'POST',
    body: JSON.stringify(request)
  });
}

export async function completeComplexAdminProfile(
  request: CompleteComplexAdminRequest,
  accessToken: string
): Promise<CompleteComplexAdminResponse> {
  return apiClient<CompleteComplexAdminResponse>(
    '/api/v1/auth/complete-complex-admin',
    {
      method: 'POST',
      body: JSON.stringify(request)
    },
    accessToken
  );
}
