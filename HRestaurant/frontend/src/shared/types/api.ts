export interface ApiError {
  code: string;
  message: string;
  field?: string | null;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: ApiError[];
  statusCode: number;
  traceId?: string;
}

export interface PagedResponse<T> extends ApiResponse<T[]> {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PaginationParams {
  pageNumber?: number;
  pageSize?: number;
  type?: ViewType;
}

export enum ViewType {
  All = 0,
  Deleted = 1,
  Active = 2,
}
