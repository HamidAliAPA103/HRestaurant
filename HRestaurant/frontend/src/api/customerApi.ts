import { getData, getPage, send } from "@/api/apiClient";
import type { CustomerDto, CustomerInput, ListParams } from "@/api/contracts";
export const customerKeys = { all: ["customers"] as const, detail: (id: string) => ["customers", id] as const };
export const customerApi = {
  list: ({ signal, ...params }: ListParams = {}) => getPage<CustomerDto>("/customers", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<CustomerDto>(`/customers/${id}`, { signal }),
  create: (input: CustomerInput & { restaurantId: string }) => send<string>("post", "/customers", input),
  update: (id: string, input: CustomerInput) => send("put", `/customers/${id}`, input),
  remove: (id: string) => send("delete", `/customers/${id}`),
  orders: (id: string, signal?: AbortSignal) => getPage(`/customers/${id}/orders`, { signal }),
  reservations: (id: string, signal?: AbortSignal) => getPage(`/customers/${id}/reservations`, { signal }),
};
