import { 
  ApiResponse, 
  PaginatedApiResponse, 
  BaseQueryParams 
} from '@/shared/types/api.types';
import { Role } from './role.types';

/**
 * Role-specific response types
 */
export type RoleResponse = ApiResponse<Role>;
export type RolesResponse = PaginatedApiResponse<Role>;

/**
 * Role-specific query parameters (extends base)
 */
export interface RoleQueryParams extends BaseQueryParams {
  isSystemRole?: boolean;
  isActive?: boolean;
  level?: number;
  minLevel?: number;
  maxLevel?: number;
}
