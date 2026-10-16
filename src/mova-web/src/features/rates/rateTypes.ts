export interface CourtRate {
  id: string;
  courtId: string;
  daysOfWeek: number[];
  startTime: string;
  endTime: string;
  amountPerHour: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateCourtRateRequest {
  daysOfWeek: number[];
  startTime: string;
  endTime: string;
  amountPerHour: number;
}

export interface UpdateCourtRateRequest {
  daysOfWeek: number[];
  startTime: string;
  endTime: string;
  amountPerHour: number;
}

export interface UpdateCourtRateStatusRequest {
  isActive: boolean;
}

export interface PriceQuoteLine {
  startAt: string;
  endAt: string;
  hourlyRate: number;
  amount: number;
}

export interface PriceGap {
  startAt: string;
  endAt: string;
}

export interface PriceQuote {
  totalAmount: number;
  currency: string;
  isFullyCovered: boolean;
  lines: PriceQuoteLine[];
  gaps: PriceGap[];
}
