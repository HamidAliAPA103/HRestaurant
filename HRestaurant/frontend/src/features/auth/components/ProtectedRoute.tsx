import { Navigate, Outlet, useLocation } from "react-router-dom";
import type { AppRole } from "@/shared/types/auth";
import { useAuthStore } from "@/features/auth/store/auth-store";

export function ProtectedRoute() {
  const accessToken = useAuthStore((state) => state.accessToken);
  const refreshToken = useAuthStore((state) => state.refreshToken);
  const location = useLocation();

  if (!accessToken && !refreshToken) {
    return (
      <Navigate
        to="/login"
        replace
        state={{ from: location.pathname }}
      />
    );
  }

  return <Outlet />;
}

export function RoleProtectedRoute({
  roles,
}: {
  roles: AppRole[];
}) {
  const hasRole = useAuthStore((state) => state.hasRole);

  if (!hasRole(roles)) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <Outlet />;
}
