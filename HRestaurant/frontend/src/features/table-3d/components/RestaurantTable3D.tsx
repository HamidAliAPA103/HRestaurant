import { Html } from "@react-three/drei";
import { useState } from "react";
import type { ThreeEvent } from "@react-three/fiber";
import type {
  PublicRestaurantTable,
  PublicTableStatus,
} from "@/types/public";
import { TableChairs3D } from "./TableChairs3D";

interface RestaurantTable3DProps {
  table: PublicRestaurantTable;
  selected: boolean;
  reservationDate: string;
  startTime: string;
  onSelect: (table: PublicRestaurantTable) => void;
}

const statusColors: Record<PublicTableStatus, string> = {
  Available: "#3f8a68",
  Selected: "#e46a47",
  Reserved: "#c58b34",
  Occupied: "#a94343",
  Disabled: "#77716c",
  CapacityNotSuitable: "#8f7ca8",
};

const statusLabels: Record<PublicTableStatus, string> = {
  Available: "Boşdur",
  Selected: "Seçilib",
  Reserved: "Rezerv edilib",
  Occupied: "Məşğuldur",
  Disabled: "Aktiv deyil",
  CapacityNotSuitable: "Tutumu uyğun deyil",
};

export function RestaurantTable3D({
  table,
  selected,
  reservationDate,
  startTime,
  onSelect,
}: RestaurantTable3DProps) {
  const [hovered, setHovered] = useState(false);
  const status: PublicTableStatus = selected
    ? "Selected"
    : table.status;
  const canSelect = table.isAvailable;

  const handleClick = (event: ThreeEvent<MouseEvent>) => {
    event.stopPropagation();

    if (canSelect) {
      onSelect(table);
    }
  };

  return (
    <group
      position={[
        table.positionX,
        table.positionY + 0.7,
        table.positionZ,
      ]}
      rotation={[table.rotationX, table.rotationY, table.rotationZ]}
    >
      <mesh
        castShadow
        receiveShadow
        scale={selected ? 1.08 : hovered ? 1.04 : 1}
        onClick={handleClick}
        onPointerOver={(event) => {
          event.stopPropagation();
          setHovered(true);
          document.body.style.cursor = canSelect ? "pointer" : "not-allowed";
        }}
        onPointerOut={() => {
          setHovered(false);
          document.body.style.cursor = "default";
        }}
      >
        {table.shape === "Round" ? (
          <cylinderGeometry
            args={[
              Math.max(table.width, table.length) / 2,
              Math.max(table.width, table.length) / 2,
              0.5,
              32,
            ]}
          />
        ) : (
          <boxGeometry args={[table.width, 0.5, table.length]} />
        )}
        <meshStandardMaterial
          color={statusColors[status]}
          roughness={0.62}
          metalness={selected ? 0.18 : 0.05}
          emissive={selected ? "#7f2818" : "#000000"}
          emissiveIntensity={selected ? 0.22 : 0}
        />
      </mesh>
      <mesh castShadow position={[0, -0.48, 0]}>
        <cylinderGeometry args={[0.18, 0.28, 0.75, 16]} />
        <meshStandardMaterial color="#4e4238" roughness={0.7} />
      </mesh>
      <TableChairs3D
        capacity={table.capacity}
        width={table.width}
        length={table.length}
      />
      <Html
        center
        position={[0, 1.35, 0]}
        distanceFactor={8}
        style={{ pointerEvents: "none" }}
      >
        <span className="whitespace-nowrap rounded-full border border-white/20 bg-[#211d18]/90 px-2.5 py-1 text-xs font-bold text-white shadow-xl backdrop-blur">
          {table.tableNumber}
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
            <p className="mt-1 text-white/70">{statusLabels[status]}</p>
            <p className="mt-1 text-white/55">
              {reservationDate} · {startTime}
            </p>
          </div>
        </Html>
      )}
    </group>
  );
}

export { statusColors, statusLabels };
