import { apiClient } from '@/lib/api/apiClient';
import { buildQueryString } from '@/shared/utils/queryUtils';
import { Teacher, TeacherDto, UpdateTeacherDto } from '../types/teacher.types';
import { TeachersResponse, TeacherResponse, TeacherQueryParams } from '../types/api.types';

const TEACHER_ENDPOINTS = {
  GET_ALL: '/teachers',
  GET_BY_ID: (id: string) => `/teachers/${id}`,
  CREATE: '/teachers',
  UPDATE: (id: string) => `/teachers/${id}`,
  DELETE: (id: string) => `/teachers/${id}`,
} as const;

export const teacherApi = {
  async getTeachers(params?: TeacherQueryParams): Promise<TeachersResponse> {
    const queryString = buildQueryString(params);
    const endpoint = `${TEACHER_ENDPOINTS.GET_ALL}${queryString}`;
    return apiClient.getList<Teacher>(endpoint);
  },

  async getTeacherById(id: string): Promise<TeacherResponse> {
    return apiClient.get<Teacher>(TEACHER_ENDPOINTS.GET_BY_ID(id));
  },

  async createTeacher(data: TeacherDto): Promise<TeacherResponse> {
    return apiClient.post<Teacher>(TEACHER_ENDPOINTS.CREATE, data);
  },

  async updateTeacher(id: string, data: UpdateTeacherDto): Promise<TeacherResponse> {
    return apiClient.put<Teacher>(TEACHER_ENDPOINTS.UPDATE(id), data);
  },

  async deleteTeacher(id: string): Promise<void> {
    await apiClient.delete(TEACHER_ENDPOINTS.DELETE(id));
  },
};
