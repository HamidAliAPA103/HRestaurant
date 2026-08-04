import { memo } from "react";
import type { PublicBranchScene } from "@/types/public";
import { RestaurantDecorations } from "./RestaurantDecorations";
import { RestaurantFloor } from "./RestaurantFloor";
import { RestaurantLights } from "./RestaurantLights";
import { RestaurantWalls } from "./RestaurantWalls";

interface RestaurantInterior3DProps {
  scene: PublicBranchScene;
  reducedMotion: boolean;
}

export const RestaurantInterior3D = memo(function RestaurantInterior3D({
  scene,
  reducedMotion,
}: RestaurantInterior3DProps) {
  return (
    <>
      <RestaurantFloor
        width={scene.floorWidth}
        depth={scene.floorDepth}
        centerX={scene.centerX}
        centerZ={scene.centerZ}
      />
      <RestaurantWalls
        width={scene.floorWidth}
        depth={scene.floorDepth}
        height={scene.wallHeight}
        centerX={scene.centerX}
        centerZ={scene.centerZ}
      />
      <RestaurantLights
        centerX={scene.centerX}
        centerZ={scene.centerZ}
        width={scene.floorWidth}
        depth={scene.floorDepth}
        reducedMotion={reducedMotion}
      />
      <RestaurantDecorations
        width={scene.floorWidth}
        depth={scene.floorDepth}
        wallHeight={scene.wallHeight}
        centerX={scene.centerX}
        centerZ={scene.centerZ}
        reducedMotion={reducedMotion}
      />
    </>
  );
});
