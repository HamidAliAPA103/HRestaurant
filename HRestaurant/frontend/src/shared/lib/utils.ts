import axios, { type AxiosError } from "axios";
import type { ApiResponse } from "@/shared/types/api";

export function cn(
  ...values: Array<string | false | null | undefined>
) {
  return values.filter(Boolean).join(" ");
}

export function formatCurrency(value: number) {
  return new Intl.NumberFormat("az-AZ", {
    style: "currency",
    currency: "AZN",
  }).format(value);
}

export function formatDate(value?: string | null, withTime = false) {
  if (!value) return "—";

  return new Intl.DateTimeFormat("az-AZ", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    ...(withTime
      ? { hour: "2-digit", minute: "2-digit" }
      : {}),
  }).format(new Date(value));
}

export function initials(value: string) {
  return value
    .split(/[\s@._-]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join("");
}

export function getErrorMessage(error: unknown) {
  const axiosError = error as AxiosError<ApiResponse<unknown>>;

  if (axios.isAxiosError(error)) {
    if (error.code === "ECONNABORTED") return "Sorğunun vaxt limiti bitdi. Yenidən cəhd edin.";
    if (!error.response) return "Serverə qoşulmaq mümkün olmadı. İnternet bağlantısını və serveri yoxlayın.";
    if (error.response.status === 429) return "Çox sayda sorğu göndərildi. Bir qədər sonra yenidən cəhd edin.";
  }

  return (
    axiosError.response?.data?.message ??
    (error instanceof Error ? error.message : "Gözlənilməz xəta baş verdi.")
  );
}

export function shortId(id?: string | null) {
  return id ? `#${id.slice(0, 6).toUpperCase()}` : "—";
}
