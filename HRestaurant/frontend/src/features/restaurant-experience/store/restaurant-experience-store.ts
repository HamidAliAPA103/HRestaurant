import { create } from "zustand";

export type ExperienceMode = "guided" | "free";

interface RestaurantExperienceState {
  mode: ExperienceMode;
  tourStarted: boolean;
  activeHotspotIndex: number;
  selectedBranchId: string | null;
  selectedTableId: string | null;
  hoveredTableId: string | null;
  heroProgress: number;
  setMode: (mode: ExperienceMode) => void;
  startTour: (mode?: ExperienceMode) => void;
  setActiveHotspotIndex: (index: number) => void;
  setSelectedBranchId: (id: string) => void;
  setSelectedTableId: (id: string | null) => void;
  setHoveredTableId: (id: string | null) => void;
  setHeroProgress: (progress: number) => void;
  reset: () => void;
}

const initialState = {
  mode: "guided" as ExperienceMode,
  tourStarted: false,
  activeHotspotIndex: 0,
  selectedBranchId: null,
  selectedTableId: null,
  hoveredTableId: null,
  heroProgress: 0,
};

export const useRestaurantExperienceStore =
  create<RestaurantExperienceState>((set) => ({
    ...initialState,
    setMode: (mode) => set({ mode }),
    startTour: (mode = "guided") => set({ mode, tourStarted: true }),
    setActiveHotspotIndex: (activeHotspotIndex) => set({ activeHotspotIndex }),
    setSelectedBranchId: (selectedBranchId) =>
      set({ selectedBranchId, selectedTableId: null, activeHotspotIndex: 0 }),
    setSelectedTableId: (selectedTableId) => set({ selectedTableId }),
    setHoveredTableId: (hoveredTableId) => set({ hoveredTableId }),
    setHeroProgress: (heroProgress) => set({ heroProgress }),
    reset: () => set(initialState),
  }));
