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
