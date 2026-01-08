import { useMutation, useQueryClient } from '@tanstack/react-query';
import { userApi } from '../api/userApi';
import { USERS_QUERY_KEY } from './useUsers';
import { useToast } from '@/hooks/use-toast';
import { UserDto } from '../types/user.types';
import { ApiError } from '@/lib/api/apiError';

export function useCreateUser() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: (data: UserDto) => userApi.createUser(data),
    onSuccess: (response) => {
      queryClient.invalidateQueries({ queryKey: [USERS_QUERY_KEY] });
      
      toast({
        title: 'User Created',
        description: `User "${response.data.firstName} ${response.data.lastName}" has been created successfully`,
      });
    },
    onError: (error: ApiError) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to create user',
        variant: 'destructive',
      });
    },
  });
}
