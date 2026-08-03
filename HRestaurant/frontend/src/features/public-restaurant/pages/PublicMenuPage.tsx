import { useQuery } from "@tanstack/react-query";
import { Box, Clock3, Search, Star, UtensilsCrossed } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import {
  getPublicApiError,
  getPublicMenu,
  getPublicRestaurant,
} from "@/api/public-api";

export function PublicMenuPage() {
  const { restaurantSlug = "" } = useParams();
  const [search, setSearch] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [maxPrice, setMaxPrice] = useState("");
  const [popular, setPopular] = useState(false);
  const [available, setAvailable] = useState(true);
  const restaurant = useQuery({
    queryKey: ["public-restaurant", restaurantSlug],
    queryFn: () => getPublicRestaurant(restaurantSlug),
    enabled: Boolean(restaurantSlug),
  });
  const menu = useQuery({
    queryKey: ["public-menu", restaurantSlug],
    queryFn: () => getPublicMenu(restaurantSlug),
    enabled: Boolean(restaurantSlug),
  });

  useEffect(() => {
    if (restaurant.data) document.title = `${restaurant.data.name} · Menyu`;
  }, [restaurant.data]);

  const items = useMemo(
    () =>
      (menu.data ?? [])
        .flatMap((category) =>
          category.items.map((item) => ({
            ...item,
            categoryName: category.name,
          })),
        )
        .filter(
          (item) =>
            (!categoryId || item.categoryId === categoryId) &&
            (!search ||
              `${item.name} ${item.description} ${item.nutrition}`
                .toLowerCase()
                .includes(search.toLowerCase())) &&
            (!maxPrice || item.finalPrice <= Number(maxPrice)) &&
            (!popular || item.isPopular) &&
            (!available || item.isAvailable),
        ),
    [available, categoryId, maxPrice, menu.data, popular, search],
  );

  if (menu.isLoading || restaurant.isLoading) {
    return (
      <div className="mx-auto min-h-[65vh] max-w-7xl animate-pulse px-4 py-16" aria-live="polite">
        <span className="sr-only">Menyu yüklənir</span>
        <div className="h-14 w-1/2 rounded bg-black/10" />
        <div className="mt-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          <div className="h-80 rounded-3xl bg-black/10" />
          <div className="h-80 rounded-3xl bg-black/10" />
        </div>
      </div>
    );
  }

  if (menu.isError || restaurant.isError) {
    return (
      <div className="grid min-h-[60vh] place-items-center px-4 text-center">
        <div>
          <h1 className="font-serif text-4xl">Menyu yüklənmədi</h1>
          <p className="mt-3">
            {getPublicApiError(menu.error ?? restaurant.error).message}
          </p>
          <button
            type="button"
            className="mt-5 rounded-full bg-[#b5422d] px-5 py-3 font-bold text-white"
            onClick={() => {
              void menu.refetch();
              void restaurant.refetch();
            }}
          >
            Yenidən cəhd et
          </button>
        </div>
      </div>
    );
  }

  return (
    <section className="mx-auto max-w-7xl px-4 py-14 sm:px-6 lg:px-8">
      <div className="flex flex-col gap-5 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.25em] text-[#a5422f]">
            {restaurant.data?.name}
          </p>
          <h1 className="mt-2 font-serif text-5xl">Menyu</h1>
        </div>
        <Link
          to={`/restaurants/${restaurantSlug}/reservation`}
          className="rounded-full bg-[#b5422d] px-5 py-3 text-center font-bold text-white"
        >
          Masa rezerv et
        </Link>
      </div>

      <div className="mt-8 grid gap-3 rounded-3xl border bg-white p-4 md:grid-cols-[1fr_auto_auto]">
        <label className="relative">
          <span className="sr-only">Yemək axtar</span>
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2" aria-hidden />
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Yemək axtar..."
            className="h-11 w-full rounded-xl border pl-10 pr-3"
          />
        </label>
        <select
          aria-label="Kateqoriya"
          value={categoryId}
          onChange={(event) => setCategoryId(event.target.value)}
          className="h-11 rounded-xl border px-3"
        >
          <option value="">Bütün kateqoriyalar</option>
          {(menu.data ?? []).map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </select>
        <input
          aria-label="Maksimum qiymət"
          type="number"
          min="0"
          step="0.01"
          value={maxPrice}
          onChange={(event) => setMaxPrice(event.target.value)}
          placeholder="Maks. qiymət"
          className="h-11 rounded-xl border px-3"
        />
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={popular}
            onChange={(event) => setPopular(event.target.checked)}
          />
          Yalnız populyar
        </label>
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={available}
            onChange={(event) => setAvailable(event.target.checked)}
          />
          Yalnız mövcud
        </label>
      </div>

      {items.length === 0 ? (
        <div className="mt-8 rounded-3xl border bg-white p-12 text-center">
          <UtensilsCrossed className="mx-auto h-8 w-8" aria-hidden />
          <h2 className="mt-3 font-serif text-2xl">Uyğun məhsul tapılmadı</h2>
        </div>
      ) : (
        <div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {items.map((item) => (
            <article
              key={item.id}
              className="group overflow-hidden rounded-3xl border bg-white transition hover:-translate-y-1 hover:shadow-xl"
            >
              <Link
                to={`/restaurants/${restaurantSlug}/menu/${item.id}`}
                aria-label={`${item.name} üçün 3D detallara bax`}
                className="block focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[#b5422d]"
              >
                <div className="relative h-48 bg-[#e6ddd2]">
                  {item.imageUrl ? (
                    <img
                      src={item.imageUrl}
                      alt={item.name}
                      className="h-full w-full object-cover transition duration-500 group-hover:scale-105"
                    />
                  ) : (
                    <div className="grid h-full place-items-center">
                      <UtensilsCrossed className="h-8 w-8 text-[#8f8379]" aria-hidden />
                    </div>
                  )}
                  <span className="absolute right-3 top-3 inline-flex items-center gap-1 rounded-full bg-[#211914]/85 px-2.5 py-1 text-[10px] font-bold uppercase tracking-wider text-white backdrop-blur">
                    <Box className="h-3 w-3" aria-hidden />
                    {item.is3DEnabled ? "3D" : "Stilizə 3D"}
                  </span>
                </div>
                <div className="p-5">
                  <p className="text-xs font-bold uppercase text-[#a5422f]">
                    {item.categoryName}
                  </p>
                  <div className="mt-2 flex items-start justify-between gap-3">
                    <h2 className="font-serif text-2xl">{item.name}</h2>
                    {item.isPopular && (
                      <Star
                        aria-label="Populyar"
                        className="h-4 w-4 fill-amber-400 text-amber-400"
                      />
                    )}
                  </div>
                  <p className="mt-2 line-clamp-2 min-h-10 text-sm text-[#746960]">
                    {item.description}
                  </p>
                  <div className="mt-4 flex items-center justify-between border-t pt-4">
                    <span className="font-bold">{item.finalPrice.toFixed(2)} ₼</span>
                    <span className="flex items-center gap-1 text-xs">
                      <Clock3 className="h-3 w-3" aria-hidden />
                      {item.preparationTimeMinutes} dəq.
                    </span>
                  </div>
                  {!item.isAvailable && (
                    <p className="mt-3 text-xs font-bold text-red-700">
                      Hazırda mövcud deyil
                    </p>
                  )}
                </div>
              </Link>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
