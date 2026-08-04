"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { PaginationQuery } from "@/shared/types/paginated";
import { createUser, deleteUser, getUser, getUsers, updateUser } from "./api";
import type { CreateUserRequest, UpdateUserRequest } from "./types";

export function useUsers(query: PaginationQuery & { role?: string }) {
  return useQuery({
    queryKey: ["users", "list", query],
    queryFn: () => getUsers(query),
  });
}

export function useUser(id: string) {
  return useQuery({
    queryKey: ["users", "detail", id],
    queryFn: () => getUser(id),
    enabled: Boolean(id),
  });
}

export function useCreateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateUserRequest) => createUser(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });
}

export function useUpdateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateUserRequest }) =>
      updateUser(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });
}

export function useDeleteUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteUser(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });
}
