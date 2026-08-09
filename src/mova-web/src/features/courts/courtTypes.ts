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
