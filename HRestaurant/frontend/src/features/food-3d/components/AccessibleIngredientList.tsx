import { useRef } from "react";
import { useFoodViewerStore } from "@/features/food-3d/store/food-viewer-store";
import type { PublicIngredient3D } from "@/types/public";

interface AccessibleIngredientListProps {
  ingredients: PublicIngredient3D[];
}

export function AccessibleIngredientList({
  ingredients,
}: AccessibleIngredientListProps) {
  const buttonRefs = useRef<Array<HTMLButtonElement | null>>([]);
  const selectedIngredientId = useFoodViewerStore(
    (state) => state.selectedIngredientId,
  );
  const setSelectedIngredient = useFoodViewerStore(
    (state) => state.setSelectedIngredient,
  );
  const setHoveredIngredient = useFoodViewerStore(
    (state) => state.setHoveredIngredient,
  );
  const setExploded = useFoodViewerStore((state) => state.setExploded);

  const focusAt = (index: number) => {
    const normalized = (index + ingredients.length) % ingredients.length;
    buttonRefs.current[normalized]?.focus();
  };

  return (
    <section aria-labelledby="ingredient-list-title">
      <div className="flex items-end justify-between gap-4">
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.2em] text-[#b5422d]">
            Əlçatan görünüş
          </p>
          <h2 id="ingredient-list-title" className="mt-1 font-serif text-3xl">
            Ingredientlər
          </h2>
        </div>
        <p className="hidden text-xs text-[#746960] sm:block">
          Ox düymələri ilə hərəkət edin
        </p>
      </div>
      {ingredients.length === 0 ? (
        <p className="mt-4 rounded-2xl border border-dashed p-5 text-sm text-[#746960]">
          Bu yemək üçün public ingredient məlumatı yoxdur.
        </p>
      ) : (
        <ul className="mt-4 grid gap-2 sm:grid-cols-2">
          {ingredients.map((ingredient, index) => (
            <li key={ingredient.id}>
              <button
                ref={(element) => {
                  buttonRefs.current[index] = element;
                }}
                type="button"
                aria-pressed={selectedIngredientId === ingredient.id}
                className="flex w-full items-center justify-between rounded-2xl border border-[#e7ddd2] bg-white px-4 py-3 text-left transition hover:border-[#b5422d] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[#b5422d] aria-pressed:border-[#b5422d] aria-pressed:bg-[#fff4ef]"
                onFocus={() => setHoveredIngredient(ingredient.id)}
                onBlur={() => setHoveredIngredient(null)}
                onMouseEnter={() => setHoveredIngredient(ingredient.id)}
                onMouseLeave={() => setHoveredIngredient(null)}
                onClick={() => {
                  setExploded(true);
                  setSelectedIngredient(ingredient.id);
                }}
                onKeyDown={(event) => {
                  if (event.key === "ArrowRight" || event.key === "ArrowDown") {
                    event.preventDefault();
                    focusAt(index + 1);
                  } else if (event.key === "ArrowLeft" || event.key === "ArrowUp") {
                    event.preventDefault();
                    focusAt(index - 1);
                  } else if (event.key === "Home") {
                    event.preventDefault();
                    focusAt(0);
                  } else if (event.key === "End") {
                    event.preventDefault();
                    focusAt(ingredients.length - 1);
                  }
                }}
              >
                <span>
                  <strong className="block text-[#30261f]">{ingredient.name}</strong>
                  <span className="mt-0.5 block text-xs text-[#746960]">
                    {ingredient.requiredQuantity} {ingredient.unit}
                  </span>
                </span>
                <span className="text-xs font-semibold text-[#a5422f]">
                  Fokusla
                </span>
              </button>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
