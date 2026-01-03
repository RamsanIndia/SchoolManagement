import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { NavItem } from '@/types/navigation.types';
import { navigationService } from '@/services/navigation.service';
import { useAuth } from './AuthContext';

interface NavigationContextType {
  menuItems: NavItem[];
  isLoading: boolean;
  error: string | null;
  refreshMenu: () => Promise<void>;
}

const NavigationContext = createContext<NavigationContextType | undefined>(undefined);

export function NavigationProvider({ children }: { children: React.ReactNode }) {
  const { user, isAuthenticated } = useAuth();
  const [menuItems, setMenuItems] = useState<NavItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchMenu = useCallback(async () => {
    if (!isAuthenticated || !user) {
      setMenuItems([]);
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const menu = await navigationService.getUserMenu();
      setMenuItems(menu);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load menu';
      setError(errorMessage);
      console.error('Menu fetch error:', err);
    } finally {
      setIsLoading(false);
    }
  }, [isAuthenticated, user]);

  const refreshMenu = useCallback(async () => {
    navigationService.clearCache();
    await fetchMenu();
  }, [fetchMenu]);

  // Fetch menu when user logs in
  useEffect(() => {
    fetchMenu();
  }, [fetchMenu]);

  // Clear menu when user logs out
  useEffect(() => {
    if (!isAuthenticated) {
      setMenuItems([]);
      navigationService.clearCache();
    }
  }, [isAuthenticated]);

  const value: NavigationContextType = {
    menuItems,
    isLoading,
    error,
    refreshMenu,
  };

  return (
    <NavigationContext.Provider value={value}>
      {children}
    </NavigationContext.Provider>
  );
}

export function useNavigation() {
  const context = useContext(NavigationContext);
  if (context === undefined) {
    throw new Error('useNavigation must be used within a NavigationProvider');
  }
  return context;
}
