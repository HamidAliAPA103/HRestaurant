import { Html } from "@react-three/drei";
import { memo } from "react";

interface TableNumberLabelProps {
  tableNumber: string;
}

export const TableNumberLabel = memo(function TableNumberLabel({
  tableNumber,
}: TableNumberLabelProps) {
  return (
    <Html center position={[0, 0.18, 0]} distanceFactor={10} zIndexRange={[12, 2]}>
      <span className="pointer-events-none grid h-7 min-w-7 place-items-center rounded-full border border-white/70 bg-[#211914]/90 px-1.5 text-[10px] font-black text-white shadow-md">
        {tableNumber}
      </span>
    </Html>
  );
});
