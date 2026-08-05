import { Expand, Pause, Play, RotateCcw } from "lucide-react";

export function FoodViewerControls({ autoRotate, reducedMotion, onAutoRotate, onReset, onFullscreen }: { autoRotate: boolean; reducedMotion: boolean; onAutoRotate: () => void; onReset: () => void; onFullscreen: () => void }) {
  const button = "inline-flex items-center gap-1.5 rounded-full bg-black/70 px-3 py-2 text-xs font-bold text-white backdrop-blur transition hover:bg-black focus-visible:outline focus-visible:outline-2 focus-visible:outline-amber-300";
  return <div className="absolute bottom-4 left-1/2 flex max-w-[calc(100%-2rem)] -translate-x-1/2 flex-wrap justify-center gap-2"><button type="button" className={button} onClick={onReset} aria-label="Kameranı sıfırla"><RotateCcw className="h-4 w-4" />Kameranı sıfırla</button><button type="button" className={button} onClick={onAutoRotate} disabled={reducedMotion} aria-pressed={autoRotate} aria-label="Avtomatik fırlanmanı dəyiş">{autoRotate ? <Pause className="h-4 w-4" /> : <Play className="h-4 w-4" />}Avtomatik</button><button type="button" className={button} onClick={onFullscreen} aria-label="Tam ekran"><Expand className="h-4 w-4" />Tam ekran</button></div>;
}
