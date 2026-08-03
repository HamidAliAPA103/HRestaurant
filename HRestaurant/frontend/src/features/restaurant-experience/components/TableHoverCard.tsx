import { Html } from "@react-three/drei";
import { Users } from "lucide-react";
import { memo } from "react";
import { tableStatusLabels } from "@/features/restaurant-experience/lib/table-status";
import type { PublicRestaurantTable, PublicTableStatus } from "@/types/public";

interface TableHoverCardProps {
  table: PublicRestaurantTable;
  status: PublicTableStatus;
  visible: boolean;
}

export const TableHoverCard = memo(function TableHoverCard({
  table,
  status,
  visible,
}: TableHoverCardProps) {
  if (!visible) return null;
  return (
    <Html position={[0.85, 1.35, 0]} distanceFactor={7} zIndexRange={[30, 15]}>
      <div className="pointer-events-none w-44 rounded-2xl border border-white/20 bg-[#211914]/95 p-3 text-white shadow-2xl backdrop-blur">
        <p className="font-bold">Masa {table.tableNumber}</p>
        <p className="mt-1 flex items-center gap-1.5 text-xs text-white/70">
          <Users className="h-3.5 w-3.5" aria-hidden />
          {table.capacity} nəfərlik
        </p>
        <p className="mt-2 text-[11px] font-semibold text-[#ffc7a9]">
          {tableStatusLabels[status]}
        </p>
      </div>
    </Html>
  );
});
