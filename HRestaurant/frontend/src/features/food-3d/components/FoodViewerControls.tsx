import { Pause, Play, RotateCcw } from "lucide-react";
import { useFoodViewerStore } from "@/features/food-3d/store/food-viewer-store";

interface FoodViewerControlsProps {
  reducedMotion: boolean;
}

export function FoodViewerControls({ reducedMotion }: FoodViewerControlsProps) {
  const autoRotate = useFoodViewerStore((state) => state.autoRotate);
  const setAutoRotate = useFoodViewerStore((state) => state.setAutoRotate);
  const resetView = useFoodViewerStore((state) => state.resetView);

  return (
    <div className="absolute bottom-4 left-1/2 flex -translate-x-1/2 flex-wrap items-center justify-center gap-2 rounded-2xl bg-white/88 p-2 shadow-lg backdrop-blur">
      <button
        type="button"
        onClick={resetView}
        className="inline-flex items-center gap-1.5 rounded-full px-3 py-2 text-xs font-bold text-[#493a31] transition hover:bg-white"
      >
        <RotateCcw className="h-4 w-4" aria-hidden />
        Görünüşü sıfırla
      </button>
      <button
        type="button"
        aria-pressed={autoRotate}
        disabled={reducedMotion}
        onClick={() => setAutoRotate(!autoRotate)}
        className="inline-flex items-center gap-1.5 rounded-full bg-[#211914] px-3 py-2 text-xs font-bold text-white transition hover:bg-[#4b362b] disabled:cursor-not-allowed disabled:opacity-45"
      >
        {autoRotate ? <Pause className="h-4 w-4" aria-hidden /> : <Play className="h-4 w-4" aria-hidden />}
        Avtomatik fırlanma
      </button>
    </div>
  );
}
