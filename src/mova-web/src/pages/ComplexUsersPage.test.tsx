import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { Routes, Route } from 'react-router-dom';
import ComplexUsersPage from './ComplexUsersPage';
import {
  useBlockUser,
  useComplexUsers,
  useUnblockUser,
  useUserReservations
} from '../features/users/userAdminApi';
import { renderWithAuth } from '../test-utils';
import type { ComplexUser } from '../features/users/userAdminTypes';

vi.mock('../features/users/userAdminApi');

const mockUser: ComplexUser = {
  id: 'user-1',
  email: 'user@example.com',
  fullName: 'Test User',
  phoneNumber: '+598 99 123 456',
  phoneVerified: false,
  isBlocked: false,
  blockId: null,
  blockReason: null,
  blockedUntil: null
};

const mockBlockedUser: ComplexUser = {
  ...mockUser,
  isBlocked: true,
  blockId: 'block-1',
  blockReason: 'Spam',
  blockedUntil: null
};

const mockUsers = {
  items: [mockUser],
  page: 1,
  pageSize: 10,
  totalItems: 1,
  totalPages: 1
};

const mockReservations = {
  items: [
    {
      id: 'reservation-1',
      complexId: 'complex-1',
      courtId: 'court-1',
      courtName: 'Court One',
      userId: 'user-1',
      userName: 'Test User',
      startAt: '2026-08-10T14:00:00Z',
      endAt: '2026-08-10T15:00:00Z',
      status: 'Confirmed',
      source: 'Web',
      notes: null,
      createdAt: '2026-08-01T12:00:00Z',
      cancelledAt: null,
      cancellationReason: null
    }
  ],
  page: 1,
  pageSize: 10,
  totalItems: 1,
  totalPages: 1
};

describe('ComplexUsersPage', () => {
  const blockMutate = vi.fn();
  const unblockMutate = vi.fn();

  beforeEach(() => {
    cleanup();
    vi.resetAllMocks();
    blockMutate.mockClear();
    unblockMutate.mockClear();
  });

  function setupMocks(users = mockUsers) {
    vi.mocked(useComplexUsers).mockReturnValue({
      data: users,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useComplexUsers>);

    vi.mocked(useUserReservations).mockReturnValue({
      data: mockReservations,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useUserReservations>);

    vi.mocked(useBlockUser).mockReturnValue({
      mutateAsync: blockMutate,
      isPending: false,
      error: null
    } as unknown as ReturnType<typeof useBlockUser>);

    vi.mocked(useUnblockUser).mockReturnValue({
      mutateAsync: unblockMutate,
      isPending: false,
      error: null
    } as unknown as ReturnType<typeof useUnblockUser>);
  }

  it('renders the users list with details and actions', async () => {
    setupMocks();

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/users" element={<ComplexUsersPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/users' }
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Users' })).toBeTruthy();
    });

    expect(screen.getByText('Test User')).toBeTruthy();
    expect(screen.getByText('user@example.com')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'History' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Block' })).toBeTruthy();
  });

  it('renders an empty state when no users exist', async () => {
    setupMocks({ items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 });

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/users" element={<ComplexUsersPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/users' }
    );

    await waitFor(() => {
      expect(screen.getByText('No users found for this complex.')).toBeTruthy();
    });
  });

  it('opens the block dialog and calls the mutation on submit', async () => {
    setupMocks();

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/users" element={<ComplexUsersPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/users' }
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Users' })).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Block' }));

    await waitFor(() => {
      expect(screen.getByRole('dialog', { name: /Block/ })).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Block' }));
    expect(blockMutate).toHaveBeenCalled();
  });

  it('calls the unblock mutation for a blocked user', async () => {
    setupMocks({
      items: [mockBlockedUser],
      page: 1,
      pageSize: 10,
      totalItems: 1,
      totalPages: 1
    });

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId/users" element={<ComplexUsersPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/users' }
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Users' })).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Unblock' }));
    expect(unblockMutate).toHaveBeenCalledWith('block-1');
  });
});
