export type CourtStatus = 'Active' | 'Inactive';

export interface CourtListFilters {
  page: number;
  pageSize: number;
  status: CourtStatus | 'All';
  sportId: string;
  search: string;
}

export interface UpdateCourtStatusRequest {
  status: CourtStatus;
}

export interface CreateCourtRequest {
  name: string;
  description: string;
  surfaceType: string;
  indoor: boolean;
  sportIds: string[];
}

export interface UpdateCourtRequest {
  name: string;
  description: string;
  surfaceType: string;
  indoor: boolean;
  sportIds?: string[];
}

export interface AssignCourtSportsRequest {
  sportIds: string[];
}

export interface CourtAvailabilityRule {
  id?: string;
  courtId?: string;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  slotDurationMinutes: number;
  isActive: boolean;
}

export interface UpdateCourtAvailabilityRequest {
  rules: CourtAvailabilityRule[];
}
