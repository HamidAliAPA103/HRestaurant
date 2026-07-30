import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery } from "@tanstack/react-query";
import {
  CalendarDays,
  CheckCircle2,
  Clock3,
  LoaderCircle,
  MapPin,
  Search,
  Users,
  XCircle,
} from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useSearchParams } from "react-router-dom";
import {
  getPublicApiError,
  lookupPublicReservation,
  trackPublicReservation,
} from "@/api/public-api";
import {
  reservationLookupSchema,
  type ReservationLookupFormValue,
} from "@/schemas/public-reservation-schema";
import type { PublicReservationDetails } from "@/types/public";
import { formatDate, formatTime } from "@/utils/reservation-date";
import { ReservationCancelDialog } from "../components/ReservationCancelDialog";

export function ReservationTrackingPage() {
  const [searchParams] = useSearchParams();
  const urlToken = searchParams.get("token") ?? "";
  const [details, setDetails] =
    useState<PublicReservationDetails | null>(null);
  const [lookupIdentity, setLookupIdentity] =
    useState<ReservationLookupFormValue | null>(
      urlToken ? { trackingToken: urlToken } : null,
    );
  const [showCancel, setShowCancel] = useState(
    searchParams.get("action") === "cancel",
  );

  useEffect(() => {
    document.title = "Rezervasiyanı yoxla · HRestaurant";
  }, []);

  const tokenQuery = useQuery({
    queryKey: ["public-reservation-track", urlToken],
    queryFn: () => trackPublicReservation(urlToken),
    enabled: Boolean(urlToken),
  });

  useEffect(() => {
    if (tokenQuery.data) {
      setDetails(tokenQuery.data);
    }
  }, [tokenQuery.data]);

  const lookupMutation = useMutation({
    mutationFn: lookupPublicReservation,
    onSuccess: setDetails,
  });
  const form = useForm<ReservationLookupFormValue>({
    resolver: zodResolver(reservationLookupSchema),
    defaultValues: {
      confirmationCode: "",
      phone: "",
      trackingToken: "",
    },
  });

  const submitLookup = (value: ReservationLookupFormValue) => {
    const normalized = {
      confirmationCode: value.confirmationCode || undefined,
      phone: value.phone || undefined,
      trackingToken: value.trackingToken || undefined,
    };
    setLookupIdentity(normalized);
    lookupMutation.mutate(normalized);
  };

  const isLoading = tokenQuery.isPending && Boolean(urlToken);
  const error = tokenQuery.error || lookupMutation.error;

  return (
    <section className="mx-auto min-h-[75vh] max-w-5xl px-4 py-16 sm:px-6 lg:px-8">
      <div className="mx-auto max-w-2xl text-center">
        <p className="text-xs font-bold uppercase tracking-[0.25em] text-[#a5422f]">
          Public tracking
        </p>
        <h1 className="mt-3 font-serif text-4xl sm:text-5xl">
          Rezervasiyanızı yoxlayın
        </h1>
        <p className="mt-4 leading-7 text-[#70655c]">
          Təsdiq kodu və telefonla, yaxud təhlükəsiz tracking token ilə
          rezervasiyanın vəziyyətini görün.
        </p>
      </div>

      {!urlToken && (
        <form
          onSubmit={form.handleSubmit(submitLookup)}
          className="mx-auto mt-10 max-w-3xl rounded-[30px] border border-[#ddd4ca] bg-white p-6 shadow-xl shadow-black/5 sm:p-8"
        >
          <div className="grid gap-4 sm:grid-cols-2">
            <label>
              <span className="mb-2 block text-sm font-bold">
                Təsdiq kodu
              </span>
              <input
                {...form.register("confirmationCode")}
                placeholder="RSV-8F3K2M"
                className={inputClass}
              />
            </label>
            <label>
              <span className="mb-2 block text-sm font-bold">Telefon</span>
              <input
                {...form.register("phone")}
                type="tel"
                placeholder="+994 50 123 45 67"
                className={inputClass}
              />
            </label>
          </div>
          <div className="my-5 flex items-center gap-3 text-xs font-bold uppercase tracking-[0.2em] text-[#9b9189]">
            <span className="h-px flex-1 bg-[#e4ddd5]" />
            və ya
            <span className="h-px flex-1 bg-[#e4ddd5]" />
          </div>
          <label>
            <span className="mb-2 block text-sm font-bold">
              Tracking token
            </span>
            <input
              {...form.register("trackingToken")}
              className={inputClass}
              placeholder="64 simvolluq təhlükəsiz token"
            />
          </label>
          {form.formState.errors.confirmationCode && (
            <p className="mt-3 text-sm font-semibold text-red-700">
              {form.formState.errors.confirmationCode.message}
            </p>
          )}
          <button
            type="submit"
            disabled={lookupMutation.isPending}
            className="mt-6 inline-flex w-full items-center justify-center gap-2 rounded-full bg-[#b5422d] px-6 py-3 font-bold text-white disabled:opacity-60"
          >
            {lookupMutation.isPending ? (
              <LoaderCircle className="h-4 w-4 animate-spin" />
            ) : (
              <Search className="h-4 w-4" />
            )}
            Rezervasiyanı tap
          </button>
        </form>
      )}

      {isLoading && (
        <p className="mt-10 flex items-center justify-center gap-2 text-[#70655c]">
          <LoaderCircle className="h-5 w-5 animate-spin" />
          Rezervasiya yoxlanılır...
        </p>
      )}

      {error && (
        <p
          role="alert"
          className="mx-auto mt-8 max-w-3xl rounded-2xl bg-red-50 p-4 text-center font-semibold text-red-800"
        >
          {getPublicApiError(error).message}
        </p>
      )}

      {details && (
        <ReservationDetailsCard
          details={details}
          onCancel={() => setShowCancel(true)}
        />
      )}

      {showCancel && details && lookupIdentity && (
        <ReservationCancelDialog
          confirmationCode={details.confirmationCode}
          phone={lookupIdentity.phone}
          trackingToken={lookupIdentity.trackingToken}
          onClose={() => setShowCancel(false)}
          onCancelled={() => {
            setShowCancel(false);
            setDetails({
              ...details,
              status: "Cancelled",
              canCancel: false,
              cancelledAt: new Date().toISOString(),
            });
          }}
        />
      )}
    </section>
  );
}

interface ReservationDetailsCardProps {
  details: PublicReservationDetails;
  onCancel: () => void;
}

function ReservationDetailsCard({
  details,
  onCancel,
}: ReservationDetailsCardProps) {
  const cancelled = details.status === "Cancelled";

  return (
    <article className="mx-auto mt-10 max-w-3xl overflow-hidden rounded-[32px] border border-[#ddd4ca] bg-white shadow-xl shadow-black/5">
      <header className="flex flex-col gap-4 bg-[#241f1a] p-6 text-white sm:flex-row sm:items-center sm:justify-between sm:p-8">
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.2em] text-white/50">
            {details.confirmationCode}
          </p>
          <h2 className="mt-2 font-serif text-3xl">
            {details.restaurantName}
          </h2>
        </div>
        <span
          className={`inline-flex w-fit items-center gap-2 rounded-full px-3 py-1.5 text-sm font-bold ${
            cancelled
              ? "bg-red-400/20 text-red-100"
              : "bg-emerald-400/20 text-emerald-100"
          }`}
        >
          {cancelled ? (
            <XCircle className="h-4 w-4" />
          ) : (
            <CheckCircle2 className="h-4 w-4" />
          )}
          {details.status}
        </span>
      </header>
      <div className="grid gap-4 p-6 sm:grid-cols-2 sm:p-8">
        <Detail
          icon={MapPin}
          label={details.branchName}
          value={details.branchAddress}
        />
        <Detail
          icon={CalendarDays}
          label={formatDate(details.reservationDate)}
          value={`${formatTime(details.startTime)}–${formatTime(details.endTime)}`}
        />
        <Detail
          icon={Users}
          label={`${details.guestCount} qonaq`}
          value={`Masa ${details.tableNumber}`}
        />
        <Detail
          icon={Clock3}
          label={details.fullName}
          value={[details.maskedPhone, details.maskedEmail]
            .filter(Boolean)
            .join(" · ")}
        />
      </div>
      {details.specialNotes && (
        <p className="mx-6 mb-6 rounded-2xl bg-stone-50 p-4 text-sm text-[#665c54] sm:mx-8 sm:mb-8">
          {details.specialNotes}
        </p>
      )}
      {details.canCancel && (
        <div className="border-t border-[#eee8e1] p-6 text-right sm:px-8">
          <button
            type="button"
            onClick={onCancel}
            className="rounded-full border border-red-200 px-5 py-2.5 text-sm font-bold text-red-700 transition hover:bg-red-50"
          >
            Rezervasiyanı ləğv et
          </button>
        </div>
      )}
    </article>
  );
}

interface DetailProps {
  icon: typeof MapPin;
  label: string;
  value: string;
}

function Detail({ icon: Icon, label, value }: DetailProps) {
  return (
    <div className="flex gap-3 rounded-2xl bg-[#faf8f4] p-4">
      <Icon className="mt-0.5 h-4 w-4 shrink-0 text-[#a5422f]" />
      <div>
        <p className="font-bold">{label}</p>
        <p className="mt-1 text-sm text-[#766b62]">{value}</p>
      </div>
    </div>
  );
}

const inputClass =
  "w-full rounded-2xl border border-[#d9d0c6] bg-white px-4 py-3 outline-none transition focus:border-[#b5422d] focus:ring-4 focus:ring-[#b5422d]/10";
