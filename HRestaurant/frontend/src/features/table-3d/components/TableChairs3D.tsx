interface TableChairs3DProps {
  capacity: number;
  width: number;
  length: number;
}

export function TableChairs3D({
  capacity,
  width,
  length,
}: TableChairs3DProps) {
  const radiusX = Math.max(width / 2 + 0.72, 1.25);
  const radiusZ = Math.max(length / 2 + 0.72, 1.25);

  return (
    <group>
      {Array.from({ length: capacity }, (_, index) => {
        const angle = (index / capacity) * Math.PI * 2;
        const x = Math.cos(angle) * radiusX;
        const z = Math.sin(angle) * radiusZ;

        return (
          <group
            key={index}
            position={[x, -0.22, z]}
            rotation={[0, -angle + Math.PI / 2, 0]}
          >
            <mesh castShadow receiveShadow>
              <boxGeometry args={[0.52, 0.12, 0.52]} />
              <meshStandardMaterial color="#806b59" roughness={0.72} />
            </mesh>
            <mesh castShadow position={[0, 0.38, 0.22]}>
              <boxGeometry args={[0.52, 0.65, 0.1]} />
              <meshStandardMaterial color="#6f5c4d" roughness={0.76} />
            </mesh>
          </group>
        );
      })}
    </group>
  );
}
