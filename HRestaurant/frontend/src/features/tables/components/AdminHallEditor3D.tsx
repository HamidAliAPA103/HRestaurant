import { Html, OrbitControls } from "@react-three/drei";
import { Canvas, type ThreeEvent } from "@react-three/fiber";
import { useEffect, useState } from "react";
import type { DiningTable } from "@/shared/types/domain";
import { TableChairs3D } from "@/features/table-3d/components/TableChairs3D";

const colors = ["#3f8a68", "#a94343", "#c58b34", "#77716c", "#397f9a"];

export function AdminHallEditor3D({ tables, selectedId, onSelect, onMove }: {
  tables: DiningTable[];
  selectedId: string | null;
  onSelect: (id: string) => void;
  onMove: (id: string, x: number, z: number) => void;
}) {
  const [dragging, setDragging] = useState<string | null>(null);
  useEffect(() => {
    const stop = () => setDragging(null);
    window.addEventListener("pointerup", stop);
    return () => window.removeEventListener("pointerup", stop);
  }, []);
  const move = (event: ThreeEvent<PointerEvent>) => {
    if (!dragging) return;
    event.stopPropagation();
    onMove(dragging, Math.round(event.point.x * 10) / 10, Math.round(event.point.z * 10) / 10);
  };
  return <div className="h-[520px] overflow-hidden rounded-3xl border bg-[#e9e2d9]">
    <Canvas shadows dpr={[1, 1.5]} camera={{ position: [0, 14, 16], fov: 44 }} aria-label="Masa planının 3D redaktoru">
      <color attach="background" args={["#e9e2d9"]} /><ambientLight intensity={1} /><directionalLight castShadow position={[8, 14, 8]} intensity={2} />
      <mesh receiveShadow rotation={[-Math.PI / 2, 0, 0]} onPointerMove={move} onPointerUp={() => setDragging(null)}>
        <planeGeometry args={[30, 30]} /><meshStandardMaterial color="#d8cbbd" roughness={0.95} />
      </mesh>
      <gridHelper args={[30, 30, "#9a8979", "#c7b9ab"]} position={[0, 0.01, 0]} />
      {tables.map((table) => {
        const selected = selectedId === table.id; const tableHeight = Math.max(table.height, 0.3);
        return <group key={table.id} position={[table.positionX, table.positionY + tableHeight, table.positionZ]} rotation={[table.rotationX, table.rotationY, table.rotationZ]}>
          <mesh castShadow receiveShadow scale={selected ? 1.08 : 1} onPointerDown={(event) => { event.stopPropagation(); onSelect(table.id); setDragging(table.id); }}>
            {table.shape === 0 ? <cylinderGeometry args={[Math.max(table.width, table.length) / 2, Math.max(table.width, table.length) / 2, tableHeight, 32]} /> : <boxGeometry args={[table.width, tableHeight, table.length]} />}
            <meshStandardMaterial color={selected ? "#e46a47" : colors[table.status] ?? "#77716c"} emissive={selected ? "#6f2116" : "#000"} emissiveIntensity={selected ? 0.25 : 0} />
          </mesh>
          <mesh castShadow position={[0, -0.48, 0]}><cylinderGeometry args={[0.18, 0.28, 0.75, 16]} /><meshStandardMaterial color="#4e4238" /></mesh>
          <TableChairs3D capacity={table.capacity} width={table.width} length={table.length} />
          <Html center position={[0, 1.25, 0]} distanceFactor={12}><button type="button" onClick={() => onSelect(table.id)} className="whitespace-nowrap rounded-full bg-white px-2 py-1 text-xs font-bold shadow">Masa {table.tableNumber}</button></Html>
        </group>;
      })}
      <OrbitControls enabled={!dragging} makeDefault minDistance={7} maxDistance={30} maxPolarAngle={Math.PI / 2.05} />
    </Canvas>
  </div>;
}
