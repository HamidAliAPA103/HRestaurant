import { useQuery } from "@tanstack/react-query";
import { ArrowRight, LoaderCircle, UtensilsCrossed } from "lucide-react";
import { Link } from "react-router-dom";
import { getPublicMenu, getPublicRestaurants } from "@/api/public-api";
import { MenuVideoGrid } from "@/features/public-restaurant/components/menu-video/MenuVideoGrid";
import { Reveal } from "./Reveal";

export function MenuSection() {
  const restaurants = useQuery({ queryKey: ["public-restaurants"], queryFn: getPublicRestaurants });
  const restaurant = restaurants.data?.[0];
  const menu = useQuery({ queryKey: ["public-menu", restaurant?.slug], queryFn: () => getPublicMenu(restaurant!.slug), enabled: Boolean(restaurant?.slug) });
  const items = (menu.data ?? []).flatMap((category) => category.items.filter((item) => item.isAvailable).map((item) => ({ ...item, categoryName: category.name }))).slice(0, 8);
  return <section id="menu" className="bg-[#fcfbf6] px-5 py-20 sm:px-8 lg:px-12 lg:py-28"><div className="mx-auto max-w-7xl"><Reveal><p className="text-xs font-bold uppercase tracking-[.2em] text-[#5c954f]">HRestaurant menyusu</p><div className="mt-4 flex flex-col gap-5 md:flex-row md:items-end md:justify-between"><div><h2 className="max-w-3xl font-serif text-5xl leading-[.9] tracking-[-.05em] text-[#173d2b] sm:text-6xl">Menyunu görün,<br /><em className="font-normal text-[#5c954f]">dadı hiss edin.</em></h2><p className="mt-5 max-w-xl leading-7 text-[#5d6b60]">Real menyu məhsulları, video təqdimat və dəstəklənən yeməklər üçün interaktiv 3D baxış.</p></div>{restaurant && <Link to={`/restaurants/${restaurant.slug}/menu`} className="inline-flex items-center gap-2 rounded-full border border-[#173d2b]/15 px-5 py-3 text-sm font-bold text-[#173d2b] transition hover:bg-[#e7f2e3]">Bütün menyu <ArrowRight className="h-4 w-4" /></Link>}</div></Reveal>{restaurants.isLoading || menu.isLoading ? <div className="mt-12 grid place-items-center rounded-[2rem] bg-[#e7f2e3] p-16 text-[#346b43]"><LoaderCircle className="h-7 w-7 animate-spin" /><p className="mt-3 text-sm font-semibold">Menyu yüklənir</p></div> : !restaurant || items.length === 0 ? <div className="mt-12 rounded-[2rem] border border-dashed border-[#315d3b]/20 bg-[#f3f8f0] p-14 text-center"><UtensilsCrossed className="mx-auto h-9 w-9 text-[#5c954f]" /><h3 className="mt-4 font-serif text-3xl text-[#173d2b]">Menyu tezliklə burada olacaq</h3><p className="mt-2 text-[#5d6b60]">İdarəetmə panelindən məhsul əlavə etdikdə bu bölmə avtomatik yenilənəcək.</p></div> : <MenuVideoGrid items={items} restaurantSlug={restaurant.slug} />}</div></section>;
}
