import { useMutation, useQueryClient } from '@tanstack/react-query';
import { userApi } from '../api/userApi';
import { USERS_QUERY_KEY } from './useUsers';
import { useToast } from '@/hooks/use-toast';
import { UpdateUserDto } from '../types/user.types';
import { ApiError } from '@/lib/api/apiError';

interface UpdateUserParams {
  id: string;
  data: UpdateUserDto;
}

export function useUpdateUser() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: ({ id, data }: UpdateUserParams) => userApi.updateUser(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: [USERS_QUERY_KEY] });
      queryClient.invalidateQueries({ queryKey: [USERS_QUERY_KEY, id] });
      
      toast({
        title: 'Success',
        description: 'User updated successfully',
      });
    },
    onError: (error: ApiError) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to update user',
        variant: 'destructive',
      });
    },
  });
}
