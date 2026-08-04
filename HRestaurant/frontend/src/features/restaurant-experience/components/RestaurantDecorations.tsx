import { useFrame } from "@react-three/fiber";
import { memo, useMemo, useRef } from "react";
import { BufferAttribute, BufferGeometry, type Points } from "three";

interface RestaurantDecorationsProps {
  width: number;
  depth: number;
  wallHeight: number;
  centerX: number;
  centerZ: number;
  reducedMotion: boolean;
}

export const RestaurantDecorations = memo(function RestaurantDecorations({
  width,
  depth,
  wallHeight,
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
      <group position={[centerX + width * 0.34, 0, centerZ + depth * 0.28]}>
        <mesh castShadow receiveShadow position={[0, 0.55, 0]}>
          <boxGeometry args={[width * 0.24, 1.1, 1.05]} />
          <meshStandardMaterial color="#3b241d" roughness={0.68} />
        </mesh>
        {Array.from({ length: 5 }, (_, index) => (
          <mesh
            key={index}
            position={[-width * 0.09 + index * width * 0.045, 0.54, -0.535]}
          >
            <boxGeometry args={[0.055, 0.92, 0.035]} />
            <meshStandardMaterial color="#b27a4d" roughness={0.48} />
          </mesh>
        ))}
        <mesh castShadow position={[0, 1.16, 0]}>
          <boxGeometry args={[width * 0.26, 0.11, 1.2]} />
          <meshStandardMaterial color="#b98558" roughness={0.4} metalness={0.06} />
        </mesh>
        <mesh position={[0, 0.18, -0.62]} rotation={[0, 0, Math.PI / 2]}>
          <cylinderGeometry args={[0.035, 0.035, width * 0.21, 12]} />
          <meshStandardMaterial color="#c8a35c" metalness={0.72} roughness={0.26} />
        </mesh>
      </group>
      <group position={[centerX - width * 0.33, 0, centerZ - depth * 0.35]}>
        <mesh castShadow position={[0, 0.43, 0]}>
          <boxGeometry args={[width * 0.3, 0.86, 0.72]} />
          <meshStandardMaterial color="#62372d" roughness={0.66} />
        </mesh>
        <mesh castShadow position={[0, 0.88, 0.26]} rotation={[-0.08, 0, 0]}>
          <boxGeometry args={[width * 0.3, 0.68, 0.18]} />
          <meshStandardMaterial color="#874737" roughness={0.62} />
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
          {[-0.26, 0, 0.25].map((x, leafIndex) => (
            <mesh
              key={x}
              castShadow
              position={[x, 0.98 + leafIndex * 0.08, 0]}
              scale={[0.62, 0.9, 0.62]}
            >
              <sphereGeometry args={[0.58, 18, 14]} />
              <meshStandardMaterial
                color={leafIndex === 1 ? "#315d3c" : "#47784b"}
                roughness={0.88}
              />
            </mesh>
          ))}
        </group>
      ))}
      {[-0.27, 0, 0.27].map((xOffset) => (
        <group
          key={`pendant-${xOffset}`}
          position={[centerX + width * xOffset, wallHeight - 0.62, centerZ - depth * 0.05]}
        >
          <mesh position={[0, 0.52, 0]}>
            <cylinderGeometry args={[0.018, 0.018, 1.05, 8]} />
            <meshStandardMaterial color="#211713" metalness={0.45} roughness={0.42} />
          </mesh>
          <mesh castShadow rotation={[Math.PI, 0, 0]}>
            <coneGeometry args={[0.34, 0.36, 28, 1, true]} />
            <meshStandardMaterial color="#34231d" roughness={0.48} metalness={0.18} side={2} />
          </mesh>
          <mesh position={[0, -0.08, 0]}>
            <sphereGeometry args={[0.105, 16, 12]} />
            <meshStandardMaterial
              color="#fff0bd"
              emissive="#ffb257"
              emissiveIntensity={2.1}
              roughness={0.22}
            />
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
