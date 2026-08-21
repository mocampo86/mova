import { cleanup, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithAuth } from '../../test-utils';
import UserSearchDialog from './UserSearchDialog';
import { useSearchUsers } from './userAdminApi';
import type { ComplexUser } from './userAdminTypes';

vi.mock('./userAdminApi');

const mockUsers: ComplexUser[] = [
  {
    id: 'user-1',
    email: 'john@example.com',
    fullName: 'John Doe',
    phoneNumber: '+54 11 1234 5678',
    phoneVerified: true,
    isBlocked: false,
    blockId: null,
    blockReason: null,
    blockedUntil: null
  },
  {
    id: 'user-2',
    email: 'jane@example.com',
    fullName: 'Jane Smith',
    phoneNumber: null,
    phoneVerified: false,
    isBlocked: true,
    blockId: 'block-1',
    blockReason: 'No-show',
    blockedUntil: null
  }
];

function setupMock(options: Partial<ReturnType<typeof useSearchUsers>> = {}) {
  vi.mocked(useSearchUsers).mockReturnValue({
    data: { items: mockUsers, page: 1, pageSize: 10, totalItems: 2, totalPages: 1 },
    isLoading: false,
    isError: false,
    error: null,
    isSuccess: true,
    ...options
  } as unknown as ReturnType<typeof useSearchUsers>);
}

describe('UserSearchDialog', () => {
  beforeEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('renders a search field and result table when open', () => {
    setupMock();
    renderWithAuth(
      <UserSearchDialog complexId="complex-1" open onClose={vi.fn()} onSelect={vi.fn()} />
    );

    expect(screen.getByRole('dialog')).toBeTruthy();
    expect(screen.getByRole('textbox')).toBeTruthy();
    expect(screen.getByText('John Doe')).toBeTruthy();
    expect(screen.getByText('Jane Smith')).toBeTruthy();
  });

  it('does not render when closed', () => {
    setupMock();
    renderWithAuth(
      <UserSearchDialog complexId="complex-1" open={false} onClose={vi.fn()} onSelect={vi.fn()} />
    );

    expect(screen.queryByRole('dialog')).toBeNull();
  });

  it('calls onSelect with the selected user', async () => {
    setupMock();
    const onSelect = vi.fn();
    const user = userEvent.setup();
    renderWithAuth(
      <UserSearchDialog complexId="complex-1" open onClose={vi.fn()} onSelect={onSelect} />
    );

    const selectButton = screen.getAllByRole('button', { name: /select/i })[0];
    await user.click(selectButton);

    expect(onSelect).toHaveBeenCalledWith(mockUsers[0]);
  });

  it('disables the select button for blocked users', () => {
    setupMock();
    renderWithAuth(
      <UserSearchDialog complexId="complex-1" open onClose={vi.fn()} onSelect={vi.fn()} />
    );

    const selectButtons = screen.getAllByRole('button', { name: /select/i });
    expect(selectButtons[0].hasAttribute('disabled')).toBe(false);
    expect(selectButtons[1].hasAttribute('disabled')).toBe(true);
  });

  it('triggers a search when the search button is clicked', async () => {
    setupMock();
    const user = userEvent.setup();
    renderWithAuth(
      <UserSearchDialog complexId="complex-1" open onClose={vi.fn()} onSelect={vi.fn()} />
    );

    const input = screen.getByRole('textbox');
    await user.type(input, 'john');

    const searchButton = screen.getByRole('button', { name: /search/i });
    await user.click(searchButton);

    await waitFor(() => {
      expect(useSearchUsers).toHaveBeenCalled();
    });
  });

  it('renders an empty state when no users are found', () => {
    setupMock({ data: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 } });
    renderWithAuth(
      <UserSearchDialog complexId="complex-1" open onClose={vi.fn()} onSelect={vi.fn()} />
    );

    expect(screen.getByText('No users found for this complex.')).toBeTruthy();
  });

  it('calls onClose when the cancel button is clicked', async () => {
    setupMock();
    const onClose = vi.fn();
    const user = userEvent.setup();
    renderWithAuth(
      <UserSearchDialog complexId="complex-1" open onClose={onClose} onSelect={vi.fn()} />
    );

    const cancelButton = screen.getByRole('button', { name: /cancel/i });
    await user.click(cancelButton);

    expect(onClose).toHaveBeenCalled();
  });
});
