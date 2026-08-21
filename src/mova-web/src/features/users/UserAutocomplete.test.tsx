import { cleanup, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithAuth } from '../../test-utils';
import UserAutocomplete from './UserAutocomplete';
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

describe('UserAutocomplete', () => {
  beforeEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('renders the user input and a search button', () => {
    setupMock();
    renderWithAuth(<UserAutocomplete complexId="complex-1" value={null} onChange={vi.fn()} />);

    expect(screen.getByRole('combobox')).toBeTruthy();
    expect(screen.getByRole('button', { name: /search/i })).toBeTruthy();
  });

  it('suggests users as the administrator types', async () => {
    setupMock();
    const user = userEvent.setup();
    renderWithAuth(<UserAutocomplete complexId="complex-1" value={null} onChange={vi.fn()} />);

    const input = screen.getByRole('combobox');
    await user.type(input, 'jo');

    await waitFor(() => {
      expect(screen.getByText('John Doe')).toBeTruthy();
    });
  });

  it('calls onChange when a user is selected', async () => {
    setupMock();
    const onChange = vi.fn();
    const user = userEvent.setup();
    renderWithAuth(<UserAutocomplete complexId="complex-1" value={null} onChange={onChange} />);

    const input = screen.getByRole('combobox');
    await user.type(input, 'jo');

    const option = await waitFor(() => screen.getByText('John Doe'));
    await user.click(option);

    expect(onChange).toHaveBeenCalledWith(mockUsers[0]);
  });

  it('displays phone number context in the suggestion', async () => {
    setupMock();
    const user = userEvent.setup();
    renderWithAuth(<UserAutocomplete complexId="complex-1" value={null} onChange={vi.fn()} />);

    const input = screen.getByRole('combobox');
    await user.type(input, 'jo');

    await waitFor(() => {
      expect(screen.getByText('+54 11 1234 5678', { exact: false })).toBeTruthy();
    });
  });

  it('disables blocked users in the dropdown', async () => {
    setupMock();
    const user = userEvent.setup();
    renderWithAuth(<UserAutocomplete complexId="complex-1" value={null} onChange={vi.fn()} />);

    const input = screen.getByRole('combobox');
    await user.type(input, 'ja');

    const option = await waitFor(() => screen.getByText('Jane Smith'));
    const listItem = option.closest('li');
    expect(listItem).not.toBeNull();
    expect(listItem!.getAttribute('aria-disabled')).toBe('true');
  });

  it('opens the search dialog when the search button is clicked', async () => {
    setupMock();
    const user = userEvent.setup();
    renderWithAuth(<UserAutocomplete complexId="complex-1" value={null} onChange={vi.fn()} />);

    const button = screen.getByRole('button', { name: /search/i });
    await user.click(button);

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeTruthy();
    });
  });
});
