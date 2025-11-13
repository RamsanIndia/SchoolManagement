import { apiFetch } from "@/lib/apiFetch";

export interface MenuItem {
  id: string;
  name: string;
  displayName: string;
  description?: string;
  icon?: string;
  route?: string;
  parentMenuId?: string | null; // optional for flat APIs
  sortOrder?: number;
  children?: MenuItem[]; // nested menus
}


export async function getMenus(): Promise<MenuItem[]> {
  const response = await apiFetch("/Menus/user-menus");

  if (!Array.isArray(response)) {
    console.error("❌ Invalid API response for menus:", response);
    return [];
  }

  // Recursive sort helper
  const sortMenus = (menus: MenuItem[]): MenuItem[] => {
    return menus
      .map(m => ({
        ...m,
        children: m.children ? sortMenus(m.children) : []
      }))
      .sort((a, b) => {
        const orderA = a.sortOrder ?? 0;
        const orderB = b.sortOrder ?? 0;
        return orderA === orderB
          ? a.displayName.localeCompare(b.displayName)
          : orderA - orderB;
      });
  };

  // Normalize & sort recursively
  const normalized = sortMenus(response);

  return normalized;
}
