// src/permissions/hooks/usePermissions.ts
import { useQuery, UseQueryOptions } from '@tanstack/react-query';
import { permissionApi } from '../api/permissionApi';
import type { 
  PaginatedPermissions, 
  PermissionQueryParams, 
  Permission,
  PermissionsByModule,
  RolePermission 
} from '../types/permission.types';
import { ApiError } from '@/lib/api/apiError';

export const PERMISSIONS_QUERY_KEY = 'permissions';

interface UsePermissionsOptions extends Omit<UseQueryOptions<PaginatedPermissions, ApiError>, 'queryKey' | 'queryFn'> {
  params?: PermissionQueryParams;
}

/**
 * Hook for fetching paginated permissions list
 * Returns: PaginatedResponse<Permission>
 */
export function usePermissions(options?: UsePermissionsOptions) {
  const { params, ...queryOptions } = options || {};

  return useQuery<PaginatedPermissions, ApiError>({
    queryKey: [PERMISSIONS_QUERY_KEY, params],
    queryFn: () => permissionApi.getAll(params), // ✅ Changed from getPermissions
    staleTime: 5 * 60 * 1000, // 5 minutes
    ...queryOptions,
  });
}

/**
 * Hook for getting a single permission by ID
 * Returns: Permission
 */
export function usePermission(id: string, enabled: boolean = true) {
  return useQuery<Permission, ApiError>({
    queryKey: [PERMISSIONS_QUERY_KEY, id],
    queryFn: () => permissionApi.getById(id), // ✅ Changed from getPermissionById
    enabled: enabled && !!id,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook for getting permissions grouped by module
 * Returns: PermissionsByModule[]
 */
export function usePermissionsByModule() {
  return useQuery<PermissionsByModule[], ApiError>({
    queryKey: [PERMISSIONS_QUERY_KEY, 'by-module'],
    queryFn: () => permissionApi.getByModule(), // ✅ Changed from getPermissionsByModule
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook for getting all unique modules
 * Returns: string[]
 */
export function usePermissionModules() {
  return useQuery<string[], ApiError>({
    queryKey: [PERMISSIONS_QUERY_KEY, 'modules'],
    queryFn: () => permissionApi.getModules(),
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook for getting role permissions
 * Returns: RolePermission
 */
export function useRolePermissions(roleId: string, enabled: boolean = true) {
  return useQuery<RolePermission, ApiError>({
    queryKey: [PERMISSIONS_QUERY_KEY, 'role', roleId],
    queryFn: () => permissionApi.getRolePermissions(roleId),
    enabled: enabled && !!roleId,
    staleTime: 5 * 60 * 1000,
  });
}
