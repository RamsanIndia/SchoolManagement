import { useMutation, useQueryClient } from '@tanstack/react-query';
import { permissionApi } from '../api/permissionApi';
import { PERMISSIONS_QUERY_KEY } from './usePermissions';
import { useToast } from '@/hooks/use-toast';
import { ApiError } from '@/lib/api/apiError';

export function useDeletePermission() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation<void, ApiError, string>({
    mutationFn: (permissionId: string) => permissionApi.deletePermission(permissionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PERMISSIONS_QUERY_KEY] });
      
      toast({
        title: 'Success',
        description: 'Permission deleted successfully',
      });
    },
    onError: (error) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to delete permission',
        variant: 'destructive',
      });
    },
  });
}
