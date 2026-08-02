import { getData, getPage, send } from "@/api/apiClient";
import type { ListParams } from "@/api/contracts";
export interface SupplierDto { id: string; restaurantId: string; name: string; contactPerson: string; phone: string; email: string; address: string; isActive: boolean; creatAt: string; updateAt: string | null }
export interface SupplierInput { restaurantId?: string; name: string; contactPerson: string; phone: string; email: string; address: string }
export const supplierKeys = { all: ["suppliers"] as const };
export const supplierApi = {
  list: ({ signal, ...params }: ListParams & { restaurantId?: string; isActive?: boolean } = {}) => getPage<SupplierDto>("/suppliers", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<SupplierDto>(`/suppliers/${id}`, { signal }),
  create: (input: SupplierInput & { restaurantId: string }) => send<string>("post", "/suppliers", input),
  update: (id: string, input: SupplierInput) => send("put", `/suppliers/${id}`, input),
  remove: (id: string) => send("delete", `/suppliers/${id}`),
  setActive: (id: string, active: boolean) => send("patch", `/suppliers/${id}/${active ? "activate" : "deactivate"}`),
};
