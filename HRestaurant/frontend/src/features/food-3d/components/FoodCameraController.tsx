import { OrbitControls } from "@react-three/drei";
import { useThree } from "@react-three/fiber";
import gsap from "gsap";
import { useReducedMotion } from "motion/react";
import { type ComponentRef, memo, useEffect, useMemo, useRef } from "react";
import { Vector3 } from "three";
import { getIngredientTransform } from "@/features/food-3d/lib/scene-utils";
import { useFoodViewerStore } from "@/features/food-3d/store/food-viewer-store";
import type { PublicIngredient3D } from "@/types/public";

interface FoodCameraControllerProps {
  ingredients: PublicIngredient3D[];
}

export const FoodCameraController = memo(function FoodCameraController({
  ingredients,
}: FoodCameraControllerProps) {
  const controlsRef = useRef<ComponentRef<typeof OrbitControls>>(null);
  const camera = useThree((state) => state.camera);
  const selectedIngredientId = useFoodViewerStore(
    (state) => state.selectedIngredientId,
  );
  const isExploded = useFoodViewerStore((state) => state.isExploded);
  const autoRotate = useFoodViewerStore((state) => state.autoRotate);
  const viewResetVersion = useFoodViewerStore((state) => state.viewResetVersion);
  const reducedMotion = useReducedMotion();
  const visibleIngredients = useMemo(
    () => ingredients.filter((ingredient) => ingredient.isVisibleIn3D),
    [ingredients],
  );

  useEffect(() => {
    const controls = controlsRef.current;
    if (!controls) return;

    const selectedIndex = visibleIngredients.findIndex(
      (ingredient) => ingredient.id === selectedIngredientId,
    );
    const selected = visibleIngredients[selectedIndex];
    const transform = selected
      ? getIngredientTransform(selected, selectedIndex, visibleIngredients.length)
      : null;
    const shouldFocusIngredient = Boolean(selected && isExploded && transform);
    const target = new Vector3(
      shouldFocusIngredient ? (transform?.position[0] ?? 0) : 0,
      shouldFocusIngredient ? (transform?.position[1] ?? 0.65) : 0.65,
      shouldFocusIngredient ? (transform?.position[2] ?? 0) : 0,
    );
    const destination = selected && isExploded
      ? target.clone().add(new Vector3(2.35, 1.65, 3.1))
      : new Vector3(4.8, 3.25, 5.7);

    if (reducedMotion) {
      camera.position.copy(destination);
      controls.target.copy(target);
      controls.update();
      return;
    }

    const timeline = gsap.timeline({
      defaults: {
        duration: 0.78,
        ease: "power3.inOut",
        overwrite: true,
        onUpdate: () => controls.update(),
      },
    });
    timeline
      .to(
        camera.position,
        { x: destination.x, y: destination.y, z: destination.z },
        0,
      )
      .to(
        controls.target,
        { x: target.x, y: target.y, z: target.z },
        0,
      );
    return () => {
      timeline.kill();
    };
  }, [camera, isExploded, reducedMotion, selectedIngredientId, viewResetVersion, visibleIngredients]);

  return (
    <OrbitControls
      ref={controlsRef}
      makeDefault
      enableDamping
      autoRotate={autoRotate && !reducedMotion}
      autoRotateSpeed={1.15}
      dampingFactor={0.08}
      enablePan={false}
      minDistance={2.2}
      maxDistance={10}
      minPolarAngle={0.35}
      maxPolarAngle={Math.PI / 2.05}
      target={[0, 0.65, 0]}
    />
  );
});
