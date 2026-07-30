import { CheckCircle2, Users } from "lucide-react";
import type { PublicRestaurantTable } from "@/types/public";

interface SelectedTablePanelProps {
  table: PublicRestaurantTable | null;
}

export function SelectedTablePanel({ table }: SelectedTablePanelProps) {
  if (!table) {
    return (
      <div className="rounded-3xl border border-dashed border-[#cfc3b7] bg-white/60 p-5 text-sm text-[#766b62]">
        Davam etmək üçün 3D zaldan və ya siyahıdan uyğun masa seçin.
      </div>
    );
  }

  return (
    <div
      role="status"
      className="flex items-center justify-between gap-4 rounded-3xl border border-[#e6a18c] bg-[#fff3ed] p-5"
    >
      <div>
        <p className="flex items-center gap-2 font-bold text-[#8f2f1e]">
          <CheckCircle2 className="h-5 w-5" />
          Masa {table.tableNumber} seçildi
        </p>
        <p className="mt-1 flex items-center gap-1 text-sm text-[#766b62]">
          <Users className="h-4 w-4" />
          {table.capacity} nəfərlik · {table.shape}
        </p>
      </div>
      <span className="rounded-full bg-[#b5422d] px-3 py-1 text-xs font-bold text-white">
        Seçilib
      </span>
    </div>
  );
}
