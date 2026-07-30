import { useQuery } from "@tanstack/react-query";
import { Clock3, LoaderCircle, MapPin, RefreshCw } from "lucide-react";
import { useEffect } from "react";
import { Link, useParams } from "react-router-dom";
import {
  getPublicApiError,
  getPublicRestaurant,
} from "@/api/public-api";
import { ReservationWizard } from "@/features/reservations/components/ReservationWizard";
import { formatTime } from "@/utils/reservation-date";
import { RestaurantHero } from "../components/RestaurantHero";

export function PublicRestaurantPage() {
  const { restaurantSlug = "" } = useParams();
  const query = useQuery({
    queryKey: ["public-restaurant", restaurantSlug],
    queryFn: () => getPublicRestaurant(restaurantSlug),
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
