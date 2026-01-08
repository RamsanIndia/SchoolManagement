/**
 * Teacher entity
 */
export interface Teacher {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  gender: 'Male' | 'Female' | 'Other';
  employeeId: string;
  joiningDate: string;
  departmentId: string;
  departmentName?: string;
  designation: string;
  qualification: string;
  experience: number;
  employmentType: 'fulltime' | 'parttime' | 'contract';
  salary: number;
  bloodGroup?: string;
  address: string;
  city: string;
  state: string;
  pinCode: string;
  emergencyContact: string;
  emergencyContactName: string;
  isActive: boolean;
  profilePhoto?: string;
  subjects?: TeacherSubject[];
  createdAt: string;
  updatedAt: string;
}

/**
 * Teacher subject mapping
 */
export interface TeacherSubject {
  subjectId: string;
  subjectName: string;
  classId: string;
  className: string;
  sectionId?: string;
  sectionName?: string;
}

/**
 * DTO for creating a new teacher
 */
export interface TeacherDto {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  gender: 'Male' | 'Female' | 'Other';
  employeeId: string;
  joiningDate: string;
  departmentId: string;
  designation: string;
  qualification: string;
  experience: number;
  employmentType: 'fulltime' | 'parttime' | 'contract';
  salary: number;
  bloodGroup?: string;
  address: string;
  city: string;
  state: string;
  pinCode: string;
  emergencyContact: string;
  emergencyContactName: string;
}

/**
 * DTO for updating an existing teacher
 */
export interface UpdateTeacherDto extends Partial<TeacherDto> {
  isActive?: boolean;
}
