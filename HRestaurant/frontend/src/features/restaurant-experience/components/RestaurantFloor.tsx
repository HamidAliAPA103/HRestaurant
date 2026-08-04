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
  const plankRows = Math.max(8, Math.ceil(depth / 0.7));
  const plankColumns = Math.max(5, Math.ceil(width / 1.8));

  return (
    <group position={[centerX, -0.08, centerZ]}>
      <mesh receiveShadow>
        <boxGeometry args={[width, 0.16, depth]} />
        <meshStandardMaterial color="#9a6a43" roughness={0.7} metalness={0.02} />
      </mesh>
      <mesh receiveShadow position={[0, 0.085, 0]}>
        <boxGeometry args={[width - 0.34, 0.018, depth - 0.34]} />
        <meshStandardMaterial color="#b98252" roughness={0.66} />
      </mesh>
      {Array.from({ length: plankRows - 1 }, (_, index) => {
        const z = -depth / 2 + ((index + 1) * depth) / plankRows;
        return (
          <mesh key={`row-${index}`} position={[0, 0.102, z]}>
            <boxGeometry args={[width - 0.38, 0.012, 0.018]} />
            <meshStandardMaterial color="#68442f" roughness={0.76} />
          </mesh>
        );
      })}
      {Array.from({ length: plankColumns - 1 }, (_, column) =>
        Array.from({ length: plankRows }, (_, row) => {
          const segmentDepth = depth / plankRows;
          const x = -width / 2 + ((column + 1) * width) / plankColumns;
          const stagger = row % 2 === 0 ? 0 : width / plankColumns / 2;
          const z = -depth / 2 + segmentDepth * (row + 0.5);
          return (
            <mesh key={`joint-${column}-${row}`} position={[x + stagger, 0.103, z]}>
              <boxGeometry args={[0.016, 0.012, segmentDepth - 0.04]} />
              <meshStandardMaterial color="#795137" roughness={0.78} />
            </mesh>
          );
        }),
      )}
      {[
        [0, -depth / 2 + 0.16, width - 0.12, 0.1],
        [0, depth / 2 - 0.16, width - 0.12, 0.1],
        [-width / 2 + 0.16, 0, 0.1, depth - 0.12],
        [width / 2 - 0.16, 0, 0.1, depth - 0.12],
      ].map(([x, z, railWidth, railDepth], index) => (
        <mesh key={`border-${index}`} position={[x, 0.11, z]}>
          <boxGeometry args={[railWidth, 0.025, railDepth]} />
          <meshStandardMaterial color="#3f2a21" roughness={0.58} metalness={0.08} />
        </mesh>
      ))}
    </group>
  );
});
