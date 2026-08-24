export interface TimezoneOption {
  id: string;
  label: string;
  offsetLabel: string;
}

export const DEFAULT_TIME_ZONE_ID = 'America/Montevideo';

const FALLBACK_OPTIONS: TimezoneOption[] = [
  { id: 'America/Montevideo', label: '(UTC-03:00) America/Montevideo', offsetLabel: 'UTC-03:00' },
  { id: 'America/Argentina/Buenos_Aires', label: '(UTC-03:00) America/Argentina/Buenos_Aires', offsetLabel: 'UTC-03:00' },
  { id: 'America/Sao_Paulo', label: '(UTC-03:00) America/Sao_Paulo', offsetLabel: 'UTC-03:00' },
  { id: 'America/Santiago', label: '(UTC-04:00) America/Santiago', offsetLabel: 'UTC-04:00' },
  { id: 'America/Bogota', label: '(UTC-05:00) America/Bogota', offsetLabel: 'UTC-05:00' },
  { id: 'America/Lima', label: '(UTC-05:00) America/Lima', offsetLabel: 'UTC-05:00' },
  { id: 'America/Mexico_City', label: '(UTC-06:00) America/Mexico_City', offsetLabel: 'UTC-06:00' },
  { id: 'America/New_York', label: '(UTC-04:00) America/New_York', offsetLabel: 'UTC-04:00' },
  { id: 'America/Chicago', label: '(UTC-05:00) America/Chicago', offsetLabel: 'UTC-05:00' },
  { id: 'America/Denver', label: '(UTC-06:00) America/Denver', offsetLabel: 'UTC-06:00' },
  { id: 'America/Los_Angeles', label: '(UTC-07:00) America/Los_Angeles', offsetLabel: 'UTC-07:00' },
  { id: 'America/Toronto', label: '(UTC-04:00) America/Toronto', offsetLabel: 'UTC-04:00' },
  { id: 'Europe/London', label: '(UTC+01:00) Europe/London', offsetLabel: 'UTC+01:00' },
  { id: 'Europe/Paris', label: '(UTC+02:00) Europe/Paris', offsetLabel: 'UTC+02:00' },
  { id: 'Europe/Berlin', label: '(UTC+02:00) Europe/Berlin', offsetLabel: 'UTC+02:00' },
  { id: 'Europe/Madrid', label: '(UTC+02:00) Europe/Madrid', offsetLabel: 'UTC+02:00' },
  { id: 'Asia/Tokyo', label: '(UTC+09:00) Asia/Tokyo', offsetLabel: 'UTC+09:00' },
  { id: 'Asia/Shanghai', label: '(UTC+08:00) Asia/Shanghai', offsetLabel: 'UTC+08:00' },
  { id: 'Australia/Sydney', label: '(UTC+10:00) Australia/Sydney', offsetLabel: 'UTC+10:00' },
  { id: 'Pacific/Auckland', label: '(UTC+12:00) Pacific/Auckland', offsetLabel: 'UTC+12:00' },
  { id: 'UTC', label: '(UTC+00:00) UTC', offsetLabel: 'UTC+00:00' }
];

function tryGetSupportedTimeZoneIds(): string[] | null {
  try {
    const supportedValuesOf = (Intl as unknown as { supportedValuesOf?: (type: string) => string[] }).supportedValuesOf;
    if (supportedValuesOf) {
      return supportedValuesOf('timeZone');
    }
  } catch {
    // Ignore and fall back to the static list.
  }
  return null;
}

function getFallbackOffsetLabel(timeZoneId: string): string {
  const fallback = FALLBACK_OPTIONS.find((option) => option.id === timeZoneId);
  return fallback?.offsetLabel ?? 'UTC+00:00';
}

function getOffsetLabel(timeZoneId: string): string {
  if (timeZoneId === 'UTC') {
    return 'UTC+00:00';
  }

  try {
    const formatter = new Intl.DateTimeFormat('en-US', {
      timeZone: timeZoneId,
      timeZoneName: 'longOffset'
    });
    const parts = formatter.formatToParts(new Date());
    const timeZoneName = parts.find((part) => part.type === 'timeZoneName')?.value;

    if (timeZoneName) {
      const offset = timeZoneName.replace('GMT', 'UTC').trim();
      if (offset.includes('+') || offset.includes('-')) {
        return offset;
      }
    }
  } catch {
    // Fall through to the fallback map.
  }

  return getFallbackOffsetLabel(timeZoneId);
}

export function getTimeZoneOptions(): TimezoneOption[] {
  const supportedIds = tryGetSupportedTimeZoneIds();

  const options = supportedIds
    ? supportedIds.map((id) => {
        const offsetLabel = getOffsetLabel(id);
        return {
          id,
          label: `(${offsetLabel}) ${id}`,
          offsetLabel
        };
      })
    : [...FALLBACK_OPTIONS];

  return options.sort((a, b) => {
    if (a.offsetLabel !== b.offsetLabel) {
      return a.offsetLabel.localeCompare(b.offsetLabel);
    }
    return a.id.localeCompare(b.id);
  });
}

export function todayInTimeZone(timeZoneId?: string | null): string {
  const now = new Date();

  if (!timeZoneId) {
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  try {
    return now.toLocaleDateString('en-CA', { timeZone: timeZoneId });
  } catch {
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}

function pad(value: number): string {
  return String(value).padStart(2, '0');
}

function getTimeZoneOffsetMinutes(timeZoneId: string | undefined | null, date: Date): number {
  if (!timeZoneId) {
    return -date.getTimezoneOffset();
  }

  try {
    const formatter = new Intl.DateTimeFormat('en-US', {
      timeZone: timeZoneId,
      timeZoneName: 'longOffset'
    });
    const parts = formatter.formatToParts(date);
    const timeZoneName = parts.find((part) => part.type === 'timeZoneName')?.value;

    if (timeZoneName) {
      const normalized = timeZoneName.replace('GMT', 'UTC').trim();
      const sign = normalized.includes('-') ? -1 : 1;
      const numbers = normalized.replace(/[^0-9:]/g, '').split(':').map(Number);
      const hours = numbers[0] || 0;
      const minutes = numbers[1] || 0;
      return sign * (hours * 60 + minutes);
    }
  } catch {
    // Fall back to the browser local offset.
  }

  return -date.getTimezoneOffset();
}

function parseDateTimeInput(value: string): { year: number; month: number; day: number; hour: number; minute: number; second: number } {
  const [datePart, timePart = '00:00'] = value.split('T');
  const [year, month, day] = datePart.split('-').map(Number);
  const [hour, minute, second = 0] = timePart.split(':').map(Number);
  return { year, month, day, hour, minute, second };
}

function naiveUtcMillis(value: string): number {
  const { year, month, day, hour, minute, second } = parseDateTimeInput(value);
  return Date.UTC(year, month - 1, day, hour, minute, second);
}

export function localDateTimeToUtc(timeZoneId: string | undefined | null, localDateTime: string): string {
  const localMillis = naiveUtcMillis(localDateTime);
  let candidateUtcMillis = localMillis;

  for (let i = 0; i < 5; i++) {
    const offsetMinutes = getTimeZoneOffsetMinutes(timeZoneId, new Date(candidateUtcMillis));
    const adjustedUtcMillis = localMillis - offsetMinutes * 60 * 1000;

    if (Math.abs(adjustedUtcMillis - candidateUtcMillis) < 1000) {
      return new Date(adjustedUtcMillis).toISOString();
    }

    candidateUtcMillis = adjustedUtcMillis;
  }

  return new Date(candidateUtcMillis).toISOString();
}

export function utcToLocalDateTime(timeZoneId: string | undefined | null, utcIso: string): string {
  const date = new Date(utcIso);

  if (!timeZoneId) {
    const year = date.getFullYear();
    const month = pad(date.getMonth() + 1);
    const day = pad(date.getDate());
    const hours = pad(date.getHours());
    const minutes = pad(date.getMinutes());
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  try {
    const formatter = new Intl.DateTimeFormat('en-US', {
      timeZone: timeZoneId,
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
    });
    const parts = formatter.formatToParts(date);
    const part = (type: string) => parts.find((p) => p.type === type)?.value ?? '00';
    const year = part('year');
    const month = part('month');
    const day = part('day');
    const hour = part('hour');
    const minute = part('minute');
    return `${year}-${month}-${day}T${hour}:${minute}`;
  } catch {
    const year = date.getFullYear();
    const month = pad(date.getMonth() + 1);
    const day = pad(date.getDate());
    const hours = pad(date.getHours());
    const minutes = pad(date.getMinutes());
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }
}

export function nowInTimeZone(timeZoneId?: string | null): string {
  if (!timeZoneId) {
    const now = new Date();
    const rounded = new Date(now.getFullYear(), now.getMonth(), now.getDate(), now.getHours(), 0, 0, 0);
    const nextHour = new Date(rounded.getTime() + 60 * 60 * 1000);
    const year = nextHour.getFullYear();
    const month = pad(nextHour.getMonth() + 1);
    const day = pad(nextHour.getDate());
    const hours = pad(nextHour.getHours());
    const minutes = pad(nextHour.getMinutes());
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  try {
    const now = new Date();
    const rounded = new Date(now.getFullYear(), now.getMonth(), now.getDate(), now.getHours(), 0, 0, 0);
    const nextHour = new Date(rounded.getTime() + 60 * 60 * 1000);
    const parts = new Intl.DateTimeFormat('en-US', {
      timeZone: timeZoneId,
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
    }).formatToParts(nextHour);
    const part = (type: string) => parts.find((p) => p.type === type)?.value ?? '00';
    const year = part('year');
    const month = part('month');
    const day = part('day');
    const hour = part('hour');
    const minute = part('minute');
    return `${year}-${month}-${day}T${hour}:${minute}`;
  } catch {
    return nowInTimeZone();
  }
}
