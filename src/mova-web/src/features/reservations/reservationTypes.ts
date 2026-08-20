export type ReservationStatus =
  | 'Pending'
  | 'Confirmed'
  | 'CancelledByUser'
  | 'CancelledByAdmin'
  | 'Completed'
  | 'NoShow';

export type ReservationSource = 'Web' | 'Admin' | 'Recurring';

export interface Reservation {
  id: string;
  complexId: string;
  courtId: string;
  courtName: string;
  userId: string;
  userName: string;
  startAt: string;
  endAt: string;
  status: ReservationStatus;
  source: ReservationSource;
  recurringReservationId?: string | null;
  notes?: string | null;
  createdAt: string;
  cancelledAt?: string | null;
  cancellationReason?: string | null;
  cancelledByUserId?: string | null;
  cancelledByUserName?: string | null;
}

export interface CreateMyRecurringReservationRequest {
  courtId: string;
  dayOfWeek: number;
  startTime: string;
  durationMinutes: number;
  startDate: string;
  endDate: string;
  notes?: string;
  utcOffsetMinutes?: number;
}

export interface CreateRecurringReservationForCustomerRequest {
  userId: string;
  courtId: string;
  dayOfWeek: number;
  startTime: string;
  durationMinutes: number;
  startDate: string;
  endDate: string;
  notes?: string;
  utcOffsetMinutes?: number;
}

export interface RecurringReservation {
  id: string;
  complexId: string;
  courtId: string;
  userId: string;
  dayOfWeek: number;
  startTime: string;
  durationMinutes: number;
  startDate: string;
  endDate: string;
  status: 'Active' | 'Cancelled';
  createdAt: string;
  occurrences: Reservation[];
}

export interface RecurringReservationListItem {
  id: string;
  complexId: string;
  courtId: string;
  courtName: string;
  userId: string;
  userName: string;
  dayOfWeek: number;
  startTime: string;
  durationMinutes: number;
  startDate: string;
  endDate: string;
  status: 'Active' | 'Cancelled';
  createdAt: string;
  updatedAt: string | null;
}

export interface RecurringReservationListFilters {
  page: number;
  pageSize: number;
  userId?: string;
  courtId?: string;
  status?: string;
  sort?: string;
}

export interface ReservationListFilters {
  page: number;
  pageSize: number;
  courtId: string;
  status: ReservationStatus | 'All';
  date: string;
  sort?: string;
}

export interface UserReservationsFilters {
  page: number;
  pageSize: number;
}

export interface CreateReservationRequest {
  courtId: string;
  userId: string;
  startAt: string;
  endAt: string;
  notes?: string;
}

export interface CreateMyReservationRequest {
  courtId: string;
  startAt: string;
  endAt: string;
  notes?: string;
}

export interface CancelReservationRequest {
  reason?: string;
}

export interface UpdateReservationStatusRequest {
  status: 'Completed' | 'NoShow';
}
