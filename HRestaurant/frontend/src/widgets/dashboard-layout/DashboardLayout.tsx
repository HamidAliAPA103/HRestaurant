import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Outlet, useNavigate } from "react-router-dom";
import { logout } from "@/features/auth/api/auth-api";
import { useAuthStore } from "@/features/auth/store/auth-store";
import { Header } from "@/widgets/dashboard-layout/Header";
import { Sidebar } from "@/widgets/dashboard-layout/Sidebar";

export function DashboardLayout() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const [collapsed, setCollapsed] = useState(false);
  const refreshToken = useAuthStore((state) => state.refreshToken);
  const clearSession = useAuthStore((state) => state.clearSession);
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const logoutMutation = useMutation({
    mutationFn: async () => {
      if (refreshToken) await logout(refreshToken);
    },
    onSettled: () => {
      clearSession();
      queryClient.clear();
      navigate("/login", { replace: true });
    },
  });

  return (
    <div className="min-h-screen bg-[#f8f6f1] lg:flex">
      <Sidebar
        mobileOpen={mobileOpen}
        collapsed={collapsed}
        onMobileClose={() => setMobileOpen(false)}
        onToggleCollapse={() => setCollapsed((value) => !value)}
        onLogout={() => logoutMutation.mutate()}
      />
      <div className="min-w-0 flex-1">
        <Header onMenuOpen={() => setMobileOpen(true)} />
        <main className="mx-auto max-w-[1600px] p-4 sm:p-6 lg:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
