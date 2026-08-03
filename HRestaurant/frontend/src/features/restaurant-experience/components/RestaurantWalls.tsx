import { memo } from "react";

interface RestaurantWallsProps {
  width: number;
  depth: number;
  height: number;
  centerX: number;
  centerZ: number;
}

export const RestaurantWalls = memo(function RestaurantWalls({
  width,
  depth,
  height,
  centerX,
  centerZ,
}: RestaurantWallsProps) {
  return (
    <group position={[centerX, height / 2, centerZ]}>
      <mesh castShadow receiveShadow position={[0, 0, -depth / 2]}>
        <boxGeometry args={[width, height, 0.2]} />
        <meshStandardMaterial color="#eadfce" roughness={0.86} />
      </mesh>
      <mesh castShadow receiveShadow position={[-width / 2, 0, 0]}>
        <boxGeometry args={[0.2, height, depth]} />
        <meshStandardMaterial color="#eadfce" roughness={0.86} />
      </mesh>
      <mesh castShadow receiveShadow position={[width / 2, 0, 0]}>
        <boxGeometry args={[0.2, height, depth]} />
        <meshStandardMaterial color="#eadfce" roughness={0.86} />
      </mesh>
      <mesh castShadow position={[-width * 0.34, 0, depth / 2]}>
        <boxGeometry args={[width * 0.3, height, 0.2]} />
        <meshStandardMaterial color="#eadfce" roughness={0.86} />
      </mesh>
      <mesh castShadow position={[width * 0.34, 0, depth / 2]}>
        <boxGeometry args={[width * 0.3, height, 0.2]} />
        <meshStandardMaterial color="#eadfce" roughness={0.86} />
      </mesh>
    </group>
  );
});
