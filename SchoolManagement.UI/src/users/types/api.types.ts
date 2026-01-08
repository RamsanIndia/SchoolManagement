import { 
  ApiResponse, 
  PaginatedApiResponse, 
  BaseQueryParams 
} from '@/shared/types/api.types';
import { User } from './user.types';

/**
 * User-specific response types
 */
export type UserResponse = ApiResponse<User>;
export type UsersResponse = PaginatedApiResponse<User>;

/**
 * User-specific query parameters
 */
export interface UserQueryParams extends BaseQueryParams {
  role?: string;
  isEmailVerified?: boolean;
  isPhoneVerified?: boolean;
  isActive?: boolean;
  // These are inherited from BaseQueryParams but explicitly defined for clarity
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}
