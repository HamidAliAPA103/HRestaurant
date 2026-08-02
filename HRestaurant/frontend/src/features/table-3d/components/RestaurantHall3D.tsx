import { Bounds } from "@react-three/drei";
import { Canvas } from "@react-three/fiber";
import { Suspense, useMemo } from "react";
import type { PublicRestaurantTable } from "@/types/public";
import { HallFloor } from "./HallFloor";
import { HallWalls } from "./HallWalls";
import { RestaurantTable3D } from "./RestaurantTable3D";
import { CameraController } from "./CameraController";

interface RestaurantHall3DProps {
  tables: PublicRestaurantTable[];
  selectedTable: PublicRestaurantTable | null;
  reservationDate: string;
  startTime: string;
  onSelect: (table: PublicRestaurantTable) => void;
}

export function RestaurantHall3D({
  tables,
  selectedTable,
  reservationDate,
  startTime,
  onSelect,
}: RestaurantHall3DProps) {
  const laidOutTables = useMemo(() => applyFallbackLayout(tables), [tables]);

  return (
    <div className="h-[430px] overflow-hidden rounded-[28px] border border-[#d9d0c6] bg-[#e8e0d6] sm:h-[560px]">
      <Canvas
        shadows
        dpr={[1, 1.5]}
        frameloop="demand"
        camera={{ position: [0, 13, 17], fov: 42, near: 0.1, far: 100 }}
        gl={{ antialias: true, powerPreference: "high-performance" }}
        aria-label="Restoran zalının interaktiv 3D görünüşü"
      >
        <color attach="background" args={["#e9e2d9"]} />
        <fog attach="fog" args={["#e9e2d9", 18, 36]} />
        <ambientLight intensity={0.9} />
        <directionalLight
          castShadow
          position={[8, 14, 10]}
          intensity={2.2}
          shadow-mapSize-width={1024}
          shadow-mapSize-height={1024}
        />
        <hemisphereLight
          args={["#fff6e9", "#786c60", 0.8]}
          position={[0, 10, 0]}
        />
        <HallFloor />
        <HallWalls />
        <Suspense fallback={null}>
          <Bounds fit clip observe margin={1.25}>
            <group>
              {laidOutTables.map((table) => (
                <RestaurantTable3D
                  key={table.id}
                  table={table}
                  selected={selectedTable?.id === table.id}
                  reservationDate={reservationDate}
                  startTime={startTime}
                  onSelect={onSelect}
                />
              ))}
            </group>
          </Bounds>
        </Suspense>
        <CameraController tables={laidOutTables} />
      </Canvas>
    </div>
  );
}

function applyFallbackLayout(tables: PublicRestaurantTable[]) {
  const coordinates = new Set(
    tables.map((table) => `${table.positionX}:${table.positionZ}`),
  );

  if (tables.length <= 1 || coordinates.size > 1) {
    return tables;
  }

  return tables.map((table, index) => ({
    ...table,
    positionX: (index % 4) * 3.4 - 5.1,
    positionZ: Math.floor(index / 4) * 3.4 - 3.4,
  }));
}
