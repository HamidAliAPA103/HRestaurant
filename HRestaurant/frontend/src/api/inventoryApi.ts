import { getData, getPage, send } from "@/api/apiClient";
import type { InventoryDto, InventoryInput, ListParams } from "@/api/contracts";
export const inventoryKeys = { all: ["inventory"] as const, detail: (id: string) => ["inventory", id] as const };
export const inventoryApi = {
  list: ({ signal, ...params }: ListParams & { restaurantId?: string; branchId?: string; ingredientId?: string; supplierId?: string; isActive?: boolean } = {}) => getPage<InventoryDto>("/inventory", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<InventoryDto>(`/inventory/${id}`, { signal }),
  create: (input: InventoryInput) => send<string>("post", "/inventory", input),
  update: (id: string, input: Omit<InventoryInput, "restaurantId" | "branchId" | "ingredientId" | "currentQuantity"> & { isActive: boolean; rowVersion: string }) => send("put", `/inventory/${id}`, input),
  remove: (id: string) => send("delete", `/inventory/${id}`),
  stockIn: (id: string, input: object) => send<InventoryDto>("post", `/inventory/${id}/stock-in`, input),
  stockOut: (id: string, input: object) => send<InventoryDto>("post", `/inventory/${id}/stock-out`, input),
  adjust: (id: string, input: object) => send<InventoryDto>("post", `/inventory/${id}/adjust`, input),
};
