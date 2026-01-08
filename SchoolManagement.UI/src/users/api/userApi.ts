// src/users/api/userApi.ts
import { apiClient } from '@/lib/api/apiClient';
import { USER_ENDPOINTS, AUTH_ENDPOINTS } from './userEndpoints';
import type { 
  User,
  UserDetails,
  CreateUserDto,
  UpdateUserDto,
  UserRoleAssignmentDto,
  ChangePasswordDto,
  ResetPasswordDto,
  ForgotPasswordDto,
  UserPermissions,
  ActivityLog,
  UserSession,
  UserStatistics,
  PaginatedUsers,
  UserQueryParams,
} from '@/users/types/user.types';

/**
 * User API Service
 * Handles all user-related API calls
 */
export const userApi = {
  /**
   * Get all users with pagination and filtering
   * Returns: PaginatedResponse<UserDetails>
   */
  async getAll(params?: UserQueryParams): Promise<PaginatedUsers> {
    // ✅ getList already returns PaginatedResponse<T> directly
    return await apiClient.getList<UserDetails>(USER_ENDPOINTS.GET_ALL, { params });
  },

  /**
   * Get current logged-in user profile
   * GET /Auth/profile
   */
  async getCurrentUser(): Promise<UserDetails> {
    const response = await apiClient.get<UserDetails>(USER_ENDPOINTS.GET_PROFILE);
    return response.data;
  },

  /**
   * Get a single user by ID with full details
   * GET /Auth/{id}
   */
  async getById(id: string): Promise<UserDetails> {
    const response = await apiClient.get<UserDetails>(USER_ENDPOINTS.GET_BY_ID(id));
    return response.data;
  },

  /**
   * Get basic user info (without extended details)
   */
  async getBasicById(id: string): Promise<User> {
    const response = await apiClient.get<User>(`${USER_ENDPOINTS.GET_BY_ID(id)}/basic`);
    return response.data;
  },

  /**
   * Create a new user
   */
  async create(data: CreateUserDto): Promise<User> {
    const response = await apiClient.post<User>(USER_ENDPOINTS.CREATE, data);
    return response.data;
  },

  /**
   * Update an existing user
   */
  async update(id: string, data: UpdateUserDto): Promise<User> {
    const response = await apiClient.put<User>(USER_ENDPOINTS.UPDATE(id), data);
    return response.data;
  },

  /**
   * Partially update user (PATCH)
   */
  async partialUpdate(id: string, data: Partial<UpdateUserDto>): Promise<User> {
    const response = await apiClient.patch<User>(USER_ENDPOINTS.UPDATE(id), data);
    return response.data;
  },

  /**
   * Delete a user (soft delete)
   */
  async delete(id: string): Promise<void> {
    await apiClient.delete(USER_ENDPOINTS.DELETE(id));
  },

  /**
   * Permanently delete a user (hard delete)
   */
  async permanentDelete(id: string): Promise<void> {
    await apiClient.delete(`${USER_ENDPOINTS.DELETE(id)}/permanent`);
  },

  /**
   * Restore a soft-deleted user
   */
  async restore(id: string): Promise<User> {
    const response = await apiClient.post<User>(`${USER_ENDPOINTS.GET_BY_ID(id)}/restore`);
    return response.data;
  },

  // ==================== ROLE MANAGEMENT ====================

  /**
   * Assign roles to a user
   */
  async assignRoles(data: UserRoleAssignmentDto): Promise<User> {
    const response = await apiClient.post<User>(
      USER_ENDPOINTS.ASSIGN_ROLES(data.userId), 
      { roleIds: data.roleIds }
    );
    return response.data;
  },

  /**
   * Remove roles from a user
   */
  async removeRoles(userId: string, roleIds: string[]): Promise<User> {
    const response = await apiClient.post<User>(
      USER_ENDPOINTS.REMOVE_ROLES(userId),
      { roleIds }
    );
    return response.data;
  },

  /**
   * Update user roles (replace existing)
   */
  async updateRoles(userId: string, roleIds: string[]): Promise<User> {
    const response = await apiClient.put<User>(
      `${USER_ENDPOINTS.GET_BY_ID(userId)}/roles`,
      { roleIds }
    );
    return response.data;
  },

  // ==================== PERMISSIONS ====================

  /**
   * Get user's effective permissions
   * GET /Permissions/user/{id}
   */
  async getPermissions(id: string): Promise<UserPermissions> {
    const response = await apiClient.get<UserPermissions>(USER_ENDPOINTS.GET_PERMISSIONS(id));
    return response.data;
  },

  /**
   * Check if user has specific permission
   */
  async hasPermission(id: string, permission: string): Promise<boolean> {
    const response = await apiClient.get<{ hasPermission: boolean }>(
      `${USER_ENDPOINTS.GET_BY_ID(id)}/permissions/check`,
      { params: { permission } }
    );
    return response.data.hasPermission;
  },

  // ==================== VERIFICATION ====================

  /**
   * Verify user email with token
   */
  async verifyEmail(id: string, token: string): Promise<User> {
    const response = await apiClient.post<User>(USER_ENDPOINTS.VERIFY_EMAIL, { 
      userId: id, 
      token 
    });
    return response.data;
  },

  /**
   * Verify user phone with OTP code
   */
  async verifyPhone(id: string, code: string): Promise<User> {
    const response = await apiClient.post<User>(USER_ENDPOINTS.VERIFY_PHONE, { 
      userId: id, 
      code 
    });
    return response.data;
  },

  /**
   * Resend verification email
   */
  async resendEmailVerification(id: string): Promise<void> {
    await apiClient.post(USER_ENDPOINTS.RESEND_VERIFICATION(id), {
      type: 'email'
    });
  },

  /**
   * Resend phone verification OTP
   */
  async resendPhoneVerification(id: string): Promise<void> {
    await apiClient.post(USER_ENDPOINTS.RESEND_VERIFICATION(id), {
      type: 'phone'
    });
  },

  // ==================== PASSWORD MANAGEMENT ====================

  /**
   * Change user password (requires current password)
   */
  async changePassword(id: string, data: ChangePasswordDto): Promise<void> {
    await apiClient.post(USER_ENDPOINTS.CHANGE_PASSWORD(id), data);
  },

  /**
   * Request password reset (forgot password)
   */
  async forgotPassword(data: ForgotPasswordDto): Promise<void> {
    await apiClient.post(AUTH_ENDPOINTS.FORGOT_PASSWORD, data);
  },

  /**
   * Reset password with token
   */
  async resetPassword(data: ResetPasswordDto): Promise<void> {
    await apiClient.post(AUTH_ENDPOINTS.RESET_PASSWORD, data);
  },

  /**
   * Admin reset password (sends reset link to user)
   */
  async adminResetPassword(id: string): Promise<void> {
    await apiClient.post(USER_ENDPOINTS.ADMIN_RESET_PASSWORD(id));
  },

  /**
   * Set temporary password (admin only)
   */
  async setTemporaryPassword(id: string, temporaryPassword: string): Promise<void> {
    await apiClient.post(USER_ENDPOINTS.SET_TEMPORARY_PASSWORD(id), {
      temporaryPassword,
    });
  },

  // ==================== USER STATUS ====================

  /**
   * Activate a user account
   */
  async activate(id: string): Promise<User> {
    const response = await apiClient.patch<User>(USER_ENDPOINTS.ACTIVATE(id));
    return response.data;
  },

  /**
   * Deactivate a user account
   */
  async deactivate(id: string): Promise<User> {
    const response = await apiClient.patch<User>(USER_ENDPOINTS.DEACTIVATE(id));
    return response.data;
  },

  /**
   * Lock user account (after failed login attempts)
   */
  async lock(id: string, reason?: string): Promise<User> {
    const response = await apiClient.patch<User>(USER_ENDPOINTS.LOCK(id), { reason });
    return response.data;
  },

  /**
   * Unlock user account
   */
  async unlock(id: string): Promise<User> {
    const response = await apiClient.patch<User>(USER_ENDPOINTS.UNLOCK(id));
    return response.data;
  },

  // ==================== ACTIVITY & SESSIONS ====================

  /**
   * Get user activity log
   * GET /AuditLogs/user/{id}?pageNumber=1&pageSize=50
   */
  async getActivityLog(
    id: string, 
    params?: { pageNumber?: number; pageSize?: number; startDate?: string; endDate?: string }
  ): Promise<ActivityLog[]> {
    const response = await apiClient.get<ActivityLog[]>(
      USER_ENDPOINTS.GET_ACTIVITY_LOG(id),
      { params }
    );
    return response.data;
  },

  /**
   * Get user's active sessions
   */
  async getSessions(id: string): Promise<UserSession[]> {
    const response = await apiClient.get<UserSession[]>(USER_ENDPOINTS.GET_SESSIONS(id));
    return response.data;
  },

  /**
   * Terminate a specific session
   */
  async terminateSession(userId: string, sessionId: string): Promise<void> {
    await apiClient.delete(USER_ENDPOINTS.TERMINATE_SESSION(userId, sessionId));
  },

  /**
   * Terminate all sessions except current
   */
  async terminateAllSessions(id: string): Promise<void> {
    await apiClient.delete(USER_ENDPOINTS.TERMINATE_ALL_SESSIONS(id));
  },

  /**
   * Get user statistics
   */
  async getStatistics(id: string): Promise<UserStatistics> {
    const response = await apiClient.get<UserStatistics>(USER_ENDPOINTS.GET_STATISTICS(id));
    return response.data;
  },

  // ==================== PROFILE MANAGEMENT ====================

  /**
   * Upload user profile photo
   */
  async uploadPhoto(userId: string, file: File): Promise<string> {
    const response = await apiClient.upload<{ photoUrl: string }>(
      USER_ENDPOINTS.UPLOAD_PHOTO(userId),
      file,
      'photo'
    );
    return response.data.photoUrl;
  },

  /**
   * Delete user profile photo
   */
  async deletePhoto(id: string): Promise<void> {
    await apiClient.delete(USER_ENDPOINTS.DELETE_PHOTO(id));
  },

  /**
   * Update user preferences
   */
  async updatePreferences(id: string, preferences: Record<string, any>): Promise<void> {
    await apiClient.put(USER_ENDPOINTS.UPDATE_PREFERENCES(id), preferences);
  },

  /**
   * Get user preferences
   */
  async getPreferences(id: string): Promise<Record<string, any>> {
    const response = await apiClient.get<Record<string, any>>(
      USER_ENDPOINTS.GET_PREFERENCES(id)
    );
    return response.data;
  },

  // ==================== BULK OPERATIONS ====================

  /**
   * Bulk create users (import)
   */
  async bulkCreate(
    users: CreateUserDto[]
  ): Promise<{ success: number; failed: number; errors: any[] }> {
    const response = await apiClient.post<{ success: number; failed: number; errors: any[] }>(
      USER_ENDPOINTS.BULK_CREATE,
      { users }
    );
    return response.data;
  },

  /**
   * Bulk update users
   */
  async bulkUpdate(updates: Array<{ id: string; data: UpdateUserDto }>): Promise<void> {
    await apiClient.put(USER_ENDPOINTS.BULK_UPDATE, { updates });
  },

  /**
   * Bulk delete users
   */
  async bulkDelete(userIds: string[]): Promise<void> {
    await apiClient.post(USER_ENDPOINTS.BULK_DELETE, { userIds });
  },

  /**
   * Bulk assign roles
   */
  async bulkAssignRoles(userIds: string[], roleIds: string[]): Promise<void> {
    await apiClient.post(USER_ENDPOINTS.BULK_ASSIGN_ROLES, {
      userIds,
      roleIds,
    });
  },

  // ==================== SEARCH & FILTER ====================

  /**
   * Search users by query
   */
  async search(query: string, filters?: Partial<UserQueryParams>): Promise<UserDetails[]> {
    const params = { searchTerm: query, ...filters };
    const response = await apiClient.get<UserDetails[]>(
      USER_ENDPOINTS.SEARCH,
      { params }
    );
    return response.data;
  },

  /**
   * Get users by department
   * Returns: PaginatedResponse<UserDetails>
   */
  async getByDepartment(
    department: string, 
    params?: Omit<UserQueryParams, 'department'>
  ): Promise<PaginatedUsers> {
    // ✅ getList already returns PaginatedResponse<T> directly
    return await apiClient.getList<UserDetails>(
      USER_ENDPOINTS.GET_ALL,
      { params: { ...params, department } }
    );
  },

  /**
   * Get users by role
   * Returns: PaginatedResponse<UserDetails>
   */
  async getByRole(
    role: string, 
    params?: Omit<UserQueryParams, 'role'>
  ): Promise<PaginatedUsers> {
    // ✅ getList already returns PaginatedResponse<T> directly
    return await apiClient.getList<UserDetails>(
      USER_ENDPOINTS.GET_ALL,
      { params: { ...params, role } }
    );
  },

  // ==================== EXPORT ====================

  /**
   * Export users to CSV
   */
  async exportToCSV(params?: UserQueryParams): Promise<void> {
    await apiClient.download(
      USER_ENDPOINTS.EXPORT_CSV,
      `users-export-${new Date().toISOString().split('T')[0]}.csv`,
      { params }
    );
  },

  /**
   * Export users to Excel
   */
  async exportToExcel(params?: UserQueryParams): Promise<void> {
    await apiClient.download(
      USER_ENDPOINTS.EXPORT_EXCEL,
      `users-export-${new Date().toISOString().split('T')[0]}.xlsx`,
      { params }
    );
  },

  /**
   * Export user activity log
   */
  async exportActivityLog(userId: string, params?: { startDate?: string; endDate?: string }): Promise<void> {
    await apiClient.download(
      `${USER_ENDPOINTS.GET_ACTIVITY_LOG(userId)}/export`,
      `user-${userId}-activity-${new Date().toISOString().split('T')[0]}.csv`,
      { params }
    );
  },
};

export type UserApiType = typeof userApi;

export default userApi;
