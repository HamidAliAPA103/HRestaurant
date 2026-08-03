import { memo } from "react";

interface TableChairs3DProps {
  capacity: number;
  width: number;
  length: number;
}

export const TableChairs3D = memo(function TableChairs3D({
  capacity,
  width,
  length,
}: TableChairs3DProps) {
  const chairCount = Math.min(Math.max(capacity, 1), 12);
  const radiusX = Math.max(width / 2 + 0.62, 1.05);
  const radiusZ = Math.max(length / 2 + 0.62, 1.05);

  return (
    <group>
      {Array.from({ length: chairCount }, (_, index) => {
        const angle = (index / chairCount) * Math.PI * 2;
        return (
          <group
            key={index}
            position={[Math.cos(angle) * radiusX, -0.25, Math.sin(angle) * radiusZ]}
            rotation={[0, -angle + Math.PI / 2, 0]}
          >
            <mesh castShadow receiveShadow>
              <boxGeometry args={[0.48, 0.11, 0.48]} />
              <meshStandardMaterial color="#6e5544" roughness={0.76} />
            </mesh>
            <mesh castShadow position={[0, 0.34, 0.2]}>
              <boxGeometry args={[0.48, 0.58, 0.09]} />
              <meshStandardMaterial color="#5a4437" roughness={0.8} />
            </mesh>
          </group>
        );
      })}
    </group>
  );
});
