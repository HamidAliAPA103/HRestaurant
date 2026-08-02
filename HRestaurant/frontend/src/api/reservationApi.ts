import { getData, getPage, send } from "@/api/apiClient";
import type { ListParams, ReservationDto, ReservationInput } from "@/api/contracts";
import type { ReservationStatus } from "@/shared/types/domain";
export const reservationKeys = { all: ["reservations"] as const, detail: (id: string) => ["reservations", id] as const };
export const reservationApi = {
  list: ({ signal, ...params }: ListParams & { branchId?: string; tableId?: string; status?: ReservationStatus; from?: string; to?: string } = {}) => getPage<ReservationDto>("/reservations", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<ReservationDto>(`/reservations/${id}`, { signal }),
  create: (input: ReservationInput) => send<string>("post", "/reservations", input),
  update: (id: string, input: ReservationInput) => send("put", `/reservations/${id}`, input),
  setStatus: (id: string, status: ReservationStatus, reason?: string) => send("patch", `/reservations/${id}/status`, { status, reason }),
  remove: (id: string) => send("delete", `/reservations/${id}`),
};
