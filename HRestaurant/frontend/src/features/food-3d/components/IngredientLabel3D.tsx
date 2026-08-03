import { Html } from "@react-three/drei";
import { memo } from "react";
import type { PublicIngredient3D } from "@/types/public";

interface IngredientLabel3DProps {
  ingredient: PublicIngredient3D;
  visible: boolean;
}

export const IngredientLabel3D = memo(function IngredientLabel3D({
  ingredient,
  visible,
}: IngredientLabel3DProps) {
  if (!visible) return null;

  return (
    <Html center position={[0, 0.82, 0]} distanceFactor={7} zIndexRange={[30, 10]}>
      <div className="pointer-events-none w-48 rounded-2xl border border-white/60 bg-[#211914]/95 p-3 text-left text-white shadow-xl">
        <p className="text-sm font-bold">{ingredient.name}</p>
        <p className="mt-1 text-xs text-white/75">
          {ingredient.requiredQuantity} {ingredient.unit}
        </p>
        <p className="mt-2 text-[11px] text-amber-200">
          {ingredient.allergenInformation
            ? `Allergen: ${ingredient.allergenInformation}`
            : "Allergen məlumatı göstərilməyib"}
        </p>
      </div>
    </Html>
  );
});
