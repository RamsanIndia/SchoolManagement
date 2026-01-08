import { useMutation, useQueryClient } from '@tanstack/react-query';
import { permissionApi } from '../api/permissionApi';
import { PERMISSIONS_QUERY_KEY } from './usePermissions';
import { useToast } from '@/hooks/use-toast';
import { UpdatePermissionDto } from '../types/permission.types';
import { ApiError } from '@/lib/api/apiError';

interface UpdatePermissionParams {
  id: string;
  data: UpdatePermissionDto;
}

export function useUpdatePermission() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: ({ id, data }: UpdatePermissionParams) => 
      permissionApi.updatePermission(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: [PERMISSIONS_QUERY_KEY] });
      queryClient.invalidateQueries({ queryKey: [PERMISSIONS_QUERY_KEY, id] });
      
      toast({
        title: 'Success',
        description: 'Permission updated successfully',
      });
    },
    onError: (error: ApiError) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to update permission',
        variant: 'destructive',
      });
    },
  });
}
