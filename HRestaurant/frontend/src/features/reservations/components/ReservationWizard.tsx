import { useMutation, useQuery } from "@tanstack/react-query";
import {
  ArrowLeft,
  ArrowRight,
  Check,
  LoaderCircle,
  RefreshCw,
} from "lucide-react";
import { lazy, Suspense, useEffect } from "react";
import {
  createPublicReservation,
  getAvailableTables,
  getPublicApiError,
} from "@/api/public-api";
import { BranchSelector } from "@/features/public-restaurant/components/BranchSelector";
import { AccessibleTableList } from "@/features/table-3d/components/AccessibleTableList";
import { SelectedTablePanel } from "@/features/table-3d/components/SelectedTablePanel";
import { TableStatusLegend } from "@/features/table-3d/components/TableStatusLegend";
import type { CustomerInformationFormValue } from "@/schemas/public-reservation-schema";
import type { PublicRestaurant } from "@/types/public";
import { CustomerInformationForm } from "./CustomerInformationForm";
import { GuestCountSelector } from "./GuestCountSelector";
import { ReservationDateStep } from "./ReservationDateStep";
import { ReservationSuccess } from "./ReservationSuccess";
import { ReservationSummary } from "./ReservationSummary";
import { ReservationTimeStep } from "./ReservationTimeStep";
import { useReservationStore } from "../store/reservation-store";

const RestaurantHall3D = lazy(() =>
  import("@/features/table-3d/components/RestaurantHall3D").then(
    (module) => ({
      default: module.RestaurantHall3D,
    }),
  ),
);

interface ReservationWizardProps {
  restaurant: PublicRestaurant;
}

const reservationStepTitles = [
  "Filial",
  "Tarix və vaxt",
  "Qonaq sayı",
  "3D masa seçimi",
  "Müştəri məlumatları",
  "Yekun",
  "Uğurlu",
];

export function ReservationWizard({
  restaurant,
}: ReservationWizardProps) {
  const state = useReservationStore();

  useEffect(() => {
    state.setRestaurant(restaurant);

    return () => state.reset();
  }, [restaurant.id]);

  const tableQuery = useQuery({
    queryKey: [
      "public-table-availability",
      state.selectedBranch?.id,
      state.reservationDate,
      state.startTime,
      state.guestCount,
      state.durationMinutes,
    ],
    queryFn: () =>
      getAvailableTables(state.selectedBranch!.id, {
        reservationDate: state.reservationDate,
        startTime: state.startTime,
        guestCount: state.guestCount,
        durationMinutes: state.durationMinutes,
      }),
    enabled: Boolean(
      state.selectedBranch &&
        state.reservationDate &&
        state.startTime,
    ),
  });

  useEffect(() => {
    if (!tableQuery.data) {
      return;
    }

    state.setAvailableTables(tableQuery.data);

    if (
      state.selectedTable &&
      !tableQuery.data.some(
        (table) =>
          table.id === state.selectedTable?.id && table.isAvailable,
      )
    ) {
      state.selectTable(null);
    }
  }, [tableQuery.data]);

  const createMutation = useMutation({
    mutationFn: () =>
      createPublicReservation({
        branchId: state.selectedBranch!.id,
        tableId: state.selectedTable!.id,
        reservationDate: state.reservationDate,
        startTime: state.startTime,
        durationMinutes: state.durationMinutes,
        guestCount: state.guestCount,
        fullName: state.customerInformation!.fullName,
        phone: state.customerInformation!.phone,
        email: state.customerInformation!.email || undefined,
        specialNotes:
          state.customerInformation!.specialNotes || undefined,
        termsAccepted: state.customerInformation!.termsAccepted,
      }),
    onSuccess: state.setSuccess,
    onError: (error) => {
      if (getPublicApiError(error).status === 409) {
        state.selectTable(null);
        state.setCurrentStep(4);
        void tableQuery.refetch();
      }
    },
  });

  if (state.success && state.currentStep === 7) {
    return (
      <ReservationSuccess
        reservation={state.success}
        onCreateAnother={() => {
          state.reset();
          state.setRestaurant(restaurant);
        }}
      />
    );
  }

  return (
    <section
      id="reservation"
      aria-labelledby="reservation-title"
      className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8 lg:py-24"
    >
      <div className="mx-auto mb-10 max-w-2xl text-center">
        <p className="text-xs font-bold uppercase tracking-[0.26em] text-[#a5422f]">
          Onlayn rezervasiya
        </p>
        <h2
          id="reservation-title"
          className="mt-3 font-serif text-4xl tracking-tight sm:text-5xl"
        >
          Axşamınızı planlayın
        </h2>
        <p className="mt-4 leading-7 text-[#70655c]">
          Filial və vaxtı seçin, sonra zalı 3D görünüşdə araşdıraraq
          masanızı rezerv edin.
        </p>
      </div>

      <ol
        aria-label="Rezervasiya addımları"
        className="mx-auto mb-8 grid max-w-5xl grid-cols-7 gap-1 sm:gap-3"
      >
        {reservationStepTitles.map((title, index) => {
          const step = index + 1;
          const completed = step < state.currentStep;
          const active = step === state.currentStep;

          return (
            <li key={title} className="text-center">
              <span
                aria-current={active ? "step" : undefined}
                className={`mx-auto grid h-9 w-9 place-items-center rounded-full text-xs font-bold transition ${
                  active
                    ? "bg-[#b5422d] text-white shadow-lg shadow-[#b5422d]/20"
                    : completed
                      ? "bg-[#496f5a] text-white"
                      : "bg-white text-[#81766d]"
                }`}
              >
                {completed ? <Check className="h-4 w-4" /> : step}
              </span>
              <span className="mt-2 hidden text-[11px] font-bold text-[#756a62] sm:block">
                {title}
              </span>
            </li>
          );
        })}
      </ol>

      <div className="rounded-[34px] border border-[#ddd4ca] bg-white p-5 shadow-xl shadow-[#4a3425]/5 sm:p-8 lg:p-10">
        {state.currentStep === 1 && (
          <WizardPanel
            title="Filialı seçin"
            description="Sizə ən uyğun ünvanı və iş saatlarını yoxlayın."
          >
            <BranchSelector
              branches={restaurant.branches}
              selectedBranch={state.selectedBranch}
              onSelect={state.setBranch}
            />
            <WizardActions
              canContinue={Boolean(state.selectedBranch)}
              onContinue={() => state.setCurrentStep(2)}
            />
          </WizardPanel>
        )}

        {state.currentStep === 2 && state.selectedBranch && (
          <WizardPanel
            title="Tarix və vaxt"
            description={`${state.selectedBranch.name} filialının iş saatlarına uyğun seçim edin.`}
          >
            <div className="grid gap-5 lg:grid-cols-3">
              <ReservationDateStep
                value={state.reservationDate}
                workingHours={state.selectedBranch.workingHours}
                onChange={state.setReservationDate}
              />
              <div className="lg:col-span-2">
                <ReservationTimeStep
                  workingHours={state.selectedBranch.workingHours}
                  reservationDate={state.reservationDate}
                  startTime={state.startTime}
                  durationMinutes={state.durationMinutes}
                  timeZoneId={state.selectedBranch.timeZoneId}
                  onStartTimeChange={state.setStartTime}
                  onDurationChange={state.setDurationMinutes}
                />
              </div>
            </div>
            <WizardActions
              canContinue={Boolean(state.startTime)}
              onBack={() => state.setCurrentStep(1)}
              onContinue={() => state.setCurrentStep(3)}
            />
          </WizardPanel>
        )}

        {state.currentStep === 3 && (
          <WizardPanel
            title="Qonaq sayı"
            description="Masa tutumunu düzgün hesablamaq üçün qonaqların sayını seçin."
          >
            <GuestCountSelector
              value={state.guestCount}
              onChange={state.setGuestCount}
            />
            <WizardActions
              canContinue={state.guestCount > 0}
              onBack={() => state.setCurrentStep(2)}
              onContinue={() => state.setCurrentStep(4)}
            />
          </WizardPanel>
        )}

        {state.currentStep === 4 && (
          <WizardPanel
            title="Masanızı seçin"
            description="Zalı böyüdə, döndərə və masaların üzərinə toxuna bilərsiniz."
          >
            <TableStatusLegend />
            <div className="mt-5">
              {tableQuery.isFetching ? (
                <InlineState
                  icon={LoaderCircle}
                  spin
                  title="Masalar yoxlanılır"
                  message="Seçilən vaxt üçün ən son uyğunluğu hesablayırıq."
                />
              ) : tableQuery.isError ? (
                <InlineState
                  icon={RefreshCw}
                  title="Masaları yükləmək mümkün olmadı"
                  message={getPublicApiError(tableQuery.error).message}
                  action={() => tableQuery.refetch()}
                />
              ) : state.availableTables.length === 0 ? (
                <InlineState
                  icon={RefreshCw}
                  title="Bu filialda aktiv masa yoxdur"
                  message="Başqa filial və ya vaxt seçərək yenidən yoxlayın."
                />
              ) : (
                <div className="space-y-6">
                  <Suspense
                    fallback={
                      <InlineState
                        icon={LoaderCircle}
                        spin
                        title="3D zal hazırlanır"
                        message="İnteraktiv görünüş yüklənir."
                      />
                    }
                  >
                    <RestaurantHall3D
                      tables={state.availableTables}
                      selectedTable={state.selectedTable}
                      reservationDate={state.reservationDate}
                      startTime={state.startTime}
                      onSelect={state.selectTable}
                    />
                  </Suspense>
                  <SelectedTablePanel table={state.selectedTable} />
                  {state.availableTables.every(
                    (table) =>
                      table.unavailableReason === "CapacityNotSuitable",
                  ) && (
                    <p className="rounded-2xl bg-violet-50 p-4 text-sm font-semibold text-violet-900">
                      Bütün masaların tutumu seçilən qonaq sayı üçün
                      kiçikdir. Qonaq sayını dəyişin.
                    </p>
                  )}
                  <AccessibleTableList
                    tables={state.availableTables}
                    selectedTable={state.selectedTable}
                    onSelect={state.selectTable}
                  />
                </div>
              )}
            </div>
            <WizardActions
              canContinue={Boolean(state.selectedTable)}
              onBack={() => state.setCurrentStep(3)}
              onContinue={() => state.setCurrentStep(5)}
            />
          </WizardPanel>
        )}

        {state.currentStep === 5 && (
          <WizardPanel
            title="Əlaqə məlumatları"
            description="Rezervasiyanı təsdiqləmək üçün məlumatlarınızı daxil edin."
          >
            <CustomerInformationForm
              initialValue={state.customerInformation}
              onBack={() => state.setCurrentStep(4)}
              onSubmit={(value: CustomerInformationFormValue) => {
                state.setCustomerInformation(value);
                state.setCurrentStep(6);
              }}
            />
          </WizardPanel>
        )}

        {state.currentStep === 6 &&
          state.selectedBranch &&
          state.selectedTable &&
          state.customerInformation && (
            <WizardPanel
              title="Son yoxlama"
              description="Təsdiqdən əvvəl tarix, masa və əlaqə məlumatlarını yoxlayın."
            >
              <ReservationSummary
                restaurant={restaurant}
                branch={state.selectedBranch}
                table={state.selectedTable}
                reservationDate={state.reservationDate}
                startTime={state.startTime}
                durationMinutes={state.durationMinutes}
                guestCount={state.guestCount}
                customer={state.customerInformation}
              />
              {createMutation.isError && (
                <p className="mt-5 rounded-2xl bg-red-50 p-4 text-sm font-semibold text-red-800">
                  {getPublicApiError(createMutation.error).message}
                  {getPublicApiError(createMutation.error).status === 409 &&
                    " Uyğunluq siyahısı yeniləndi; başqa masa seçin."}
                </p>
              )}
              <div className="mt-7 flex flex-col-reverse gap-3 sm:flex-row sm:justify-between">
                <button
                  type="button"
                  onClick={() => state.setCurrentStep(5)}
                  className="inline-flex items-center justify-center gap-2 rounded-full border border-[#d6ccc1] px-5 py-3 font-bold"
                >
                  <ArrowLeft className="h-4 w-4" />
                  Geri
                </button>
                <button
                  type="button"
                  disabled={createMutation.isPending}
                  onClick={() => createMutation.mutate()}
                  className="inline-flex items-center justify-center gap-2 rounded-full bg-[#b5422d] px-7 py-3 font-bold text-white disabled:opacity-60"
                >
                  {createMutation.isPending && (
                    <LoaderCircle className="h-4 w-4 animate-spin" />
                  )}
                  Rezervasiyanı təsdiqlə
                </button>
              </div>
            </WizardPanel>
          )}
      </div>
    </section>
  );
}

interface WizardPanelProps {
  title: string;
  description: string;
  children: React.ReactNode;
}

function WizardPanel({
  title,
  description,
  children,
}: WizardPanelProps) {
  return (
    <div>
      <div className="mb-7">
        <h3 className="font-serif text-3xl">{title}</h3>
        <p className="mt-2 text-sm leading-6 text-[#756a62]">
          {description}
        </p>
      </div>
      {children}
    </div>
  );
}

interface WizardActionsProps {
  canContinue: boolean;
  onContinue: () => void;
  onBack?: () => void;
}

function WizardActions({
  canContinue,
  onContinue,
  onBack,
}: WizardActionsProps) {
  return (
    <div
      className={`mt-7 flex flex-col-reverse gap-3 sm:flex-row ${
        onBack ? "sm:justify-between" : "sm:justify-end"
      }`}
    >
      {onBack && (
        <button
          type="button"
          onClick={onBack}
          className="inline-flex items-center justify-center gap-2 rounded-full border border-[#d6ccc1] px-5 py-3 font-bold"
        >
          <ArrowLeft className="h-4 w-4" />
          Geri
        </button>
      )}
      <button
        type="button"
        disabled={!canContinue}
        onClick={onContinue}
        className="inline-flex items-center justify-center gap-2 rounded-full bg-[#b5422d] px-6 py-3 font-bold text-white transition hover:bg-[#983622] disabled:cursor-not-allowed disabled:opacity-40"
      >
        Davam et
        <ArrowRight className="h-4 w-4" />
      </button>
    </div>
  );
}

interface InlineStateProps {
  icon: typeof LoaderCircle;
  title: string;
  message: string;
  spin?: boolean;
  action?: () => void;
}

function InlineState({
  icon: Icon,
  title,
  message,
  spin,
  action,
}: InlineStateProps) {
  return (
    <div className="grid min-h-64 place-items-center rounded-[28px] border border-dashed border-[#d7cec4] bg-stone-50 p-8 text-center">
      <div>
        <Icon
          className={`mx-auto h-8 w-8 text-[#a5422f] ${
            spin ? "animate-spin" : ""
          }`}
        />
        <p className="mt-4 font-serif text-2xl">{title}</p>
        <p className="mt-2 text-sm text-[#756a62]">{message}</p>
        {action && (
          <button
            type="button"
            onClick={action}
            className="mt-4 rounded-full border border-[#d0c5ba] bg-white px-4 py-2 text-sm font-bold"
          >
            Yenidən yoxla
          </button>
        )}
      </div>
    </div>
  );
}
