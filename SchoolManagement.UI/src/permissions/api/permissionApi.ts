// src/permissions/api/permissionApi.ts
import { apiClient } from '@/lib/api/apiClient';
import { PERMISSION_ENDPOINTS } from './permissionEndpoints';
import type {
  Permission,
  PermissionDto,
  UpdatePermissionDto,
  PaginatedPermissions,
  PermissionQueryParams,
  PermissionsByModule,
  RolePermission,
} from '../types/permission.types';

export const permissionApi = {
  /**
   * Get all permissions with pagination and filtering
   * Returns: PaginatedResponse<Permission>
   */
  async getAll(params?: PermissionQueryParams): Promise<PaginatedPermissions> {
    return await apiClient.getList<Permission>(PERMISSION_ENDPOINTS.GET_ALL, { params });
  },

  /**
   * Get a single permission by ID
   * Returns: Permission
   */
  async getById(id: string): Promise<Permission> {
    const response = await apiClient.get<Permission>(PERMISSION_ENDPOINTS.GET_BY_ID(id));
    return response.data;
  },

  /**
   * Create a new permission
   * Returns: Permission
   */
  async create(data: PermissionDto): Promise<Permission> {
    const response = await apiClient.post<Permission>(PERMISSION_ENDPOINTS.CREATE, data);
    return response.data;
  },

  /**
   * Update an existing permission
   * Returns: Permission
   */
  async update(id: string, data: UpdatePermissionDto): Promise<Permission> {
    const response = await apiClient.put<Permission>(PERMISSION_ENDPOINTS.UPDATE(id), data);
    return response.data;
  },

  /**
   * Delete a permission
   */
  async delete(id: string): Promise<void> {
    await apiClient.delete(PERMISSION_ENDPOINTS.DELETE(id));
  },

  /**
   * Get permissions grouped by module
   * Returns: PermissionsByModule[]
   */
  async getByModule(): Promise<PermissionsByModule[]> {
    const response = await apiClient.get<PermissionsByModule[]>(
      PERMISSION_ENDPOINTS.GET_BY_MODULE
    );
    return response.data;
  },

  /**
   * Get all unique modules
   * Returns: string[]
   */
  async getModules(): Promise<string[]> {
    const response = await apiClient.get<string[]>(PERMISSION_ENDPOINTS.GET_MODULES);
    return response.data;
  },

  /**
   * Get permissions for a specific role
   * Returns: RolePermission
   */
  async getRolePermissions(roleId: string): Promise<RolePermission> {
    const response = await apiClient.get<RolePermission>(
      PERMISSION_ENDPOINTS.GET_ROLE_PERMISSIONS(roleId)
    );
    return response.data;
  },

  /**
   * Assign permissions to a role
   * Returns: RolePermission
   */
  async assignPermissionsToRole(
    roleId: string,
    permissionIds: string[]
  ): Promise<RolePermission> {
    const response = await apiClient.post<RolePermission>(
      PERMISSION_ENDPOINTS.ASSIGN_TO_ROLE(roleId),
      { permissionIds }
    );
    return response.data;
  },

  /**
   * Remove permissions from a role
   */
  async removePermissionsFromRole(
    roleId: string,
    permissionIds: string[]
  ): Promise<void> {
    await apiClient.post(
      PERMISSION_ENDPOINTS.REMOVE_FROM_ROLE(roleId),
      { permissionIds }
    );
  },

  /**
   * Sync role permissions (replace all)
   * Returns: RolePermission
   */
  async syncRolePermissions(
    roleId: string,
    permissionIds: string[]
  ): Promise<RolePermission> {
    const response = await apiClient.post<RolePermission>(
      PERMISSION_ENDPOINTS.SYNC_ROLE_PERMISSIONS(roleId),
      { permissionIds }
    );
    return response.data;
  },

  /**
   * Get permissions by specific module name
   * Returns: Permission[]
   */
  async getByModuleName(module: string): Promise<Permission[]> {
    const response = await apiClient.get<Permission[]>(
      PERMISSION_ENDPOINTS.GET_BY_MODULE_NAME(module)
    );
    return response.data;
  },

  /**
   * Get user permissions
   * Returns: Permission[]
   */
  async getUserPermissions(userId: string): Promise<Permission[]> {
    const response = await apiClient.get<Permission[]>(
      PERMISSION_ENDPOINTS.GET_USER_PERMISSIONS(userId)
    );
    return response.data;
  },
};

export default permissionApi;
