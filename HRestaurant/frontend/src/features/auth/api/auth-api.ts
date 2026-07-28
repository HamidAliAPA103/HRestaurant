import { apiClient } from "@/shared/api/client";
import type { ApiResponse } from "@/shared/types/api";
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
} from "@/shared/types/auth";

export async function login(input: LoginRequest) {
  const { data } = await apiClient.post<ApiResponse<AuthResponse>>(
    "/auth/login",
    input,
  );
  return data;
}

export async function register(input: RegisterRequest) {
  const { data } = await apiClient.post<ApiResponse<AuthResponse>>(
    "/auth/register",
    input,
  );
  return data;
}

export async function logout(refreshToken: string) {
  const { data } = await apiClient.post<ApiResponse<null>>(
    "/auth/logout",
    { refreshToken },
  );
  return data;
}
