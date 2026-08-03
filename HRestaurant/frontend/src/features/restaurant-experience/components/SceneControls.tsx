import { ChevronLeft, ChevronRight, Compass, Route, Utensils } from "lucide-react";
import { useRestaurantExperienceStore } from "@/features/restaurant-experience/store/restaurant-experience-store";
import type { PublicBranchScene, PublicRestaurantTable } from "@/types/public";

interface SceneControlsProps {
  scene: PublicBranchScene;
  tables: PublicRestaurantTable[];
  onChooseInZone: () => void;
}

export function SceneControls({
  scene,
  tables,
  onChooseInZone,
}: SceneControlsProps) {
  const mode = useRestaurantExperienceStore((state) => state.mode);
  const tourStarted = useRestaurantExperienceStore((state) => state.tourStarted);
  const activeHotspotIndex = useRestaurantExperienceStore(
    (state) => state.activeHotspotIndex,
  );
  const setMode = useRestaurantExperienceStore((state) => state.setMode);
  const startTour = useRestaurantExperienceStore((state) => state.startTour);
  const setActiveHotspotIndex = useRestaurantExperienceStore(
    (state) => state.setActiveHotspotIndex,
  );
  const hotspot = scene.hotspots[activeHotspotIndex] ?? scene.hotspots[0];
  const availableCount = hotspot
    ? tables.filter(
        (table) => hotspot.tableIds.includes(table.id) && table.isAvailable,
      ).length
    : 0;

  const move = (direction: number) => {
    const next =
      (activeHotspotIndex + direction + scene.hotspots.length) %
      scene.hotspots.length;
    setActiveHotspotIndex(next);
  };

  return (
    <div className="absolute inset-x-4 bottom-4 z-10 rounded-3xl border border-white/15 bg-[#211914]/92 p-3 text-white shadow-2xl backdrop-blur-xl lg:left-auto lg:w-[30rem] lg:p-4">
      <div className="flex gap-2" role="group" aria-label="Tur rejimi">
        <button
          type="button"
          aria-pressed={mode === "guided"}
          onClick={() => {
            setMode("guided");
            startTour("guided");
          }}
          className="inline-flex flex-1 items-center justify-center gap-2 rounded-full border border-white/15 px-3 py-2 text-xs font-bold aria-pressed:bg-[#ef6542]"
        >
          <Route className="h-4 w-4" aria-hidden /> Guided Tour
        </button>
        <button
          type="button"
          aria-pressed={mode === "free"}
          onClick={() => {
            setMode("free");
            startTour("free");
          }}
          className="inline-flex flex-1 items-center justify-center gap-2 rounded-full border border-white/15 px-3 py-2 text-xs font-bold aria-pressed:bg-[#ef6542]"
        >
          <Compass className="h-4 w-4" aria-hidden /> Free Explore
        </button>
      </div>

      {tourStarted && mode === "guided" && hotspot && (
        <div className="mt-3 border-t border-white/10 pt-3">
          <div className="flex items-start justify-between gap-3">
            <div>
              <p className="text-[10px] font-bold uppercase tracking-[0.16em] text-[#f4a17e]">
                {activeHotspotIndex + 1}/{scene.hotspots.length} · Guided Tour
              </p>
              <h2 className="mt-1 font-serif text-xl">{hotspot.name}</h2>
              <p className="mt-1 text-xs leading-5 text-white/65">{hotspot.description}</p>
              <p className="mt-2 text-xs font-bold text-emerald-300">
                Bu zonada {availableCount} mövcud masa
              </p>
            </div>
            <div className="flex gap-1">
              <button
                type="button"
                aria-label="Əvvəlki zona"
                onClick={() => move(-1)}
                className="grid h-9 w-9 place-items-center rounded-full border border-white/15"
              >
                <ChevronLeft className="h-4 w-4" aria-hidden />
              </button>
              <button
                type="button"
                aria-label="Növbəti zona"
                onClick={() => move(1)}
                className="grid h-9 w-9 place-items-center rounded-full border border-white/15"
              >
                <ChevronRight className="h-4 w-4" aria-hidden />
              </button>
            </div>
          </div>
          <button
            type="button"
            disabled={availableCount === 0}
            onClick={onChooseInZone}
            className="mt-3 inline-flex w-full items-center justify-center gap-2 rounded-full bg-white px-4 py-2.5 text-xs font-bold text-[#30261f] disabled:cursor-not-allowed disabled:opacity-45"
          >
            <Utensils className="h-4 w-4" aria-hidden />
            Bu zonada masa seç
          </button>
        </div>
      )}

      {tourStarted && mode === "free" && (
        <p className="mt-3 border-t border-white/10 pt-3 text-xs leading-5 text-white/65">
          Sürüşdürərək fırladın, iki barmaq və ya siçan təkəri ilə zoom edin.
          Kamera restoran sərhədləri daxilində saxlanılır.
        </p>
      )}
    </div>
  );
}
