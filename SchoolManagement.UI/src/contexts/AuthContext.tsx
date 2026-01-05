import React, { createContext, useContext, useState, useEffect, useCallback, useRef } from 'react';
import api from '@/services/api';
import { AxiosError } from 'axios';

export type UserRole = 'Admin' | 'Teacher' | 'Student' | 'HR' | 'Accountant';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  isEmailVerified: boolean;
  isPhoneVerified: boolean;
  roles: UserRole[];
  lastLoginAt: string;
  createdAt: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  refreshAccessToken: () => Promise<void>;
}

interface AuthState {
  user: User | null;
  tokens: AuthTokens | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

interface LoginResponse {
  data: {
    accessToken: string;
    refreshToken: string;
    expiresIn: number;
    tokenType: string;
    user: User;
  };
  status: boolean;
  message: string;
  errors: string[];
}

interface RefreshTokenResponse {
  data: {
    accessToken: string;
    refreshToken: string;
    expiresIn: number;
    tokenType: string;
  };
  status: boolean;
  message: string;
  errors: string[];
}

// Constants
const STORAGE_KEYS = {
  USER: 'sms_user',
  ACCESS_TOKEN: 'sms_access_token',
  REFRESH_TOKEN: 'sms_refresh_token',
  TOKEN_EXPIRY: 'sms_token_expiry',
} as const;

const TOKEN_REFRESH_THRESHOLD = 5 * 60 * 1000; // Refresh 5 minutes before expiry
const MAX_RETRY_ATTEMPTS = 3;

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [authState, setAuthState] = useState<AuthState>({
    user: null,
    tokens: null,
    isAuthenticated: false,
    isLoading: true,
  });

  const refreshTimerRef = useRef<NodeJS.Timeout | null>(null);
  const isRefreshingRef = useRef(false);

  // Secure storage utilities
  const secureStorage = {
    set: (key: string, value: string) => {
      try {
        localStorage.setItem(key, value);
      } catch (error) {
        console.error('Failed to store data:', error);
      }
    },
    get: (key: string): string | null => {
      try {
        return localStorage.getItem(key);
      } catch (error) {
        console.error('Failed to retrieve data:', error);
        return null;
      }
    },
    remove: (key: string) => {
      try {
        localStorage.removeItem(key);
      } catch (error) {
        console.error('Failed to remove data:', error);
      }
    },
    clearAll: () => {
      Object.values(STORAGE_KEYS).forEach(key => {
        secureStorage.remove(key);
      });
    },
  };

  // Token validation and expiry check
  const isTokenValid = useCallback((expiresAt: number): boolean => {
    return Date.now() < expiresAt - TOKEN_REFRESH_THRESHOLD;
  }, []);

  // Clear refresh timer
  const clearRefreshTimer = useCallback(() => {
    if (refreshTimerRef.current) {
      clearTimeout(refreshTimerRef.current);
      refreshTimerRef.current = null;
    }
  }, []);

  // Schedule token refresh
  const scheduleTokenRefresh = useCallback((expiresAt: number) => {
    clearRefreshTimer();

    const refreshTime = expiresAt - Date.now() - TOKEN_REFRESH_THRESHOLD;
    
    if (refreshTime > 0) {
      refreshTimerRef.current = setTimeout(() => {
        refreshAccessToken();
      }, refreshTime);
    }
  }, []);

  // Refresh access token using refresh token
  const refreshAccessToken = useCallback(async () => {
    if (isRefreshingRef.current) return;
    
    isRefreshingRef.current = true;

    try {
      const refreshToken = secureStorage.get(STORAGE_KEYS.REFRESH_TOKEN);
      
      if (!refreshToken) {
        throw new Error('No refresh token available');
      }

      const response = await api.post<RefreshTokenResponse>('/Auth/refresh-token', {
        refreshToken,
      });

      if (!response.data.status) {
        throw new Error(response.data.message || 'Token refresh failed');
      }

      const { accessToken, refreshToken: newRefreshToken, expiresIn } = response.data.data;
      const expiresAt = Date.now() + expiresIn * 1000;

      // Update stored tokens
      secureStorage.set(STORAGE_KEYS.ACCESS_TOKEN, accessToken);
      secureStorage.set(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken);
      secureStorage.set(STORAGE_KEYS.TOKEN_EXPIRY, expiresAt.toString());

      // Update state
      setAuthState(prev => ({
        ...prev,
        tokens: { 
          accessToken, 
          refreshToken: newRefreshToken, 
          expiresIn,
          tokenType: 'Bearer' 
        },
      }));

      // Schedule next refresh
      scheduleTokenRefresh(expiresAt);

    } catch (error) {
      console.error('Token refresh failed:', error);
      await logout();
    } finally {
      isRefreshingRef.current = false;
    }
  }, [scheduleTokenRefresh]);

  // Login with retry mechanism
  const login = async (email: string, password: string) => {
    setAuthState(prev => ({ ...prev, isLoading: true }));
    
    let attempt = 0;
    let delay = 1000;

    while (attempt < MAX_RETRY_ATTEMPTS) {
      try {
        if (!email || !password) {
          throw new Error('Email and password are required');
        }

        const response = await api.post<LoginResponse>('/Auth/login', {
          email: email.trim().toLowerCase(),
          password,
        });

        // Check if login was successful
        if (!response.data.status) {
          throw new Error(response.data.message || 'Login failed');
        }

        const { user, accessToken, refreshToken, expiresIn, tokenType } = response.data.data;
        
        // Calculate token expiry (if expiresIn is 0, default to 1 hour)
        const expiresInSeconds = expiresIn > 0 ? expiresIn : 3600;
        const expiresAt = Date.now() + expiresInSeconds * 1000;

        // Store authentication data securely
        secureStorage.set(STORAGE_KEYS.USER, JSON.stringify(user));
        secureStorage.set(STORAGE_KEYS.ACCESS_TOKEN, accessToken);
        secureStorage.set(STORAGE_KEYS.REFRESH_TOKEN, refreshToken);
        secureStorage.set(STORAGE_KEYS.TOKEN_EXPIRY, expiresAt.toString());

        // Update auth state
        setAuthState({
          user,
          tokens: { accessToken, refreshToken, expiresIn: expiresInSeconds, tokenType },
          isAuthenticated: true,
          isLoading: false,
        });

        // Schedule automatic token refresh
        scheduleTokenRefresh(expiresAt);

        return;

      } catch (error) {
        attempt++;
        
        const isNetworkError = 
          error instanceof AxiosError && 
          (!error.response || error.response.status >= 500);

        if (attempt >= MAX_RETRY_ATTEMPTS || !isNetworkError) {
          setAuthState(prev => ({ ...prev, isLoading: false }));
          
          const errorMessage = error instanceof AxiosError
            ? error.response?.data?.message || error.response?.data?.errors?.[0] || 'Authentication failed'
            : error instanceof Error
            ? error.message
            : 'An unexpected error occurred';
          
          throw new Error(errorMessage);
        }

        console.log(`Login attempt ${attempt} failed, retrying in ${delay}ms...`);
        await new Promise(resolve => setTimeout(resolve, delay));
        delay *= 2;
      }
    }
  };

  // Logout with server notification
  const logout = async () => {
    try {
      const refreshToken = secureStorage.get(STORAGE_KEYS.REFRESH_TOKEN);
      
      if (refreshToken) {
        await api.post('/Auth/logout', { refreshToken }).catch(() => {
          // Ignore logout API errors
        });
      }
    } catch (error) {
      console.error('Logout notification failed:', error);
    } finally {
      clearRefreshTimer();
      secureStorage.clearAll();

      setAuthState({
        user: null,
        tokens: null,
        isAuthenticated: false,
        isLoading: false,
      });
    }
  };

  // Initialize authentication state on mount
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const storedUser = secureStorage.get(STORAGE_KEYS.USER);
        const accessToken = secureStorage.get(STORAGE_KEYS.ACCESS_TOKEN);
        const refreshToken = secureStorage.get(STORAGE_KEYS.REFRESH_TOKEN);
        const expiryStr = secureStorage.get(STORAGE_KEYS.TOKEN_EXPIRY);

        if (!storedUser || !accessToken || !refreshToken || !expiryStr) {
          setAuthState(prev => ({ ...prev, isLoading: false }));
          return;
        }

        const user = JSON.parse(storedUser);
        const expiresAt = parseInt(expiryStr, 10);
        const expiresIn = Math.floor((expiresAt - Date.now()) / 1000);

        if (isTokenValid(expiresAt)) {
          setAuthState({
            user,
            tokens: { accessToken, refreshToken, expiresIn, tokenType: 'Bearer' },
            isAuthenticated: true,
            isLoading: false,
          });
          scheduleTokenRefresh(expiresAt);
        } else {
          await refreshAccessToken();
        }
      } catch (error) {
        console.error('Auth initialization failed:', error);
        secureStorage.clearAll();
        setAuthState(prev => ({ ...prev, isLoading: false }));
      }
    };

    initializeAuth();

    return () => {
      clearRefreshTimer();
    };
  }, []);

  // Handle visibility change to refresh token when tab becomes active
  useEffect(() => {
    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible' && authState.tokens) {
        const expiryStr = secureStorage.get(STORAGE_KEYS.TOKEN_EXPIRY);
        if (expiryStr) {
          const expiresAt = parseInt(expiryStr, 10);
          if (!isTokenValid(expiresAt)) {
            refreshAccessToken();
          }
        }
      }
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => {
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, [authState.tokens, isTokenValid, refreshAccessToken]);

  const value: AuthContextType = {
    user: authState.user,
    isAuthenticated: authState.isAuthenticated,
    isLoading: authState.isLoading,
    login,
    logout,
    refreshAccessToken,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

// Helper to get user's full name
export function useUserFullName(): string {
  const { user } = useAuth();
  if (!user) return '';
  return `${user.firstName} ${user.lastName}`.trim();
}

// Helper to check if user has specific role
export function useHasRole(role: UserRole): boolean {
  const { user } = useAuth();
  return user?.roles.includes(role) ?? false;
}

// Helper to get primary role
export function usePrimaryRole(): UserRole | null {
  const { user } = useAuth();
  return user?.roles[0] ?? null;
}
