import { useMutation, useQueryClient } from '@tanstack/react-query';
import { permissionApi } from '../api/permissionApi';
import { PERMISSIONS_QUERY_KEY } from './usePermissions';
import { ROLES_QUERY_KEY } from '@/roles/hooks/useRoles';
import { useToast } from '@/hooks/use-toast';
import { ApiError } from '@/lib/api/apiError';

interface SyncPermissionsParams {
  roleId: string;
  permissionIds: string[];
}

export function useSyncRolePermissions() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: ({ roleId, permissionIds }: SyncPermissionsParams) => 
      permissionApi.syncRolePermissions(roleId, permissionIds),
    onSuccess: (_, { roleId }) => {
      queryClient.invalidateQueries({ queryKey: [PERMISSIONS_QUERY_KEY, 'role', roleId] });
      queryClient.invalidateQueries({ queryKey: [ROLES_QUERY_KEY] });
      
      toast({
        title: 'Success',
        description: 'Role permissions updated successfully',
      });
    },
    onError: (error: ApiError) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to update role permissions',
        variant: 'destructive',
      });
    },
  });
}
