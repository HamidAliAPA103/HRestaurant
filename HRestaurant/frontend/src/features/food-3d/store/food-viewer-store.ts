import { create } from "zustand";

interface FoodViewerState {
  isExploded: boolean;
  hoveredIngredientId: string | null;
  selectedIngredientId: string | null;
  setExploded: (value: boolean) => void;
  setHoveredIngredient: (id: string | null) => void;
  setSelectedIngredient: (id: string | null) => void;
  reset: () => void;
}

const initialState = {
  isExploded: false,
  hoveredIngredientId: null,
  selectedIngredientId: null,
};

export const useFoodViewerStore = create<FoodViewerState>((set) => ({
  ...initialState,
  setExploded: (isExploded) => set({ isExploded }),
  setHoveredIngredient: (hoveredIngredientId) =>
    set({ hoveredIngredientId }),
  setSelectedIngredient: (selectedIngredientId) =>
    set({ selectedIngredientId }),
  reset: () => set(initialState),
}));
