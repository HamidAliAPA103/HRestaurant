import { Html } from "@react-three/drei";
import { createElement, memo } from "react";
import {
  tableStatusColors,
  tableStatusIcons,
  tableStatusLabels,
} from "@/features/restaurant-experience/lib/table-status";
import type { PublicTableStatus } from "@/types/public";

interface TableStatusIndicatorProps {
  status: PublicTableStatus;
  visible: boolean;
}

export const TableStatusIndicator = memo(function TableStatusIndicator({
  status,
  visible,
}: TableStatusIndicatorProps) {
  if (!visible) return null;
  const Icon = tableStatusIcons[status];

  return (
    <Html center position={[0, 1.18, 0]} distanceFactor={8} zIndexRange={[25, 10]}>
      <span className="pointer-events-none inline-flex items-center gap-1.5 whitespace-nowrap rounded-full bg-white px-2.5 py-1 text-[10px] font-bold text-[#30261f] shadow-lg">
        {createElement(Icon, {
          size: 12,
          color: tableStatusColors[status],
          "aria-hidden": true,
        })}
        {tableStatusLabels[status]}
      </span>
    </Html>
  );
});
