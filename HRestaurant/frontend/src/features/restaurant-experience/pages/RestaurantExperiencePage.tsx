import { useQuery } from "@tanstack/react-query";
import gsap from "gsap";
import { ScrollTrigger } from "gsap/ScrollTrigger";
import { CalendarDays, Compass, LoaderCircle, RefreshCw, Users } from "lucide-react";
import { useReducedMotion } from "motion/react";
import { lazy, Suspense, useEffect, useLayoutEffect, useMemo, useRef, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  getAvailableTables,
  getPublicApiError,
  getPublicRestaurantExperience,
  getPublicRestaurantScene,
} from "@/api/public-api";
import { AccessibleTableList } from "@/features/restaurant-experience/components/AccessibleTableList";
import { SceneLoadingScreen } from "@/features/restaurant-experience/components/SceneLoadingScreen";
import { SelectedTablePanel } from "@/features/restaurant-experience/components/SelectedTablePanel";
import { useRestaurantExperienceStore } from "@/features/restaurant-experience/store/restaurant-experience-store";
import type {
  PublicBranch,
  PublicRestaurantTable,
  PublicSceneTable,
} from "@/types/public";
import { generateTimeSlots, getTodayInputValue } from "@/utils/reservation-date";

gsap.registerPlugin(ScrollTrigger);

const RestaurantScene = lazy(() =>
  import("@/features/restaurant-experience/components/RestaurantScene").then(
    (module) => ({ default: module.RestaurantScene }),
  ),
);

const durationMinutes = 120;

export function RestaurantExperiencePage() {
  const { restaurantSlug = "" } = useParams();
  const navigate = useNavigate();
  const reducedMotion = Boolean(useReducedMotion());
  const heroRef = useRef<HTMLElement>(null);
  const heroCopyRef = useRef<HTMLDivElement>(null);
  const controlsRef = useRef<HTMLDivElement>(null);
  const [reservationDate, setReservationDate] = useState("");
  const [startTime, setStartTime] = useState("");
  const [guestCount, setGuestCount] = useState(2);
  const selectedBranchId = useRestaurantExperienceStore(
    (state) => state.selectedBranchId,
  );
  const selectedTableId = useRestaurantExperienceStore(
    (state) => state.selectedTableId,
  );
  const setSelectedBranchId = useRestaurantExperienceStore(
    (state) => state.setSelectedBranchId,
  );
  const setSelectedTableId = useRestaurantExperienceStore(
    (state) => state.setSelectedTableId,
  );
  const setHeroProgress = useRestaurantExperienceStore(
    (state) => state.setHeroProgress,
  );
  const startTour = useRestaurantExperienceStore((state) => state.startTour);
  const reset = useRestaurantExperienceStore((state) => state.reset);

  const experienceQuery = useQuery({
    queryKey: ["public-restaurant-experience", restaurantSlug],
    queryFn: () => getPublicRestaurantExperience(restaurantSlug),
    enabled: Boolean(restaurantSlug),
    staleTime: 5 * 60_000,
  });
  const sceneQuery = useQuery({
    queryKey: ["public-restaurant-scene", restaurantSlug],
    queryFn: () => getPublicRestaurantScene(restaurantSlug),
    enabled: Boolean(restaurantSlug),
    staleTime: 5 * 60_000,
  });

  const restaurant = experienceQuery.data?.restaurant;
  const branch = restaurant?.branches.find((item) => item.id === selectedBranchId);
  const branchScene = sceneQuery.data?.branches.find(
    (item) => item.branchId === selectedBranchId,
  );
  const slots = useMemo(
    () =>
      branch
        ? generateTimeSlots(branch.workingHours, reservationDate, durationMinutes, {
            timeZoneId: branch.timeZoneId,
          })
        : [],
    [branch, reservationDate],
  );
  const availabilityQuery = useQuery({
    queryKey: [
      "public-experience-availability",
      selectedBranchId,
      reservationDate,
      startTime,
      guestCount,
      durationMinutes,
    ],
    queryFn: () =>
      getAvailableTables(selectedBranchId!, {
        reservationDate,
        startTime,
        guestCount,
        durationMinutes,
      }),
    enabled: Boolean(selectedBranchId && reservationDate && startTime),
    refetchOnWindowFocus: true,
  });

  useEffect(() => {
    reset();
    return reset;
  }, [restaurantSlug, reset]);

  useEffect(() => {
    if (!experienceQuery.data || selectedBranchId) return;
    const firstBranchId =
      experienceQuery.data.defaultBranchId ??
      experienceQuery.data.restaurant.branches[0]?.id;
    if (firstBranchId) setSelectedBranchId(firstBranchId);
  }, [experienceQuery.data, selectedBranchId, setSelectedBranchId]);

  useEffect(() => {
    if (!branch) return;
    const firstSlot = findFirstReservationSlot(branch);
    setReservationDate(firstSlot.date);
    setStartTime(firstSlot.time);
    setSelectedTableId(null);
  }, [branch?.id, setSelectedTableId]);

  useEffect(() => {
    if (!restaurant) return;
    document.title = `${restaurant.name} · 3D virtual tur`;
  }, [restaurant]);

  useEffect(() => {
    if (!availabilityQuery.data || !selectedTableId) return;
    const selectedStillAvailable = availabilityQuery.data.some(
      (table) => table.id === selectedTableId && table.isAvailable,
    );
    if (!selectedStillAvailable) setSelectedTableId(null);
  }, [availabilityQuery.data, selectedTableId, setSelectedTableId]);

  useLayoutEffect(() => {
    if (!heroRef.current || reducedMotion) {
      setHeroProgress(1);
      return;
    }
    const context = gsap.context(() => {
      ScrollTrigger.create({
        trigger: heroRef.current,
        start: "top top",
        end: "bottom top",
        scrub: true,
        onUpdate: (self) => setHeroProgress(self.progress),
      });
      if (heroCopyRef.current) {
        gsap.to(heroCopyRef.current, {
          opacity: 0,
          y: -70,
          ease: "none",
          scrollTrigger: {
            trigger: heroRef.current,
            start: "top top",
            end: "65% top",
            scrub: true,
          },
        });
      }
    }, heroRef);
    return () => context.revert();
  }, [reducedMotion, setHeroProgress]);

  const tables = useMemo(() => {
    if (!branchScene) return [];
    return mergeAvailability(
      branchScene.tables,
      availabilityQuery.data,
      availabilityQuery.isFetching || !startTime,
    );
  }, [availabilityQuery.data, availabilityQuery.isFetching, branchScene, startTime]);
  const selectedTable = tables.find((table) => table.id === selectedTableId) ?? null;

  const reserveSelectedTable = () => {
    if (!selectedTable || !branch) return;
    const params = new URLSearchParams({
      branchId: branch.id,
      date: reservationDate,
      time: startTime,
      guests: String(guestCount),
      tableId: selectedTable.id,
    });
    navigate(`/restaurants/${restaurantSlug}/reservation?${params.toString()}#reservation`);
  };

  if (experienceQuery.isLoading || sceneQuery.isLoading) {
    return <SceneLoadingScreen />;
  }
  if (experienceQuery.isError || sceneQuery.isError) {
    const error = getPublicApiError(experienceQuery.error ?? sceneQuery.error);
    return (
      <div className="grid min-h-[70vh] place-items-center px-4 text-center">
        <div>
          <RefreshCw className="mx-auto h-9 w-9 text-[#b5422d]" aria-hidden />
          <h1 className="mt-4 font-serif text-4xl">Virtual tur yüklənmədi</h1>
          <p className="mt-3 text-[#70655c]">{error.message}</p>
          <button
            type="button"
            className="mt-5 rounded-full bg-[#b5422d] px-5 py-3 font-bold text-white"
            onClick={() => {
              void experienceQuery.refetch();
              void sceneQuery.refetch();
            }}
          >
            Yenidən yoxla
          </button>
        </div>
      </div>
    );
  }
  if (!restaurant || !branchScene || !branch) {
    return (
      <div className="grid min-h-[65vh] place-items-center px-4 text-center">
        <div>
          <h1 className="font-serif text-4xl">Aktiv filial tapılmadı</h1>
          <p className="mt-3 text-[#70655c]">Virtual tur üçün aktiv filial və real masa planı tələb olunur.</p>
        </div>
      </div>
    );
  }

  return (
    <main>
      <section ref={heroRef} className="relative bg-[#17110e] px-4 pb-20 pt-8 text-white sm:px-6 lg:px-8">
        <div className="mx-auto max-w-[92rem]">
          <div ref={heroCopyRef} className="relative z-20 mb-10 grid gap-7 pt-8 lg:grid-cols-[1fr_auto] lg:items-end">
            <div className="max-w-3xl">
            <p className="text-xs font-bold uppercase tracking-[0.28em] text-[#f09a73]">
              {restaurant.name} · Virtual Experience
            </p>
            <h1 className="mt-4 font-serif text-5xl leading-[0.94] sm:text-7xl">
              Restoranı içəridən kəşf edin
            </h1>
            <p className="mt-5 max-w-xl leading-7 text-white/68">
              Real masa planında zonaları gəzin, uyğun masanı seçin və rezervasiyaya davam edin.
            </p>
            </div>
            <div className="flex flex-wrap gap-3 lg:justify-end">
              <button
                type="button"
                onClick={() => startTour("guided")}
                className="inline-flex items-center gap-2 rounded-full bg-[#ef6542] px-6 py-3 font-bold text-white"
              >
                <Compass className="h-4 w-4" aria-hidden /> Restoranı kəşf et
              </button>
              <button
                type="button"
                onClick={() => controlsRef.current?.scrollIntoView({ behavior: reducedMotion ? "auto" : "smooth" })}
                className="rounded-full border border-white/25 bg-white/10 px-6 py-3 font-bold backdrop-blur"
              >
                Masa rezerv et
              </button>
            </div>
          </div>
          <div id="scene-shell" className="relative z-10">
            <Suspense fallback={<SceneLoadingScreen />}>
              <RestaurantScene scene={branchScene} tables={tables} reducedMotion={reducedMotion} />
            </Suspense>
          </div>
        </div>
      </section>

      <section ref={controlsRef} className="mx-auto max-w-7xl px-4 py-14 sm:px-6 lg:px-8">
        <div className="grid gap-6 rounded-3xl border border-[#ddd4ca] bg-white p-5 shadow-sm md:grid-cols-4 md:p-7">
          <label className="text-sm font-bold text-[#51463e]">
            Filial
            <select
              value={selectedBranchId ?? ""}
              onChange={(event) => setSelectedBranchId(event.target.value)}
              className="mt-2 h-11 w-full rounded-xl border border-[#d8cec3] px-3 font-normal"
            >
              {restaurant.branches.map((item) => (
                <option key={item.id} value={item.id}>{item.name}</option>
              ))}
            </select>
          </label>
          <label className="text-sm font-bold text-[#51463e]">
            <span className="flex items-center gap-1.5"><CalendarDays className="h-4 w-4" aria-hidden /> Tarix</span>
            <input
              type="date"
              min={getTodayInputValue()}
              value={reservationDate}
              onChange={(event) => {
                const date = event.target.value;
                setReservationDate(date);
                const nextSlots = generateTimeSlots(branch.workingHours, date, durationMinutes, { timeZoneId: branch.timeZoneId });
                setStartTime(nextSlots[0] ?? "");
                setSelectedTableId(null);
              }}
              className="mt-2 h-11 w-full rounded-xl border border-[#d8cec3] px-3 font-normal"
            />
          </label>
          <label className="text-sm font-bold text-[#51463e]">
            Saat
            <select
              value={startTime}
              onChange={(event) => {
                setStartTime(event.target.value);
                setSelectedTableId(null);
              }}
              className="mt-2 h-11 w-full rounded-xl border border-[#d8cec3] px-3 font-normal"
            >
              {slots.length === 0 && <option value="">Uyğun saat yoxdur</option>}
              {slots.map((slot) => <option key={slot} value={slot}>{slot}</option>)}
            </select>
          </label>
          <label className="text-sm font-bold text-[#51463e]">
            <span className="flex items-center gap-1.5"><Users className="h-4 w-4" aria-hidden /> Qonaq sayı</span>
            <input
              type="number"
              min={1}
              max={50}
              value={guestCount}
              onChange={(event) => {
                setGuestCount(Math.max(1, Number(event.target.value)));
                setSelectedTableId(null);
              }}
              className="mt-2 h-11 w-full rounded-xl border border-[#d8cec3] px-3 font-normal"
            />
          </label>
        </div>
        {availabilityQuery.isFetching && (
          <p className="mt-4 flex items-center gap-2 text-sm text-[#70655c]" aria-live="polite">
            <LoaderCircle className="h-4 w-4 animate-spin" aria-hidden /> Masa uyğunluğu yenilənir
          </p>
        )}
        {availabilityQuery.isError && (
          <p className="mt-4 rounded-2xl bg-red-50 p-4 text-sm font-semibold text-red-800">
            {getPublicApiError(availabilityQuery.error).message}
          </p>
        )}

        <div className="mt-8 grid gap-6 lg:grid-cols-[1fr_22rem]">
          <AccessibleTableList
            tables={tables}
            selectedTableId={selectedTableId}
            onFocus={(table) => setSelectedTableId(table.id)}
            onSelect={(table) => setSelectedTableId(table.id)}
          />
          <SelectedTablePanel
            table={selectedTable}
            reservationDate={reservationDate}
            startTime={startTime}
            onReserve={reserveSelectedTable}
          />
        </div>
      </section>
    </main>
  );
}

function findFirstReservationSlot(branch: PublicBranch) {
  const now = new Date();
  for (let offset = 0; offset < 14; offset += 1) {
    const date = new Date(now);
    date.setDate(now.getDate() + offset);
    const dateValue = getTodayInputValue(date);
    const slots = generateTimeSlots(branch.workingHours, dateValue, durationMinutes, {
      timeZoneId: branch.timeZoneId,
      now,
    });
    if (slots.length > 0) return { date: dateValue, time: slots[0] };
  }
  return { date: getTodayInputValue(now), time: "" };
}

function mergeAvailability(
  sceneTables: PublicSceneTable[],
  availableTables: PublicRestaurantTable[] | undefined,
  checking: boolean,
): PublicRestaurantTable[] {
  const availability = new Map(availableTables?.map((table) => [table.id, table]));
  return sceneTables.map((table) => {
    const current = availability.get(table.id);
    if (current && !checking) return current;
    if (current) {
      return { ...current, isAvailable: false, unavailableReason: "Disabled" };
    }
    return {
      ...table,
      isAvailable: false,
      unavailableReason: checking ? "Disabled" : table.status,
    };
  });
}
