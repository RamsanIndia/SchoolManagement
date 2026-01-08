import { useMutation, useQueryClient } from '@tanstack/react-query';
import { permissionApi } from '../api/permissionApi';
import { PERMISSIONS_QUERY_KEY } from './usePermissions';
import { useToast } from '@/hooks/use-toast';
import { PermissionDto } from '../types/permission.types';
import { ApiError } from '@/lib/api/apiError';

export function useCreatePermission() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: (data: PermissionDto) => permissionApi.createPermission(data),
    onSuccess: (response) => {
      queryClient.invalidateQueries({ queryKey: [PERMISSIONS_QUERY_KEY] });
      
      toast({
        title: 'Permission Created',
        description: `Permission "${response.data.displayName}" has been created successfully`,
      });
    },
    onError: (error: ApiError) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to create permission',
        variant: 'destructive',
      });
    },
  });
}
