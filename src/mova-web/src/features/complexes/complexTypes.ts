export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface SportsComplex {
  id: string;
  name: string;
  description: string;
  address: string;
  city: string;
  phoneNumber: string;
  email: string;
  latitude?: number | null;
  longitude?: number | null;
  status?: string;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface UpdateComplexRequest {
  name: string;
  description: string;
  address: string;
  city: string;
  phoneNumber: string;
  email: string;
  latitude?: number | null;
  longitude?: number | null;
  status: string;
}

export interface Court {
  id: string;
  sportsComplexId: string;
  name: string;
  description: string;
  surfaceType: string;
  indoor: boolean;
  status: string;
  sportIds: string[];
  createdAt?: string;
  updatedAt?: string | null;
}

export interface Sport {
  id: string;
  name: string;
}

export interface CourtAvailabilitySlot {
  courtId: string;
  startAt: string;
  endAt: string;
}

export interface DashboardComplexSummary {
  id: string;
  name: string;
  status: string;
  lastUpdatedAt?: string | null;
}

export interface DashboardCourtSummary {
  active: number;
  inactive: number;
}

export interface DashboardReservationsSummary {
  confirmed: number;
  cancelled: number;
  completed: number;
}

export interface ComplexDashboard {
  complex: DashboardComplexSummary;
  courts: DashboardCourtSummary;
  reservationsToday: DashboardReservationsSummary;
  blockedUsers: number;
}
