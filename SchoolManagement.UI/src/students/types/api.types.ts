import { 
  ApiResponse, 
  PaginatedApiResponse, 
  BaseQueryParams 
} from '@/shared/types/api.types';
import { Student } from './student.types';

/**
 * Student-specific response types
 */
export type StudentResponse = ApiResponse<Student>;
export type StudentsResponse = PaginatedApiResponse<Student>;

/**
 * Student-specific query parameters
 */
export interface StudentQueryParams extends BaseQueryParams {
  classId?: string;
  sectionId?: string;
  academicYear?: string;
  enrollmentStatus?: 'active' | 'inactive' | 'suspended' | 'graduated';
  gender?: 'Male' | 'Female' | 'Other';
  bloodGroup?: string;
}
