import { useQuery } from "@tanstack/react-query";
import { Clock3, LoaderCircle, MapPin, RefreshCw, UtensilsCrossed } from "lucide-react";
import { useEffect } from "react";
import { Link, useLocation, useParams } from "react-router-dom";
import {
  getPublicApiError,
  getPublicMenu,
  getPublicRestaurant,
} from "@/api/public-api";
import { ReservationWizard } from "@/features/reservations/components/ReservationWizard";
import { formatTime } from "@/utils/reservation-date";
import { RestaurantHero } from "../components/RestaurantHero";

export function PublicRestaurantPage() {
  const { restaurantSlug = "" } = useParams();
  const location = useLocation();
  const query = useQuery({
    queryKey: ["public-restaurant", restaurantSlug],
    queryFn: () => getPublicRestaurant(restaurantSlug),
    enabled: Boolean(restaurantSlug),
  });
  const menuQuery = useQuery({
    queryKey: ["public-menu", restaurantSlug],
    queryFn: () => getPublicMenu(restaurantSlug),
    enabled: Boolean(restaurantSlug),
  });

  useEffect(() => {
    if (!query.data) {
      return;
    }

    const description =
      query.data.description ||
      `${query.data.name} restoranında onlayn masa rezervasiyası.`;
    document.title = `${query.data.name} · Masa rezervasiyası`;
    setMeta("name", "description", description);
    setMeta("property", "og:title", document.title);
    setMeta("property", "og:description", description);

    if (query.data.coverImageUrl) {
      setMeta("property", "og:image", query.data.coverImageUrl);
    }
  }, [query.data]);

  useEffect(() => {
    if (!query.data || !location.pathname.endsWith("/reservation")) return;
    requestAnimationFrame(() => document.getElementById("reservation")?.scrollIntoView());
  }, [location.pathname, query.data]);

  if (query.isPending) {
    return (
      <PageState
        icon={LoaderCircle}
        spin
        title="Restoran hazırlanır"
        message="Məlumatları və filialları yükləyirik."
      />
    );
  }

  if (query.isError) {
    const error = getPublicApiError(query.error);

    return (
      <PageState
        icon={error.status === 404 ? MapPin : RefreshCw}
        title={
          error.status === 404
            ? "Restoran tapılmadı"
            : "Səhifə yüklənmədi"
        }
        message={error.message}
        action={() => query.refetch()}
      />
    );
  }

  const restaurant = query.data;

  return (
    <article>
      <RestaurantHero
        restaurant={restaurant}
        onReserve={() =>
          document
            .getElementById("reservation")
            ?.scrollIntoView({ behavior: "smooth" })
        }
      />
      <section
        aria-labelledby="working-hours-title"
        className="mx-auto grid max-w-7xl gap-8 px-4 py-16 sm:px-6 lg:grid-cols-[0.8fr_1.2fr] lg:px-8"
      >
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.24em] text-[#a5422f]">
            Restoran haqqında
          </p>
          <h2 className="mt-3 font-serif text-4xl">
            Hər detal qonaq üçün
          </h2>
          <p className="mt-5 max-w-xl leading-7 text-[#70655c]">
            {restaurant.description ||
              "Mətbəx, rahatlıq və diqqətli xidmət bir arada. Filialı seçərək uyğun saatları və boş masaları görə bilərsiniz."}
          </p>
          <Link
            to={`/restaurants/${restaurant.slug}/menu`}
            className="mt-6 mr-3 inline-flex rounded-full bg-[#b5422d] px-5 py-2.5 text-sm font-bold text-white"
          >
            Menyuya bax
          </Link>
          <Link
            to="/reservation/track"
            className="mt-6 inline-flex rounded-full border border-[#d2c7bb] bg-white px-5 py-2.5 text-sm font-bold"
          >
            Mövcud rezervasiyanı yoxla
          </Link>
        </div>
        <div className="rounded-[30px] border border-[#ddd4ca] bg-white p-6 sm:p-8">
          <h2
            id="working-hours-title"
            className="flex items-center gap-2 font-serif text-2xl"
          >
            <Clock3 className="h-5 w-5 text-[#a5422f]" />
            İş saatları
          </h2>
          <dl className="mt-5 divide-y divide-[#eee8e1]">
            {restaurant.workingHours.map((entry) => (
              <div
                key={entry.dayOfWeek}
                className="flex items-center justify-between gap-4 py-3 text-sm"
              >
                <dt className="font-semibold">{entry.dayName}</dt>
                <dd className="text-[#746960]">
                  {entry.isClosed || !entry.opensAt || !entry.closesAt
                    ? "Bağlı"
                    : `${formatTime(entry.opensAt)}–${formatTime(entry.closesAt)}`}
                </dd>
              </div>
            ))}
          </dl>
        </div>
      </section>
      <section aria-labelledby="featured-menu-title" className="bg-[#211d18] px-4 py-16 text-white sm:px-6 lg:px-8">
        <div className="mx-auto max-w-7xl"><div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between"><div><p className="text-xs font-bold uppercase tracking-[0.24em] text-[#e8a36c]">Seçilmiş dadlar</p><h2 id="featured-menu-title" className="mt-2 font-serif text-4xl">Populyar menyu</h2></div><Link to={`/restaurants/${restaurant.slug}/menu`} className="rounded-full border border-white/20 px-5 py-2.5 text-center text-sm font-bold">Bütün menyu</Link></div>
          {menuQuery.isLoading ? <div className="mt-8 grid animate-pulse gap-5 sm:grid-cols-2 lg:grid-cols-4">{[0,1,2,3].map((item) => <div key={item} className="h-64 rounded-3xl bg-white/10" />)}</div> : menuQuery.isError ? <p className="mt-8 rounded-2xl bg-white/10 p-5 text-white/70">Menyu hazırda yüklənmədi.</p> : (() => { const featured = (menuQuery.data ?? []).flatMap((category) => category.items).filter((item) => item.isAvailable).sort((a,b) => Number(b.isPopular) - Number(a.isPopular)).slice(0,4); return featured.length === 0 ? <p className="mt-8 rounded-2xl bg-white/10 p-5 text-white/70">Menyu məhsulu əlavə edilməyib.</p> : <div className="mt-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">{featured.map((item) => <article key={item.id} className="overflow-hidden rounded-3xl bg-white/8"><div className="h-40 bg-white/10">{item.imageUrl ? <img src={item.imageUrl} alt={item.name} className="h-full w-full object-cover" /> : <div className="grid h-full place-items-center"><UtensilsCrossed className="h-7 w-7 text-white/40" /></div>}</div><div className="p-5"><h3 className="font-serif text-2xl">{item.name}</h3><p className="mt-2 line-clamp-2 min-h-10 text-sm text-white/55">{item.description}</p><p className="mt-4 font-bold text-[#f0b47e]">{item.finalPrice.toFixed(2)} ₼</p></div></article>)}</div>; })()}
        </div>
      </section>
      <ReservationWizard restaurant={restaurant} />
    </article>
  );
}

interface PageStateProps {
  icon: typeof LoaderCircle;
  title: string;
  message: string;
  spin?: boolean;
  action?: () => void;
}

function PageState({
  icon: Icon,
  title,
  message,
  spin,
  action,
}: PageStateProps) {
  return (
    <div className="grid min-h-[70vh] place-items-center px-4 text-center">
      <div className="max-w-md">
        <Icon
          className={`mx-auto h-10 w-10 text-[#a5422f] ${
            spin ? "animate-spin" : ""
          }`}
        />
        <h1 className="mt-5 font-serif text-4xl">{title}</h1>
        <p className="mt-3 leading-7 text-[#70655c]">{message}</p>
        {action && (
          <button
            type="button"
            onClick={action}
            className="mt-6 rounded-full bg-[#b5422d] px-5 py-3 font-bold text-white"
          >
            Yenidən cəhd et
          </button>
        )}
      </div>
    </div>
  );
}

function setMeta(
  attribute: "name" | "property",
  key: string,
  content: string,
) {
  let element = document.head.querySelector<HTMLMetaElement>(
    `meta[${attribute}="${key}"]`,
  );

  if (!element) {
    element = document.createElement("meta");
    element.setAttribute(attribute, key);
    document.head.appendChild(element);
  }

  element.content = content;
}
