"use client";

import { useMutation } from "@tanstack/react-query";
import { clearSession, getRefreshToken, saveSession } from "@/shared/lib/auth-storage";
import * as api from "./api";
import type { LoginRequest, LoginResponse } from "./types";

export function useLogin() {
  return useMutation<LoginResponse, unknown, LoginRequest>({
    mutationFn: api.login,
    onSuccess: (data) => {
      saveSession({
        accessToken: data.accessToken,
        accessTokenExpiresAtUtc: data.accessTokenExpiresAtUtc,
        refreshToken: data.refreshToken,
        refreshTokenExpiresAtUtc: data.refreshTokenExpiresAtUtc,
        role: data.role,
        name: data.name,
      });
    },
  });
}

export function useLogout() {
  return useMutation<void, unknown, void>({
    mutationFn: () => api.logout(getRefreshToken() ?? ""),
    onSettled: () => {
      clearSession();
      // A full navigation (not router.push) is intentional here, for the same reason as
      // api-client.ts's own 401 handler: it guarantees the React Query cache is dropped along
      // with the session, rather than leaving stale "authenticated" data in memory.
      window.location.href = "/login";
    },
  });
}
