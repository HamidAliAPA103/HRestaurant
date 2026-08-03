import { getData, getPage, send } from "@/api/apiClient";
import type { ListParams, RestaurantDto, RestaurantInput, WorkingHour } from "@/api/contracts";
import { serializeWorkingHours } from "@/api/workingHours";

export const restaurantKeys = { all: ["restaurants"] as const, detail: (id: string) => ["restaurants", id] as const };
export const restaurantApi = {
  list: ({ signal, ...params }: ListParams = {}) => getPage<RestaurantDto>("/Restaurant", { params, signal }),
  current: (signal?: AbortSignal) => getData<RestaurantDto>("/Restaurant/current", { signal }),
  get: (id: string, signal?: AbortSignal) => getData<RestaurantDto>(`/Restaurant/${id}`, { signal }),
  create: (input: RestaurantInput) => send<string>("post", "/Restaurant", input),
  update: (id: string, input: Partial<RestaurantInput>) => send("patch", `/Restaurant/${id}`, input),
  remove: (id: string) => send("delete", `/Restaurant/${id}`),
  setActive: (id: string, active: boolean) => send("patch", `/Restaurant/${id}/${active ? "activate" : "deactivate"}`),
  updateSettings: (id: string, currency: string, taxRate: number) => send("put", `/Restaurant/${id}/settings`, { currency, taxRate }),
  updateWorkingHours: (id: string, workingHours: WorkingHour[]) => send("put", `/Restaurant/${id}/working-hours`, {
    workingHours: serializeWorkingHours(workingHours),
  }),
};
