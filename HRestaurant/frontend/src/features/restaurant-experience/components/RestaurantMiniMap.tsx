import { Map } from "lucide-react";
import type { PublicBranchScene, PublicRestaurantTable } from "@/types/public";

interface RestaurantMiniMapProps {
  scene: PublicBranchScene;
  tables: PublicRestaurantTable[];
  activeHotspotIndex: number;
  selectedTableId: string | null;
  onHotspotSelect: (index: number) => void;
}

export function RestaurantMiniMap({
  scene,
  tables,
  activeHotspotIndex,
  selectedTableId,
  onHotspotSelect,
}: RestaurantMiniMapProps) {
  const left = (x: number) =>
    `${((x - (scene.centerX - scene.floorWidth / 2)) / scene.floorWidth) * 100}%`;
  const top = (z: number) =>
    `${((z - (scene.centerZ - scene.floorDepth / 2)) / scene.floorDepth) * 100}%`;

  return (
    <div className="absolute bottom-24 left-4 z-10 hidden w-48 rounded-2xl border border-white/20 bg-[#211914]/88 p-3 text-white shadow-xl backdrop-blur md:block">
      <p className="flex items-center gap-1.5 text-[10px] font-bold uppercase tracking-[0.14em] text-white/65">
        <Map className="h-3.5 w-3.5" aria-hidden /> Mini xəritə
      </p>
      <div className="relative mt-2 aspect-[4/3] overflow-hidden rounded-xl border border-white/15 bg-[#c8aa86]/25">
        {tables.map((table) => (
          <span
            key={table.id}
            className={`absolute h-2 w-2 -translate-x-1/2 -translate-y-1/2 rounded-sm ${
              selectedTableId === table.id ? "bg-[#ff6a45] ring-2 ring-white" : "bg-white/65"
            }`}
            style={{ left: left(table.positionX), top: top(table.positionZ) }}
            aria-hidden
          />
        ))}
        {scene.hotspots.map((hotspot, index) => (
          <button
            key={hotspot.key}
            type="button"
            aria-label={`${hotspot.name} zonasına keç`}
            onClick={() => onHotspotSelect(index)}
            className={`absolute h-3 w-3 -translate-x-1/2 -translate-y-1/2 rounded-full border-2 border-[#211914] ${
              index === activeHotspotIndex ? "bg-[#ff6a45]" : "bg-amber-200"
            }`}
            style={{ left: left(hotspot.positionX), top: top(hotspot.positionZ) }}
          />
        ))}
      </div>
    </div>
  );
}
