import axios, { AxiosError, AxiosRequestConfig, InternalAxiosRequestConfig } from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'https://localhost:7045/api',
  timeout: 30000, // 30 second timeout
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: false, // Set to true if you need cookies
});

// Queue for failed requests during token refresh
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: Error | null = null) => {
  failedQueue.forEach(promise => {
    if (error) {
      promise.reject(error);
    } else {
      promise.resolve();
    }
  });
  failedQueue = [];
};

// Request interceptor - add auth token
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('sms_access_token');
    
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Add request ID for tracking
    config.headers['X-Request-ID'] = crypto.randomUUID();

    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor - handle errors and token refresh
api.interceptors.response.use(
  response => response,
  async (error: AxiosError<{ status: boolean; message: string; errors: string[] }>) => {
    const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };

    // Handle 401 Unauthorized - token expired
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then(() => api(originalRequest))
          .catch(err => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const refreshToken = localStorage.getItem('sms_refresh_token');
        
        if (!refreshToken) {
          throw new Error('No refresh token');
        }

        const response = await axios.post(
          `${api.defaults.baseURL}/Auth/refresh-token`,
          { refreshToken }
        );

        if (response.data.status) {
          const { accessToken, refreshToken: newRefreshToken } = response.data.data;

          localStorage.setItem('sms_access_token', accessToken);
          localStorage.setItem('sms_refresh_token', newRefreshToken);

          processQueue();
          isRefreshing = false;

          return api(originalRequest);
        } else {
          throw new Error(response.data.message);
        }
      } catch (refreshError) {
        processQueue(refreshError as Error);
        isRefreshing = false;

        // Clear auth and redirect to login
        localStorage.clear();
        window.location.href = '/login';

        return Promise.reject(refreshError);
      }
    }

    // Handle 403 Forbidden - insufficient permissions
    if (error.response?.status === 403) {
      console.error('Access denied - insufficient permissions');
    }

    // Handle network errors
    if (!error.response) {
      console.error('Network error - please check your connection');
    }

    return Promise.reject(error);
  }
);

export default api;
