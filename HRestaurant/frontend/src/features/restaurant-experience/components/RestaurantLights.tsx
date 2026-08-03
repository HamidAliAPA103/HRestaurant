import { useFrame } from "@react-three/fiber";
import { memo, useRef } from "react";
import type { PointLight } from "three";

interface RestaurantLightsProps {
  centerX: number;
  centerZ: number;
  reducedMotion: boolean;
}

export const RestaurantLights = memo(function RestaurantLights({
  centerX,
  centerZ,
  reducedMotion,
}: RestaurantLightsProps) {
  const accentRef = useRef<PointLight>(null);
  useFrame(({ clock }) => {
    if (!accentRef.current || reducedMotion) return;
    accentRef.current.intensity = 11 + Math.sin(clock.elapsedTime * 0.8) * 1.5;
  });

  return (
    <>
      <ambientLight intensity={0.72} />
      <hemisphereLight args={["#fff2da", "#49372d", 0.9]} />
      <directionalLight
        castShadow
        position={[centerX + 7, 12, centerZ + 6]}
        intensity={2.1}
        color="#fff0d7"
        shadow-mapSize={[1024, 1024]}
      />
      <pointLight
        ref={accentRef}
        position={[centerX - 3, 3.2, centerZ]}
        intensity={11}
        distance={9}
        color="#ff9863"
      />
      <pointLight
        position={[centerX + 3, 3.1, centerZ - 2]}
        intensity={8}
        distance={8}
        color="#ffd89a"
      />
    </>
  );
});
