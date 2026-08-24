import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, render, screen } from '@testing-library/react';
import TimezoneSelector from './TimezoneSelector';

function renderSelector(props: Partial<React.ComponentProps<typeof TimezoneSelector>> = {}) {
  return render(
    <TimezoneSelector
      value="America/Montevideo"
      onChange={vi.fn()}
      label="Time zone"
      helperText="Select the time zone"
      {...props}
    />
  );
}

describe('TimezoneSelector', () => {
  afterEach(() => {
    cleanup();
  });

  it('renders a labelled autocomplete with the selected time zone', () => {
    renderSelector();
    expect(screen.getByLabelText('Time zone')).toBeTruthy();
    expect(screen.getByText('Select the time zone')).toBeTruthy();
  });

  it('renders with no initial value', () => {
    renderSelector({ value: undefined });
    expect(screen.getByLabelText('Time zone')).toBeTruthy();
  });
});
