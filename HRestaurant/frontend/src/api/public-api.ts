import axios, { AxiosError } from "axios";
import type { ApiResponse } from "@/shared/types/api";
import type {
  PublicBranch,
  PublicCancelReservationRequest,
  PublicCreateReservationRequest,
  PublicReservationCreated,
  PublicReservationDetails,
  PublicReservationLookupRequest,
  PublicRestaurant,
  PublicRestaurantTable,
  PublicTableLayout,
  TableAvailabilityRequest,
} from "@/types/public";

const publicApiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "/api",
  timeout: 15_000,
  headers: {
    Accept: "application/json",
    "Content-Type": "application/json",
  },
});

async function unwrap<T>(request: Promise<{ data: ApiResponse<T> }>) {
  const { data } = await request;

  if (!data.success || data.data === null) {
    throw new Error(data.message);
  }

  return data.data;
}

export function getPublicRestaurant(slug: string) {
  return unwrap<PublicRestaurant>(
    publicApiClient.get(`/public/restaurants/${encodeURIComponent(slug)}`),
  );
}

export function getPublicBranches(restaurantSlug: string) {
  return unwrap<PublicBranch[]>(
    publicApiClient.get(
      `/public/restaurants/${encodeURIComponent(restaurantSlug)}/branches`,
    ),
  );
}

export function getAvailableTables(
  branchId: string,
  request: TableAvailabilityRequest,
) {
  return unwrap<PublicRestaurantTable[]>(
    publicApiClient.get(`/public/branches/${branchId}/available-tables`, {
      params: request,
    }),
  );
}

export function getPublicTableLayout(branchId: string) {
  return unwrap<PublicTableLayout[]>(
    publicApiClient.get(`/public/branches/${branchId}/tables/layout`),
  );
}

export function createPublicReservation(
  request: PublicCreateReservationRequest,
) {
  return unwrap<PublicReservationCreated>(
    publicApiClient.post("/public/reservations", request),
  );
}

export function lookupPublicReservation(
  request: PublicReservationLookupRequest,
) {
  return unwrap<PublicReservationDetails>(
    publicApiClient.post("/public/reservations/lookup", request),
  );
}

export function trackPublicReservation(trackingToken: string) {
  return unwrap<PublicReservationDetails>(
    publicApiClient.get(
      `/public/reservations/track/${encodeURIComponent(trackingToken)}`,
    ),
  );
}

export async function cancelPublicReservation(
  confirmationCode: string,
  request: PublicCancelReservationRequest,
) {
  const { data } = await publicApiClient.post<ApiResponse<null>>(
    `/public/reservations/${encodeURIComponent(confirmationCode)}/cancel`,
    request,
  );

  if (!data.success) {
    throw new Error(data.message);
  }
}

export function getPublicApiError(error: unknown) {
  if (error instanceof AxiosError) {
    const response = error.response?.data as ApiResponse<unknown> | undefined;

    return {
      status: error.response?.status,
      message:
        response?.message ||
        "Sorğu tamamlanmadı. Zəhmət olmasa yenidən cəhd edin.",
      traceId: response?.traceId,
    };
  }

  return {
    status: undefined,
    message:
      error instanceof Error
        ? error.message
        : "Gözlənilməz xəta baş verdi.",
    traceId: undefined,
  };
}
