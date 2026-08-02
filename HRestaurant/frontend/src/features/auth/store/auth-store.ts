import { create } from "zustand";
import { persist } from "zustand/middleware";
import type {
  AppRole,
  AuthResponse,
  AuthUser,
  JwtPayload,
} from "@/shared/types/auth";

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  accessTokenExpiresAtUtc: string | null;
  refreshTokenExpiresAtUtc: string | null;
  user: AuthUser | null;
  setSession: (response: AuthResponse) => void;
  clearSession: () => void;
  hasRole: (roles: AppRole[]) => boolean;
  hasPermission: (permission: string) => boolean;
}

function parseJwt(token: string): JwtPayload | null {
  try {
    const payload = token.split(".")[1];
    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    const decoded = decodeURIComponent(
      atob(normalized)
        .split("")
        .map(
          (character) =>
            `%${character.charCodeAt(0).toString(16).padStart(2, "0")}`,
        )
        .join(""),
    );

    return JSON.parse(decoded) as JwtPayload;
  } catch {
    return null;
  }
}

function toUser(accessToken: string): AuthUser | null {
  const payload = parseJwt(accessToken);
  if (!payload) return null;

  const roles = Array.isArray(payload.role)
    ? payload.role
    : payload.role
      ? [payload.role]
      : [];
  const permissions = Array.isArray(payload.permission)
    ? payload.permission
    : payload.permission
      ? [payload.permission]
      : [];

  return {
    id: payload.user_id,
    fullName: payload.full_name,
    email: payload.email,
    restaurantId: payload.restaurant_id,
    branchId: payload.branch_id,
    roles,
    permissions,
  };
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      refreshToken: null,
      accessTokenExpiresAtUtc: null,
      refreshTokenExpiresAtUtc: null,
      user: null,
      setSession: (response) =>
        set({
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          accessTokenExpiresAtUtc: response.accessTokenExpiresAtUtc,
          refreshTokenExpiresAtUtc: response.refreshTokenExpiresAtUtc,
          user: toUser(response.accessToken),
        }),
      clearSession: () =>
        set({
          accessToken: null,
          refreshToken: null,
          accessTokenExpiresAtUtc: null,
          refreshTokenExpiresAtUtc: null,
          user: null,
        }),
      hasRole: (roles) => {
        const currentRoles = get().user?.roles ?? [];
        return roles.some((role) => currentRoles.includes(role));
      },
      hasPermission: (permission) => {
        const permissions = get().user?.permissions ?? [];
        const isSuperAdmin = get().user?.roles.includes("SuperAdmin") ?? false;
        return isSuperAdmin || permissions.includes("*") || permissions.includes(permission);
      },
    }),
    {
      name: "hrestaurant-auth",
      partialize: (state) => ({
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        accessTokenExpiresAtUtc: state.accessTokenExpiresAtUtc,
        refreshTokenExpiresAtUtc: state.refreshTokenExpiresAtUtc,
        user: state.user,
      }),
    },
  ),
);
