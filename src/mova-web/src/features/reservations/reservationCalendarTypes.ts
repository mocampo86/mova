import type { Court } from '../complexes/complexTypes';
import type { Reservation, ReservationStatus } from './reservationTypes';

export interface FreeCalendarSlot {
  type: 'free';
  courtId: string;
  courtName: string;
  startAt: string;
  endAt: string;
}

export interface ReservationCalendarSlot {
  type: 'reservation';
  courtId: string;
  courtName: string;
  startAt: string;
  endAt: string;
  reservation: Reservation;
  status: ReservationStatus;
}

export type CalendarSlot = FreeCalendarSlot | ReservationCalendarSlot;

export interface CalendarCourtColumn {
  court: Court;
  slots: CalendarSlot[];
}
