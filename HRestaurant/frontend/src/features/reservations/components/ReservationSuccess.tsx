import { CheckCircle2, Copy, ExternalLink, Mail } from "lucide-react";
import { Link } from "react-router-dom";
import type { PublicReservationCreated } from "@/types/public";

interface ReservationSuccessProps {
  reservation: PublicReservationCreated;
  onCreateAnother: () => void;
}

export function ReservationSuccess({
  reservation,
  onCreateAnother,
}: ReservationSuccessProps) {
  const trackingUrl = `/reservation/track?token=${encodeURIComponent(
    reservation.trackingToken,
  )}`;

  return (
    <div className="mx-auto max-w-2xl rounded-[32px] border border-emerald-200 bg-white p-6 text-center shadow-xl shadow-emerald-900/5 sm:p-10">
      <span className="mx-auto grid h-16 w-16 place-items-center rounded-full bg-emerald-100 text-emerald-700">
        <CheckCircle2 className="h-9 w-9" />
      </span>
      <p className="mt-5 text-xs font-bold uppercase tracking-[0.24em] text-emerald-700">
        Rezervasiya yaradıldı
      </p>
      <h3 className="mt-2 font-serif text-3xl">
        Sizi gözləyirik!
      </h3>
      <p className="mt-3 text-[#6b6159]">
        {reservation.restaurantName} · {reservation.branchName} · Masa{" "}
        {reservation.tableNumber}
      </p>
      <div className="mx-auto mt-7 max-w-sm rounded-3xl bg-[#f5f1eb] p-5">
        <p className="text-xs font-bold uppercase tracking-[0.18em] text-[#857970]">
          Təsdiq kodu
        </p>
        <p className="mt-2 font-mono text-3xl font-bold tracking-wider">
          {reservation.confirmationCode}
        </p>
        <button
          type="button"
          onClick={() =>
            navigator.clipboard?.writeText(reservation.confirmationCode)
          }
          className="mt-3 inline-flex items-center gap-1.5 text-xs font-bold text-[#9a4130]"
        >
          <Copy className="h-3.5 w-3.5" />
          Kodu kopyala
        </button>
      </div>
      {reservation.emailDeliveryQueued && (
        <p className="mt-5 inline-flex items-center gap-2 text-sm text-[#6b6159]">
          <Mail className="h-4 w-4" />
          Təsdiq e-poçtu göndərilmək üçün növbəyə əlavə edildi.
        </p>
      )}
      <div className="mt-7 flex flex-col justify-center gap-3 sm:flex-row">
        <Link
          to={trackingUrl}
          className="inline-flex items-center justify-center gap-2 rounded-full bg-[#b5422d] px-6 py-3 font-bold text-white"
        >
          Rezervasiyanı aç
          <ExternalLink className="h-4 w-4" />
        </Link>
        <button
          type="button"
          onClick={onCreateAnother}
          className="rounded-full border border-[#d9d0c6] px-6 py-3 font-bold"
        >
          Yeni rezervasiya
        </button>
      </div>
      <p className="mt-6 text-xs leading-5 text-[#8b8077]">
        Təhlükəsiz tracking token yalnız bu ekranda göstərilir. Linki
        saxlayın və ictimai şəkildə paylaşmayın.
      </p>
    </div>
  );
}
