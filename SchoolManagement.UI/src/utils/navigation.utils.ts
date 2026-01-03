import { NavItem } from "@/config/navigation.config";
import { User, UserRole } from "@/contexts/AuthContext";

/**
 * Filter navigation items based on user roles
 */
export function filterNavigationByRole(items: NavItem[], userRoles: UserRole[]): NavItem[] {
  return items
    .filter((item) => item.roles.some((role) => userRoles.includes(role)))
    .map((item) => {
      if (item.children) {
        return {
          ...item,
          children: filterNavigationByRole(item.children, userRoles),
        };
      }
      return item;
    })
    .filter((item) => !item.children || item.children.length > 0);
}

/**
 * Check if navigation item is accessible to user
 */
export function hasAccessToNavItem(item: NavItem, user: User | null): boolean {
  if (!user) return false;
  return item.roles.some((role) => user.roles.includes(role));
}

/**
 * Get active parent path for nested navigation
 */
export function getActiveParentPath(items: NavItem[], currentPath: string): string | null {
  for (const item of items) {
    if (item.url === currentPath) {
      return item.url;
    }
    if (item.children) {
      for (const child of item.children) {
        if (child.url === currentPath) {
          return item.url;
        }
      }
    }
  }
  return null;
}

/**
 * Check if path is active or has active children
 */
export function isPathActive(item: NavItem, currentPath: string): boolean {
  if (item.url === currentPath) return true;
  
  if (item.children) {
    return item.children.some((child) => child.url === currentPath);
  }
  
  return false;
}

/**
 * Get breadcrumb trail for current path
 */
export function getBreadcrumbs(items: NavItem[], currentPath: string): NavItem[] {
  const breadcrumbs: NavItem[] = [];
  
  for (const item of items) {
    if (item.url === currentPath) {
      breadcrumbs.push(item);
      return breadcrumbs;
    }
    
    if (item.children) {
      for (const child of item.children) {
        if (child.url === currentPath) {
          breadcrumbs.push(item, child);
          return breadcrumbs;
        }
      }
    }
  }
  
  return breadcrumbs;
}
