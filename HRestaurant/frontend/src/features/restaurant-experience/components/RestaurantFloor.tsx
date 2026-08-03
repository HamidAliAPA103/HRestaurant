import { memo } from "react";

interface RestaurantFloorProps {
  width: number;
  depth: number;
  centerX: number;
  centerZ: number;
}

export const RestaurantFloor = memo(function RestaurantFloor({
  width,
  depth,
  centerX,
  centerZ,
}: RestaurantFloorProps) {
  return (
    <group position={[centerX, -0.08, centerZ]}>
      <mesh receiveShadow>
        <boxGeometry args={[width, 0.16, depth]} />
        <meshStandardMaterial color="#c8aa86" roughness={0.82} />
      </mesh>
      <gridHelper
        args={[Math.max(width, depth), Math.ceil(Math.max(width, depth)), "#ad8a66", "#d7c0a5"]}
        position={[0, 0.085, 0]}
      />
    </group>
  );
});
