import { useFrame } from "@react-three/fiber";
import { memo, useMemo, useRef } from "react";
import { BufferAttribute, BufferGeometry, type Points } from "three";

interface RestaurantDecorationsProps {
  width: number;
  depth: number;
  centerX: number;
  centerZ: number;
  reducedMotion: boolean;
}

export const RestaurantDecorations = memo(function RestaurantDecorations({
  width,
  depth,
  centerX,
  centerZ,
  reducedMotion,
}: RestaurantDecorationsProps) {
  const particlesRef = useRef<Points>(null);
  const particleGeometry = useMemo(() => {
    const positions = new Float32Array(72 * 3);
    for (let index = 0; index < 72; index += 1) {
      const angle = index * 2.399963;
      const radius = 1 + (index % 11) * 0.42;
      positions[index * 3] = centerX + Math.cos(angle) * Math.min(radius, width / 2);
      positions[index * 3 + 1] = 1.2 + (index % 9) * 0.31;
      positions[index * 3 + 2] = centerZ + Math.sin(angle) * Math.min(radius, depth / 2);
    }
    const geometry = new BufferGeometry();
    geometry.setAttribute("position", new BufferAttribute(positions, 3));
    return geometry;
  }, [centerX, centerZ, depth, width]);

  useFrame((_, delta) => {
    if (!particlesRef.current || reducedMotion) return;
    particlesRef.current.rotation.y += delta * 0.018;
  });

  return (
    <group>
      <group position={[centerX + width * 0.34, 0, centerZ + depth * 0.27]}>
        <mesh castShadow position={[0, 0.55, 0]}>
          <boxGeometry args={[width * 0.22, 1.1, 1.1]} />
          <meshStandardMaterial color="#5a3525" roughness={0.74} />
        </mesh>
        <mesh position={[0, 1.16, 0]}>
          <boxGeometry args={[width * 0.24, 0.1, 1.2]} />
          <meshStandardMaterial color="#c18a5e" roughness={0.46} />
        </mesh>
      </group>
      <group position={[centerX - width * 0.33, 0, centerZ - depth * 0.35]}>
        <mesh castShadow position={[0, 0.72, 0]}>
          <boxGeometry args={[width * 0.3, 1.44, 0.85]} />
          <meshStandardMaterial color="#80766c" metalness={0.18} roughness={0.5} />
        </mesh>
      </group>
      {[-1, 1].map((side) => (
        <group
          key={side}
          position={[centerX + side * width * 0.4, 0, centerZ - depth * 0.38]}
        >
          <mesh castShadow position={[0, 0.32, 0]}>
            <cylinderGeometry args={[0.34, 0.28, 0.64, 18]} />
            <meshStandardMaterial color="#9a6545" roughness={0.8} />
          </mesh>
          <mesh castShadow position={[0, 1.05, 0]}>
            <sphereGeometry args={[0.65, 18, 14]} />
            <meshStandardMaterial color="#416b42" roughness={0.9} />
          </mesh>
        </group>
      ))}
      <points ref={particlesRef} geometry={particleGeometry}>
        <pointsMaterial
          color="#ffd5a3"
          size={0.035}
          transparent
          opacity={0.42}
          depthWrite={false}
        />
      </points>
    </group>
  );
});
