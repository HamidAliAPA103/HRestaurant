import { getData, getPage, send } from "@/api/apiClient";
import type { DiningTable, ListParams, TableStatus } from "@/api/contracts";
export interface TableInput {
  restaurantId?: string; branchId?: string; tableNumber: string; capacity: number;
  status?: TableStatus; shape: number; positionX: number; positionY: number; positionZ: number;
  rotationX: number; rotationY: number; rotationZ: number; width: number; length: number;
  height: number; isActive?: boolean;
}
export const tableKeys = { all: ["tables"] as const };
export const tableApi = {
  list: ({ signal, ...params }: ListParams & { restaurantId?: string; branchId?: string; status?: TableStatus; isActive?: boolean } = {}) => getPage<DiningTable>("/tables", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<DiningTable>(`/tables/${id}`, { signal }),
  create: (input: TableInput & { restaurantId: string; branchId: string }) => send<string>("post", "/tables", input),
  update: (id: string, input: TableInput) => send("put", `/tables/${id}`, input),
  remove: (id: string) => send("delete", `/tables/${id}`),
  setStatus: (id: string, status: TableStatus) => send("patch", `/tables/${id}/status`, { status }),
  setActive: (id: string, active: boolean) => send("patch", `/tables/${id}/${active ? "activate" : "deactivate"}`),
  saveLayout: (branchId: string, tables: object[]) => send("put", "/tables/layout", { branchId, tables }),
};
