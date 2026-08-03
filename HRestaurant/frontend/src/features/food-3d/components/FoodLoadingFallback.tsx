import { Html } from "@react-three/drei";
import { LoaderCircle } from "lucide-react";

interface FoodLoadingFallbackProps {
  inCanvas?: boolean;
  label?: string;
}

export function FoodLoadingFallback({
  inCanvas = false,
  label = "3D model yüklənir",
}: FoodLoadingFallbackProps) {
  const content = (
    <div
      className="flex min-w-52 items-center justify-center gap-3 rounded-full bg-white/95 px-5 py-3 text-sm font-semibold text-[#30261f] shadow-lg"
      aria-live="polite"
      role="status"
    >
      <LoaderCircle className="h-5 w-5 animate-spin text-[#b5422d]" aria-hidden />
      {label}
    </div>
  );

  return inCanvas ? <Html center>{content}</Html> : content;
}
