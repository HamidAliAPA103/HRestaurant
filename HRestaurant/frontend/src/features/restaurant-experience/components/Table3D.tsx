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
  const height = Math.max(table.height, 0.2);

  useEffect(() => {
    const group = groupRef.current;
    if (!group) return;
    const targetY = table.positionY + height + (hovered || selected ? 0.14 : 0);
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
  }, [height, hovered, reducedMotion, selected, table.positionY]);

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
      position={[table.positionX, table.positionY + height, table.positionZ]}
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
            args={[Math.max(table.width, table.length) / 2, Math.max(table.width, table.length) / 2, height, 36]}
          />
        ) : (
          <boxGeometry args={[table.width, height, table.length]} />
        )}
        <meshStandardMaterial
          color={tableStatusColors[status]}
          roughness={0.58}
          metalness={selected ? 0.18 : 0.04}
          emissive={selected ? "#bd351d" : "#000000"}
          emissiveIntensity={selected ? 0.28 : 0}
        />
      </mesh>
      <mesh castShadow position={[0, -0.48, 0]}>
        <cylinderGeometry args={[0.17, 0.28, 0.76, 18]} />
        <meshStandardMaterial color="#45372d" roughness={0.74} />
      </mesh>
      <TableChairs3D capacity={table.capacity} width={table.width} length={table.length} />
      <TableNumberLabel tableNumber={table.tableNumber} />
      <TableStatusIndicator status={status} visible={selected || hovered} />
      <TableHoverCard table={table} status={status} visible={hovered && !selected} />
      {selected && (
        <mesh ref={ringRef} position={[0, -height / 2 - 0.03, 0]} rotation={[-Math.PI / 2, 0, 0]}>
          <torusGeometry args={[Math.max(table.width, table.length) * 0.76, 0.055, 12, 64]} />
          <meshBasicMaterial color="#ff6a45" transparent opacity={0.92} />
        </mesh>
      )}
    </group>
  );
});
