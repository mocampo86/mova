// TimeSpan values arrive as "HH:mm:ss" or "d.HH:mm:ss" (e.g. 24:00 is "1.00:00:00").
export function formatTimeOfDay(value: string): string {
  const match = /^(?:(\d+)\.)?(\d{1,2}):(\d{2})(?::(\d{2}))?$/.exec(value.trim());
  if (!match) return value;

  const days = Number(match[1] ?? 0);
  const hours = days * 24 + Number(match[2]);
  const minutes = match[3];
  return `${String(hours).padStart(2, '0')}:${minutes}`;
}

// Converts an "HH:mm" time input value to a TimeSpan string the API accepts.
// For end times, "00:00" means end of day (24:00).
export function timeInputToTimeSpan(value: string, isEndTime = false): string {
  if (isEndTime && value === '00:00') {
    return '1.00:00:00';
  }

  return `${value}:00`;
}

export function formatMoney(amount: number, currency: string): string {
  try {
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency,
      minimumFractionDigits: 2
    }).format(amount);
  } catch {
    return `${currency} ${amount.toFixed(2)}`;
  }
}
