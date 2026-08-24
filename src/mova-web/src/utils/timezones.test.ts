import { describe, expect, it } from 'vitest';
import { getTimeZoneOptions, todayInTimeZone } from './timezones';

describe('timezones', () => {
  it('returns a list of time zone options including the default', () => {
    const options = getTimeZoneOptions();
    expect(options.length).toBeGreaterThan(0);
    expect(options.some((option) => option.id === 'America/Montevideo')).toBe(true);
  });

  it('produces a valid ISO date for a given time zone', () => {
    const date = todayInTimeZone('America/Montevideo');
    expect(date).toMatch(/^\d{4}-\d{2}-\d{2}$/);
  });

  it('falls back to the local date when no time zone is supplied', () => {
    const date = todayInTimeZone();
    expect(date).toMatch(/^\d{4}-\d{2}-\d{2}$/);
  });
});
