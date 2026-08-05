import { lazy, Suspense, useCallback, useState } from "react";
import type { PublicMenuItem } from "@/types/public";
import { MenuVideoCard } from "./MenuVideoCard";

const Food3DModal = lazy(() => import("@/components/menu-3d/Food3DModal").then((module) => ({ default: module.Food3DModal })));
type Item = PublicMenuItem & { categoryName: string };
export function MenuVideoGrid({ items, restaurantSlug }: { items: Item[]; restaurantSlug: string }) {
  const [activeVideoId, setActiveVideoId] = useState<string | null>(null);
  const [selected, setSelected] = useState<Item | null>(null);
  const setActive = useCallback((id: string | null) => setActiveVideoId(id), []);
  return <><div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">{items.map((item) => <MenuVideoCard key={item.id} item={item} restaurantSlug={restaurantSlug} activeVideoId={activeVideoId} onActiveChange={setActive} onOpen3D={setSelected} />)}</div>{selected && <Suspense fallback={null}><Food3DModal item={selected} onClose={() => setSelected(null)} /></Suspense>}</>;
}
