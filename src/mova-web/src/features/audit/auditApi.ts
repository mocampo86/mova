import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { useAuth } from '../auth/useAuth';
import type { AuditLogFilters, AuditLogPagedResult } from './auditTypes';

export function useAuditLogs(filters: AuditLogFilters, enabled = true) {
  const { accessToken } = useAuth();

  const params = new URLSearchParams({
    page: String(filters.page + 1),
    pageSize: String(filters.pageSize)
  });

  if (filters.action) {
    params.set('action', filters.action);
  }

  if (filters.entityType) {
    params.set('entityType', filters.entityType);
  }

  if (filters.entityId) {
    params.set('entityId', filters.entityId);
  }

  if (filters.sportsComplexId) {
    params.set('sportsComplexId', filters.sportsComplexId);
  }

  if (filters.userId) {
    params.set('userId', filters.userId);
  }

  if (filters.from) {
    params.set('from', filters.from);
  }

  if (filters.to) {
    params.set('to', filters.to);
  }

  return useQuery({
    queryKey: ['audit-logs', filters],
    queryFn: () =>
      apiClient<AuditLogPagedResult>(`/api/v1/admin/audit-logs?${params}`, {}, accessToken ?? undefined),
    enabled: Boolean(accessToken) && enabled
  });
}
