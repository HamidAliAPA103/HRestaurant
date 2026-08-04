import { UtensilsCrossed } from "lucide-react";
import type { PublicMenuItem } from "@/types/public";

export function MenuVideoFallback({ item }: { item: PublicMenuItem }) {
  return <div className="relative aspect-video overflow-hidden bg-[#30251f]">{item.imageUrl ? <img loading="lazy" src={item.imageUrl} alt={item.name} className="h-full w-full object-cover" /> : <div className="grid h-full place-items-center"><UtensilsCrossed className="h-10 w-10 text-[#d8c9b6]" aria-hidden /></div>}<div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent" /></div>;
}

export function MenuVideoSkeleton() { return <div className="aspect-video animate-pulse rounded-[1.5rem] bg-white/10" aria-label="Video yüklənir" />; }
