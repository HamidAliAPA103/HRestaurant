import { useQuery } from "@tanstack/react-query";
import { ArrowLeft, Clock3, Combine, Layers3, Rotate3D } from "lucide-react";
import { motion, useReducedMotion } from "motion/react";
import { useEffect } from "react";
import { Link, useParams } from "react-router-dom";
import {
  getPublicApiError,
  getPublicMenuItem3D,
  getPublicMenuItemIngredients3D,
} from "@/api/public-api";
import { AccessibleIngredientList } from "@/features/food-3d/components/AccessibleIngredientList";
import { Food3DViewer } from "@/features/food-3d/components/Food3DViewer";
import { FoodLoadingFallback } from "@/features/food-3d/components/FoodLoadingFallback";
import { IngredientInfoPanel } from "@/features/food-3d/components/IngredientInfoPanel";
import { IngredientLegend } from "@/features/food-3d/components/IngredientLegend";
import { useFoodViewerStore } from "@/features/food-3d/store/food-viewer-store";

const azn = new Intl.NumberFormat("az-AZ", {
  style: "currency",
  currency: "AZN",
});

export function FoodDetailPage() {
  const { restaurantSlug = "", menuItemId = "" } = useParams();
  const reducedMotion = useReducedMotion();
  const isExploded = useFoodViewerStore((state) => state.isExploded);
  const setExploded = useFoodViewerStore((state) => state.setExploded);
  const setSelectedIngredient = useFoodViewerStore(
    (state) => state.setSelectedIngredient,
  );
  const resetViewer = useFoodViewerStore((state) => state.reset);
  const foodQuery = useQuery({
    queryKey: ["public-menu-item-3d", menuItemId],
    queryFn: () => getPublicMenuItem3D(menuItemId),
    enabled: Boolean(menuItemId),
    staleTime: 5 * 60_000,
  });
  const ingredientsQuery = useQuery({
    queryKey: ["public-menu-item-ingredients-3d", menuItemId],
    queryFn: () => getPublicMenuItemIngredients3D(menuItemId),
    enabled: Boolean(menuItemId),
    staleTime: 5 * 60_000,
  });

  useEffect(() => {
    resetViewer();
    return resetViewer;
  }, [menuItemId, resetViewer]);

  useEffect(() => {
    if (foodQuery.data) document.title = `${foodQuery.data.name} · 3D menyu`;
  }, [foodQuery.data]);

  if (foodQuery.isLoading || ingredientsQuery.isLoading) {
    return (
      <div className="grid min-h-[70vh] place-items-center px-4">
        <FoodLoadingFallback label="Yemək və ingredientlər yüklənir" />
      </div>
    );
  }

  if (foodQuery.isError || ingredientsQuery.isError) {
    const error = getPublicApiError(foodQuery.error ?? ingredientsQuery.error);
    return (
      <div className="grid min-h-[70vh] place-items-center px-4 text-center">
        <div className="max-w-lg rounded-3xl border bg-white p-8 shadow-sm">
          <h1 className="font-serif text-4xl">3D menyu açıla bilmədi</h1>
          <p className="mt-3 text-[#746960]">{error.message}</p>
          {error.traceId && (
            <p className="mt-2 text-xs text-[#9a8d83]">TraceId: {error.traceId}</p>
          )}
          <button
            type="button"
            className="mt-6 rounded-full bg-[#b5422d] px-5 py-3 font-bold text-white"
            onClick={() => {
              void foodQuery.refetch();
              void ingredientsQuery.refetch();
            }}
          >
            Yenidən cəhd et
          </button>
        </div>
      </div>
    );
  }

  const food = foodQuery.data;
  const ingredients = ingredientsQuery.data ?? [];
  if (!food || food.restaurantSlug.toLowerCase() !== restaurantSlug.toLowerCase()) {
    return (
      <div className="grid min-h-[70vh] place-items-center px-4 text-center">
        <div>
          <h1 className="font-serif text-4xl">Yemək tapılmadı</h1>
          <Link
            to={`/restaurants/${restaurantSlug}/menu`}
            className="mt-5 inline-flex rounded-full bg-[#b5422d] px-5 py-3 font-bold text-white"
          >
            Menyuya qayıt
          </Link>
        </div>
      </div>
    );
  }

  const toggleExplodedView = () => {
    const nextValue = !isExploded;
    setExploded(nextValue);
    if (!nextValue) setSelectedIngredient(null);
  };

  return (
    <motion.main
      initial={reducedMotion ? false : { opacity: 0, y: 14 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: reducedMotion ? 0 : 0.4 }}
      className="mx-auto max-w-[90rem] px-4 py-10 sm:px-6 lg:px-8"
    >
      <Link
        to={`/restaurants/${restaurantSlug}/menu`}
        className="inline-flex items-center gap-2 text-sm font-semibold text-[#6d5e53] hover:text-[#b5422d]"
      >
        <ArrowLeft className="h-4 w-4" aria-hidden />
        Menyuya qayıt
      </Link>

      <div className="mt-6 grid gap-8 xl:grid-cols-[minmax(0,1.6fr)_minmax(20rem,0.75fr)]">
        <section className="mobile-3d-sheet">
          <Food3DViewer food={food} ingredients={ingredients} />
          <div className="mt-4 flex flex-wrap items-center justify-between gap-3 rounded-3xl border border-[#e7ddd2] bg-white p-4">
            <p className="flex items-center gap-2 text-sm text-[#6d5e53]">
              <Rotate3D className="h-5 w-5 text-[#b5422d]" aria-hidden />
              Modeli rotate və zoom edə bilərsiniz
            </p>
            <button
              type="button"
              disabled={ingredients.length === 0}
              aria-pressed={isExploded}
              onClick={toggleExplodedView}
              className="inline-flex items-center gap-2 rounded-full bg-[#b5422d] px-5 py-3 text-sm font-bold text-white shadow-lg shadow-[#b5422d]/20 transition hover:bg-[#963623] disabled:cursor-not-allowed disabled:opacity-45"
            >
              {isExploded ? (
                <Combine className="h-4 w-4" aria-hidden />
              ) : (
                <Layers3 className="h-4 w-4" aria-hidden />
              )}
              {isExploded ? "Yeməyi birləşdir" : "Ingredientləri göstər"}
            </button>
          </div>
        </section>

        <aside className="space-y-5">
          <div className="rounded-3xl border border-[#e7ddd2] bg-white p-6 shadow-sm">
            <p className="text-xs font-bold uppercase tracking-[0.22em] text-[#b5422d]">
              {food.categoryName}
            </p>
            <h1 className="mt-2 font-serif text-4xl text-[#241b16] sm:text-5xl">
              {food.name}
            </h1>
            <p className="mt-4 leading-7 text-[#746960]">{food.description}</p>
            <div className="mt-5 flex flex-wrap items-center gap-x-5 gap-y-2 border-t border-[#eee4db] pt-5">
              <strong className="text-2xl text-[#241b16]">
                {azn.format(food.finalPrice)}
              </strong>
              <span className="flex items-center gap-1.5 text-sm text-[#746960]">
                <Clock3 className="h-4 w-4" aria-hidden />
                {food.preparationTimeMinutes} dəq.
              </span>
              {!food.isAvailable && (
                <span className="rounded-full bg-red-50 px-3 py-1 text-xs font-bold text-red-700">
                  Hazırda mövcud deyil
                </span>
              )}
            </div>
            {food.nutrition && (
              <p className="mt-4 rounded-2xl bg-[#f7f2ec] p-4 text-sm text-[#66584e]">
                {food.nutrition}
              </p>
            )}
          </div>
          <IngredientInfoPanel ingredients={ingredients} />
          <IngredientLegend />
        </aside>
      </div>

      <div className="mt-10 rounded-3xl border border-[#e7ddd2] bg-[#fcfaf7] p-5 sm:p-7">
        <AccessibleIngredientList ingredients={ingredients} />
      </div>
    </motion.main>
  );
}
