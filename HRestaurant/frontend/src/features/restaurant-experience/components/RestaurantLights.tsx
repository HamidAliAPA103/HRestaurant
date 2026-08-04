import { useFrame } from "@react-three/fiber";
import { memo, useRef } from "react";
import type { PointLight } from "three";

interface RestaurantLightsProps {
  centerX: number;
  centerZ: number;
  width: number;
  depth: number;
  reducedMotion: boolean;
}

export const RestaurantLights = memo(function RestaurantLights({
  centerX,
  centerZ,
  width,
  depth,
  reducedMotion,
}: RestaurantLightsProps) {
  const accentRef = useRef<PointLight>(null);
  useFrame(({ clock }) => {
    if (!accentRef.current || reducedMotion) return;
    accentRef.current.intensity = 5.4 + Math.sin(clock.elapsedTime * 0.8) * 0.35;
  });

  return (
    <>
      <ambientLight intensity={0.42} />
      <hemisphereLight args={["#fff0d2", "#35251f", 0.72]} />
      <directionalLight
        castShadow
        position={[centerX + 7, 12, centerZ + 6]}
        intensity={2.6}
        color="#fff0d7"
        shadow-mapSize={[1024, 1024]}
        shadow-bias={-0.0004}
      />
      <pointLight
        ref={accentRef}
        position={[centerX - width * 0.23, 2.8, centerZ + depth * 0.1]}
        intensity={5.4}
        distance={7}
        color="#ffad73"
      />
      <pointLight
        position={[centerX + width * 0.24, 2.9, centerZ - depth * 0.18]}
        intensity={5.2}
        distance={7}
        color="#ffd39a"
      />
      <pointLight position={[centerX, 2.7, centerZ]} intensity={3.8} distance={6} color="#ffe4b7" />
    </>
  );
});
