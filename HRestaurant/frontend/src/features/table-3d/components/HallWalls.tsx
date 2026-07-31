interface WallProps {
  position: [number, number, number];
  size: [number, number, number];
}

function Wall({ position, size }: WallProps) {
  return (
    <mesh receiveShadow position={position}>
      <boxGeometry args={size} />
      <meshStandardMaterial color="#c9baaa" roughness={0.92} />
    </mesh>
  );
}

export function HallWalls() {
  return (
    <group>
      <Wall position={[0, 2.5, -8.8]} size={[24, 5, 0.25]} />
      <Wall position={[-11.8, 2.5, 0]} size={[0.25, 5, 18]} />
      <Wall position={[11.8, 2.5, 0]} size={[0.25, 5, 18]} />
    </group>
  );
}
