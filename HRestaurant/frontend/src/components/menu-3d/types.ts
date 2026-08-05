import type { PublicFood3D, PublicMenuItem } from "@/types/public";

export type MenuViewerMode = "model" | "video";
export type MenuCardItem = PublicMenuItem & { categoryName: string };
export interface Food3DModalProps { item: MenuCardItem; onClose: () => void }
export interface Food3DViewerProps { food: PublicFood3D; onModelError?: () => void }

export const ingredientPartNames: Record<string, string> = {
  TopBun: "Üst çörək", Lettuce: "Kahı", Tomato: "Pomidor", Cheese: "Pendir",
  Patty: "Ət", Sauce: "Sous", BottomBun: "Alt çörək",
};
