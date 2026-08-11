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
  notes?: string | null;
  createdAt: string;
  cancelledAt?: string | null;
  cancellationReason?: string | null;
}

export interface ReservationListFilters {
  page: number;
  pageSize: number;
  courtId: string;
  status: ReservationStatus | 'All';
  date: string;
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
