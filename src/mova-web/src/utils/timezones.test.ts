import { describe, expect, it } from 'vitest';
import {
  getTimeZoneOptions,
  localDateTimeToUtc,
  nowInTimeZone,
  todayInTimeZone,
  utcToLocalDateTime
} from './timezones';

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

  it('converts a local wall-clock date/time to UTC using the complex time zone', () => {
    const utc = localDateTimeToUtc('America/Montevideo', '2026-08-10T10:00');
    expect(utc).toBe('2026-08-10T13:00:00.000Z');
  });

  it('converts a local wall-clock date/time to UTC across a positive offset', () => {
    const utc = localDateTimeToUtc('Europe/Berlin', '2026-08-10T10:00');
    expect(utc).toBe('2026-08-10T08:00:00.000Z');
  });

  it('converts UTC to a local date/time input value in the complex time zone', () => {
    const local = utcToLocalDateTime('America/Montevideo', '2026-08-10T13:00:00Z');
    expect(local).toBe('2026-08-10T10:00');
  });

  it('converts UTC to a local date/time input value across a positive offset', () => {
    const local = utcToLocalDateTime('Europe/Berlin', '2026-08-10T08:00:00Z');
    expect(local).toBe('2026-08-10T10:00');
  });

  it('produces a date/time input value for the current time in a given time zone', () => {
    const value = nowInTimeZone('America/Montevideo');
    expect(value).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/);
  });

  it('falls back to the browser local offset when an unknown time zone is supplied', () => {
    const utc = localDateTimeToUtc('Not/A/Zone', '2026-08-10T10:00');
    expect(typeof utc).toBe('string');
    expect(utc).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/);
  });
});
