import { BaseQueryParams, FilterParams } from '@/shared/types/api.types';

/**
 * Build query string from parameters
 */
export function buildQueryString(
  params?: BaseQueryParams | FilterParams
): string {
  if (!params) return '';

  const queryString = new URLSearchParams(
    Object.entries(params).reduce((acc, [key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        acc[key] = String(value);
      }
      return acc;
    }, {} as Record<string, string>)
  ).toString();

  return queryString ? `?${queryString}` : '';
}

/**
 * Merge query parameters with defaults
 */
export function mergeQueryParams<T extends BaseQueryParams>(
  params?: Partial<T>,
  defaults?: Partial<T>
): T {
  return {
    pageNumber: 1,
    pageSize: 10,
    ...defaults,
    ...params,
  } as T;
}
