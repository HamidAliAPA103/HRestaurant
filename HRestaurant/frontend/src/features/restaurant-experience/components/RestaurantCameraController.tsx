import { OrbitControls } from "@react-three/drei";
import { useFrame, useThree } from "@react-three/fiber";
import gsap from "gsap";
import { type ComponentRef, memo, useEffect, useRef } from "react";
import { MathUtils } from "three";
import { useRestaurantExperienceStore } from "@/features/restaurant-experience/store/restaurant-experience-store";
import type { PublicBranchScene, PublicRestaurantTable } from "@/types/public";

interface RestaurantCameraControllerProps {
  scene: PublicBranchScene;
  tables: PublicRestaurantTable[];
  reducedMotion: boolean;
}

export const RestaurantCameraController = memo(function RestaurantCameraController({
  scene,
  tables,
  reducedMotion,
}: RestaurantCameraControllerProps) {
  const camera = useThree((state) => state.camera);
  const controlsRef = useRef<ComponentRef<typeof OrbitControls>>(null);
  const mode = useRestaurantExperienceStore((state) => state.mode);
  const tourStarted = useRestaurantExperienceStore((state) => state.tourStarted);
  const activeHotspotIndex = useRestaurantExperienceStore(
    (state) => state.activeHotspotIndex,
  );
  const selectedTableId = useRestaurantExperienceStore(
    (state) => state.selectedTableId,
  );
  const heroProgress = useRestaurantExperienceStore((state) => state.heroProgress);

  useEffect(() => {
    if (!tourStarted) return;
    const controls = controlsRef.current;
    if (!controls) return;
    const selectedTable = tables.find((table) => table.id === selectedTableId);
    const hotspot = scene.hotspots[activeHotspotIndex] ?? scene.hotspots[0];
    const targetX = selectedTable?.positionX ?? hotspot?.positionX ?? scene.centerX;
    const targetY = selectedTable ? selectedTable.height : hotspot?.positionY ?? 0.7;
    const targetZ = selectedTable?.positionZ ?? hotspot?.positionZ ?? scene.centerZ;
    const cameraX = selectedTable
      ? selectedTable.positionX + 3
      : hotspot?.cameraX ?? scene.centerX + 5;
    const cameraY = selectedTable ? 2.65 : hotspot?.cameraY ?? 3;
    const cameraZ = selectedTable
      ? selectedTable.positionZ + 3.2
      : hotspot?.cameraZ ?? scene.centerZ + 5;

    if (reducedMotion) {
      camera.position.set(cameraX, cameraY, cameraZ);
      controls.target.set(targetX, targetY, targetZ);
      controls.update();
      return;
    }

    const timeline = gsap.timeline({
      defaults: {
        duration: 1.05,
        ease: "power3.inOut",
        overwrite: true,
        onUpdate: () => controls.update(),
      },
    });
    timeline
      .to(camera.position, { x: cameraX, y: cameraY, z: cameraZ }, 0)
      .to(controls.target, { x: targetX, y: targetY, z: targetZ }, 0);
    return () => {
      timeline.kill();
    };
  }, [
    activeHotspotIndex,
    camera,
    reducedMotion,
    scene,
    selectedTableId,
    tables,
    tourStarted,
  ]);

  useFrame(({ pointer }) => {
    const controls = controlsRef.current;
    if (!controls) return;
    if (!tourStarted) {
      const progress = reducedMotion ? 1 : heroProgress;
      const entrance = scene.hotspots[0];
      const outsideX = scene.centerX + scene.floorWidth * 0.42;
      const outsideY = scene.wallHeight + 3.8;
      const outsideZ = scene.centerZ + scene.floorDepth * 0.9;
      const insideX = entrance?.cameraX ?? scene.centerX + 3;
      const insideY = entrance?.cameraY ?? 2.5;
      const insideZ = entrance?.cameraZ ?? scene.centerZ + 4;
      const parallaxX = reducedMotion ? 0 : pointer.x * 0.45;
      const parallaxY = reducedMotion ? 0 : pointer.y * 0.22;
      camera.position.x = MathUtils.lerp(outsideX, insideX, progress) + parallaxX;
      camera.position.y = MathUtils.lerp(outsideY, insideY, progress) + parallaxY;
      camera.position.z = MathUtils.lerp(outsideZ, insideZ, progress);
      controls.target.set(scene.centerX, 1, scene.centerZ);
      controls.update();
      return;
    }

    if (mode === "free") {
      const margin = 0.65;
      camera.position.x = MathUtils.clamp(
        camera.position.x,
        scene.centerX - scene.floorWidth / 2 + margin,
        scene.centerX + scene.floorWidth / 2 - margin,
      );
      camera.position.z = MathUtils.clamp(
        camera.position.z,
        scene.centerZ - scene.floorDepth / 2 + margin,
        scene.centerZ + scene.floorDepth / 2 - margin,
      );
      controls.target.x = MathUtils.clamp(
        controls.target.x,
        scene.centerX - scene.floorWidth / 2 + margin,
        scene.centerX + scene.floorWidth / 2 - margin,
      );
      controls.target.z = MathUtils.clamp(
        controls.target.z,
        scene.centerZ - scene.floorDepth / 2 + margin,
        scene.centerZ + scene.floorDepth / 2 - margin,
      );
    }
  });

  return (
    <OrbitControls
      ref={controlsRef}
      makeDefault
      enabled={tourStarted && mode === "free"}
      enableDamping
      dampingFactor={0.08}
      enablePan
      screenSpacePanning={false}
      minDistance={2.4}
      maxDistance={Math.max(scene.floorWidth, scene.floorDepth) * 0.72}
      minPolarAngle={0.38}
      maxPolarAngle={Math.PI / 2.08}
      minAzimuthAngle={-Math.PI}
      maxAzimuthAngle={Math.PI}
      touches={{ ONE: 0, TWO: 2 }}
      target={[scene.centerX, 0.8, scene.centerZ]}
    />
  );
});
