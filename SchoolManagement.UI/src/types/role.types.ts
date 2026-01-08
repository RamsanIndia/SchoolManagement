export interface Role {
  id: string;
  name: string;
  displayName: string;
  description: string;
  userCount: number;
  isSystemRole: boolean;
  isActive: boolean;
  level: number;
}

export interface RoleDto {
  name: string;
  displayName: string;
  description: string;
  level: number;
}

export interface UpdateRoleDto extends Partial<RoleDto> {
  isActive?: boolean;
}
