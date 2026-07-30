import { useMutation } from "@tanstack/react-query";
import { AlertTriangle, X } from "lucide-react";
import { useState } from "react";
import {
  cancelPublicReservation,
  getPublicApiError,
} from "@/api/public-api";

interface ReservationCancelDialogProps {
  confirmationCode: string;
  phone?: string;
  trackingToken?: string;
  onCancelled: () => void;
  onClose: () => void;
}

export function ReservationCancelDialog({
  confirmationCode,
  phone,
  trackingToken,
  onCancelled,
  onClose,
}: ReservationCancelDialogProps) {
  const [reason, setReason] = useState("");
  const mutation = useMutation({
    mutationFn: () =>
      cancelPublicReservation(confirmationCode, {
        phone,
        trackingToken,
        reason,
      }),
    onSuccess: onCancelled,
  });

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="cancel-reservation-title"
      className="fixed inset-0 z-50 grid place-items-center bg-black/55 p-4 backdrop-blur-sm"
    >
      <div className="w-full max-w-lg rounded-[30px] bg-white p-6 shadow-2xl sm:p-8">
        <div className="flex items-start justify-between gap-4">
          <span className="grid h-12 w-12 place-items-center rounded-full bg-amber-100 text-amber-800">
            <AlertTriangle className="h-6 w-6" />
          </span>
          <button
            type="button"
            aria-label="Dialoqu bağla"
            onClick={onClose}
            className="grid h-10 w-10 place-items-center rounded-full bg-stone-100"
          >
            <X className="h-4 w-4" />
          </button>
        </div>
        <h2
          id="cancel-reservation-title"
          className="mt-5 font-serif text-3xl"
        >
          Rezervasiyanı ləğv et
        </h2>
        <p className="mt-2 text-sm leading-6 text-[#6b6159]">
          Bu əməliyyatdan sonra masa yenidən digər qonaqlar üçün
          əlçatan olacaq.
        </p>
        <label className="mt-5 block">
          <span className="mb-2 block text-sm font-bold">
            Səbəb (istəyə bağlı)
          </span>
          <textarea
            value={reason}
            maxLength={300}
            rows={3}
            onChange={(event) => setReason(event.target.value)}
            className="w-full resize-none rounded-2xl border border-[#d9d0c6] px-4 py-3 outline-none focus:border-[#b5422d]"
          />
        </label>
        {mutation.isError && (
          <p className="mt-3 rounded-2xl bg-red-50 p-3 text-sm font-semibold text-red-800">
            {getPublicApiError(mutation.error).message}
          </p>
        )}
        <div className="mt-6 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
          <button
            type="button"
            onClick={onClose}
            className="rounded-full border border-[#d9d0c6] px-5 py-3 font-bold"
          >
            Geri qayıt
          </button>
          <button
            type="button"
            disabled={mutation.isPending}
            onClick={() => mutation.mutate()}
            className="rounded-full bg-red-700 px-5 py-3 font-bold text-white disabled:opacity-60"
          >
            {mutation.isPending ? "Ləğv edilir..." : "Ləğvi təsdiqlə"}
          </button>
        </div>
      </div>
    </div>
  );
}
