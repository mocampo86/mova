import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { useAuth } from '../auth/useAuth';
import type {
  BlockUserRequest,
  BlockedUser,
  ComplexUserListResult,
  UserListFilters,
  UserReservationFilters,
  UserReservationListResult
} from './userAdminTypes';

function generateIdempotencyKey(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }

  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = Math.random() * 16 | 0;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

export function useSearchUsers(complexId: string, filters: UserListFilters, enabled = true) {
  const { accessToken } = useAuth();
  const params = new URLSearchParams({
    page: String(filters.page + 1),
    pageSize: String(filters.pageSize),
    sort: filters.sort
  });

  if (filters.search.trim()) {
    params.set('search', filters.search.trim());
  }

  return useQuery({
    queryKey: ['search-users', complexId, filters],
    queryFn: () =>
      apiClient<ComplexUserListResult>(
        `/api/v1/complexes/${complexId}/users/search?${params}`,
        {},
        accessToken ?? undefined
      ),
    enabled: Boolean(complexId && accessToken) && enabled
  });
}

export function useComplexUsers(complexId: string, filters: UserListFilters) {
  const { accessToken } = useAuth();
  const params = new URLSearchParams({
    page: String(filters.page + 1),
    pageSize: String(filters.pageSize),
    sort: filters.sort
  });

  if (filters.search.trim()) {
    params.set('search', filters.search.trim());
  }

  return useQuery({
    queryKey: ['complex-users', complexId, filters],
    queryFn: () =>
      apiClient<ComplexUserListResult>(
        `/api/v1/complexes/${complexId}/users?${params}`,
        {},
        accessToken ?? undefined
      ),
    enabled: Boolean(complexId && accessToken)
  });
}

export function useUserReservations(
  complexId: string,
  userId: string,
  filters: UserReservationFilters
) {
  const { accessToken } = useAuth();
  const params = new URLSearchParams({
    page: String(filters.page + 1),
    pageSize: String(filters.pageSize),
    sort: filters.sort
  });

  return useQuery({
    queryKey: ['user-reservations', complexId, userId, filters],
    queryFn: () =>
      apiClient<UserReservationListResult>(
        `/api/v1/complexes/${complexId}/users/${userId}/reservations?${params}`,
        {},
        accessToken ?? undefined
      ),
    enabled: Boolean(complexId && userId && accessToken)
  });
}

export async function blockUser(
  complexId: string,
  request: BlockUserRequest,
  accessToken: string
): Promise<BlockedUser> {
  return apiClient<BlockedUser>(
    `/api/v1/complexes/${complexId}/blocked-users`,
    {
      method: 'POST',
      body: JSON.stringify(request),
      headers: { 'Idempotency-Key': generateIdempotencyKey() }
    },
    accessToken
  );
}

export function useBlockUser(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<BlockedUser, Error, BlockUserRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to block a user.');
      }

      return blockUser(complexId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['complex-users', complexId] });
      queryClient.invalidateQueries({ queryKey: ['complex-dashboard', complexId] });
    }
  });
}

export async function unblockUser(
  complexId: string,
  blockedUserId: string,
  accessToken: string
): Promise<BlockedUser> {
  return apiClient<BlockedUser>(
    `/api/v1/complexes/${complexId}/blocked-users/${blockedUserId}`,
    {
      method: 'DELETE'
    },
    accessToken
  );
}

export function useUnblockUser(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<BlockedUser, Error, string>({
    mutationFn: async (blockedUserId) => {
      if (!accessToken) {
        throw new Error('You must be logged in to unblock a user.');
      }

      return unblockUser(complexId, blockedUserId, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['complex-users', complexId] });
      queryClient.invalidateQueries({ queryKey: ['complex-dashboard', complexId] });
    }
  });
}
