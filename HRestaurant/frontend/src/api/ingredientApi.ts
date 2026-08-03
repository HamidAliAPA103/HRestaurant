import { getData, getPage, send } from "@/api/apiClient";
import type { IngredientUnit, ListParams } from "@/api/contracts";
export interface IngredientDto { id: string; restaurantId: string; name: string; unit: IngredientUnit; isActive: boolean; creatAt: string; updateAt: string | null }
export const ingredientKeys = { all: ["ingredients"] as const };
const ingredientRoute = "/Ingredient";
export const ingredientApi = {
  list: ({ signal, ...params }: ListParams & { restaurantId?: string; isActive?: boolean } = {}) => getPage<IngredientDto>(ingredientRoute, { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<IngredientDto>(`${ingredientRoute}/${id}`, { signal }),
  create: (input: { restaurantId: string; name: string; unit: IngredientUnit }) => send<string>("post", ingredientRoute, input),
  update: (id: string, input: { name: string; unit: IngredientUnit; isActive: boolean }) => send("put", `${ingredientRoute}/${id}`, input),
  remove: (id: string) => send("delete", `${ingredientRoute}/${id}`),
};
