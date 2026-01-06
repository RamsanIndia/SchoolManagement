import api from './api';
import { ApiMenuResponse, ApiMenuItem, NavItem, MenuPermissions } from '@/types/navigation.types';
import { getIconComponent } from '@/utils/iconMapper';

interface CachedMenu {
  menu: SerializedNavItem[];
  timestamp: number;
}

interface SerializedNavItem {
  id: string;
  title: string;
  url: string;
  iconName: string;
  permissions: MenuPermissions;
  children?: SerializedNavItem[];
}

class NavigationService {
  private readonly CACHE_KEY = 'sms_user_menu';
  private readonly CACHE_DURATION = 30 * 60 * 1000; // 30 minutes

  /**
   * Fetch user menu from API
   */
  async getUserMenu(): Promise<NavItem[]> {
    try {
      // Check cache first
      const cached = this.getCachedMenu();
      if (cached) {
        return cached;
      }

      // Updated endpoint - use /Menus/user-menus (plural)
      const response = await api.get<ApiMenuResponse>('/Menus/user-menus');
      
      if (!response.data.status) {
        throw new Error(response.data.message || 'Failed to fetch menu');
      }

      // Transform and cache
      const transformedMenu = this.transformApiMenu(response.data.data);
      this.cacheMenu(transformedMenu);

      return transformedMenu;
    } catch (error) {
      console.error('Failed to fetch user menu:', error);
      throw error;
    }
  }

  /**
   * Transform API menu structure to component-friendly format
   */
  private transformApiMenu(apiItems: ApiMenuItem[]): NavItem[] {
    return apiItems
      .filter(item => item.permissions.canView) // Only include items user can view
      .sort((a, b) => a.sortOrder - b.sortOrder)
      .map(item => this.transformMenuItem(item));
  }

  /**
   * Transform single menu item
   */
  private transformMenuItem(apiItem: ApiMenuItem): NavItem {
    const navItem: NavItem = {
      id: apiItem.id,
      title: apiItem.displayName,
      url: apiItem.route,
      icon: getIconComponent(apiItem.icon),
      permissions: apiItem.permissions,
    };

    // Process children if they exist
    if (apiItem.children && apiItem.children.length > 0) {
      navItem.children = apiItem.children
        .filter(child => child.permissions.canView)
        .sort((a, b) => a.sortOrder - b.sortOrder)
        .map(child => this.transformMenuItem(child));
    }

    return navItem;
  }

  /**
   * Serialize NavItem for caching (convert icon component to string)
   */
  private serializeMenuItem(item: NavItem): SerializedNavItem {
    return {
      id: item.id,
      title: item.title,
      url: item.url,
      iconName: item.icon.name || 'BarChart3',
      permissions: item.permissions,
      children: item.children?.map(child => this.serializeMenuItem(child)),
    };
  }

  /**
   * Deserialize cached item (convert icon string to component)
   */
  private deserializeMenuItem(item: SerializedNavItem): NavItem {
    return {
      id: item.id,
      title: item.title,
      url: item.url,
      icon: getIconComponent(item.iconName),
      permissions: item.permissions,
      children: item.children?.map(child => this.deserializeMenuItem(child)),
    };
  }

  /**
   * Cache menu in localStorage
   */
  private cacheMenu(menu: NavItem[]): void {
    try {
      const serializedMenu = menu.map(item => this.serializeMenuItem(item));
      const cacheData: CachedMenu = {
        menu: serializedMenu,
        timestamp: Date.now(),
      };
      localStorage.setItem(this.CACHE_KEY, JSON.stringify(cacheData));
    } catch (error) {
      console.error('Failed to cache menu:', error);
    }
  }

  /**
   * Get cached menu if valid
   */
  private getCachedMenu(): NavItem[] | null {
    try {
      const cached = localStorage.getItem(this.CACHE_KEY);
      if (!cached) return null;

      const cacheData: CachedMenu = JSON.parse(cached);
      
      // Check if cache is still valid
      if (Date.now() - cacheData.timestamp > this.CACHE_DURATION) {
        this.clearCache();
        return null;
      }

      // Deserialize and restore icon components
      return cacheData.menu.map(item => this.deserializeMenuItem(item));
    } catch (error) {
      console.error('Failed to get cached menu:', error);
      return null;
    }
  }

  /**
   * Clear menu cache
   */
  clearCache(): void {
    try {
      localStorage.removeItem(this.CACHE_KEY);
    } catch (error) {
      console.error('Failed to clear cache:', error);
    }
  }

  /**
   * Check if user has specific permission for a menu item
   */
  hasPermission(permissions: MenuPermissions, action: keyof MenuPermissions): boolean {
    return permissions[action] === true;
  }

  /**
   * Find menu item by URL
   */
  findItemByUrl(items: NavItem[], url: string): NavItem | null {
    for (const item of items) {
      if (item.url === url) {
        return item;
      }
      if (item.children) {
        const found = this.findItemByUrl(item.children, url);
        if (found) return found;
      }
    }
    return null;
  }

  /**
   * Get all accessible URLs for the user
   */
  getAccessibleUrls(items: NavItem[]): string[] {
    const urls: string[] = [];
    
    const collectUrls = (navItems: NavItem[]) => {
      navItems.forEach(item => {
        if (item.permissions.canView) {
          urls.push(item.url);
        }
        if (item.children) {
          collectUrls(item.children);
        }
      });
    };
    
    collectUrls(items);
    return urls;
  }

  /**
   * Check if URL is accessible to user
   */
  isUrlAccessible(items: NavItem[], url: string): boolean {
    const item = this.findItemByUrl(items, url);
    return item?.permissions.canView ?? false;
  }
}

export const navigationService = new NavigationService();
