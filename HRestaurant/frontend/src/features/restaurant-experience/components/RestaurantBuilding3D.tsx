import { memo } from "react";

interface RestaurantBuilding3DProps {
  width: number;
  depth: number;
  height: number;
  centerX: number;
  centerZ: number;
}

export const RestaurantBuilding3D = memo(function RestaurantBuilding3D({
  width,
  depth,
  height,
  centerX,
  centerZ,
}: RestaurantBuilding3DProps) {
  return (
    <group position={[centerX, 0, centerZ + depth / 2]}>
      <mesh castShadow position={[-width * 0.17, height / 2, 0.06]}>
        <boxGeometry args={[0.32, height, 0.32]} />
        <meshStandardMaterial color="#4e3024" roughness={0.72} />
      </mesh>
      <mesh castShadow position={[width * 0.17, height / 2, 0.06]}>
        <boxGeometry args={[0.32, height, 0.32]} />
        <meshStandardMaterial color="#4e3024" roughness={0.72} />
      </mesh>
      <mesh castShadow position={[0, height - 0.55, 0.32]}>
        <boxGeometry args={[width * 0.42, 0.9, 0.22]} />
        <meshStandardMaterial color="#231a15" roughness={0.5} />
      </mesh>
      <mesh castShadow position={[0, height - 1.25, 0.48]} rotation={[0.08, 0, 0]}>
        <boxGeometry args={[width * 0.5, 0.16, 1.45]} />
        <meshStandardMaterial color="#b94b31" roughness={0.64} />
      </mesh>
      <mesh position={[0, 1.45, 0.02]}>
        <boxGeometry args={[width * 0.25, 2.8, 0.12]} />
        <meshPhysicalMaterial
          color="#9ab0b2"
          transparent
          opacity={0.24}
          transmission={0.55}
          roughness={0.12}
        />
      </mesh>
    </group>
  );
});
