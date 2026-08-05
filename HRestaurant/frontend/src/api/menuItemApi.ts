import { getData, getPage, send } from "@/api/apiClient";
import type { ListParams, MenuItem } from "@/api/contracts";
export interface MenuItemInput {
  name: string; price: number; discountPercentage: number; preparationTimeMinutes: number;
  desc: string; categoryId: string; nutrition: string; imageUrl?: string;
  model3DUrl?: string; modelPosterUrl?: string; modelScale: number;
  modelRotationX: number; modelRotationY: number; modelRotationZ: number;
  is3DEnabled: boolean;
  enableIngredientAnimation: boolean;
  videoUrl?: string; videoPosterUrl?: string; videoDurationSeconds?: number;
  isVideoEnabled: boolean; videoDisplayOrder: number;
  image?: File; ingredients?: Array<{ ingredientId: string; requiredQuantity: number }>;
}
function form(input: MenuItemInput) {
  const data = new FormData();
  data.append("name", input.name); data.append("price", String(input.price));
  data.append("discountPercentage", String(input.discountPercentage));
  data.append("preparationTimeMinutes", String(input.preparationTimeMinutes));
  data.append("desc", input.desc); data.append("categoryId", input.categoryId);
  data.append("nutrition", input.nutrition);
  if (input.model3DUrl) data.append("model3DUrl", input.model3DUrl);
  if (input.modelPosterUrl) data.append("modelPosterUrl", input.modelPosterUrl);
  data.append("modelScale", String(input.modelScale));
  data.append("modelRotationX", String(input.modelRotationX));
  data.append("modelRotationY", String(input.modelRotationY));
  data.append("modelRotationZ", String(input.modelRotationZ));
  data.append("is3DEnabled", String(input.is3DEnabled));
  data.append("enableIngredientAnimation", String(input.enableIngredientAnimation));
  if (input.videoUrl) data.append("videoUrl", input.videoUrl);
  if (input.videoPosterUrl) data.append("videoPosterUrl", input.videoPosterUrl);
  if (input.videoDurationSeconds != null) data.append("videoDurationSeconds", String(input.videoDurationSeconds));
  data.append("isVideoEnabled", String(input.isVideoEnabled));
  data.append("videoDisplayOrder", String(input.videoDisplayOrder));
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
