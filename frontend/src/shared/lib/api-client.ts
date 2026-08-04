import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import { ApiError, type ApiProblemDetails } from "@/shared/types/api-error";
import {
  clearSession,
  getAccessToken,
  getRefreshToken,
  saveSession,
} from "@/shared/lib/auth-storage";
import type { Role } from "@/shared/types/role";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000/api";

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
});

interface RetriableRequestConfig extends InternalAxiosRequestConfig {
  _retried?: boolean;
}

interface RefreshResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  role: Role;
  name: string;
}

apiClient.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) {
    config.headers.set("Authorization", `Bearer ${token}`);
  }
  return config;
});

let refreshPromise: Promise<string | null> | null = null;

async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return null;

  try {
    const response = await axios.post<RefreshResponse>(`${API_BASE_URL}/auth/refresh`, {
      refreshToken,
    });

    saveSession(response.data);
    return response.data.accessToken;
  } catch {
    clearSession();
    return null;
  }
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ApiProblemDetails>) => {
    const originalRequest = error.config as RetriableRequestConfig | undefined;
    const isAuthEndpoint = originalRequest?.url?.includes("/auth/login") || originalRequest?.url?.includes("/auth/refresh");

    if (error.response?.status === 401 && originalRequest && !originalRequest._retried && !isAuthEndpoint) {
      originalRequest._retried = true;

      refreshPromise ??= refreshAccessToken().finally(() => {
        refreshPromise = null;
      });

      const newAccessToken = await refreshPromise;

      if (newAccessToken) {
        originalRequest.headers.set("Authorization", `Bearer ${newAccessToken}`);
        return apiClient.request(originalRequest);
      }

      clearSession();
      if (typeof window !== "undefined") {
        // A full navigation (not router.push) is intentional here: this runs inside an axios
        // interceptor, outside any component/render context, so React Router hooks aren't
        // available — and a hard reload is actually desirable on session expiry anyway, since
        // it guarantees the entire in-memory React Query cache is dropped along with the
        // invalid session rather than lingering as stale "authenticated" data.
        // eslint-disable-next-line @next/next/no-location-assign-relative-destination
        window.location.href = "/login";
      }
    }

    if (error.response?.data) {
      throw new ApiError(error.response.data);
    }

    throw new ApiError({
      title: "Network error",
      status: error.response?.status ?? 0,
      detail: error.message || "Could not reach the server. Please check your connection.",
    });
  },
);
