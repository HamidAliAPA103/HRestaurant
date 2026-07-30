import { CalendarDays } from "lucide-react";
import type { PublicWorkingHour } from "@/types/public";
import {
  getTodayInputValue,
  getWorkingHour,
} from "@/utils/reservation-date";

interface ReservationDateStepProps {
  value: string;
  workingHours: PublicWorkingHour[];
  onChange: (value: string) => void;
}

export function ReservationDateStep({
  value,
  workingHours,
  onChange,
}: ReservationDateStepProps) {
  const selectedSchedule = getWorkingHour(workingHours, value);
  const isClosed = selectedSchedule?.isClosed;

  return (
    <label className="block">
      <span className="mb-2 flex items-center gap-2 text-sm font-bold text-[#4d443d]">
        <CalendarDays className="h-4 w-4 text-[#b5422d]" />
        Tarix
      </span>
      <input
        type="date"
        value={value}
        min={getTodayInputValue()}
        onChange={(event) => onChange(event.target.value)}
        className="w-full rounded-2xl border border-[#d9d0c6] bg-white px-4 py-3 outline-none transition focus:border-[#b5422d] focus:ring-4 focus:ring-[#b5422d]/10"
      />
      {isClosed && (
        <span className="mt-2 block text-sm text-amber-700">
          Seçilən gün filial bağlıdır.
        </span>
      )}
    </label>
  );
}
