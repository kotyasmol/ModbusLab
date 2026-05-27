import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  changeUserRole,
  changeUserStatus,
  createUser,
  getRoles,
  getUsers,
} from "./usersApi";
import type { ChangeUserRoleRequest, ChangeUserStatusRequest, CreateUserRequest } from "./types";

export function useUsers() {
  const queryClient = useQueryClient();

  const usersQuery = useQuery({
    queryKey: ["users"],
    queryFn: getUsers,
  });

  const rolesQuery = useQuery({
    queryKey: ["user-roles"],
    queryFn: getRoles,
  });

  const createUserMutation = useMutation({
    mutationFn: createUser,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });

  const changeRoleMutation = useMutation({
    mutationFn: ({ userId, request }: { userId: string; request: ChangeUserRoleRequest }) =>
      changeUserRole(userId, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });

  const changeStatusMutation = useMutation({
    mutationFn: ({ userId, request }: { userId: string; request: ChangeUserStatusRequest }) =>
      changeUserStatus(userId, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });

  return {
    usersQuery,
    rolesQuery,
    createUser: (request: CreateUserRequest) => createUserMutation.mutateAsync(request),
    creatingUser: createUserMutation.isPending,
    createUserError: createUserMutation.error,
    changeUserRole: (userId: string, role: string) =>
      changeRoleMutation.mutateAsync({ userId, request: { role } }),
    changingRole: changeRoleMutation.isPending,
    changeRoleError: changeRoleMutation.error,
    changeUserStatus: (userId: string, isEnabled: boolean) =>
      changeStatusMutation.mutateAsync({ userId, request: { isEnabled } }),
    changingStatus: changeStatusMutation.isPending,
    changeStatusError: changeStatusMutation.error,
  };
}
