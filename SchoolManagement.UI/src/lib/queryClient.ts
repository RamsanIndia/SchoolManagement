import { QueryClient, QueryCache, MutationCache } from '@tanstack/react-query';
import { ApiError } from './api/apiError';
import { toast } from '@/hooks/use-toast';

// Global error handler that works with your AuthContext
const handleError = (error: unknown) => {
  if (error instanceof ApiError) {
    if (error.isAuthError) {
      // Don't show toast for auth errors - your AuthContext handles logout
      // The auth context will automatically redirect to login
      console.log('Authentication error - user will be logged out');
      
      // Trigger logout via custom event that AuthContext can listen to
      window.dispatchEvent(new CustomEvent('auth:logout'));
      return;
    }

    toast({
      title: 'Error',
      description: error.message,
      variant: 'destructive',
    });
  } else if (error instanceof Error) {
    toast({
      title: 'Error',
      description: error.message,
      variant: 'destructive',
    });
  } else {
    toast({
      title: 'Error',
      description: 'An unexpected error occurred',
      variant: 'destructive',
    });
  }
};

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
      refetchOnWindowFocus: false,
      refetchOnReconnect: true,
      retry: (failureCount, error) => {
        // Don't retry on auth errors
        if (error instanceof ApiError && error.isAuthError) {
          return false;
        }
        // Retry up to 2 times for other errors
        return failureCount < 2;
      },
    },
    mutations: {
      retry: false,
    },
  },
  queryCache: new QueryCache({
    onError: handleError,
  }),
  mutationCache: new MutationCache({
    onError: handleError,
  }),
});
