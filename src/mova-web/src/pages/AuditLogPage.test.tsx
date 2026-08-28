import { cleanup, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useAuditLogs } from '../features/audit/auditApi';
import AuditLogPage from './AuditLogPage';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/audit/auditApi');

const mockUseAuditLogs = vi.mocked(useAuditLogs);

const emptyPagedResult = {
  items: [],
  page: 1,
  pageSize: 25,
  totalItems: 0,
  totalPages: 0
};

beforeEach(() => {
  cleanup();
  vi.resetAllMocks();
});

describe('AuditLogPage', () => {
  it('renders the audit log table with entries', () => {
    mockUseAuditLogs.mockReturnValue({
      data: {
        ...emptyPagedResult,
        items: [
          {
            id: 'audit-1',
            userId: 'user-1',
            sportsComplexId: 'complex-1',
            action: 'Court.Create',
            entityType: 'Court',
            entityId: 'court-1',
            createdAt: '2026-08-28T10:00:00Z',
            metadata: '{"name":"Court One"}'
          }
        ],
        totalItems: 1,
        totalPages: 1
      },
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useAuditLogs>);

    renderWithAuth(<AuditLogPage />);

    expect(screen.getByRole('heading', { name: 'Audit log' })).toBeTruthy();
    expect(screen.getByText('Court.Create')).toBeTruthy();
    expect(screen.getByText('Court')).toBeTruthy();
    expect(screen.getByText('court-1')).toBeTruthy();
  });

  it('renders an empty state when no audit logs are available', () => {
    mockUseAuditLogs.mockReturnValue({
      data: emptyPagedResult,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useAuditLogs>);

    renderWithAuth(<AuditLogPage />);

    expect(screen.getByText('No audit log entries match the selected filters.')).toBeTruthy();
  });

  it('renders an error alert when the query fails', () => {
    mockUseAuditLogs.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      error: new Error('Failed to load audit logs')
    } as unknown as ReturnType<typeof useAuditLogs>);

    renderWithAuth(<AuditLogPage />);

    expect(screen.getByText('Failed to load audit logs')).toBeTruthy();
  });
});
