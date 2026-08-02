import { getData, getPage, send } from "@/api/apiClient";
import type { IngredientUnit, ListParams } from "@/api/contracts";
export interface IngredientDto { id: string; restaurantId: string; name: string; unit: IngredientUnit; isActive: boolean; creatAt: string; updateAt: string | null }
export const ingredientKeys = { all: ["ingredients"] as const };
export const ingredientApi = {
  list: ({ signal, ...params }: ListParams & { restaurantId?: string; isActive?: boolean } = {}) => getPage<IngredientDto>("/ingredients", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<IngredientDto>(`/ingredients/${id}`, { signal }),
  create: (input: { restaurantId: string; name: string; unit: IngredientUnit }) => send<string>("post", "/ingredients", input),
  update: (id: string, input: { name: string; unit: IngredientUnit; isActive: boolean }) => send("put", `/ingredients/${id}`, input),
  remove: (id: string) => send("delete", `/ingredients/${id}`),
};
