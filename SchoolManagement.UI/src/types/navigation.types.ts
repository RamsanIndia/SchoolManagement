import { LucideIcon } from "lucide-react";

export interface MenuPermissions {
  canView: boolean;
  canAdd: boolean;
  canEdit: boolean;
  canDelete: boolean;
  canExport: boolean;
  canPrint: boolean;
  canApprove: boolean;
  canReject: boolean;
}

export interface ApiMenuItem {
  id: string;
  name: string;
  displayName: string;
  icon: string;
  route: string;
  type: string;
  sortOrder: number;
  parentId?: string;
  permissions: MenuPermissions;
  children: ApiMenuItem[];
}

export interface ApiMenuResponse {
  data: ApiMenuItem[];
  status: boolean;
  message: string;
  errors: string[];
}

export interface NavItem {
  id: string;
  title: string;
  url: string;
  icon: LucideIcon;
  permissions: MenuPermissions;
  children?: NavItem[];
}
