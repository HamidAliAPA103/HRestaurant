import { AdaptiveDpr } from "@react-three/drei";
import { Canvas } from "@react-three/fiber";
import { Suspense, useMemo, useState } from "react";
import { ACESFilmicToneMapping, PCFSoftShadowMap } from "three";
import { supportsWebGL } from "@/features/food-3d/lib/scene-utils";
import { useRestaurantExperienceStore } from "@/features/restaurant-experience/store/restaurant-experience-store";
import type { PublicBranchScene, PublicRestaurantTable } from "@/types/public";
import { RestaurantBuilding3D } from "./RestaurantBuilding3D";
import { RestaurantCameraController } from "./RestaurantCameraController";
import { RestaurantHotspot } from "./RestaurantHotspot";
import { RestaurantInterior3D } from "./RestaurantInterior3D";
import { RestaurantMiniMap } from "./RestaurantMiniMap";
import { RestaurantTablesLayer } from "./RestaurantTablesLayer";
import { SceneControls } from "./SceneControls";
import { SceneLoadingScreen } from "./SceneLoadingScreen";
import { detectPerformanceProfile } from "@/features/three-performance/performance-profile";

interface RestaurantSceneProps {
  scene: PublicBranchScene;
  tables: PublicRestaurantTable[];
  reducedMotion: boolean;
}

export function RestaurantScene({
  scene,
  tables,
  reducedMotion,
}: RestaurantSceneProps) {
  const profile = useMemo(() => detectPerformanceProfile(reducedMotion), [reducedMotion]);
  const [webGLAvailable] = useState(() => supportsWebGL());
  const mode = useRestaurantExperienceStore((state) => state.mode);
  const tourStarted = useRestaurantExperienceStore((state) => state.tourStarted);
  const activeHotspotIndex = useRestaurantExperienceStore(
    (state) => state.activeHotspotIndex,
  );
  const selectedTableId = useRestaurantExperienceStore(
    (state) => state.selectedTableId,
  );
  const setActiveHotspotIndex = useRestaurantExperienceStore(
    (state) => state.setActiveHotspotIndex,
  );
  const setSelectedTableId = useRestaurantExperienceStore(
    (state) => state.setSelectedTableId,
  );

  const chooseInZone = () => {
    const hotspot = scene.hotspots[activeHotspotIndex];
    const table = tables.find(
      (candidate) =>
        candidate.isAvailable && hotspot?.tableIds.includes(candidate.id),
    );
    if (table) setSelectedTableId(table.id);
  };

  if (!webGLAvailable) {
    return (
      <div className="relative">
        <div className="grid min-h-[38rem] place-items-center rounded-[2rem] bg-[#211914] p-8 text-center text-white">
          <div className="max-w-md">
            <h2 className="font-serif text-3xl">3D tur dəstəklənmir</h2>
            <p className="mt-3 text-sm leading-6 text-white/65">
              Brauzer WebGL-i aktiv etməyib. Masa seçimi aşağıdakı əlçatan siyahıda işləməyə davam edir.
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="relative h-[clamp(36rem,78svh,50rem)] overflow-hidden rounded-[2rem] border border-white/10 bg-[#17110e] shadow-[0_35px_100px_rgba(34,20,13,0.32)]">
      <Canvas
        aria-label={`${scene.branchName} filialının interaktiv 3D virtual turu`}
        camera={{
          position: [
            scene.centerX + scene.floorWidth * 0.3,
            scene.wallHeight + 1.9,
            scene.centerZ + scene.floorDepth * 0.48,
          ],
          fov: 46,
          near: 0.1,
          far: 150,
        }}
        dpr={profile.dpr}
        shadows={profile.shadows}
        frameloop={profile.frameloop}
        gl={{ antialias: true, alpha: false, powerPreference: "high-performance" }}
        onCreated={({ gl }) => {
          gl.toneMapping = ACESFilmicToneMapping;
          gl.toneMappingExposure = 1.08;
          gl.shadowMap.type = PCFSoftShadowMap;
        }}
        fallback={<SceneLoadingScreen />}
        onPointerMissed={() => setSelectedTableId(null)}
      >
        <color attach="background" args={["#140e0c"]} />
        <fog attach="fog" args={["#1b130f", 22, 62]} />
        <Suspense fallback={null}>
          <RestaurantBuilding3D
            width={scene.floorWidth}
            depth={scene.floorDepth}
            height={scene.wallHeight}
            centerX={scene.centerX}
            centerZ={scene.centerZ}
          />
          <RestaurantInterior3D scene={scene} reducedMotion={reducedMotion} />
          <RestaurantTablesLayer
            tables={tables}
            reducedMotion={reducedMotion}
            onSelect={(table) => setSelectedTableId(table.id)}
          />
          {scene.hotspots.map((hotspot, index) => (
            <RestaurantHotspot
              key={hotspot.key}
              hotspot={hotspot}
              active={activeHotspotIndex === index}
              visible={tourStarted && mode === "guided"}
              onSelect={() => setActiveHotspotIndex(index)}
            />
          ))}
        </Suspense>
        <RestaurantCameraController
          scene={scene}
          tables={tables}
          reducedMotion={reducedMotion}
        />
        <AdaptiveDpr pixelated />
      </Canvas>
      <RestaurantMiniMap
        scene={scene}
        tables={tables}
        activeHotspotIndex={activeHotspotIndex}
        selectedTableId={selectedTableId}
        onHotspotSelect={setActiveHotspotIndex}
      />
      <SceneControls scene={scene} tables={tables} onChooseInZone={chooseInZone} />
      {!tourStarted && (
        <div className="pointer-events-none absolute left-4 top-4 rounded-full bg-black/45 px-3 py-1.5 text-[10px] font-bold uppercase tracking-[0.16em] text-white backdrop-blur">
          Canlı 3D virtual tur
        </div>
      )}
    </div>
  );
}
