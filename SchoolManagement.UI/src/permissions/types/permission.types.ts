// src/permissions/types/permission.types.ts
import type { PaginatedResponse } from '@/shared/types/api.types';

/**
 * Permission entity - matches your API response
 */
export interface Permission {
  id: string;
  name: string;
  displayName: string;
  description: string;
  module: string;
  action: string;
  resource: string;
  isSystemPermission: boolean;
  createdAt?: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
}

/**
 * DTO for creating a new permission
 */
export interface PermissionDto {
  name: string;
  displayName: string;
  description: string;
  module: string;
  action: string;
  resource: string;
}

/**
 * Alias for consistency
 */
export type CreatePermissionDto = PermissionDto;

/**
 * DTO for updating an existing permission
 */
export interface UpdatePermissionDto extends Partial<PermissionDto> {}

/**
 * Query params for filtering/sorting permissions
 */
export interface PermissionQueryParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: 'name' | 'displayName' | 'module' | 'action' | 'createdAt';
  sortDirection?: 'asc' | 'desc';
  module?: string;
  action?: string;
  isSystemPermission?: boolean;
}

/**
 * Paginated response for permissions list
 */
export type PaginatedPermissions = PaginatedResponse<Permission>;

/**
 * Permission grouped by module
 */
export interface PermissionsByModule {
  module: string;
  permissions: Permission[];
  count?: number;
}

/**
 * Role-Permission mapping
 */
export interface RolePermission {
  roleId: string;
  roleName: string;
  permissionIds: string[];
  permissions: Permission[];
}

/**
 * DTO for assigning permissions to a role
 */
export interface AssignPermissionsDto {
  permissionIds: string[];
}

/**
 * DTO for syncing role permissions (replace all)
 */
export interface SyncPermissionsDto {
  permissionIds: string[];
}
