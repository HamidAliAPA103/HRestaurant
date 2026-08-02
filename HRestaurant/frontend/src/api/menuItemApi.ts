import { getData, getPage, send } from "@/api/apiClient";
import type { ListParams, MenuItem } from "@/api/contracts";
export interface MenuItemInput {
  name: string; price: number; discountPercentage: number; preparationTimeMinutes: number;
  desc: string; categoryId: string; nutrition: string; imageUrl?: string;
  image?: File; ingredients?: Array<{ ingredientId: string; requiredQuantity: number }>;
}
function form(input: MenuItemInput) {
  const data = new FormData();
  data.append("name", input.name); data.append("price", String(input.price));
  data.append("discountPercentage", String(input.discountPercentage));
  data.append("preparationTimeMinutes", String(input.preparationTimeMinutes));
  data.append("desc", input.desc); data.append("categoryId", input.categoryId);
  data.append("nutrition", input.nutrition);
  if (input.imageUrl) data.append("imageUrl", input.imageUrl);
  if (input.image) data.append("image", input.image);
  (input.ingredients ?? []).forEach((item, index) => {
    data.append(`ingredients[${index}].ingredientId`, item.ingredientId);
    data.append(`ingredients[${index}].requiredQuantity`, String(item.requiredQuantity));
  });
  return data;
}
export const menuItemKeys = { all: ["menu-items"] as const };
export const menuItemApi = {
  list: ({ signal, ...params }: ListParams & { restaurantId?: string; categoryId?: string; isAvailable?: boolean; isPopular?: boolean } = {}) => getPage<MenuItem>("/Menu", { params, signal }),
  get: (id: string, signal?: AbortSignal) => getData<MenuItem>(`/Menu/${id}`, { signal }),
  create: (input: MenuItemInput) => send<string>("post", "/Menu", form(input)),
  update: (id: string, input: MenuItemInput) => send("put", `/Menu/${id}`, form(input)),
  remove: (id: string) => send("delete", `/Menu/${id}`),
  setAvailability: (id: string, value: boolean) => send("patch", `/Menu/${id}/availability/${value}`),
  setPopular: (id: string, value: boolean) => send("patch", `/Menu/${id}/popular/${value}`),
};
