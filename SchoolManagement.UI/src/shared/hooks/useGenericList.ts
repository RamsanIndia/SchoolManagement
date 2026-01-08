import { useQuery, UseQueryOptions } from '@tanstack/react-query';
import { PaginatedApiResponse, BaseQueryParams } from '@/shared/types/api.types';
import { ApiError } from '@/lib/api/apiError';

interface UseGenericListOptions<T, TParams extends BaseQueryParams> 
  extends Omit<UseQueryOptions<PaginatedApiResponse<T>, ApiError>, 'queryKey' | 'queryFn'> {
  queryKey: string;
  queryFn: (params?: TParams) => Promise<PaginatedApiResponse<T>>;
  params?: TParams;
}

export function useGenericList<T, TParams extends BaseQueryParams = BaseQueryParams>({
  queryKey,
  queryFn,
  params,
  ...options
}: UseGenericListOptions<T, TParams>) {
  return useQuery<PaginatedApiResponse<T>, ApiError>({
    queryKey: [queryKey, params],
    queryFn: () => queryFn(params),
    ...options,
  });
}
