import { getData, getPage, send } from "@/api/apiClient";
import type { KitchenDashboard, ListParams, Order, OrderCreateInput, OrderStatus } from "@/api/contracts";
export const orderKeys = { all: ["orders"] as const, kitchen: ["orders", "kitchen"] as const };
export const orderApi = {
  list: ({ signal, ...params }: ListParams & { restaurantId?: string; branchId?: string; waiterId?: string; status?: OrderStatus } = {}) => getPage<Order>("/orders", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<Order>(`/orders/${id}`, { signal }),
  create: (input: OrderCreateInput) => send<string>("post", "/orders", input),
  update: (id: string, input: { notes?: string; isPriority: boolean; rowVersion: string }) => send("put", `/orders/${id}`, input),
  setStatus: (id: string, status: OrderStatus, rowVersion: string) => send("patch", `/orders/${id}/status`, { status, rowVersion }),
  cancel: (id: string, reason: string, requestRefund: boolean, rowVersion: string) => send("post", `/orders/${id}/cancel`, { reason, requestRefund, rowVersion }),
  changeTable: (id: string, tableId: string, rowVersion: string) => send("patch", `/orders/${id}/table`, { tableId, rowVersion }),
  applyDiscount: (id: string, discountPercentage: number, rowVersion: string) => send("post", `/orders/${id}/discount`, { discountPercentage, rowVersion }),
  merge: (id: string, sourceOrderIds: string[], rowVersion: string) => send("post", `/orders/${id}/merge`, { sourceOrderIds, rowVersion }),
  split: (id: string, items: Array<{ orderItemId: string; quantity: number }>, tableId: string | null, rowVersion: string) =>
    send<string>("post", `/orders/${id}/split`, { items, tableId, rowVersion }),
  kitchen: (branchId?: string, signal?: AbortSignal) => getData<KitchenDashboard>("/orders/kitchen", { params: { branchId }, signal }),
};
