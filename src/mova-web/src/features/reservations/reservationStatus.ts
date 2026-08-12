import type { ReservationStatus } from './reservationTypes';

export function getReservationStatusKey(status: ReservationStatus): string {
  return status.charAt(0).toLowerCase() + status.slice(1);
}

export function getReservationStatusColor(
  status: ReservationStatus
): 'success' | 'info' | 'warning' | 'error' | 'default' | 'primary' {
  if (status === 'Confirmed') return 'primary';
  if (status === 'Completed') return 'info';
  if (status === 'Pending') return 'warning';
  if (status === 'CancelledByUser' || status === 'CancelledByAdmin') return 'error';
  if (status === 'NoShow') return 'default';
  return 'default';
}
