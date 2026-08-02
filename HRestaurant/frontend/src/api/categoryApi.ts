import { getData, getPage, send } from "@/api/apiClient";
import type { ListParams } from "@/api/contracts";
export interface CategoryDto { id: string; resdaranId: string; name: string; description: string | null; imageUrl: string | null; displayOrder: number; isActive: boolean; creatAt: string; updateAt: string | null }
export interface CategoryInput { resdaranId?: string; name: string; description?: string; imageUrl?: string; displayOrder: number }
export const categoryKeys = { all: ["menu-categories"] as const };
export const categoryApi = {
  list: ({ signal, ...params }: ListParams & { restaurantId?: string; isActive?: boolean } = {}) => getPage<CategoryDto>("/MenuCategory", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<CategoryDto>(`/MenuCategory/${id}`, { signal }),
  create: (input: CategoryInput & { resdaranId: string }) => send<string>("post", "/MenuCategory", input),
  update: (id: string, input: Omit<CategoryInput, "resdaranId">) => send("put", `/MenuCategory/${id}`, input),
  remove: (id: string) => send("delete", `/MenuCategory/${id}`),
  setActive: (id: string, active: boolean) => send("patch", `/MenuCategory/${id}/${active ? "activate" : "deactivate"}`),
  reorder: (id: string, displayOrder: number) => send("patch", `/MenuCategory/${id}/display-order`, { displayOrder }),
};
