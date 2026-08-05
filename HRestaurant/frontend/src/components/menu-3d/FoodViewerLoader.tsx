import { Html, useProgress } from "@react-three/drei";

export function FoodViewerLoader() {
  const { progress } = useProgress();
  return <Html center><div role="status" aria-live="polite" className="min-w-52 rounded-full bg-black/75 px-5 py-3 text-center text-sm font-semibold text-white backdrop-blur">Model yüklənir · {Math.round(progress)}%</div></Html>;
}
