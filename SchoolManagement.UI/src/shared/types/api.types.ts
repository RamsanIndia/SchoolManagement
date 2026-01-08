/**
 * Generic paginated response wrapper
 * Can be used across all features in the application
 */
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Generic API response wrapper for single items
 */
export interface ApiResponse<T> {
  data: T;
  status: boolean;
  message: string;
  errors: string[];
}

/**
 * Generic API response wrapper for paginated data
 */
export interface PaginatedApiResponse<T> {
  data: PaginatedResponse<T>;
  status: boolean;
  message: string;
  errors: string[];
}

/**
 * Generic API response for operations without data (DELETE, etc.)
 */
export interface ApiOperationResponse {
  status: boolean;
  message: string;
  errors: string[];
}

/**
 * Base query parameters for list/search operations
 */
export interface BaseQueryParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

/**
 * Extended query params with common filters
 */
export interface QueryParams extends BaseQueryParams {
  isActive?: boolean;
  startDate?: string;
  endDate?: string;
}

/**
 * Generic filter params that can be extended per feature
 */
export interface FilterParams {
  [key: string]: string | number | boolean | undefined | null;
}

/**
 * Pagination metadata
 */
export interface PaginationMeta {
  currentPage: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Sort configuration
 */
export interface SortConfig<T = any> {
  field: keyof T | string;
  direction: 'asc' | 'desc';
}
