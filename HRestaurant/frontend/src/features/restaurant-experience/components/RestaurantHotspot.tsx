import { Html } from "@react-three/drei";
import { MapPin } from "lucide-react";
import { memo } from "react";
import type { PublicSceneHotspot } from "@/types/public";

interface RestaurantHotspotProps {
  hotspot: PublicSceneHotspot;
  active: boolean;
  visible: boolean;
  onSelect: () => void;
}

export const RestaurantHotspot = memo(function RestaurantHotspot({
  hotspot,
  active,
  visible,
  onSelect,
}: RestaurantHotspotProps) {
  if (!visible) return null;
  return (
    <group position={[hotspot.positionX, hotspot.positionY, hotspot.positionZ]}>
      <mesh onClick={onSelect} position={[0, 0.14, 0]}>
        <cylinderGeometry args={[active ? 0.28 : 0.2, active ? 0.34 : 0.26, 0.08, 32]} />
        <meshStandardMaterial
          color={active ? "#ef6542" : "#f3c29c"}
          emissive={active ? "#b52f18" : "#4a2417"}
          emissiveIntensity={active ? 0.65 : 0.18}
        />
      </mesh>
      <Html center position={[0, 0.72, 0]} distanceFactor={9} zIndexRange={[18, 6]}>
        <button
          type="button"
          onClick={(event) => {
            event.stopPropagation();
            onSelect();
          }}
          className={`inline-flex items-center gap-1.5 whitespace-nowrap rounded-full px-3 py-1.5 text-[10px] font-bold shadow-lg transition ${
            active ? "bg-[#ef6542] text-white" : "bg-white/95 text-[#3b2c24]"
          }`}
        >
          <MapPin className="h-3 w-3" aria-hidden />
          {hotspot.name}
        </button>
      </Html>
    </group>
  );
});
