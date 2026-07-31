interface HallFloorProps {
  width?: number;
  length?: number;
}

export function HallFloor({ width = 24, length = 18 }: HallFloorProps) {
  return (
    <mesh receiveShadow rotation={[-Math.PI / 2, 0, 0]}>
      <planeGeometry args={[width, length]} />
      <meshStandardMaterial color="#d7cbbc" roughness={0.95} />
    </mesh>
  );
}
