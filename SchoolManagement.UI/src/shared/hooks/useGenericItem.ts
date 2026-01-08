import { useQuery, UseQueryOptions } from '@tanstack/react-query';
import { ApiResponse } from '@/shared/types/api.types';
import { ApiError } from '@/lib/api/apiError';

interface UseGenericItemOptions<T> 
  extends Omit<UseQueryOptions<ApiResponse<T>, ApiError>, 'queryKey' | 'queryFn'> {
  queryKey: string;
  id: string;
  queryFn: (id: string) => Promise<ApiResponse<T>>;
  enabled?: boolean;
}

export function useGenericItem<T>({
  queryKey,
  id,
  queryFn,
  enabled = true,
  ...options
}: UseGenericItemOptions<T>) {
  return useQuery<ApiResponse<T>, ApiError>({
    queryKey: [queryKey, id],
    queryFn: () => queryFn(id),
    enabled: enabled && !!id,
    ...options,
  });
}
