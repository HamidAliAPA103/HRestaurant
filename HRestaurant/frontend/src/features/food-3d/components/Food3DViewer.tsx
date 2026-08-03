import { AdaptiveDpr, useGLTF } from "@react-three/drei";
import { Canvas } from "@react-three/fiber";
import { Suspense, useEffect, useMemo, useState } from "react";
import { supportsWebGL } from "@/features/food-3d/lib/scene-utils";
import { useFoodViewerStore } from "@/features/food-3d/store/food-viewer-store";
import type { PublicFood3D, PublicIngredient3D } from "@/types/public";
import { FoodCameraController } from "./FoodCameraController";
import { FoodLighting } from "./FoodLighting";
import { FoodLoadingFallback } from "./FoodLoadingFallback";
import { FoodModel } from "./FoodModel";
import { FoodPlate } from "./FoodPlate";
import { IngredientExplodedView } from "./IngredientExplodedView";
import { SceneErrorBoundary } from "./SceneErrorBoundary";

interface Food3DViewerProps {
  food: PublicFood3D;
  ingredients: PublicIngredient3D[];
}

export function Food3DViewer({ food, ingredients }: Food3DViewerProps) {
  const [webGLAvailable] = useState(() => supportsWebGL());
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
  }, [food.is3DEnabled, food.model3DUrl, ingredients]);

  if (!webGLAvailable) {
    const fallbackImage = food.modelPosterUrl ?? food.imageUrl;
    return (
      <div className="relative grid min-h-[34rem] place-items-center overflow-hidden rounded-[2rem] bg-[#211914] text-center text-white">
        {fallbackImage && (
          <img
            src={fallbackImage}
            alt={`${food.name} üçün statik görünüş`}
            className="absolute inset-0 h-full w-full object-cover opacity-45"
          />
        )}
        <div className="relative z-10 max-w-md p-8">
          <h2 className="font-serif text-3xl">3D görünüş dəstəklənmir</h2>
          <p className="mt-3 text-sm leading-6 text-white/75">
            Brauzer və ya cihaz WebGL-i aktiv etməyib. Ingredient məlumatları
            aşağıdakı əlçatan HTML siyahısında tam təqdim olunur.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="relative min-h-[34rem] overflow-hidden rounded-[2rem] border border-[#e7ddd2] bg-[radial-gradient(circle_at_50%_28%,#fff8ed_0%,#ead9c8_55%,#c9ae97_100%)] shadow-[0_24px_80px_rgba(74,46,30,0.18)]">
      <Canvas
        aria-label={`${food.name} üçün interaktiv 3D görünüş. Fırlatmaq üçün sürüşdürün, yaxınlaşdırmaq üçün təkərdən istifadə edin.`}
        camera={{ position: [4.8, 3.25, 5.7], fov: 42, near: 0.1, far: 100 }}
        dpr={[1, 1.75]}
        shadows
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
            fallback={<FoodModel food={food} forceProcedural />}
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
      <p className="pointer-events-none absolute bottom-4 left-1/2 -translate-x-1/2 rounded-full bg-white/85 px-4 py-2 text-center text-xs font-medium text-[#564b43] shadow-sm backdrop-blur">
        Fırlatmaq üçün sürüşdürün · Zoom üçün təkərdən istifadə edin
      </p>
    </div>
  );
}
