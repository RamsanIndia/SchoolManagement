// src/users/types/user.types.ts
import type { PaginatedResponse } from '@/shared/types/api.types';

/**
 * Base User entity - matches backend API response
 */
export interface User {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  isEmailVerified: boolean;
  isPhoneVerified: boolean;
  roles: string[];
  lastLoginAt: string | null;
  createdAt: string;
  updatedAt?: string;
}

/**
 * Extended user with computed/additional properties
 */
export interface UserDetails extends User {
  fullName: string;
  profilePhoto?: string;
  department?: string;
  location?: string;
  status: 'active' | 'inactive' | 'locked';
}

/**
 * DTO for creating a new user
 */
export interface CreateUserDto {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  password: string;
  confirmPassword?: string;
  roleIds?: string[];
  department?: string;
  location?: string;
}

export type UserDto = CreateUserDto;

/**
 * DTO for updating an existing user
 */
export interface UpdateUserDto {
  username?: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  department?: string;
  location?: string;
  roleIds?: string[];
  status?: 'active' | 'inactive' | 'locked';
  profilePhoto?: string;
}

/**
 * User role assignment/management
 */
export interface UserRoleAssignmentDto {
  userId: string;
  roleIds: string[];
}

/**
 * Password management DTOs
 */
export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ResetPasswordRequestDto {
  email: string;
}

export interface ResetPasswordDto {
  email: string;
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ForgotPasswordDto {
  email: string;
}

/**
 * User permissions response
 */
export interface UserPermissions {
  [category: string]: string[];
}

/**
 * Activity log entry
 */
export interface ActivityLog {
  id: string;
  userId: string;
  action: string;
  description?: string;
  timestamp: string;
  ipAddress: string;
  device: string;
  userAgent?: string;
  metadata?: Record<string, any>;
}

/**
 * User session information
 */
export interface UserSession {
  id: string;
  userId: string;
  device: string;
  browser?: string;
  operatingSystem?: string;
  ipAddress: string;
  location: string;
  lastActive: string;
  createdAt: string;
  expiresAt?: string;
  isCurrent: boolean;
}

/**
 * User statistics for profile dashboard
 */
export interface UserStatistics {
  totalPermissions: number;
  actionsToday: number;
  activeSessions: number;
  loginSuccessRate: number;
}

/**
 * Paginated user list response
 * ✅ Uses the shared PaginatedResponse type
 */
export type PaginatedUsers = PaginatedResponse<UserDetails>;

/**
 * User query/filter parameters
 * ✅ Updated to match your BaseQueryParams
 */
export interface UserQueryParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: 'firstName' | 'lastName' | 'email' | 'createdAt' | 'lastLoginAt' | 'username'; // ✅ Added 'username'
  sortDirection?: 'asc' | 'desc';
  role?: string;
  status?: 'active' | 'inactive' | 'locked';
  department?: string;
  isEmailVerified?: boolean;
  isPhoneVerified?: boolean;
}