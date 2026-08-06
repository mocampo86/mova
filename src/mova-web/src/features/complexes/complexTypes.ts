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
}

export interface Court {
  id: string;
  sportsComplexId: string;
  name: string;
  description: string;
  surfaceType: string;
  indoor: boolean;
  sportIds: string[];
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
