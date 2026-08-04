import { memo } from "react";

interface TableChairs3DProps {
  capacity: number;
  width: number;
  length: number;
  tableHeight: number;
}

export const TableChairs3D = memo(function TableChairs3D({
  capacity,
  width,
  length,
  tableHeight,
}: TableChairs3DProps) {
  const chairCount = Math.min(Math.max(capacity, 1), 12);
  const radiusX = Math.max(width / 2 + 0.62, 1.05);
  const radiusZ = Math.max(length / 2 + 0.62, 1.05);
  const seatY = 0.48 - tableHeight;

  return (
    <group>
      {Array.from({ length: chairCount }, (_, index) => {
        const angle = (index / chairCount) * Math.PI * 2;
        return (
          <group
            key={index}
            position={[Math.cos(angle) * radiusX, seatY, Math.sin(angle) * radiusZ]}
            rotation={[0, -angle + Math.PI / 2, 0]}
          >
            <mesh castShadow receiveShadow position={[0, 0, 0]}>
              <boxGeometry args={[0.5, 0.12, 0.48]} />
              <meshStandardMaterial color="#8f4f36" roughness={0.62} />
            </mesh>
            <mesh castShadow position={[0, 0.34, 0.21]} rotation={[-0.08, 0, 0]}>
              <boxGeometry args={[0.5, 0.58, 0.09]} />
              <meshStandardMaterial color="#78402f" roughness={0.68} />
            </mesh>
            {[-0.19, 0.19].flatMap((x) =>
              [-0.17, 0.17].map((z) => (
                <mesh key={`${x}-${z}`} castShadow position={[x, -0.27, z]}>
                  <cylinderGeometry args={[0.025, 0.035, 0.48, 8]} />
                  <meshStandardMaterial color="#35251f" roughness={0.72} metalness={0.08} />
                </mesh>
              )),
            )}
          </group>
        );
      })}
    </group>
  );
});
