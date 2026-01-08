import { useMutation, useQueryClient } from '@tanstack/react-query';
import { roleApi } from '../api/roleApi';
import { ROLES_QUERY_KEY } from './useRoles';
import { useToast } from '@/hooks/use-toast';
import { UpdateRoleDto } from '../types/role.types';
import { ApiError } from '@/lib/api/apiError';

interface UpdateRoleParams {
  id: string;
  data: UpdateRoleDto;
}

export function useUpdateRole() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: ({ id, data }: UpdateRoleParams) => roleApi.updateRole(id, data),
    onSuccess: (_, { id }) => {
      // Invalidate both the list and the specific role
      queryClient.invalidateQueries({ queryKey: [ROLES_QUERY_KEY] });
      queryClient.invalidateQueries({ queryKey: [ROLES_QUERY_KEY, id] });
      
      toast({
        title: 'Success',
        description: 'Role updated successfully',
      });
    },
    onError: (error: ApiError) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to update role',
        variant: 'destructive',
      });
    },
  });
}
