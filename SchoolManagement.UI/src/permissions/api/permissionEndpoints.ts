// src/permissions/api/permissionEndpoints.ts

const PERMISSIONS_BASE = '/Permissions';

/**
 * Permission API endpoints
 */
export const PERMISSION_ENDPOINTS = {
  // ==================== CRUD OPERATIONS ====================
  GET_ALL: PERMISSIONS_BASE,
  GET_BY_ID: (id: string) => `${PERMISSIONS_BASE}/${id}`,
  CREATE: PERMISSIONS_BASE,
  UPDATE: (id: string) => `${PERMISSIONS_BASE}/${id}`,
  DELETE: (id: string) => `${PERMISSIONS_BASE}/${id}`,

  // ==================== GROUPING & FILTERING ====================
  GET_BY_MODULE: `${PERMISSIONS_BASE}/by-module`,
  GET_BY_MODULE_NAME: (module: string) => `${PERMISSIONS_BASE}/module/${module}`,
  GET_MODULES: `${PERMISSIONS_BASE}/modules`,

  // ==================== ROLE PERMISSIONS ====================
  GET_ROLE_PERMISSIONS: (roleId: string) => `${PERMISSIONS_BASE}/roles/${roleId}`,
  ASSIGN_TO_ROLE: (roleId: string) => `${PERMISSIONS_BASE}/roles/${roleId}/assign`,
  REMOVE_FROM_ROLE: (roleId: string) => `${PERMISSIONS_BASE}/roles/${roleId}/remove`,
  SYNC_ROLE_PERMISSIONS: (roleId: string) => `${PERMISSIONS_BASE}/roles/${roleId}/sync`,

  // ==================== USER PERMISSIONS ====================
  GET_USER_PERMISSIONS: (userId: string) => `${PERMISSIONS_BASE}/user/${userId}`,
} as const;

export type PermissionEndpointKey = keyof typeof PERMISSION_ENDPOINTS;
