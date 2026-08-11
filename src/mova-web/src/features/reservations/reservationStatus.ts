import type { ReservationStatus } from './reservationTypes';

export function getReservationStatusKey(status: ReservationStatus): string {
  return status.charAt(0).toLowerCase() + status.slice(1);
}
