import { LogOut, PanelLeftClose, Utensils, X } from "lucide-react";
import { NavLink } from "react-router-dom";
import { cn, initials } from "@/shared/lib/utils";
import { useAuthStore } from "@/features/auth/store/auth-store";
import { navigationGroups } from "@/widgets/dashboard-layout/navigation";

export function Sidebar({
  mobileOpen,
  collapsed,
  onMobileClose,
  onToggleCollapse,
  onLogout,
}: {
  mobileOpen: boolean;
  collapsed: boolean;
  onMobileClose: () => void;
  onToggleCollapse: () => void;
  onLogout: () => void;
}) {
  const user = useAuthStore((state) => state.user);
  const hasRole = useAuthStore((state) => state.hasRole);

  return (
    <>
      {mobileOpen && (
        <button
          type="button"
          aria-label="Menyunu bağla"
          className="fixed inset-0 z-40 bg-[#17130f]/45 backdrop-blur-sm lg:hidden"
          onClick={onMobileClose}
        />
      )}
      <aside
        className={cn(
          "fixed inset-y-0 left-0 z-50 flex flex-col bg-[#211c19] text-white transition-all duration-300",
          "lg:sticky lg:top-0 lg:z-20 lg:h-screen lg:translate-x-0",
          mobileOpen ? "translate-x-0" : "-translate-x-full",
          collapsed ? "w-[88px]" : "w-[270px]",
        )}
      >
        <div
          className={cn(
            "flex h-20 items-center border-b border-white/7",
            collapsed ? "justify-center px-3" : "justify-between px-5",
          )}
        >
          <div className="flex items-center gap-3 overflow-hidden">
            <div className="grid h-10 w-10 shrink-0 place-items-center rounded-2xl bg-[#e85d3f]">
              <Utensils className="h-5 w-5" />
            </div>
            {!collapsed && (
              <div className="whitespace-nowrap">
                <div className="font-bold tracking-tight">HRestaurant</div>
                <div className="text-[10px] uppercase tracking-[0.18em] text-white/35">
                  Operations
                </div>
              </div>
            )}
          </div>
          {!collapsed && (
            <button
              type="button"
              onClick={onMobileClose}
              className="grid h-9 w-9 place-items-center rounded-xl text-white/50 hover:bg-white/7 hover:text-white lg:hidden"
              aria-label="Menyunu bağla"
            >
              <X className="h-5 w-5" />
            </button>
          )}
        </div>

        <nav className="sidebar-scroll flex-1 overflow-y-auto px-3 py-5">
          {navigationGroups.map((group) => {
            const items = group.items.filter(
              (item) => !item.roles || hasRole(item.roles),
            );

            if (items.length === 0) return null;

            return (
              <div key={group.label} className="mb-6">
                {!collapsed && (
                  <div className="mb-2 px-3 text-[10px] font-bold uppercase tracking-[0.19em] text-white/28">
                    {group.label}
                  </div>
                )}
                <div className="space-y-1">
                  {items.map((item) => {
                    const Icon = item.icon;
                    return (
                      <NavLink
                        key={item.path}
                        to={item.path}
                        onClick={onMobileClose}
                        title={collapsed ? item.label : undefined}
                        className={({ isActive }) =>
                          cn(
                            "group relative flex h-11 items-center rounded-xl text-sm font-medium transition",
                            collapsed
                              ? "justify-center px-2"
                              : "gap-3 px-3",
                            isActive
                              ? "bg-[#e85d3f] text-white shadow-[0_8px_20px_rgba(232,93,63,.18)]"
                              : "text-white/52 hover:bg-white/6 hover:text-white",
                          )
                        }
                      >
                        <Icon className="h-[18px] w-[18px] shrink-0" />
                        {!collapsed && (
                          <>
                            <span className="truncate">{item.label}</span>
                            {item.badge && (
                              <span className="ml-auto rounded-full bg-[#f3ae53]/15 px-2 py-0.5 text-[10px] font-bold text-[#f3ae53]">
                                {item.badge}
                              </span>
                            )}
                          </>
                        )}
                      </NavLink>
                    );
                  })}
                </div>
              </div>
            );
          })}
        </nav>

        <div className="border-t border-white/7 p-3">
          <button
            type="button"
            onClick={onLogout}
            className={cn(
              "mb-2 flex h-10 w-full items-center rounded-xl text-sm text-white/45 transition hover:bg-white/6 hover:text-white",
              collapsed ? "justify-center" : "gap-3 px-3",
            )}
            title={collapsed ? "Çıxış" : undefined}
          >
            <LogOut className="h-4 w-4" />
            {!collapsed && "Çıxış"}
          </button>
          <div
            className={cn(
              "flex items-center rounded-xl bg-white/5",
              collapsed ? "justify-center p-2" : "gap-3 p-2.5",
            )}
          >
            <div className="grid h-9 w-9 shrink-0 place-items-center rounded-xl bg-[#f0aa51] text-xs font-bold text-[#2a2119]">
              {initials(user?.email ?? "HR")}
            </div>
            {!collapsed && (
              <div className="min-w-0">
                <div className="truncate text-xs font-semibold">
                  {user?.email ?? "HRestaurant"}
                </div>
                <div className="mt-0.5 truncate text-[10px] text-white/34">
                  {user?.roles.join(" · ") || "İstifadəçi"}
                </div>
              </div>
            )}
          </div>
          <button
            type="button"
            onClick={onToggleCollapse}
            className="mt-2 hidden h-9 w-full items-center justify-center rounded-xl text-white/30 transition hover:bg-white/5 hover:text-white lg:flex"
            aria-label={collapsed ? "Menyunu genişləndir" : "Menyunu yığ"}
          >
            <PanelLeftClose
              className={cn(
                "h-4 w-4 transition-transform",
                collapsed && "rotate-180",
              )}
            />
          </button>
        </div>
      </aside>
    </>
  );
}
