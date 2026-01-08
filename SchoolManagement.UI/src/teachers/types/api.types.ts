import { 
  ApiResponse, 
  PaginatedApiResponse, 
  BaseQueryParams 
} from '@/shared/types/api.types';
import { Teacher } from './teacher.types';

/**
 * Teacher-specific response types
 */
export type TeacherResponse = ApiResponse<Teacher>;
export type TeachersResponse = PaginatedApiResponse<Teacher>;

/**
 * Teacher-specific query parameters
 */
export interface TeacherQueryParams extends BaseQueryParams {
  departmentId?: string;
  subjectId?: string;
  employmentType?: 'fulltime' | 'parttime' | 'contract';
  designation?: string;
  minExperience?: number;
  maxExperience?: number;
  gender?: 'Male' | 'Female' | 'Other';
}
