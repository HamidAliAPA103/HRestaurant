import { memo } from "react";
import { useRestaurantExperienceStore } from "@/features/restaurant-experience/store/restaurant-experience-store";
import type { PublicRestaurantTable } from "@/types/public";
import { Table3D } from "./Table3D";

interface RestaurantTablesLayerProps {
  tables: PublicRestaurantTable[];
  reducedMotion: boolean;
  onSelect: (table: PublicRestaurantTable) => void;
}

export const RestaurantTablesLayer = memo(function RestaurantTablesLayer({
  tables,
  reducedMotion,
  onSelect,
}: RestaurantTablesLayerProps) {
  const selectedTableId = useRestaurantExperienceStore(
    (state) => state.selectedTableId,
  );
  const hoveredTableId = useRestaurantExperienceStore(
    (state) => state.hoveredTableId,
  );
  const setHoveredTableId = useRestaurantExperienceStore(
    (state) => state.setHoveredTableId,
  );

  return (
    <group>
      {tables.map((table) => (
        <Table3D
          key={table.id}
          table={table}
          selected={selectedTableId === table.id}
          hovered={hoveredTableId === table.id}
          reducedMotion={reducedMotion}
          onHover={setHoveredTableId}
          onSelect={onSelect}
        />
      ))}
    </group>
  );
});
