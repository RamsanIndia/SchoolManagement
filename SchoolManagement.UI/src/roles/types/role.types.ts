// src/roles/types/role.types.ts
import type { PaginatedResponse } from '@/shared/types/api.types';

/**
 * Role entity
 */
export interface Role {
  id: string;
  name: string;
  description?: string;
  permissions: string[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
}

/**
 * DTO for creating a new role
 */
export interface CreateRoleDto {
  name: string;
  description?: string;
  permissions?: string[];
}

/**
 * DTO for updating an existing role
 */
export interface UpdateRoleDto {
  name?: string;
  description?: string;
  permissions?: string[];
  isActive?: boolean;
}

/**
 * Query params for filtering/sorting roles
 */
export interface RoleQueryParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: 'name' | 'createdAt';
  sortDirection?: 'asc' | 'desc';
  isActive?: boolean;
}

/**
 * Paginated response for roles list
 */
export type PaginatedRoles = PaginatedResponse<Role>;

/**
 * Role assignment DTO
 */
export interface RoleAssignmentDto {
  userId: string;
  roleIds: string[];
}

/**
 * Permission entity (if needed)
 */
export interface Permission {
  id: string;
  name: string;
  description?: string;
  module: string;
  action: string;
}
