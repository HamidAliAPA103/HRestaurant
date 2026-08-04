import { useQuery } from "@tanstack/react-query";
import { Search, UtensilsCrossed } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { getPublicApiError, getPublicMenu, getPublicRestaurant } from "@/api/public-api";
import { MenuVideoGrid } from "@/features/public-restaurant/components/menu-video/MenuVideoGrid";

export function PublicMenuPage() {
  const { restaurantSlug = "" } = useParams();
  const [search, setSearch] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [popular, setPopular] = useState(false);
  const [available, setAvailable] = useState(true);
  const restaurant = useQuery({ queryKey: ["public-restaurant", restaurantSlug], queryFn: () => getPublicRestaurant(restaurantSlug), enabled: Boolean(restaurantSlug) });
  const menu = useQuery({ queryKey: ["public-menu", restaurantSlug], queryFn: () => getPublicMenu(restaurantSlug), enabled: Boolean(restaurantSlug) });

  useEffect(() => { if (restaurant.data) document.title = `${restaurant.data.name} · Menyu`; }, [restaurant.data]);
  const items = useMemo(() => (menu.data ?? []).flatMap((category) => category.items.map((item) => ({ ...item, categoryName: category.name }))).filter((item) =>
    (!categoryId || item.categoryId === categoryId) && (!search || `${item.name} ${item.description} ${item.ingredients.join(" ")}`.toLowerCase().includes(search.toLowerCase())) && (!popular || item.isPopular) && (!available || item.isAvailable)), [available, categoryId, menu.data, popular, search]);

  if (menu.isLoading || restaurant.isLoading) return <div className="mx-auto min-h-[65vh] max-w-7xl animate-pulse px-4 py-16" aria-live="polite"><span className="sr-only">Menyu yüklənir</span><div className="h-14 w-1/2 rounded bg-black/10" /><div className="mt-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-3"><div className="h-80 rounded-3xl bg-black/10" /><div className="h-80 rounded-3xl bg-black/10" /></div></div>;
  if (menu.isError || restaurant.isError) return <div className="grid min-h-[60vh] place-items-center px-4 text-center"><div><h1 className="font-serif text-4xl">Menyu yüklənmədi</h1><p className="mt-3">{getPublicApiError(menu.error ?? restaurant.error).message}</p><button type="button" className="mt-5 rounded-full bg-[#b5422d] px-5 py-3 font-bold text-white" onClick={() => { void menu.refetch(); void restaurant.refetch(); }}>Yenidən cəhd et</button></div></div>;

  return <section className="min-h-screen bg-[#1d1714] px-4 py-16 text-[#fff7e8] sm:px-6 lg:px-10 lg:py-24"><div className="mx-auto max-w-[90rem]">
    <div className="flex flex-col gap-5 sm:flex-row sm:items-end sm:justify-between"><div><p className="text-xs font-bold uppercase tracking-[0.25em] text-[#e7774f]">{restaurant.data?.name}</p><h1 className="display-type mt-3 text-6xl sm:text-8xl">Video menyu</h1><p className="mt-4 max-w-xl text-[#d8c9b6]">Yeməkləri qısa videolarla kəşf edin, tərkib və qiymət məlumatlarına baxın.</p></div><Link to={`/restaurants/${restaurantSlug}/reservation`} className="rounded-full bg-[#c55232] px-6 py-3.5 text-center font-bold text-white">Masa rezerv et</Link></div>
    <div className="mt-10 grid gap-3 rounded-[2rem] border border-white/10 bg-white/5 p-4 backdrop-blur md:grid-cols-[1fr_auto_auto]"><label className="relative"><span className="sr-only">Yemək axtar</span><Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2" /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Yemək və ya ingredient axtar..." className="h-11 w-full rounded-xl border border-white/10 bg-black/20 pl-10 pr-3" /></label><select aria-label="Kateqoriya" value={categoryId} onChange={(event) => setCategoryId(event.target.value)} className="h-11 rounded-xl border border-white/10 bg-[#2b211c] px-3"><option value="">Bütün kateqoriyalar</option>{(menu.data ?? []).map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select><div className="flex flex-wrap gap-4"><label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={popular} onChange={(event) => setPopular(event.target.checked)} />Yalnız populyar</label><label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={available} onChange={(event) => setAvailable(event.target.checked)} />Yalnız mövcud</label></div></div>
    {items.length === 0 ? <div className="mt-8 rounded-3xl border border-white/10 p-12 text-center"><UtensilsCrossed className="mx-auto h-8 w-8" /><h2 className="mt-3 font-serif text-2xl">Uyğun məhsul tapılmadı</h2></div> : <MenuVideoGrid items={items} restaurantSlug={restaurantSlug} />}
  </div></section>;
}
