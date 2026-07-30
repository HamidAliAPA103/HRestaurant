import {
  CalendarDays,
  Clock3,
  MapPin,
  Phone,
  Users,
  Utensils,
} from "lucide-react";
import type {
  CustomerInformation,
  PublicBranch,
  PublicRestaurant,
  PublicRestaurantTable,
} from "@/types/public";
import { formatDate } from "@/utils/reservation-date";

interface ReservationSummaryProps {
  restaurant: PublicRestaurant;
  branch: PublicBranch;
  table: PublicRestaurantTable;
  reservationDate: string;
  startTime: string;
  durationMinutes: number;
  guestCount: number;
  customer: CustomerInformation;
}

export function ReservationSummary({
  restaurant,
  branch,
  table,
  reservationDate,
  startTime,
  durationMinutes,
  guestCount,
  customer,
}: ReservationSummaryProps) {
  const endTime = addMinutes(startTime, durationMinutes);

  return (
    <div className="grid gap-6 lg:grid-cols-[1fr_0.8fr]">
      <div className="rounded-3xl bg-[#241f1a] p-6 text-white sm:p-8">
        <p className="text-xs font-bold uppercase tracking-[0.22em] text-[#e7a48f]">
          Rezervasiya xülasəsi
        </p>
        <h3 className="mt-3 font-serif text-3xl">{restaurant.name}</h3>
        <SummaryRow icon={MapPin} label={branch.name} value={branch.address} />
        <SummaryRow
          icon={CalendarDays}
          label={formatDate(reservationDate)}
          value={`${startTime}–${endTime}`}
        />
        <SummaryRow
          icon={Users}
          label={`${guestCount} qonaq`}
          value={`${durationMinutes} dəqiqə`}
        />
        <SummaryRow
          icon={Utensils}
          label={`Masa ${table.tableNumber}`}
          value={`${table.capacity} nəfərlik · ${table.shape}`}
        />
      </div>
      <div className="rounded-3xl border border-[#dfd6cc] bg-white p-6 sm:p-8">
        <p className="text-xs font-bold uppercase tracking-[0.22em] text-[#9a4130]">
          Əlaqə məlumatları
        </p>
        <p className="mt-4 font-serif text-2xl font-semibold">
          {customer.fullName}
        </p>
        <p className="mt-3 flex items-center gap-2 text-sm text-[#655b53]">
          <Phone className="h-4 w-4" />
          {customer.phone}
        </p>
        {customer.email && (
          <p className="mt-2 text-sm text-[#655b53]">{customer.email}</p>
        )}
        {customer.specialNotes && (
          <div className="mt-5 rounded-2xl bg-stone-50 p-4 text-sm leading-6 text-[#655b53]">
            {customer.specialNotes}
          </div>
        )}
      </div>
    </div>
  );
}

interface SummaryRowProps {
  icon: typeof Clock3;
  label: string;
  value: string;
}

function SummaryRow({ icon: Icon, label, value }: SummaryRowProps) {
  return (
    <div className="mt-5 flex items-start gap-3 border-t border-white/10 pt-5">
      <Icon className="mt-1 h-4 w-4 shrink-0 text-[#e7a48f]" />
      <div>
        <p className="font-bold">{label}</p>
        <p className="mt-0.5 text-sm text-white/55">{value}</p>
      </div>
    </div>
  );
}

function addMinutes(startTime: string, durationMinutes: number) {
  const [hours, minutes] = startTime.split(":").map(Number);
  const total = (hours * 60 + minutes + durationMinutes) % (24 * 60);
  return `${Math.floor(total / 60)
    .toString()
    .padStart(2, "0")}:${(total % 60).toString().padStart(2, "0")}`;
}
