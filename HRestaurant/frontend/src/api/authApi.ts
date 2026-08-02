import { login, logout, register } from "@/features/auth/api/auth-api";
import { getData, send } from "@/api/apiClient";
import type { AuthUser } from "@/shared/types/auth";

export const authApi = {
  login,
  register,
  logout,
  current: (signal?: AbortSignal) => getData<AuthUser>("/auth/me", { signal }),
  forgotPassword: (email: string) => send("post", "/auth/forgot-password", { email }),
  resetPassword: (input: { email: string; token: string; newPassword: string; confirmPassword: string }) => send("post", "/auth/reset-password", input),
  resendVerification: (email: string) => send("post", "/auth/resend-verification", { email }),
  verifyEmail: (userId: string, token: string) => send("post", "/auth/verify-email", { userId, token }),
};
export { login, register, logout };
