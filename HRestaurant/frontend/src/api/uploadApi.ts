import { send } from "@/api/apiClient";

export interface UploadedImage { url: string; fileName: string; size: number }
export type UploadProgressHandler = (percentage: number) => void;

export const uploadApi = {
  image: (
    file: File,
    category: "restaurant-logo" | "restaurant-cover" | "employee-avatar",
    restaurantId?: string,
    oldImageUrl?: string,
    onProgress?: UploadProgressHandler,
  ) => {
    if (!["image/jpeg", "image/png", "image/webp"].includes(file.type))
      return Promise.reject(new Error("Yalnız JPEG, PNG və WebP şəkilləri qəbul edilir."));
    if (file.size <= 0 || file.size > 5 * 1024 * 1024)
      return Promise.reject(new Error("Şəkil boş ola və ya 5 MB-dan böyük ola bilməz."));
    const form = new FormData(); form.append("file", file); form.append("category", category);
    if (restaurantId) form.append("restaurantId", restaurantId);
    if (oldImageUrl) form.append("oldImageUrl", oldImageUrl);
    return send<UploadedImage>("post", "/uploads/images", form, {
      onUploadProgress: (event) => {
        if (!onProgress || !event.total) return;
        onProgress(Math.min(100, Math.round((event.loaded / event.total) * 100)));
      },
    });
  },
  remove: (imageUrl: string, restaurantId?: string) => send("delete", "/uploads/images", undefined, { params: { imageUrl, restaurantId } }),
};
