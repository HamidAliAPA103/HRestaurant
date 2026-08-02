import axios, {
  AxiosError,
  type InternalAxiosRequestConfig,
} from "axios";
import { useAuthStore } from "@/features/auth/store/auth-store";
import type { ApiResponse } from "@/shared/types/api";
import type { AuthResponse } from "@/shared/types/auth";

const baseURL = import.meta.env.VITE_API_BASE_URL || "/api";
const configuredTimeout = Number(import.meta.env.VITE_API_TIMEOUT_MS);
const timeout = Number.isFinite(configuredTimeout) && configuredTimeout > 0
  ? configuredTimeout
  : 15_000;

export const apiClient = axios.create({
  baseURL,
  timeout,
  headers: {
    Accept: "application/json",
  },
});

const refreshClient = axios.create({
  baseURL,
  timeout,
  headers: {
    Accept: "application/json",
  },
});

interface RetryableRequest extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

let refreshPromise: Promise<string> | null = null;

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ApiResponse<unknown>>) => {
    const request = error.config as RetryableRequest | undefined;
    const isAuthRequest = request?.url?.startsWith("/auth/");

    if (
      error.response?.status !== 401 ||
      !request ||
      request._retry ||
      isAuthRequest
    ) {
      return Promise.reject(error);
    }

    request._retry = true;
    const store = useAuthStore.getState();

    if (!store.refreshToken) {
      store.clearSession();
      window.location.assign("/login");
      return Promise.reject(error);
    }

    refreshPromise ??= refreshClient
      .post<ApiResponse<AuthResponse>>("/auth/refresh", {
        refreshToken: store.refreshToken,
      })
      .then(({ data }) => {
        if (!data.success || !data.data) {
          throw new Error(data.message);
        }

        useAuthStore.getState().setSession(data.data);
        return data.data.accessToken;
      })
      .finally(() => {
        refreshPromise = null;
      });

    try {
      const accessToken = await refreshPromise;
      request.headers.Authorization = `Bearer ${accessToken}`;
      return apiClient(request);
    } catch (refreshError) {
      useAuthStore.getState().clearSession();
      window.location.assign("/login");
      return Promise.reject(refreshError);
    }
  },
);
