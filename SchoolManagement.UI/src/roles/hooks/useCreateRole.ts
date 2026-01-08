import { useMutation, useQueryClient } from '@tanstack/react-query';
import { roleApi } from '../api/roleApi';
import { ROLES_QUERY_KEY } from './useRoles';
import { useToast } from '@/hooks/use-toast';
import { RoleDto } from '../types/role.types';
import { ApiError } from '@/lib/api/apiError';

export function useCreateRole() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: (data: RoleDto) => roleApi.createRole(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ROLES_QUERY_KEY] });
      
      toast({
        title: 'Success',
        description: 'Role created successfully',
      });
    },
    onError: (error: ApiError) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to create role',
        variant: 'destructive',
      });
    },
  });
}
