import { useFrame } from "@react-three/fiber";
import type { ThreeEvent } from "@react-three/fiber";
import gsap from "gsap";
import { memo, useEffect, useRef } from "react";
import type { Group, Mesh } from "three";
import {
  isSelectableStatus,
  tableStatusColors,
} from "@/features/restaurant-experience/lib/table-status";
import type { PublicRestaurantTable, PublicTableStatus } from "@/types/public";
import { TableChairs3D } from "./TableChairs3D";
import { TableHoverCard } from "./TableHoverCard";
import { TableNumberLabel } from "./TableNumberLabel";
import { TableStatusIndicator } from "./TableStatusIndicator";

interface Table3DProps {
  table: PublicRestaurantTable;
  selected: boolean;
  hovered: boolean;
  reducedMotion: boolean;
  onHover: (id: string | null) => void;
  onSelect: (table: PublicRestaurantTable) => void;
}

export const Table3D = memo(function Table3D({
  table,
  selected,
  hovered,
  reducedMotion,
  onHover,
  onSelect,
}: Table3DProps) {
  const groupRef = useRef<Group>(null);
  const ringRef = useRef<Mesh>(null);
  const status: PublicTableStatus = selected ? "Selected" : table.status;
  const canSelect = table.isAvailable && isSelectableStatus(status);
  const tableHeight = Math.max(table.height, 0.68);
  const topThickness = Math.min(Math.max(tableHeight * 0.2, 0.16), 0.24);
  const pedestalHeight = Math.max(tableHeight - topThickness / 2, 0.5);

  useEffect(() => {
    const group = groupRef.current;
    if (!group) return;
    const targetY = table.positionY + tableHeight + (hovered || selected ? 0.1 : 0);
    if (reducedMotion) {
      group.position.y = targetY;
      return;
    }
    const tween = gsap.to(group.position, {
      y: targetY,
      duration: 0.22,
      ease: "power2.out",
      overwrite: true,
    });
    return () => {
      tween.kill();
    };
  }, [hovered, reducedMotion, selected, table.positionY, tableHeight]);

  useFrame(({ clock }) => {
    if (!ringRef.current || !selected || reducedMotion) return;
    ringRef.current.rotation.z = clock.elapsedTime * 0.8;
    const scale = 1 + Math.sin(clock.elapsedTime * 3) * 0.045;
    ringRef.current.scale.setScalar(scale);
  });

  const select = (event: ThreeEvent<MouseEvent>) => {
    event.stopPropagation();
    if (canSelect) onSelect(table);
  };

  return (
    <group
      ref={groupRef}
      position={[table.positionX, table.positionY + tableHeight, table.positionZ]}
      rotation={[table.rotationX, table.rotationY, table.rotationZ]}
    >
      <mesh
        castShadow
        receiveShadow
        onClick={select}
        onPointerOver={(event) => {
          event.stopPropagation();
          onHover(table.id);
          document.body.style.cursor = canSelect ? "pointer" : "not-allowed";
        }}
        onPointerOut={() => {
          onHover(null);
          document.body.style.cursor = "default";
        }}
      >
        {table.shape === "Round" ? (
          <cylinderGeometry
            args={[Math.max(table.width, table.length) / 2, Math.max(table.width, table.length) / 2, topThickness, 36]}
          />
        ) : (
          <boxGeometry args={[table.width, topThickness, table.length]} />
        )}
        <meshStandardMaterial
          color="#70452f"
          roughness={0.48}
          metalness={0.06}
        />
      </mesh>
      <mesh castShadow position={[0, -pedestalHeight / 2, 0]}>
        <cylinderGeometry args={[0.14, 0.25, pedestalHeight, 18]} />
        <meshStandardMaterial color="#30221c" roughness={0.56} metalness={0.22} />
      </mesh>
      <mesh position={[0, -topThickness / 2 - 0.025, 0]}>
        {table.shape === "Round" ? (
          <cylinderGeometry args={[Math.max(table.width, table.length) * 0.51, Math.max(table.width, table.length) * 0.51, 0.045, 36]} />
        ) : (
          <boxGeometry args={[table.width + 0.08, 0.045, table.length + 0.08]} />
        )}
        <meshStandardMaterial
          color={tableStatusColors[status]}
          emissive={tableStatusColors[status]}
          emissiveIntensity={selected || hovered ? 1.05 : 0.35}
          roughness={0.4}
        />
      </mesh>
      <group position={[table.width * 0.2, topThickness / 2 + 0.08, 0]}>
        <mesh castShadow>
          <cylinderGeometry args={[0.09, 0.075, 0.17, 16]} />
          <meshStandardMaterial color="#eee2ca" roughness={0.58} />
        </mesh>
        <mesh position={[0, 0.13, 0]}>
          <sphereGeometry args={[0.035, 12, 8]} />
          <meshStandardMaterial
            color="#ffd69a"
            emissive="#ff9d45"
            emissiveIntensity={1.35}
            roughness={0.3}
          />
        </mesh>
      </group>
      <TableChairs3D
        capacity={table.capacity}
        width={table.width}
        length={table.length}
        tableHeight={tableHeight}
      />
      <TableNumberLabel tableNumber={table.tableNumber} />
      <TableStatusIndicator status={status} visible={selected || hovered} />
      <TableHoverCard table={table} status={status} visible={hovered && !selected} />
      {selected && (
        <mesh ref={ringRef} position={[0, -tableHeight + 0.035, 0]} rotation={[-Math.PI / 2, 0, 0]}>
          <torusGeometry args={[Math.max(table.width, table.length) * 0.76, 0.055, 12, 64]} />
          <meshBasicMaterial color="#ff6a45" transparent opacity={0.92} />
        </mesh>
      )}
    </group>
  );
});
