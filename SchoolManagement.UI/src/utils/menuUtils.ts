import { MenuItem } from "@/services/MenuService";

/**
 * Recursively sorts menus and their children by sortOrder and displayName.
 */
function sortMenusRecursively(menus: MenuItem[]): MenuItem[] {
  return menus
    .map(menu => ({
      ...menu,
      children: menu.children ? sortMenusRecursively(menu.children) : []
    }))
    .sort((a, b) => {
      const orderA = a.sortOrder ?? 0;
      const orderB = b.sortOrder ?? 0;

      if (orderA === orderB) {
        return a.displayName.localeCompare(b.displayName);
      }
      return orderA - orderB;
    });
}

/**
 * Builds a hierarchical menu tree.
 * Works for both flat (with parentMenuId) and nested (with children) structures.
 */
export function buildMenuTree(flatMenus: MenuItem[]): MenuItem[] {
  if (!flatMenus || flatMenus.length === 0) return [];

  // If data is already nested (children exist), just sort recursively.
  const isNested = flatMenus.some(m => Array.isArray(m.children) && m.children.length > 0);
  if (isNested) {
    return sortMenusRecursively(flatMenus);
  }

  // Otherwise, handle flat list using parentMenuId
  const menuMap = new Map<string, MenuItem>();
  flatMenus.forEach(menu => {
    menu.children = [];
    menuMap.set(menu.id, menu);
  });

  const rootMenus: MenuItem[] = [];

  flatMenus.forEach(menu => {
    if (menu.parentMenuId) {
      const parent = menuMap.get(menu.parentMenuId);
      if (parent) {
        parent.children!.push(menu);
      } else {
        rootMenus.push(menu);
      }
    } else {
      rootMenus.push(menu);
    }
  });

  return sortMenusRecursively(rootMenus);
}
