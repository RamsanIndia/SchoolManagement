/**
 * Student entity
 */
export interface Student {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  gender: 'Male' | 'Female' | 'Other';
  admissionNumber: string;
  admissionDate: string;
  classId: string;
  className?: string;
  sectionId: string;
  sectionName?: string;
  rollNumber: string;
  bloodGroup?: string;
  address: string;
  city: string;
  state: string;
  pinCode: string;
  parentName: string;
  parentPhone: string;
  parentEmail: string;
  emergencyContact: string;
  enrollmentStatus: 'active' | 'inactive' | 'suspended' | 'graduated';
  isActive: boolean;
  profilePhoto?: string;
  createdAt: string;
  updatedAt: string;
}

/**
 * DTO for creating a new student
 */
export interface StudentDto {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  gender: 'Male' | 'Female' | 'Other';
  admissionNumber: string;
  admissionDate: string;
  classId: string;
  sectionId: string;
  rollNumber: string;
  bloodGroup?: string;
  address: string;
  city: string;
  state: string;
  pinCode: string;
  parentName: string;
  parentPhone: string;
  parentEmail: string;
  emergencyContact: string;
}

/**
 * DTO for updating an existing student
 */
export interface UpdateStudentDto extends Partial<StudentDto> {
  enrollmentStatus?: 'active' | 'inactive' | 'suspended' | 'graduated';
  isActive?: boolean;
}
