// src/lib/api/apiClient.ts
import { ApiError } from './apiError';
import { 
  ApiResponse, 
  PaginatedResponse, // ✅ Added this import
  PaginatedApiResponse, 
  ApiOperationResponse 
} from '@/shared/types/api.types';

// ==================== TYPES ====================

interface RequestConfig extends RequestInit {
  requiresAuth?: boolean;
  timeout?: number;
  params?: Record<string, any>; // Query parameters
}

interface RefreshTokenResponse {
  success: boolean;
  data: {
    accessToken: string;
    refreshToken: string;
  };
}

// ==================== API CLIENT ====================

class ApiClient {
  private baseURL: string;
  private defaultTimeout: number = 30000;
  private refreshTokenPromise: Promise<string> | null = null;

  constructor(baseURL: string) {
    this.baseURL = baseURL;
  }

  // ==================== TOKEN MANAGEMENT ====================

  /**
   * Get access token from localStorage
   */
  private getAccessToken(): string | null {
    try {
      return localStorage.getItem('sms_access_token');
    } catch (error) {
      console.error('Failed to get access token:', error);
      return null;
    }
  }

  /**
   * Get refresh token from localStorage
   */
  private getRefreshToken(): string | null {
    try {
      return localStorage.getItem('sms_refresh_token');
    } catch (error) {
      console.error('Failed to get refresh token:', error);
      return null;
    }
  }

  /**
   * Save tokens to localStorage
   */
  private saveTokens(accessToken: string, refreshToken: string): void {
    try {
      localStorage.setItem('sms_access_token', accessToken);
      localStorage.setItem('sms_refresh_token', refreshToken);
    } catch (error) {
      console.error('Failed to save tokens:', error);
    }
  }

  /**
   * Clear tokens and redirect to login
   */
  private clearAuthAndRedirect(): void {
    try {
      localStorage.removeItem('sms_access_token');
      localStorage.removeItem('sms_refresh_token');
      localStorage.removeItem('sms_user');
      
      // Redirect to login
      window.location.href = '/login';
    } catch (error) {
      console.error('Failed to clear auth:', error);
    }
  }

  /**
   * Refresh access token using refresh token
   */
  private async refreshAccessToken(): Promise<string> {
    // Prevent multiple simultaneous refresh requests
    if (this.refreshTokenPromise) {
      return this.refreshTokenPromise;
    }

    this.refreshTokenPromise = (async () => {
      try {
        const refreshToken = this.getRefreshToken();
        
        if (!refreshToken) {
          throw new Error('No refresh token available');
        }

        const response = await fetch(`${this.baseURL}/auth/refresh`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ refreshToken }),
        });

        if (!response.ok) {
          throw new Error('Failed to refresh token');
        }

        const data: RefreshTokenResponse = await response.json();
        
        if (!data.success || !data.data?.accessToken) {
          throw new Error('Invalid refresh token response');
        }

        // Save new tokens
        this.saveTokens(data.data.accessToken, data.data.refreshToken);

        return data.data.accessToken;
      } catch (error) {
        console.error('Token refresh failed:', error);
        this.clearAuthAndRedirect();
        throw error;
      } finally {
        this.refreshTokenPromise = null;
      }
    })();

    return this.refreshTokenPromise;
  }

  // ==================== HEADERS ====================

  /**
   * Build request headers
   */
  private getHeaders(requiresAuth: boolean = true, isFormData: boolean = false): HeadersInit {
    const headers: HeadersInit = {};

    // Don't set Content-Type for FormData (browser sets it with boundary)
    if (!isFormData) {
      headers['Content-Type'] = 'application/json';
    }

    if (requiresAuth) {
      const token = this.getAccessToken();
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
    }

    return headers;
  }

  // ==================== RESPONSE HANDLING ====================

  /**
   * Handle API response and errors
   */
  private async handleResponse<T>(response: Response): Promise<T> {
    // Handle 204 No Content
    if (response.status === 204) {
      return { 
        status: true, 
        message: 'Operation successful',
        data: null 
      } as T;
    }

    // Try to parse JSON response
    let data: any;
    try {
      data = await response.json();
    } catch (error) {
      if (!response.ok) {
        throw new ApiError(
          response.statusText || 'An error occurred',
          response.status,
          []
        );
      }
      // If response is ok but no JSON, return empty success
      return { status: true } as T;
    }

    // Handle error responses
    if (!response.ok || !data.status) {
      throw new ApiError(
        data.message || data.title || response.statusText || 'An error occurred',
        response.status,
        data.errors || []
      );
    }

    return data;
  }

  // ==================== FETCH WITH TIMEOUT ====================

  /**
   * Fetch with timeout and abort controller
   */
  private async fetchWithTimeout(
    url: string,
    config: RequestConfig
  ): Promise<Response> {
    const timeout = config.timeout || this.defaultTimeout;
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeout);

    try {
      const response = await fetch(url, {
        ...config,
        signal: controller.signal,
      });
      clearTimeout(timeoutId);
      return response;
    } catch (error) {
      clearTimeout(timeoutId);
      if (error instanceof Error && error.name === 'AbortError') {
        throw new ApiError('Request timeout', 408, ['The request took too long to complete']);
      }
      throw error;
    }
  }

  // ==================== REQUEST WITH RETRY ====================

  /**
   * Make request with automatic token refresh on 401
   */
  private async requestWithRetry<T>(
    url: string,
    config: RequestConfig,
    attempt: number = 1
  ): Promise<T> {
    try {
      const response = await this.fetchWithTimeout(url, config);

      // Handle 401 Unauthorized - attempt token refresh
      if (response.status === 401 && attempt === 1) {
        const { requiresAuth = true } = config;
        
        if (requiresAuth) {
          try {
            // Refresh the token
            const newToken = await this.refreshAccessToken();
            
            // Retry request with new token
            const headers = config.headers as Record<string, string>;
            headers['Authorization'] = `Bearer ${newToken}`;
            
            return this.requestWithRetry<T>(url, config, 2);
          } catch (refreshError) {
            // If refresh fails, let the original 401 error propagate
            return this.handleResponse<T>(response);
          }
        }
      }

      return this.handleResponse<T>(response);
    } catch (error) {
      if (error instanceof ApiError) {
        throw error;
      }
      
      // Network or other errors
      throw new ApiError(
        error instanceof Error ? error.message : 'Network error occurred',
        0,
        []
      );
    }
  }

  // ==================== UTILITY METHODS ====================

  /**
   * Build URL with query parameters
   */
  private buildURL(endpoint: string, params?: Record<string, any>): string {
    const url = `${this.baseURL}${endpoint}`;
    
    if (!params || Object.keys(params).length === 0) {
      return url;
    }

    const searchParams = new URLSearchParams();
    
    Object.entries(params).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        if (Array.isArray(value)) {
          value.forEach(v => searchParams.append(key, String(v)));
        } else {
          searchParams.append(key, String(value));
        }
      }
    });

    const queryString = searchParams.toString();
    return queryString ? `${url}?${queryString}` : url;
  }

  // ==================== HTTP METHODS ====================

  /**
   * GET request
   */
  async get<T>(
    endpoint: string,
    config: RequestConfig = {}
  ): Promise<ApiResponse<T>> {
    const { requiresAuth = true, params, ...restConfig } = config;
    const url = this.buildURL(endpoint, params);

    return this.requestWithRetry<ApiResponse<T>>(url, {
      method: 'GET',
      headers: this.getHeaders(requiresAuth),
      ...restConfig,
    });
  }

  /**
   * GET request for paginated lists
   * ✅ FIXED: Returns unwrapped PaginatedResponse<T>
   */
  async getList<T>(
    endpoint: string,
    config: RequestConfig = {}
  ): Promise<PaginatedResponse<T>> {
    const { requiresAuth = true, params, ...restConfig } = config;
    const url = this.buildURL(endpoint, params);

    const response = await this.requestWithRetry<PaginatedApiResponse<T>>(url, {
      method: 'GET',
      headers: this.getHeaders(requiresAuth),
      ...restConfig,
    });

    // ✅ Return unwrapped data (PaginatedResponse<T>)
    return response.data;
  }

  /**
   * POST request
   */
  async post<T>(
    endpoint: string,
    body?: any,
    config: RequestConfig = {}
  ): Promise<ApiResponse<T>> {
    const { requiresAuth = true, params, ...restConfig } = config;
    const url = this.buildURL(endpoint, params);
    const isFormData = body instanceof FormData;

    return this.requestWithRetry<ApiResponse<T>>(url, {
      method: 'POST',
      headers: this.getHeaders(requiresAuth, isFormData),
      body: isFormData ? body : JSON.stringify(body),
      ...restConfig,
    });
  }

  /**
   * PUT request
   */
  async put<T>(
    endpoint: string,
    body?: any,
    config: RequestConfig = {}
  ): Promise<ApiResponse<T>> {
    const { requiresAuth = true, params, ...restConfig } = config;
    const url = this.buildURL(endpoint, params);
    const isFormData = body instanceof FormData;

    return this.requestWithRetry<ApiResponse<T>>(url, {
      method: 'PUT',
      headers: this.getHeaders(requiresAuth, isFormData),
      body: isFormData ? body : JSON.stringify(body),
      ...restConfig,
    });
  }

  /**
   * PATCH request
   */
  async patch<T>(
    endpoint: string,
    body?: any,
    config: RequestConfig = {}
  ): Promise<ApiResponse<T>> {
    const { requiresAuth = true, params, ...restConfig } = config;
    const url = this.buildURL(endpoint, params);
    const isFormData = body instanceof FormData;

    return this.requestWithRetry<ApiResponse<T>>(url, {
      method: 'PATCH',
      headers: this.getHeaders(requiresAuth, isFormData),
      body: isFormData ? body : JSON.stringify(body),
      ...restConfig,
    });
  }

  /**
   * DELETE request
   */
  async delete(
    endpoint: string,
    config: RequestConfig = {}
  ): Promise<ApiOperationResponse> {
    const { requiresAuth = true, params, ...restConfig } = config;
    const url = this.buildURL(endpoint, params);

    return this.requestWithRetry<ApiOperationResponse>(url, {
      method: 'DELETE',
      headers: this.getHeaders(requiresAuth),
      ...restConfig,
    });
  }

  // ==================== FILE OPERATIONS ====================

  /**
   * Upload file(s)
   */
  async upload<T>(
    endpoint: string,
    file: File | File[],
    fieldName: string = 'file',
    additionalData?: Record<string, any>,
    config: RequestConfig = {}
  ): Promise<ApiResponse<T>> {
    const formData = new FormData();

    // Append file(s)
    if (Array.isArray(file)) {
      file.forEach((f) => formData.append(fieldName, f));
    } else {
      formData.append(fieldName, file);
    }

    // Append additional data
    if (additionalData) {
      Object.entries(additionalData).forEach(([key, value]) => {
        if (value !== null && value !== undefined) {
          formData.append(key, typeof value === 'object' ? JSON.stringify(value) : String(value));
        }
      });
    }

    return this.post<T>(endpoint, formData, config);
  }

  /**
   * Download file
   */
  async download(
    endpoint: string,
    filename?: string,
    config: RequestConfig = {}
  ): Promise<Blob> {
    const { requiresAuth = true, params, ...restConfig } = config;
    const url = this.buildURL(endpoint, params);

    const response = await this.fetchWithTimeout(url, {
      method: 'GET',
      headers: this.getHeaders(requiresAuth),
      ...restConfig,
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({
        message: response.statusText,
      }));
      throw new ApiError(
        errorData.message || 'Download failed',
        response.status,
        []
      );
    }

    const blob = await response.blob();

    // Auto-download if filename provided
    if (filename) {
      const link = document.createElement('a');
      link.href = window.URL.createObjectURL(blob);
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(link.href);
    }

    return blob;
  }

  // ==================== UTILITY METHODS ====================

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  /**
   * Get base URL
   */
  getBaseURL(): string {
    return this.baseURL;
  }

  /**
   * Set custom timeout
   */
  setTimeout(timeout: number): void {
    this.defaultTimeout = timeout;
  }
}

// ==================== SINGLETON INSTANCE ====================
console.log('API Base URL:', import.meta.env.VITE_API_URL);

export const apiClient = new ApiClient(
  import.meta.env.VITE_API_URL || 'https://localhost:7045/api'
);

export default apiClient;
