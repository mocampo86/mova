import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { useAuth } from '../auth/useAuth';
import type { AuditLogFilters, AuditLogPagedResult } from './auditTypes';

function toUtcIso(value: string): string | null {
  if (!value) {
    return null;
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return null;
  }

  return date.toISOString();
}

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

  const fromUtc = toUtcIso(filters.from);
  if (fromUtc) {
    params.set('from', fromUtc);
  }

  const toUtc = toUtcIso(filters.to);
  if (toUtc) {
    params.set('to', toUtc);
  }

  return useQuery({
    queryKey: ['audit-logs', filters],
    queryFn: () =>
      apiClient<AuditLogPagedResult>(`/api/v1/admin/audit-logs?${params}`, {}, accessToken ?? undefined),
    enabled: Boolean(accessToken) && enabled
  });
}
