// src/roles/api/roleEndpoints.ts

const ROLES_BASE = '/Roles';

const ROLE_ENDPOINTS = {
  // ==================== CRUD OPERATIONS ====================
  GET_ALL: ROLES_BASE,
  GET_BY_ID: (id: string) => `${ROLES_BASE}/${id}`,
  CREATE: ROLES_BASE,
  UPDATE: (id: string) => `${ROLES_BASE}/${id}`,
  DELETE: (id: string) => `${ROLES_BASE}/${id}`,

  // ==================== STATUS MANAGEMENT ====================
  ACTIVATE: (id: string) => `${ROLES_BASE}/${id}/activate`,
  DEACTIVATE: (id: string) => `${ROLES_BASE}/${id}/deactivate`,

  // ==================== PERMISSIONS ====================
  GET_PERMISSIONS: (id: string) => `${ROLES_BASE}/${id}/permissions`,
  ASSIGN_PERMISSIONS: (id: string) => `${ROLES_BASE}/${id}/permissions/assign`,
  REMOVE_PERMISSIONS: (id: string) => `${ROLES_BASE}/${id}/permissions/remove`,

  // ==================== USERS ====================
  GET_USERS: (id: string) => `${ROLES_BASE}/${id}/users`,
  ASSIGN_USERS: (id: string) => `${ROLES_BASE}/${id}/users/assign`,
  REMOVE_USERS: (id: string) => `${ROLES_BASE}/${id}/users/remove`,
} as const;

export { ROLE_ENDPOINTS };
export default ROLE_ENDPOINTS;

export type RoleEndpointKey = keyof typeof ROLE_ENDPOINTS;
