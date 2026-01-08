import { useMutation, useQueryClient } from '@tanstack/react-query';
import { userApi } from '../api/userApi';
import { USERS_QUERY_KEY } from './useUsers';
import { useToast } from '@/hooks/use-toast';
import { ApiError } from '@/lib/api/apiError';

interface AssignRolesParams {
  userId: string;
  roleIds: string[];
}

export function useAssignRoles() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: ({ userId, roleIds }: AssignRolesParams) => 
      userApi.assignRoles(userId, roleIds),
    onSuccess: (_, { userId }) => {
      queryClient.invalidateQueries({ queryKey: [USERS_QUERY_KEY] });
      queryClient.invalidateQueries({ queryKey: [USERS_QUERY_KEY, userId] });
      
      toast({
        title: 'Success',
        description: 'Roles assigned successfully',
      });
    },
    onError: (error: ApiError) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to assign roles',
        variant: 'destructive',
      });
    },
  });
}
