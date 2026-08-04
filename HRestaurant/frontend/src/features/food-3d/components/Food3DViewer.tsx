import { AdaptiveDpr, useGLTF } from "@react-three/drei";
import { Canvas } from "@react-three/fiber";
import { Suspense, useEffect, useMemo, useState } from "react";
import { useReducedMotion } from "motion/react";
import { supportsWebGL } from "@/features/food-3d/lib/scene-utils";
import { useFoodViewerStore } from "@/features/food-3d/store/food-viewer-store";
import type { PublicFood3D, PublicIngredient3D } from "@/types/public";
import { FoodCameraController } from "./FoodCameraController";
import { Food3DFallback } from "./Food3DFallback";
import { FoodLighting } from "./FoodLighting";
import { FoodLoadingFallback } from "./FoodLoadingFallback";
import { FoodModel } from "./FoodModel";
import { FoodPlate } from "./FoodPlate";
import { FoodViewerControls } from "./FoodViewerControls";
import { IngredientExplodedView } from "./IngredientExplodedView";
import { SceneErrorBoundary } from "./SceneErrorBoundary";
import { detectPerformanceProfile } from "@/features/three-performance/performance-profile";

interface Food3DViewerProps {
  food: PublicFood3D;
  ingredients: PublicIngredient3D[];
}

export function Food3DViewer({ food, ingredients }: Food3DViewerProps) {
  const reducedMotion = Boolean(useReducedMotion());
  const profile = useMemo(() => detectPerformanceProfile(reducedMotion), [reducedMotion]);
  const [webGLAvailable] = useState(() => supportsWebGL());
  const [modelFailed, setModelFailed] = useState(false);
  const setAutoRotate = useFoodViewerStore((state) => state.setAutoRotate);
  const setSelectedIngredient = useFoodViewerStore(
    (state) => state.setSelectedIngredient,
  );
  const proceduralIngredients = useMemo(
    () =>
      ingredients.map((ingredient) => ({
        ...ingredient,
        model3DUrl: null,
        usesProceduralFallback: true,
      })),
    [ingredients],
  );
  const ingredientModelKey = useMemo(
    () => ingredients.map((ingredient) => ingredient.model3DUrl ?? "fallback").join("|"),
    [ingredients],
  );

  useEffect(() => {
    if (food.is3DEnabled && food.model3DUrl) useGLTF.preload(food.model3DUrl);
    ingredients.forEach((ingredient) => {
      if (ingredient.model3DUrl) useGLTF.preload(ingredient.model3DUrl);
    });
    return () => {
      if (food.model3DUrl) useGLTF.clear(food.model3DUrl);
      ingredients.forEach((ingredient) => {
        if (ingredient.model3DUrl) useGLTF.clear(ingredient.model3DUrl);
      });
    };
  }, [food.is3DEnabled, food.model3DUrl, ingredients]);

  useEffect(() => {
    setModelFailed(false);
    setAutoRotate(false);
  }, [food.model3DUrl, setAutoRotate]);

  if (!webGLAvailable) {
    return (
      <Food3DFallback
        food={food}
        title="3D görünüş dəstəklənmir"
        message="Brauzer və ya cihaz WebGL-i aktiv etməyib. Yeməyin statik görünüşü göstərilir."
      />
    );
  }

  if (modelFailed) {
    return (
      <Food3DFallback
        food={food}
        title="3D model yüklənmədi"
        message="Model faylı açıla bilmədi. Yeməyin poster və ya menyu şəkli göstərilir."
      />
    );
  }

  return (
    <div className="relative h-[clamp(28rem,66svh,38rem)] overflow-hidden rounded-[2rem] border border-[#e7ddd2] bg-[radial-gradient(circle_at_50%_28%,#fff8ed_0%,#ead9c8_55%,#c9ae97_100%)] shadow-[0_24px_80px_rgba(74,46,30,0.18)]">
      <Canvas
        aria-label={`${food.name} üçün interaktiv 3D görünüş. Fırlatmaq üçün sürüşdürün, yaxınlaşdırmaq üçün təkərdən istifadə edin.`}
        camera={{ position: [4.8, 3.25, 5.7], fov: 42, near: 0.1, far: 100 }}
        dpr={[profile.dpr[0], Math.min(profile.dpr[1], 1.5)]}
        shadows={profile.shadows}
        frameloop={profile.frameloop}
        gl={{
          antialias: true,
          alpha: true,
          powerPreference: "high-performance",
        }}
        fallback={<FoodLoadingFallback label="WebGL başladılmadı" />}
        onPointerMissed={() => setSelectedIngredient(null)}
      >
        <FoodLighting />
        <Suspense fallback={<FoodLoadingFallback inCanvas />}>
          <FoodPlate />
          <SceneErrorBoundary
            resetKey={food.model3DUrl ?? "procedural-food"}
            fallback={null}
            onError={() => setModelFailed(true)}
          >
            <FoodModel food={food} />
          </SceneErrorBoundary>
          <SceneErrorBoundary
            resetKey={ingredientModelKey}
            fallback={<IngredientExplodedView ingredients={proceduralIngredients} />}
          >
            <IngredientExplodedView ingredients={ingredients} />
          </SceneErrorBoundary>
        </Suspense>
        <FoodCameraController ingredients={ingredients} />
        <AdaptiveDpr pixelated />
      </Canvas>
      <div className="pointer-events-none absolute left-4 top-4 rounded-full bg-[#211914]/85 px-3 py-1.5 text-[11px] font-bold uppercase tracking-[0.14em] text-white backdrop-blur">
        {food.usesProceduralFallback
          ? "Stilizə edilmiş 3D təsvir"
          : "3D model"}
      </div>
      <span className="pointer-events-none absolute right-4 top-4 rounded-full bg-white/75 px-3 py-1 text-[10px] font-bold uppercase tracking-wider text-[#564b43]">{profile.level} profile</span>
      <FoodViewerControls reducedMotion={reducedMotion} />
    </div>
  );
}
