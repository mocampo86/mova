import type { PagedResult } from '../complexes/complexTypes';
import type { Reservation } from '../reservations/reservationTypes';

export interface ComplexUser {
  id: string;
  email: string;
  fullName: string;
  phoneNumber: string | null;
  phoneVerified: boolean;
  isBlocked: boolean;
  blockId: string | null;
  blockReason: string | null;
  blockedUntil: string | null;
}

export interface UserListFilters {
  page: number;
  pageSize: number;
  search: string;
  sort: string;
}

export interface BlockUserRequest {
  userId: string;
  reason?: string;
  blockedUntil?: string;
}

export interface BlockedUser {
  id: string;
  sportsComplexId: string;
  userId: string;
  reason: string | null;
  blockedAt: string;
  blockedUntil: string | null;
  blockedByUserId: string;
  status: string;
}

export interface UserReservationFilters {
  page: number;
  pageSize: number;
  sort: string;
}

export type ComplexUserListResult = PagedResult<ComplexUser>;
export type UserReservationListResult = PagedResult<Reservation>;
