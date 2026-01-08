// src/users/hooks/useUsers.ts
import { useQuery, UseQueryOptions } from '@tanstack/react-query';
import { userApi } from '../api/userApi';
import type { PaginatedUsers, UserQueryParams, UserDetails } from '../types/user.types';
import { ApiError } from '@/lib/api/apiError';

export const USERS_QUERY_KEY = 'users';

interface UseUsersOptions extends Omit<UseQueryOptions<PaginatedUsers, ApiError>, 'queryKey' | 'queryFn'> {
  params?: UserQueryParams;
}

/**
 * Hook for fetching paginated users list
 * Returns: PaginatedResponse<UserDetails>
 */
export function useUsers(options?: UseUsersOptions) {
  const { params, ...queryOptions } = options || {};

  return useQuery<PaginatedUsers, ApiError>({
    queryKey: [USERS_QUERY_KEY, params],
    queryFn: () => userApi.getAll(params), // ✅ Changed from getUsers to getAll
    staleTime: 5 * 60 * 1000, // 5 minutes
    ...queryOptions,
  });
}

/**
 * Hook for getting a single user by ID
 * Returns: UserDetails
 */
export function useUser(id: string, enabled: boolean = true) {
  return useQuery<UserDetails, ApiError>({
    queryKey: [USERS_QUERY_KEY, id],
    queryFn: () => userApi.getById(id), // ✅ Changed from getUserById to getById
    enabled: enabled && !!id,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook for getting current logged-in user profile
 * Returns: UserDetails
 */
export function useCurrentUser() {
  return useQuery<UserDetails, ApiError>({
    queryKey: [USERS_QUERY_KEY, 'current'],
    queryFn: () => userApi.getCurrentUser(),
    staleTime: 10 * 60 * 1000, // 10 minutes
    retry: 1, // Only retry once for auth errors
  });
}
