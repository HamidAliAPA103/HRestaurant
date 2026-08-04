import { Star } from "lucide-react";
import { Link } from "react-router-dom";
import type { PublicMenuItem } from "@/types/public";
import { MenuVideoFallback } from "./MenuVideoFallback";
import { MenuVideoPlayer } from "./MenuVideoPlayer";

type Item = PublicMenuItem & { categoryName: string };
export function MenuVideoCard({ item, restaurantSlug, activeVideoId, onActiveChange }: { item: Item; restaurantSlug: string; activeVideoId: string | null; onActiveChange: (id: string | null) => void }) {
  const hasVideo = item.isVideoEnabled && Boolean(item.videoUrl);
  return <article className="group overflow-hidden rounded-[1.75rem] border border-white/10 bg-[#2b211c] text-[#fff7e8] shadow-xl transition motion-safe:hover:-translate-y-1">
    {hasVideo ? <MenuVideoPlayer item={item} activeVideoId={activeVideoId} onActiveChange={onActiveChange} /> : <MenuVideoFallback item={item} />}
    <Link to={`/restaurants/${restaurantSlug}/menu/${item.id}`} className="block p-5 focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#d7a64a]">
      <div className="flex items-center justify-between gap-3"><p className="text-xs font-bold uppercase tracking-widest text-[#e7774f]">{item.categoryName}</p>{item.isPopular && <span className="inline-flex items-center gap-1 text-xs text-[#f0c96a]"><Star className="h-4 w-4 fill-current" /> Populyar</span>}</div>
      <h2 className="mt-2 font-serif text-2xl">{item.name}</h2><p className="mt-2 line-clamp-2 min-h-10 text-sm text-[#d8c9b6]">{item.description}</p>
      {item.ingredients.length > 0 && <p className="mt-2 line-clamp-1 text-xs text-[#b9a998]">{item.ingredients.join(" · ")}</p>}
      <div className="mt-4 flex items-center justify-between border-t border-white/10 pt-4"><div><strong className="rounded-full bg-[#d7a64a] px-3 py-1 text-[#241a15]">{item.finalPrice.toFixed(2)} ₼</strong>{item.discountPercentage > 0 && <span className="ml-2 text-xs line-through text-[#b9a998]">{item.price.toFixed(2)} ₼</span>}</div><span className={`text-xs font-bold ${item.isAvailable ? "text-emerald-300" : "text-red-300"}`}>{item.isAvailable ? "Mövcuddur" : "Mövcud deyil"}</span></div>
    </Link>
  </article>;
}
