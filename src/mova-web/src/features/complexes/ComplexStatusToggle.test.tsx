import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import ComplexStatusToggle from './ComplexStatusToggle';
import { useUpdateComplexStatus } from './useUpdateComplexStatus';

vi.mock('./useUpdateComplexStatus');

const mockMutate = vi.fn();

function mockHook(overrides: Partial<ReturnType<typeof useUpdateComplexStatus>> = {}) {
  vi.mocked(useUpdateComplexStatus).mockReturnValue({
    mutate: mockMutate,
    isPending: false,
    error: null,
    reset: vi.fn(),
    ...overrides
  } as unknown as ReturnType<typeof useUpdateComplexStatus>);
}

describe('ComplexStatusToggle', () => {
  beforeEach(() => {
    vi.resetAllMocks();
    mockHook();
  });

  afterEach(cleanup);

  it('renders the current active status', () => {
    render(<ComplexStatusToggle complexId="complex-id" status="Active" />);
    expect(screen.getByLabelText('Active')).toBeTruthy();
  });

  it('renders the current inactive status', () => {
    render(<ComplexStatusToggle complexId="complex-id" status="Inactive" />);
    expect(screen.getByLabelText('Inactive')).toBeTruthy();
  });

  it('activates an inactive complex without a confirmation dialog', () => {
    render(<ComplexStatusToggle complexId="complex-id" status="Inactive" />);

    const toggle = screen.getByLabelText('Inactive');
    fireEvent.click(toggle);

    expect(mockMutate).toHaveBeenCalledTimes(1);
    expect(mockMutate).toHaveBeenCalledWith('Active');
    expect(screen.queryByText('Deactivate complex?')).toBeNull();
  });

  it('shows a confirmation dialog before deactivation', () => {
    render(<ComplexStatusToggle complexId="complex-id" status="Active" />);

    const toggle = screen.getByLabelText('Active');
    fireEvent.click(toggle);

    expect(mockMutate).not.toHaveBeenCalled();
    expect(screen.getByText('Deactivate complex?')).toBeTruthy();

    const deactivateButton = screen.getByRole('button', { name: 'Deactivate' });
    fireEvent.click(deactivateButton);

    expect(mockMutate).toHaveBeenCalledTimes(1);
    expect(mockMutate).toHaveBeenCalledWith('Inactive');
  });

  it('does not deactivate when the user cancels the confirmation dialog', async () => {
    render(<ComplexStatusToggle complexId="complex-id" status="Active" />);

    const toggle = screen.getByLabelText('Active');
    fireEvent.click(toggle);

    expect(screen.getByText('Deactivate complex?')).toBeTruthy();

    const cancelButton = screen.getByRole('button', { name: 'Cancel' });
    fireEvent.click(cancelButton);

    expect(mockMutate).not.toHaveBeenCalled();
    await waitFor(() => {
      expect(screen.queryByText('Deactivate complex?')).toBeNull();
    });
  });

  it('disables the switch while the update is pending', () => {
    mockHook({ isPending: true });
    render(<ComplexStatusToggle complexId="complex-id" status="Active" />);

    const toggle = screen.getByLabelText('Active');
    expect((toggle as HTMLInputElement).disabled).toBe(true);
  });

  it('displays an error message when the mutation fails', () => {
    mockHook({ error: new Error('Could not update status') });
    render(<ComplexStatusToggle complexId="complex-id" status="Active" />);

    expect(screen.getByText('Could not update status')).toBeTruthy();
  });
});
