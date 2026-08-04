import { ImageOff } from "lucide-react";
import type { PublicFood3D } from "@/types/public";

interface Food3DFallbackProps {
  food: PublicFood3D;
  title: string;
  message: string;
}

export function Food3DFallback({ food, title, message }: Food3DFallbackProps) {
  const imageUrl = food.modelPosterUrl ?? food.imageUrl;

  return (
    <div className="relative grid h-[clamp(28rem,66svh,38rem)] place-items-center overflow-hidden rounded-[2rem] border border-[#e7ddd2] bg-[#211914] text-center text-white shadow-[0_24px_80px_rgba(74,46,30,0.18)]">
      {imageUrl ? (
        <img
          src={imageUrl}
          alt={`${food.name} üçün statik görünüş`}
          className="absolute inset-0 h-full w-full object-cover"
        />
      ) : (
        <ImageOff className="h-16 w-16 text-white/25" aria-hidden />
      )}
      <div className="absolute inset-0 bg-gradient-to-t from-black/78 via-black/18 to-black/10" />
      <div className="relative z-10 mt-auto max-w-xl p-7 sm:p-10">
        <h2 className="font-serif text-3xl">{title}</h2>
        <p className="mt-3 text-sm leading-6 text-white/78">{message}</p>
      </div>
    </div>
  );
}
