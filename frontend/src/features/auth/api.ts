import { apiClient } from "@/shared/lib/api-client";
import type { LoginRequest, LoginResponse } from "./types";

export async function login(request: LoginRequest): Promise<LoginResponse> {
  const { data } = await apiClient.post<LoginResponse>("/auth/login", request);
  return data;
}

export async function logout(refreshToken: string): Promise<void> {
  await apiClient.post<void>("/auth/logout", { refreshToken });
}
