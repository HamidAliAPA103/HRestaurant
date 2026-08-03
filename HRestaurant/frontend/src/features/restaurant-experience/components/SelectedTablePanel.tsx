import { CalendarDays, CheckCircle2, Clock3, Users } from "lucide-react";
import type { PublicRestaurantTable } from "@/types/public";
import { formatDate, formatTime } from "@/utils/reservation-date";

interface SelectedTablePanelProps {
  table: PublicRestaurantTable | null;
  reservationDate: string;
  startTime: string;
  onReserve: () => void;
}

export function SelectedTablePanel({
  table,
  reservationDate,
  startTime,
  onReserve,
}: SelectedTablePanelProps) {
  if (!table) {
    return (
      <div className="rounded-3xl border border-dashed border-[#cfc3b7] bg-white/75 p-5 text-sm text-[#766b62]">
        Rezervasiya üçün 3D zaldan və ya əlçatan siyahıdan boş masa seçin.
      </div>
    );
  }

  return (
    <div role="status" className="rounded-3xl border border-[#ef9b80] bg-[#fff3ed] p-5 shadow-sm">
      <p className="flex items-center gap-2 font-bold text-[#8f2f1e]">
        <CheckCircle2 className="h-5 w-5" aria-hidden />
        Masa {table.tableNumber} seçildi
      </p>
      <div className="mt-3 grid gap-2 text-sm text-[#675b52] sm:grid-cols-3">
        <span className="flex items-center gap-1.5">
          <Users className="h-4 w-4" aria-hidden /> {table.capacity} nəfər
        </span>
        <span className="flex items-center gap-1.5">
          <CalendarDays className="h-4 w-4" aria-hidden /> {formatDate(reservationDate)}
        </span>
        <span className="flex items-center gap-1.5">
          <Clock3 className="h-4 w-4" aria-hidden /> {formatTime(startTime)}
        </span>
      </div>
      <button
        type="button"
        onClick={onReserve}
        className="mt-5 w-full rounded-full bg-[#b5422d] px-5 py-3 text-sm font-bold text-white shadow-lg shadow-[#b5422d]/20"
      >
        Bu masanı rezerv et
      </button>
    </div>
  );
}
