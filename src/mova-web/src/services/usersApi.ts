import { apiClient } from './apiClient';

export interface CompleteProfileRequest {
  phoneNumber: string;
}

export interface UserInfo {
  id: string;
  email: string;
  fullName: string;
  phoneNumber: string | null;
  phoneVerified: boolean;
}

export async function completeProfile(
  request: CompleteProfileRequest,
  accessToken: string
): Promise<UserInfo> {
  return apiClient<UserInfo>(
    '/api/v1/users/me',
    {
      method: 'PATCH',
      body: JSON.stringify(request)
    },
    accessToken
  );
}
