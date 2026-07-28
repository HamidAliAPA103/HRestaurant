import {
  Bell,
  ChevronDown,
  Menu,
  Search,
  Wifi,
} from "lucide-react";
import { useLocation } from "react-router-dom";
import { useAuthStore } from "@/features/auth/store/auth-store";
import { initials } from "@/shared/lib/utils";
import { pageTitles } from "@/widgets/dashboard-layout/navigation";

export function Header({ onMenuOpen }: { onMenuOpen: () => void }) {
  const location = useLocation();
  const user = useAuthStore((state) => state.user);
  const title = pageTitles[location.pathname] ?? "HRestaurant";

  return (
    <header className="sticky top-0 z-30 flex h-20 items-center justify-between border-b border-[#e8e2da] bg-[#f8f6f1]/92 px-4 backdrop-blur-xl sm:px-6 lg:px-8">
      <div className="flex items-center gap-3">
        <button
          type="button"
          onClick={onMenuOpen}
          className="grid h-10 w-10 place-items-center rounded-xl border border-[#ded8d0] bg-white text-[#5f5751] lg:hidden"
          aria-label="Menyunu aç"
        >
          <Menu className="h-5 w-5" />
        </button>
        <div>
          <div className="text-xs font-medium text-[#958a81]">
            Əməliyyat mərkəzi
          </div>
          <div className="font-bold tracking-tight text-[#2a241f]">{title}</div>
        </div>
      </div>

      <div className="hidden w-full max-w-md px-10 md:block">
        <label className="relative block">
          <Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-[#9b9188]" />
          <input
            type="search"
            placeholder="Sifariş, masa və ya müştəri axtar..."
            className="h-10 w-full rounded-xl border border-[#e2dcd4] bg-white/80 pl-10 pr-4 text-sm outline-none placeholder:text-[#a59b92] focus:border-[#e85d3f]"
          />
        </label>
      </div>

      <div className="flex items-center gap-2">
        <div className="mr-1 hidden items-center gap-1.5 rounded-full bg-[#e7f4e9] px-2.5 py-1 text-[11px] font-semibold text-[#347047] sm:flex">
          <Wifi className="h-3 w-3" />
          Sistem aktivdir
        </div>
        <button
          type="button"
          className="relative grid h-10 w-10 place-items-center rounded-xl border border-[#e1dbd3] bg-white text-[#6d645d] hover:bg-[#f1ede7]"
          aria-label="Bildirişlər"
        >
          <Bell className="h-[18px] w-[18px]" />
          <span className="absolute right-2.5 top-2 h-1.5 w-1.5 rounded-full bg-[#e85d3f] ring-2 ring-white" />
        </button>
        <button
          type="button"
          className="ml-1 flex items-center gap-2 rounded-xl p-1.5 hover:bg-[#eee9e2]"
        >
          <div className="grid h-9 w-9 place-items-center rounded-xl bg-[#26201c] text-xs font-bold text-white">
            {initials(user?.email ?? "HR")}
          </div>
          <ChevronDown className="hidden h-4 w-4 text-[#7b726a] sm:block" />
        </button>
      </div>
    </header>
  );
}
