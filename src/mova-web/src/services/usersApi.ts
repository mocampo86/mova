import { apiClient } from './apiClient';
import type { PagedResult } from '../features/complexes/complexTypes';
import type { Reservation } from '../features/reservations/reservationTypes';

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

export interface UserBlockInfo {
  id: string;
  complexId: string;
  complexName: string;
  reason?: string | null;
  blockedAt: string;
  blockedUntil?: string | null;
}

export interface MyBlockStatusInfo {
  isBlocked: boolean;
  complexId: string;
  complexName?: string | null;
  reason?: string | null;
  blockedAt?: string | null;
  blockedUntil?: string | null;
}

export interface ReservationHistorySummaryInfo {
  totalItems: number;
  recentReservations: Reservation[];
}

export interface UserDashboardInfo {
  user: UserInfo;
  upcomingReservations: PagedResult<Reservation>;
  historySummary: ReservationHistorySummaryInfo;
  activeBlocks: UserBlockInfo[];
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

export async function getUserDashboard(
  accessToken: string,
  upcomingPageSize = 5,
  historyPageSize = 3
): Promise<UserDashboardInfo> {
  const params = new URLSearchParams({
    upcomingPageSize: String(upcomingPageSize),
    historyPageSize: String(historyPageSize)
  });

  return apiClient<UserDashboardInfo>(`/api/v1/users/me/dashboard?${params}`, {}, accessToken);
}

export async function getMyBlockStatus(
  complexId: string,
  accessToken: string
): Promise<MyBlockStatusInfo> {
  return apiClient<MyBlockStatusInfo>(`/api/v1/users/me/blocks/${complexId}`, {}, accessToken);
}
