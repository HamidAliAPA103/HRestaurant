import { useCallback, useState } from "react";
import type { PublicMenuItem } from "@/types/public";
import { MenuVideoCard } from "./MenuVideoCard";

type Item = PublicMenuItem & { categoryName: string };
export function MenuVideoGrid({ items, restaurantSlug }: { items: Item[]; restaurantSlug: string }) {
  const [activeVideoId, setActiveVideoId] = useState<string | null>(null);
  const setActive = useCallback((id: string | null) => setActiveVideoId(id), []);
  return <div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">{items.map((item) => <MenuVideoCard key={item.id} item={item} restaurantSlug={restaurantSlug} activeVideoId={activeVideoId} onActiveChange={setActive} />)}</div>;
}
