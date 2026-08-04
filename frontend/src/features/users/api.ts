import { apiClient } from "@/shared/lib/api-client";
import type { Paginated, PaginationQuery } from "@/shared/types/paginated";
import type { CreateUserRequest, UpdateUserRequest, UserDto } from "./types";

export async function getUsers(
  query: PaginationQuery & { role?: string },
): Promise<Paginated<UserDto>> {
  const { data } = await apiClient.get<Paginated<UserDto>>("/users", { params: query });
  return data;
}

export async function getUser(id: string): Promise<UserDto> {
  const { data } = await apiClient.get<UserDto>(`/users/${id}`);
  return data;
}

export async function createUser(request: CreateUserRequest): Promise<UserDto> {
  const { data } = await apiClient.post<UserDto>("/users", request);
  return data;
}

export async function updateUser(id: string, request: UpdateUserRequest): Promise<UserDto> {
  const { data } = await apiClient.put<UserDto>(`/users/${id}`, request);
  return data;
}

export async function deleteUser(id: string): Promise<void> {
  await apiClient.delete(`/users/${id}`);
}
