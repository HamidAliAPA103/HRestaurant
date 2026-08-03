import { LoaderCircle } from "lucide-react";

export function SceneLoadingScreen() {
  return (
    <div
      className="grid min-h-[38rem] place-items-center rounded-[2rem] bg-[#211914] text-white"
      role="status"
      aria-live="polite"
    >
      <div className="text-center">
        <LoaderCircle className="mx-auto h-8 w-8 animate-spin text-[#ef7657]" aria-hidden />
        <p className="mt-3 font-semibold">Restoran səhnəsi hazırlanır</p>
        <p className="mt-1 text-sm text-white/55">Real masa planı yüklənir</p>
      </div>
    </div>
  );
}
