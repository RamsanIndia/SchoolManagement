// src/roles/hooks/useRoles.ts
import { useQuery, UseQueryOptions } from '@tanstack/react-query';
import { roleApi } from '../api/roleApi';
import type { PaginatedRoles, RoleQueryParams, Role } from '../types/role.types';
import { ApiError } from '@/lib/api/apiError';

export const ROLES_QUERY_KEY = 'roles';

interface UseRolesOptions extends Omit<UseQueryOptions<PaginatedRoles, ApiError>, 'queryKey' | 'queryFn'> {
  params?: RoleQueryParams;
}

export function useRoles(options?: UseRolesOptions) {
  const { params, ...queryOptions } = options || {};

  return useQuery<PaginatedRoles, ApiError>({
    queryKey: [ROLES_QUERY_KEY, params],
    queryFn: () => roleApi.getAll(params),
    staleTime: 5 * 60 * 1000,
    ...queryOptions,
  });
}

export function useRole(id: string, enabled: boolean = true) {
  return useQuery<Role, ApiError>({
    queryKey: [ROLES_QUERY_KEY, id],
    queryFn: () => roleApi.getById(id),
    enabled: enabled && !!id,
    staleTime: 5 * 60 * 1000,
  });
}
