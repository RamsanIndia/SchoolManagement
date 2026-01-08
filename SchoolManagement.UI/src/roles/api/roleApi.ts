// src/roles/api/roleApi.ts
import { apiClient } from '@/lib/api/apiClient';
import { ROLE_ENDPOINTS } from './roleEndpoints';
import type {
  Role,
  CreateRoleDto,
  UpdateRoleDto,
  PaginatedRoles,
  RoleQueryParams,
} from '../types/role.types';

export const roleApi = {
  /**
   * Get all roles with pagination and filtering
   * Returns: PaginatedResponse<Role>
   */
  async getAll(params?: RoleQueryParams): Promise<PaginatedRoles> {
    // ✅ apiClient.getList handles query params internally
    return await apiClient.getList<Role>(ROLE_ENDPOINTS.GET_ALL, { params });
  },

  /**
   * Get a single role by ID
   * Returns: Role
   */
  async getById(id: string): Promise<Role> {
    const response = await apiClient.get<Role>(ROLE_ENDPOINTS.GET_BY_ID(id));
    return response.data; // ✅ Extract data from ApiResponse
  },

  /**
   * Create a new role
   * Returns: Role
   */
  async create(data: CreateRoleDto): Promise<Role> {
    const response = await apiClient.post<Role>(ROLE_ENDPOINTS.CREATE, data);
    return response.data; // ✅ Extract data from ApiResponse
  },

  /**
   * Update an existing role
   * Returns: Role
   */
  async update(id: string, data: UpdateRoleDto): Promise<Role> {
    const response = await apiClient.put<Role>(ROLE_ENDPOINTS.UPDATE(id), data);
    return response.data; // ✅ Extract data from ApiResponse
  },

  /**
   * Partially update a role (PATCH)
   * Returns: Role
   */
  async partialUpdate(id: string, data: Partial<UpdateRoleDto>): Promise<Role> {
    const response = await apiClient.patch<Role>(ROLE_ENDPOINTS.UPDATE(id), data);
    return response.data;
  },

  /**
   * Delete a role
   */
  async delete(id: string): Promise<void> {
    await apiClient.delete(ROLE_ENDPOINTS.DELETE(id));
  },

  /**
   * Activate a role
   * Returns: Role
   */
  async activate(id: string): Promise<Role> {
    const response = await apiClient.patch<Role>(ROLE_ENDPOINTS.ACTIVATE(id));
    return response.data; // ✅ Extract data from ApiResponse
  },

  /**
   * Deactivate a role
   * Returns: Role
   */
  async deactivate(id: string): Promise<Role> {
    const response = await apiClient.patch<Role>(ROLE_ENDPOINTS.DEACTIVATE(id));
    return response.data; // ✅ Extract data from ApiResponse
  },

  /**
   * Get permissions for a specific role
   * Returns: string[] (array of permission names)
   */
  async getPermissions(id: string): Promise<string[]> {
    const response = await apiClient.get<string[]>(ROLE_ENDPOINTS.GET_PERMISSIONS(id));
    return response.data;
  },

  /**
   * Assign permissions to a role
   */
  async assignPermissions(id: string, permissions: string[]): Promise<void> {
    await apiClient.post(ROLE_ENDPOINTS.ASSIGN_PERMISSIONS(id), { permissions });
  },

  /**
   * Remove permissions from a role
   */
  async removePermissions(id: string, permissions: string[]): Promise<void> {
    await apiClient.post(ROLE_ENDPOINTS.REMOVE_PERMISSIONS(id), { permissions });
  },

  /**
   * Get users assigned to a specific role
   */
  async getUsersByRole(id: string): Promise<any[]> {
    const response = await apiClient.get<any[]>(ROLE_ENDPOINTS.GET_USERS(id));
    return response.data;
  },
};

export default roleApi;
