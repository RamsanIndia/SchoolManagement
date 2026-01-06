import { useNavigation } from '@/contexts/NavigationContext';
import { useLocation } from 'react-router-dom';
import { MenuPermissions } from '@/types/navigation.types';
import { NavItem } from '@/types/navigation.types';

export function usePermissions(): MenuPermissions | null {
  const { menuItems } = useNavigation();
  const location = useLocation();

  const findPermissions = (items: NavItem[], path: string): MenuPermissions | null => {
    for (const item of items) {
      if (item.url === path) {
        return item.permissions;
      }
      if (item.children) {
        const found = findPermissions(item.children, path);
        if (found) return found;
      }
    }
    return null;
  };

  return findPermissions(menuItems, location.pathname);
}

// Helper hooks for specific permissions
export function useCanAdd(): boolean {
  const permissions = usePermissions();
  return permissions?.canAdd ?? false;
}

export function useCanEdit(): boolean {
  const permissions = usePermissions();
  return permissions?.canEdit ?? false;
}

export function useCanDelete(): boolean {
  const permissions = usePermissions();
  return permissions?.canDelete ?? false;
}

export function useCanView(): boolean {
  const permissions = usePermissions();
  return permissions?.canView ?? false;
}

export function useCanExport(): boolean {
  const permissions = usePermissions();
  return permissions?.canExport ?? false;
}

export function useCanPrint(): boolean {
  const permissions = usePermissions();
  return permissions?.canPrint ?? false;
}

export function useCanApprove(): boolean {
  const permissions = usePermissions();
  return permissions?.canApprove ?? false;
}

export function useCanReject(): boolean {
  const permissions = usePermissions();
  return permissions?.canReject ?? false;
}
