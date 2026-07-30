import { Clock3, Mail, MapPin, Phone } from "lucide-react";
import type { PublicRestaurant } from "@/types/public";

interface RestaurantHeroProps {
  restaurant: PublicRestaurant;
  onReserve: () => void;
}

export function RestaurantHero({
  restaurant,
  onReserve,
}: RestaurantHeroProps) {
  return (
    <section className="relative isolate overflow-hidden bg-[#201c17] text-white">
      {restaurant.coverImageUrl ? (
        <img
          src={restaurant.coverImageUrl}
          alt={`${restaurant.name} cover`}
          className="absolute inset-0 -z-20 h-full w-full object-cover opacity-55"
        />
      ) : (
        <div className="absolute inset-0 -z-20 bg-[radial-gradient(circle_at_20%_20%,#a84f32_0,transparent_34%),radial-gradient(circle_at_80%_70%,#5c6d4e_0,transparent_30%),linear-gradient(135deg,#181510,#3b2d24)]" />
      )}
      <div className="absolute inset-0 -z-10 bg-gradient-to-r from-black/75 via-black/45 to-black/15" />
      <div className="mx-auto grid min-h-[590px] max-w-7xl items-end gap-10 px-4 pb-14 pt-24 sm:px-6 lg:grid-cols-[1fr_360px] lg:px-8 lg:pb-20">
        <div className="max-w-3xl">
          <div className="mb-6 flex items-center gap-4">
            {restaurant.logoUrl ? (
              <img
                src={restaurant.logoUrl}
                alt={`${restaurant.name} logo`}
                className="h-20 w-20 rounded-3xl border border-white/20 bg-white object-cover shadow-2xl"
              />
            ) : (
              <span className="grid h-20 w-20 place-items-center rounded-3xl border border-white/20 bg-white/10 font-serif text-4xl backdrop-blur">
                {restaurant.name.charAt(0)}
              </span>
            )}
            <span
              className={`inline-flex items-center gap-2 rounded-full px-3 py-1.5 text-sm font-semibold ${
                restaurant.isOpenNow
                  ? "bg-emerald-400/20 text-emerald-100"
                  : "bg-white/10 text-white/75"
              }`}
            >
              <span
                className={`h-2 w-2 rounded-full ${
                  restaurant.isOpenNow
                    ? "bg-emerald-400"
                    : "bg-white/45"
                }`}
              />
              {restaurant.isOpenNow ? "Hazırda açıqdır" : "Hazırda bağlıdır"}
            </span>
          </div>
          <p className="mb-4 text-xs font-bold uppercase tracking-[0.28em] text-[#f3b49f]">
            Dadı yadda qalan axşamlar
          </p>
          <h1 className="max-w-2xl font-serif text-5xl leading-[0.98] tracking-tight sm:text-7xl">
            {restaurant.name}
          </h1>
          <p className="mt-6 max-w-2xl text-base leading-7 text-white/75 sm:text-lg">
            {restaurant.description ||
              "Sevdiklərinizlə rahat atmosferdə unudulmaz süfrə təcrübəsi yaşayın."}
          </p>
          <button
            type="button"
            onClick={onReserve}
            className="mt-8 rounded-full bg-[#e96848] px-7 py-3.5 font-bold text-white shadow-xl shadow-black/20 transition hover:-translate-y-0.5 hover:bg-[#f17555] focus:outline-none focus-visible:ring-2 focus-visible:ring-white"
          >
            Masa rezerv et
          </button>
        </div>
        <address className="not-italic">
          <div className="rounded-[28px] border border-white/15 bg-black/25 p-6 backdrop-blur-md">
            <p className="mb-5 text-xs font-bold uppercase tracking-[0.22em] text-white/50">
              Əlaqə
            </p>
            <ContactRow icon={MapPin} value={restaurant.address} />
            <ContactRow icon={Phone} value={restaurant.phone} />
            {restaurant.email && (
              <ContactRow icon={Mail} value={restaurant.email} />
            )}
            <ContactRow
              icon={Clock3}
              value={
                restaurant.isOpenNow
                  ? "Qonaqları qəbul edirik"
                  : "İş saatlarına baxın"
              }
            />
          </div>
        </address>
      </div>
    </section>
  );
}

interface ContactRowProps {
  icon: typeof MapPin;
  value: string;
}

function ContactRow({ icon: Icon, value }: ContactRowProps) {
  return (
    <div className="flex gap-3 border-t border-white/10 py-3 first:border-0">
      <Icon className="mt-0.5 h-4 w-4 shrink-0 text-[#f3a58e]" />
      <span className="text-sm leading-5 text-white/80">{value}</span>
    </div>
  );
}
