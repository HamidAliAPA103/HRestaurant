import type { PublicTableStatus } from "@/types/public";
import { statusColors, statusLabels } from "./RestaurantTable3D";

const statuses: PublicTableStatus[] = [
  "Available",
  "Selected",
  "Reserved",
  "Occupied",
  "Disabled",
  "CapacityNotSuitable",
];

export function TableStatusLegend() {
  return (
    <div
      aria-label="Masa statusları"
      className="flex flex-wrap gap-2"
    >
      {statuses.map((status) => (
        <span
          key={status}
          className="inline-flex items-center gap-2 rounded-full border border-[#ded5cb] bg-white px-3 py-1.5 text-xs font-semibold text-[#5d544d]"
        >
          <span
            className="h-2.5 w-2.5 rounded-full"
            style={{ backgroundColor: statusColors[status] }}
          />
          {statusLabels[status]}
        </span>
      ))}
    </div>
  );
}
