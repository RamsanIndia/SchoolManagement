import { apiClient } from '@/lib/api/apiClient';
import { buildQueryString } from '@/shared/utils/queryUtils';
import { Student, StudentDto, UpdateStudentDto } from '../types/student.types';
import { StudentsResponse, StudentResponse, StudentQueryParams } from '../types/api.types';

const STUDENT_ENDPOINTS = {
  GET_ALL: '/students',
  GET_BY_ID: (id: string) => `/students/${id}`,
  CREATE: '/students',
  UPDATE: (id: string) => `/students/${id}`,
  DELETE: (id: string) => `/students/${id}`,
} as const;

export const studentApi = {
  async getStudents(params?: StudentQueryParams): Promise<StudentsResponse> {
    const queryString = buildQueryString(params);
    const endpoint = `${STUDENT_ENDPOINTS.GET_ALL}${queryString}`;
    return apiClient.getList<Student>(endpoint);
  },

  async getStudentById(id: string): Promise<StudentResponse> {
    return apiClient.get<Student>(STUDENT_ENDPOINTS.GET_BY_ID(id));
  },

  async createStudent(data: StudentDto): Promise<StudentResponse> {
    return apiClient.post<Student>(STUDENT_ENDPOINTS.CREATE, data);
  },

  async updateStudent(id: string, data: UpdateStudentDto): Promise<StudentResponse> {
    return apiClient.put<Student>(STUDENT_ENDPOINTS.UPDATE(id), data);
  },

  async deleteStudent(id: string): Promise<void> {
    await apiClient.delete(STUDENT_ENDPOINTS.DELETE(id));
  },
};
