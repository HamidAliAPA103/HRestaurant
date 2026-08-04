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
        <meshStandardMaterial color="#d8c7ad" roughness={0.82} />
      </mesh>
      <mesh castShadow receiveShadow position={[-width / 2, 0, 0]}>
        <boxGeometry args={[0.2, height, depth]} />
        <meshStandardMaterial color="#cbb99e" roughness={0.84} />
      </mesh>
      <mesh castShadow receiveShadow position={[width / 2, 0, 0]}>
        <boxGeometry args={[0.2, height, depth]} />
        <meshStandardMaterial color="#cbb99e" roughness={0.84} />
      </mesh>
      <mesh castShadow position={[-width * 0.34, 0, depth / 2]}>
        <boxGeometry args={[width * 0.3, height, 0.2]} />
        <meshStandardMaterial color="#d8c7ad" roughness={0.82} />
      </mesh>
      <mesh castShadow position={[width * 0.34, 0, depth / 2]}>
        <boxGeometry args={[width * 0.3, height, 0.2]} />
        <meshStandardMaterial color="#d8c7ad" roughness={0.82} />
      </mesh>
      <mesh receiveShadow position={[0, -height / 2 + 0.65, -depth / 2 + 0.12]}>
        <boxGeometry args={[width - 0.18, 1.25, 0.08]} />
        <meshStandardMaterial color="#3e2921" roughness={0.72} />
      </mesh>
      {Array.from({ length: 7 }, (_, index) => {
        const x = -width / 2 + (index * width) / 6;
        return (
          <mesh key={index} castShadow position={[x, -height / 2 + 0.65, -depth / 2 + 0.18]}>
            <boxGeometry args={[0.08, 1.2, 0.08]} />
            <meshStandardMaterial color="#b78a5b" roughness={0.5} />
          </mesh>
        );
      })}
      <mesh castShadow position={[0, -height / 2 + 1.32, -depth / 2 + 0.2]}>
        <boxGeometry args={[width, 0.12, 0.12]} />
        <meshStandardMaterial color="#b78a5b" roughness={0.48} />
      </mesh>
      <mesh position={[-width / 2 + 0.16, -height / 2 + 0.13, 0]}>
        <boxGeometry args={[0.12, 0.26, depth]} />
        <meshStandardMaterial color="#4a3025" roughness={0.68} />
      </mesh>
      <mesh position={[width / 2 - 0.16, -height / 2 + 0.13, 0]}>
        <boxGeometry args={[0.12, 0.26, depth]} />
        <meshStandardMaterial color="#4a3025" roughness={0.68} />
      </mesh>
      {[-0.28, 0, 0.28].map((offset, index) => (
        <group key={offset} position={[offset * width, 0.42, -depth / 2 + 0.18]}>
          <mesh castShadow>
            <boxGeometry args={[width * 0.2, height * 0.36, 0.1]} />
            <meshStandardMaterial color="#2e201b" roughness={0.52} metalness={0.12} />
          </mesh>
          <mesh position={[0, 0, 0.065]}>
            <boxGeometry args={[width * 0.17, height * 0.3, 0.035]} />
            <meshStandardMaterial
              color={index === 1 ? "#9d5138" : "#6d7656"}
              roughness={0.78}
            />
          </mesh>
        </group>
      ))}
    </group>
  );
});
