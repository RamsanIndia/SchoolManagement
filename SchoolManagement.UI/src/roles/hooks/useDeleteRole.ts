import { useMutation, useQueryClient } from '@tanstack/react-query';
import { roleApi } from '../api/roleApi';
import { ROLES_QUERY_KEY } from './useRoles';
import { useToast } from '@/hooks/use-toast';
import { ApiError } from '@/lib/api/apiError';

export function useDeleteRole() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation<void, ApiError, string>({
    mutationFn: (roleId: string) => roleApi.deleteRole(roleId),
    onSuccess: () => {
      // Invalidate and refetch roles
      queryClient.invalidateQueries({ queryKey: [ROLES_QUERY_KEY] });
      
      toast({
        title: 'Success',
        description: 'Role deleted successfully',
      });
    },
    onError: (error) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to delete role',
        variant: 'destructive',
      });
    },
  });
}
