// src/services/api.ts
import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'https://localhost:7045/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Changed to true for refresh token cookies (if needed)
});

// Storage keys (must match AuthContext)
const STORAGE_KEYS = {
  ACCESS_TOKEN: 'sms_access_token',
  REFRESH_TOKEN: 'sms_refresh_token',
  TOKEN_EXPIRY: 'sms_token_expiry',
  USER: 'sms_user',
} as const;

// Queue for failed requests during token refresh
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: any) => void;
}> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((promise) => {
    if (error) {
      promise.reject(error);
    } else {
      promise.resolve(token!);
    }
  });
  failedQueue = [];
};

// Request interceptor - add auth token
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
    
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Add request ID for tracking
    if (config.headers) {
      config.headers['X-Request-ID'] = crypto.randomUUID();
    }

    // Debug log
    if (import.meta.env.DEV) {
      console.log('🌐 API Request:', {
        method: config.method?.toUpperCase(),
        url: config.url,
        hasAuth: !!token,
      });
    }

    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor - handle errors and token refresh
api.interceptors.response.use(
  (response) => {
    // Debug log
    if (import.meta.env.DEV) {
      console.log('✅ API Response:', {
        url: response.config.url,
        status: response.status,
      });
    }
    return response;
  },
  async (error: AxiosError<{ status: boolean; message: string; errors: string[] }>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { 
      _retry?: boolean;
    };

    // Debug log
    if (import.meta.env.DEV) {
      console.error('❌ API Error:', {
        url: originalRequest?.url,
        status: error.response?.status,
        message: error.message,
      });
    }

    // Handle 401 Unauthorized - token expired
    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      // Don't retry if this is the refresh token endpoint itself
      if (originalRequest.url?.includes('/Auth/refresh-token')) {
        console.error('❌ Refresh token expired or invalid');
        
        // Clear all auth data
        Object.values(STORAGE_KEYS).forEach(key => {
          localStorage.removeItem(key);
        });
        
        // Redirect to login
        window.location.href = '/login';
        return Promise.reject(error);
      }

      // If already refreshing, queue this request
      if (isRefreshing) {
        console.log('⏳ Queuing request while token refresh in progress...');
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`;
            }
            return api(originalRequest);
          })
          .catch((err) => {
            return Promise.reject(err);
          });
      }

      // Mark as retry to prevent infinite loops
      originalRequest._retry = true;
      isRefreshing = true;

      const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);

      if (!refreshToken) {
        console.error('❌ No refresh token available');
        isRefreshing = false;
        
        // Clear auth and redirect
        Object.values(STORAGE_KEYS).forEach(key => {
          localStorage.removeItem(key);
        });
        window.location.href = '/login';
        
        return Promise.reject(new Error('No refresh token'));
      }

      try {
        console.log('🔄 Attempting to refresh access token...');

        // Call refresh token endpoint (using axios directly to avoid interceptor)
        const response = await axios.post(
          `${api.defaults.baseURL}/Auth/refresh-token`,
          { refreshToken },
          {
            headers: {
              'Content-Type': 'application/json',
            },
          }
        );

        if (response.data.status) {
          const { 
            accessToken, 
            refreshToken: newRefreshToken, 
            expiresIn 
          } = response.data.data;

          const expiresAt = Date.now() + expiresIn * 1000;

          // Store new tokens
          localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken);
          localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken);
          localStorage.setItem(STORAGE_KEYS.TOKEN_EXPIRY, expiresAt.toString());

          console.log('✅ Token refreshed successfully');

          // Update authorization header for original request
          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          }

          // Process all queued requests with new token
          processQueue(null, accessToken);

          // Dispatch event to notify AuthContext
          window.dispatchEvent(new CustomEvent('token-refreshed', { 
            detail: { 
              accessToken, 
              refreshToken: newRefreshToken, 
              expiresIn 
            } 
          }));

          // Retry the original request
          return api(originalRequest);
        } else {
          throw new Error(response.data.message || 'Token refresh failed');
        }
      } catch (refreshError) {
        console.error('❌ Token refresh failed:', refreshError);
        
        // Reject all queued requests
        processQueue(refreshError, null);
        
        // Clear all auth data
        Object.values(STORAGE_KEYS).forEach(key => {
          localStorage.removeItem(key);
        });
        
        // Redirect to login
        window.location.href = '/login';
        
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    // Handle 403 Forbidden - insufficient permissions
    if (error.response?.status === 403) {
      console.error('🚫 Access denied - insufficient permissions');
      
      // Optionally show a toast notification
      // toast.error('You do not have permission to perform this action');
    }

    // Handle 404 Not Found
    if (error.response?.status === 404) {
      console.error('🔍 Resource not found:', originalRequest?.url);
    }

    // Handle 500 Server Error
    if (error.response?.status && error.response.status >= 500) {
      console.error('💥 Server error - please try again later');
      
      // Optionally show a toast notification
      // toast.error('Server error - please try again later');
    }

    // Handle network errors
    if (!error.response) {
      console.error('🌐 Network error - please check your connection');
      
      // Optionally show a toast notification
      // toast.error('Network error - please check your connection');
    }

    return Promise.reject(error);
  }
);

export default api;
