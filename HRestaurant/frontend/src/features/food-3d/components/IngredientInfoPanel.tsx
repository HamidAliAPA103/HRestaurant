import { AnimatePresence, motion } from "motion/react";
import { AlertTriangle, MapPin, Salad } from "lucide-react";
import { useMemo } from "react";
import { useFoodViewerStore } from "@/features/food-3d/store/food-viewer-store";
import type { PublicIngredient3D } from "@/types/public";

interface IngredientInfoPanelProps {
  ingredients: PublicIngredient3D[];
}

export function IngredientInfoPanel({ ingredients }: IngredientInfoPanelProps) {
  const hoveredIngredientId = useFoodViewerStore(
    (state) => state.hoveredIngredientId,
  );
  const selectedIngredientId = useFoodViewerStore(
    (state) => state.selectedIngredientId,
  );
  const ingredient = useMemo(
    () =>
      ingredients.find(
        (item) => item.id === (hoveredIngredientId ?? selectedIngredientId),
      ) ?? null,
    [hoveredIngredientId, ingredients, selectedIngredientId],
  );

  return (
    <div className="min-h-52 rounded-3xl border border-[#e7ddd2] bg-white p-5 shadow-sm">
      <AnimatePresence mode="wait">
        {ingredient ? (
          <motion.div
            key={ingredient.id}
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -6 }}
            transition={{ duration: 0.2 }}
          >
            <p className="text-xs font-bold uppercase tracking-[0.2em] text-[#b5422d]">
              Ingredient məlumatı
            </p>
            <h2 className="mt-2 font-serif text-3xl text-[#241b16]">
              {ingredient.name}
            </h2>
            <p className="mt-1 font-semibold text-[#6d5e53]">
              {ingredient.requiredQuantity} {ingredient.unit}
            </p>
            {ingredient.description && (
              <p className="mt-3 text-sm leading-6 text-[#746960]">
                {ingredient.description}
              </p>
            )}
            <div className="mt-4 grid grid-cols-2 gap-2 text-xs sm:grid-cols-4">
              <NutritionStat label="Kalori" value={ingredient.calories} suffix=" kcal" />
              <NutritionStat label="Protein" value={ingredient.protein} suffix=" g" />
              <NutritionStat label="Karbohidrat" value={ingredient.carbohydrates} suffix=" g" />
              <NutritionStat label="Yağ" value={ingredient.fat} suffix=" g" />
            </div>
            {ingredient.origin && (
              <p className="mt-4 flex items-center gap-2 text-sm text-[#6d5e53]">
                <MapPin className="h-4 w-4 text-[#b5422d]" aria-hidden />
                Mənşə: {ingredient.origin}
              </p>
            )}
            <p className="mt-3 flex items-start gap-2 rounded-xl bg-amber-50 p-3 text-xs text-amber-900">
              <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0" aria-hidden />
              {ingredient.allergenInformation
                ? `Allergen: ${ingredient.allergenInformation}`
                : "Allergen məlumatı təqdim edilməyib. Ətraflı məlumat üçün işçi heyətinə müraciət edin."}
            </p>
          </motion.div>
        ) : (
          <motion.div
            key="empty"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="flex min-h-40 flex-col items-center justify-center text-center text-[#746960]"
          >
            <Salad className="h-8 w-8 text-[#b5422d]" aria-hidden />
            <p className="mt-3 font-semibold text-[#30261f]">Ingredient seçin</p>
            <p className="mt-1 max-w-sm text-sm">
              3D modelin üzərinə gəlin və ya aşağıdakı əlçatan siyahıdan seçim edin.
            </p>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

function NutritionStat({
  label,
  value,
  suffix,
}: {
  label: string;
  value: number | null;
  suffix: string;
}) {
  return (
    <div className="rounded-xl bg-[#f7f2ec] p-2.5">
      <span className="block text-[#8b7b70]">{label}</span>
      <strong className="mt-1 block text-[#30261f]">
        {value === null ? "—" : `${value}${suffix}`}
      </strong>
    </div>
  );
}
