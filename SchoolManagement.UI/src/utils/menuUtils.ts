import { MenuItem } from "@/services/MenuService";

/**
 * Recursively sorts children menus by displayName.
 */
function processChildren(menu: MenuItem): void {
  if (menu.children && menu.children.length > 0) {
    menu.children.forEach((child: MenuItem) => {
      processChildren(child);
    });
    menu.children.sort((a, b) => a.displayName.localeCompare(b.displayName));
  }
}

/**
 * Builds a hierarchical menu tree from a flat list.
 * Links parent-child using parentMenuId and sorts nodes recursively.
 * @param flatMenus - flat array of MenuItem objects
 * @returns nested array of root MenuItems, each with children populated
 */
export function buildMenuTree(flatMenus: MenuItem[]): MenuItem[] {
  const menuMap = new Map<string, MenuItem>();

  // Populate map of id to menu item
  flatMenus.forEach(menu => {
    menuMap.set(menu.id, menu);
  });

  const rootMenus: MenuItem[] = [];

  // Link children to parents
  flatMenus.forEach(menu => {
    if (menu.parentMenuId) {
      const parent = menuMap.get(menu.parentMenuId);
      if (parent) {
        if (!parent.children) parent.children = [];
        parent.children.push(menu);
      } else {
        // parentMenuId provided but parent not found; treat as root to avoid data loss
        rootMenus.push(menu);
      }
    } else {
      // No parentMenuId means root level
      rootMenus.push(menu);
    }
  });

  // Recursively sort children by displayName
  rootMenus.forEach(menu => {
    processChildren(menu);
  });

  // Sort roots by sortOrder or displayName
  rootMenus.sort((a, b) => {
    const orderA = a.sortOrder ?? 0;
    const orderB = b.sortOrder ?? 0;

    if (orderA === orderB) {
      return a.displayName.localeCompare(b.displayName);
    }
    return orderA - orderB;
  });

  return rootMenus;
}
