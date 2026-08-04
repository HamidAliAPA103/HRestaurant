import { create } from "zustand";

interface FoodViewerState {
  isExploded: boolean;
  autoRotate: boolean;
  viewResetVersion: number;
  hoveredIngredientId: string | null;
  selectedIngredientId: string | null;
  setExploded: (value: boolean) => void;
  setAutoRotate: (value: boolean) => void;
  resetView: () => void;
  setHoveredIngredient: (id: string | null) => void;
  setSelectedIngredient: (id: string | null) => void;
  reset: () => void;
}

const initialState = {
  isExploded: false,
  autoRotate: false,
  viewResetVersion: 0,
  hoveredIngredientId: null,
  selectedIngredientId: null,
};

export const useFoodViewerStore = create<FoodViewerState>((set) => ({
  ...initialState,
  setExploded: (isExploded) => set({ isExploded }),
  setAutoRotate: (autoRotate) => set({ autoRotate }),
  resetView: () =>
    set((state) => ({
      viewResetVersion: state.viewResetVersion + 1,
      selectedIngredientId: null,
    })),
  setHoveredIngredient: (hoveredIngredientId) =>
    set({ hoveredIngredientId }),
  setSelectedIngredient: (selectedIngredientId) =>
    set({ selectedIngredientId }),
  reset: () => set(initialState),
}));
