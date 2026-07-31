import { Html } from "@react-three/drei";
import type { PublicRestaurantTable, PublicTableStatus } from "@/types/public";
import { statusLabels } from "./RestaurantTable3D";

interface TableTooltipProps {
  table: PublicRestaurantTable;
  status: PublicTableStatus;
  hovered: boolean;
  reservationDate: string;
  startTime: string;
}

export function TableTooltip({
  table,
  status,
  hovered,
  reservationDate,
  startTime,
}: TableTooltipProps) {
  return (
    <>
      <Html
        center
        position={[0, 1.35, 0]}
        distanceFactor={8}
        style={{ pointerEvents: "none" }}
      >
        <span className="whitespace-nowrap rounded-full border border-white/20 bg-[#211d18]/90 px-2.5 py-1 text-xs font-bold text-white shadow-xl backdrop-blur">
          {table.tableNumber} · {statusLabels[status]}
        </span>
      </Html>
      {hovered && (
        <Html
          center
          position={[0, 2.25, 0]}
          distanceFactor={7}
          style={{ pointerEvents: "none" }}
        >
          <div className="w-48 rounded-2xl border border-white/10 bg-[#211d18]/95 p-3 text-xs text-white shadow-2xl backdrop-blur">
            <p className="font-bold">
              Masa {table.tableNumber} · {table.capacity} nəfər
            </p>
            <p className="mt-1 text-white/70">Status: {statusLabels[status]}</p>
            <p className="mt-1 text-white/55">
              {reservationDate} · {startTime}
            </p>
          </div>
        </Html>
      )}
    </>
  );
}
