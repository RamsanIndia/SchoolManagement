// src/users/api/userEndpoints.ts

/**
 * User API endpoints
 * Aligned with your actual backend controllers
 */

// Base paths
const AUTH_BASE = '/Auth';
const AUDIT_LOGS_BASE = '/AuditLogs';
const PERMISSIONS_BASE = '/Permissions';
const ROLES_BASE = '/Roles';

export const USER_ENDPOINTS = {
  // ==================== USER MANAGEMENT (Auth Controller) ====================
  GET_ALL: AUTH_BASE, // GET /Auth?sortBy=firstname&sortDirection=asc&pageNumber=1&pageSize=10
  GET_BY_ID: (id: string) => `${AUTH_BASE}/${id}`, // GET /Auth/{id}
  GET_PROFILE: `${AUTH_BASE}/profile`, // GET /Auth/profile (current user)
  CREATE: `${AUTH_BASE}/register`, // POST /Auth/register
  UPDATE: (id: string) => `${AUTH_BASE}/${id}`, // PUT /Auth/{id}
  DELETE: (id: string) => `${AUTH_BASE}/${id}`, // DELETE /Auth/{id}
  
  // ==================== ROLE MANAGEMENT ====================
  GET_ROLES: (id: string) => `${AUTH_BASE}/${id}/roles`, // GET /Auth/{id}/roles
  ASSIGN_ROLES: (id: string) => `${AUTH_BASE}/${id}/roles/assign`, // POST /Auth/{id}/roles/assign
  REMOVE_ROLES: (id: string) => `${AUTH_BASE}/${id}/roles/remove`, // POST /Auth/{id}/roles/remove
  
  // ==================== USER STATUS ====================
  ACTIVATE: (id: string) => `${AUTH_BASE}/${id}/activate`,
  DEACTIVATE: (id: string) => `${AUTH_BASE}/${id}/deactivate`,
  LOCK: (id: string) => `${AUTH_BASE}/${id}/lock`,
  UNLOCK: (id: string) => `${AUTH_BASE}/${id}/unlock`,
  
  // ==================== VERIFICATION ====================
  VERIFY_EMAIL: `${AUTH_BASE}/verify-email`,
  VERIFY_PHONE: `${AUTH_BASE}/verify-phone`,
  RESEND_VERIFICATION: (id: string) => `${AUTH_BASE}/${id}/resend-verification`,
  
  // ==================== PASSWORD MANAGEMENT ====================
  CHANGE_PASSWORD: (id: string) => `${AUTH_BASE}/${id}/change-password`,
  ADMIN_RESET_PASSWORD: (id: string) => `${AUTH_BASE}/${id}/reset-password`,
  SET_TEMPORARY_PASSWORD: (id: string) => `${AUTH_BASE}/${id}/set-temporary-password`,
  
  // ==================== ACTIVITY & AUDIT LOGS (AuditLogs Controller) ====================
  GET_ACTIVITY_LOG: (id: string) => `${AUDIT_LOGS_BASE}/user/${id}`, // GET /AuditLogs/user/{id}
  
  // ==================== PERMISSIONS (Permissions Controller) ====================
  GET_PERMISSIONS: (id: string) => `${PERMISSIONS_BASE}/user/${id}`, // GET /Permissions/user/{id}
  ASSIGN_PERMISSIONS: (id: string) => `${PERMISSIONS_BASE}/user/${id}/assign`,
  REMOVE_PERMISSIONS: (id: string) => `${PERMISSIONS_BASE}/user/${id}/remove`,
  
  // ==================== SESSIONS (May not exist yet - will return 404) ====================
  GET_SESSIONS: (id: string) => `${AUTH_BASE}/${id}/sessions`,
  TERMINATE_SESSION: (userId: string, sessionId: string) => `${AUTH_BASE}/${userId}/sessions/${sessionId}`,
  TERMINATE_ALL_SESSIONS: (id: string) => `${AUTH_BASE}/${id}/sessions/all`,
  
  // ==================== STATISTICS (May not exist yet - will return 404) ====================
  GET_STATISTICS: (id: string) => `${AUTH_BASE}/${id}/statistics`,
  
  // ==================== PROFILE MANAGEMENT ====================
  UPLOAD_PHOTO: (id: string) => `${AUTH_BASE}/${id}/photo`,
  DELETE_PHOTO: (id: string) => `${AUTH_BASE}/${id}/photo`,
  GET_PREFERENCES: (id: string) => `${AUTH_BASE}/${id}/preferences`,
  UPDATE_PREFERENCES: (id: string) => `${AUTH_BASE}/${id}/preferences`,
  
  // ==================== BULK OPERATIONS ====================
  BULK_CREATE: `${AUTH_BASE}/bulk`,
  BULK_UPDATE: `${AUTH_BASE}/bulk/update`,
  BULK_DELETE: `${AUTH_BASE}/bulk/delete`,
  BULK_ASSIGN_ROLES: `${AUTH_BASE}/bulk/assign-roles`,
  
  // ==================== SEARCH & FILTER ====================
  SEARCH: `${AUTH_BASE}/search`,
  BY_ROLE: (roleId: string) => `${AUTH_BASE}/by-role/${roleId}`,
  BY_DEPARTMENT: (department: string) => `${AUTH_BASE}/by-department/${department}`,
  
  // ==================== EXPORT ====================
  EXPORT_CSV: `${AUTH_BASE}/export/csv`,
  EXPORT_EXCEL: `${AUTH_BASE}/export/excel`,
} as const;

/**
 * Auth API endpoints
 * These are for authentication and self-service operations
 */
export const AUTH_ENDPOINTS = {
  // ==================== AUTHENTICATION ====================
  LOGIN: `${AUTH_BASE}/login`,
  REGISTER: `${AUTH_BASE}/register`,
  LOGOUT: `${AUTH_BASE}/logout`,
  REFRESH_TOKEN: `${AUTH_BASE}/refresh-token`,
  VERIFY_TOKEN: `${AUTH_BASE}/verify-token`,
  
  // ==================== PASSWORD RESET (Self-Service) ====================
  FORGOT_PASSWORD: `${AUTH_BASE}/forgot-password`,
  RESET_PASSWORD: `${AUTH_BASE}/reset-password`,
  VALIDATE_RESET_TOKEN: `${AUTH_BASE}/validate-reset-token`,
  
  // ==================== EMAIL/PHONE VERIFICATION (Self-Service) ====================
  VERIFY_EMAIL: `${AUTH_BASE}/verify-email`,
  VERIFY_PHONE: `${AUTH_BASE}/verify-phone`,
  RESEND_EMAIL_VERIFICATION: `${AUTH_BASE}/resend-email-verification`,
  RESEND_PHONE_VERIFICATION: `${AUTH_BASE}/resend-phone-verification`,
  
  // ==================== CURRENT USER (Profile) ====================
  GET_CURRENT_USER: `${AUTH_BASE}/profile`, // ✅ GET /Auth/profile
  UPDATE_CURRENT_USER: `${AUTH_BASE}/profile`, // PUT /Auth/profile
  CHANGE_MY_PASSWORD: `${AUTH_BASE}/change-password`, // POST /Auth/change-password
  UPDATE_MY_PREFERENCES: `${AUTH_BASE}/profile/preferences`,
  UPLOAD_MY_PHOTO: `${AUTH_BASE}/profile/photo`,
  DELETE_MY_PHOTO: `${AUTH_BASE}/profile/photo`,
  GET_MY_SESSIONS: `${AUTH_BASE}/profile/sessions`,
  TERMINATE_MY_SESSION: (sessionId: string) => `${AUTH_BASE}/profile/sessions/${sessionId}`,
  
  // ==================== TWO-FACTOR AUTHENTICATION ====================
  ENABLE_2FA: `${AUTH_BASE}/2fa/enable`,
  DISABLE_2FA: `${AUTH_BASE}/2fa/disable`,
  VERIFY_2FA: `${AUTH_BASE}/2fa/verify`,
  GENERATE_2FA_QR: `${AUTH_BASE}/2fa/qr-code`,
  GET_2FA_BACKUP_CODES: `${AUTH_BASE}/2fa/backup-codes`,
} as const;

/**
 * Audit Logs API endpoints
 */
export const AUDIT_LOG_ENDPOINTS = {
  GET_ALL: AUDIT_LOGS_BASE, // GET /AuditLogs
  GET_BY_USER: (userId: string) => `${AUDIT_LOGS_BASE}/user/${userId}`, // GET /AuditLogs/user/{id}
  GET_BY_ID: (id: string) => `${AUDIT_LOGS_BASE}/${id}`,
  GET_BY_DATE_RANGE: `${AUDIT_LOGS_BASE}/date-range`,
  GET_BY_ENTITY: (entityType: string, entityId: string) => 
    `${AUDIT_LOGS_BASE}/entity/${entityType}/${entityId}`,
  EXPORT_CSV: `${AUDIT_LOGS_BASE}/export/csv`,
  EXPORT_EXCEL: `${AUDIT_LOGS_BASE}/export/excel`,
} as const;

/**
 * Role API endpoints
 */
export const ROLE_ENDPOINTS = {
  GET_ALL: ROLES_BASE, // GET /Roles
  GET_BY_ID: (id: string) => `${ROLES_BASE}/${id}`,
  CREATE: ROLES_BASE, // POST /Roles
  UPDATE: (id: string) => `${ROLES_BASE}/${id}`,
  DELETE: (id: string) => `${ROLES_BASE}/${id}`,
  
  // Status management
  ACTIVATE: (id: string) => `${ROLES_BASE}/${id}/activate`,
  DEACTIVATE: (id: string) => `${ROLES_BASE}/${id}/deactivate`,
  
  // Permission management
  GET_PERMISSIONS: (id: string) => `${ROLES_BASE}/${id}/permissions`,
  ASSIGN_PERMISSIONS: (id: string) => `${ROLES_BASE}/${id}/permissions/assign`,
  REMOVE_PERMISSIONS: (id: string) => `${ROLES_BASE}/${id}/permissions/remove`,
  
  // User management
  GET_USERS: (id: string) => `${ROLES_BASE}/${id}/users`,
  ASSIGN_USERS: (id: string) => `${ROLES_BASE}/${id}/users/assign`,
  REMOVE_USERS: (id: string) => `${ROLES_BASE}/${id}/users/remove`,
} as const;

/**
 * Permission API endpoints
 */
export const PERMISSION_ENDPOINTS = {
  GET_ALL: PERMISSIONS_BASE, // GET /Permissions
  GET_BY_ID: (id: string) => `${PERMISSIONS_BASE}/${id}`,
  GET_BY_USER: (userId: string) => `${PERMISSIONS_BASE}/user/${userId}`, // GET /Permissions/user/{id}
  GET_BY_ROLE: (roleId: string) => `${PERMISSIONS_BASE}/role/${roleId}`,
  GET_BY_MODULE: (module: string) => `${PERMISSIONS_BASE}/module/${module}`,
  GET_MODULES: `${PERMISSIONS_BASE}/modules`,
  CREATE: PERMISSIONS_BASE,
  UPDATE: (id: string) => `${PERMISSIONS_BASE}/${id}`,
  DELETE: (id: string) => `${PERMISSIONS_BASE}/${id}`,
} as const;

// Export all endpoints as a single object for convenience
export const API_ENDPOINTS = {
  AUTH: AUTH_ENDPOINTS,
  USERS: USER_ENDPOINTS,
  AUDIT_LOGS: AUDIT_LOG_ENDPOINTS,
  ROLES: ROLE_ENDPOINTS,
  PERMISSIONS: PERMISSION_ENDPOINTS,
} as const;

// Type-safe endpoint keys
export type UserEndpointKey = keyof typeof USER_ENDPOINTS;
export type AuthEndpointKey = keyof typeof AUTH_ENDPOINTS;
export type AuditLogEndpointKey = keyof typeof AUDIT_LOG_ENDPOINTS;
export type RoleEndpointKey = keyof typeof ROLE_ENDPOINTS;
export type PermissionEndpointKey = keyof typeof PERMISSION_ENDPOINTS;

// Default export
export default USER_ENDPOINTS;
