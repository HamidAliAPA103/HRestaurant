import { Clock3 } from "lucide-react";
import type { PublicWorkingHour } from "@/types/public";
import { generateTimeSlots } from "@/utils/reservation-date";

interface ReservationTimeStepProps {
  workingHours: PublicWorkingHour[];
  reservationDate: string;
  startTime: string;
  durationMinutes: number;
  onStartTimeChange: (value: string) => void;
  onDurationChange: (value: number) => void;
}

export function ReservationTimeStep({
  workingHours,
  reservationDate,
  startTime,
  durationMinutes,
  onStartTimeChange,
  onDurationChange,
}: ReservationTimeStepProps) {
  const timeSlots = generateTimeSlots(
    workingHours,
    reservationDate,
    durationMinutes,
  );

  return (
    <div className="grid gap-4 sm:grid-cols-2">
      <label>
        <span className="mb-2 flex items-center gap-2 text-sm font-bold text-[#4d443d]">
          <Clock3 className="h-4 w-4 text-[#b5422d]" />
          Başlama saatı
        </span>
        <select
          value={startTime}
          disabled={timeSlots.length === 0}
          onChange={(event) => onStartTimeChange(event.target.value)}
          className="w-full rounded-2xl border border-[#d9d0c6] bg-white px-4 py-3 outline-none transition disabled:bg-stone-100 focus:border-[#b5422d] focus:ring-4 focus:ring-[#b5422d]/10"
        >
          <option value="">Saat seçin</option>
          {timeSlots.map((time) => (
            <option key={time} value={time}>
              {time}
            </option>
          ))}
        </select>
      </label>
      <label>
        <span className="mb-2 block text-sm font-bold text-[#4d443d]">
          Müddət
        </span>
        <select
          value={durationMinutes}
          onChange={(event) =>
            onDurationChange(Number(event.target.value))
          }
          className="w-full rounded-2xl border border-[#d9d0c6] bg-white px-4 py-3 outline-none transition focus:border-[#b5422d] focus:ring-4 focus:ring-[#b5422d]/10"
        >
          <option value={60}>1 saat</option>
          <option value={90}>1 saat 30 dəqiqə</option>
          <option value={120}>2 saat</option>
          <option value={150}>2 saat 30 dəqiqə</option>
          <option value={180}>3 saat</option>
        </select>
      </label>
    </div>
  );
}
