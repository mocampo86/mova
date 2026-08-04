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
    roles: string[];
    isProfileCompleted: boolean;
  };
}

export async function googleLogin(request: GoogleLoginRequest): Promise<GoogleLoginResponse> {
  return apiClient<GoogleLoginResponse>('/api/v1/auth/google', {
    method: 'POST',
    body: JSON.stringify(request)
  });
}
