import type { PagedResult } from '../complexes/complexTypes';

export interface AuditLog {
  id: string;
  userId: string | null;
  sportsComplexId: string | null;
  action: string;
  entityType: string;
  entityId: string;
  createdAt: string;
  metadata: string | null;
}

export interface AuditLogFilters {
  page: number;
  pageSize: number;
  action: string;
  entityType: string;
  entityId: string;
  sportsComplexId: string;
  userId: string;
  from: string;
  to: string;
}

export type AuditLogPagedResult = PagedResult<AuditLog>;
