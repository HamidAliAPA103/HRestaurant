import { create } from "zustand";
import type {
  CustomerInformation,
  PublicBranch,
  PublicReservationCreated,
  PublicRestaurant,
  PublicRestaurantTable,
} from "@/types/public";
import { getTodayInputValue } from "@/utils/reservation-date";

interface ReservationState {
  restaurant: PublicRestaurant | null;
  selectedBranch: PublicBranch | null;
  reservationDate: string;
  startTime: string;
  durationMinutes: number;
  guestCount: number;
  availableTables: PublicRestaurantTable[];
  selectedTable: PublicRestaurantTable | null;
  customerInformation: CustomerInformation | null;
  currentStep: number;
  success: PublicReservationCreated | null;
  setRestaurant: (restaurant: PublicRestaurant) => void;
  setBranch: (branch: PublicBranch) => void;
  setReservationDate: (date: string) => void;
  setStartTime: (time: string) => void;
  setDurationMinutes: (duration: number) => void;
  setGuestCount: (guestCount: number) => void;
  setAvailableTables: (tables: PublicRestaurantTable[]) => void;
  selectTable: (table: PublicRestaurantTable | null) => void;
  setCustomerInformation: (value: CustomerInformation) => void;
  setCurrentStep: (step: number) => void;
  setSuccess: (success: PublicReservationCreated) => void;
  reset: () => void;
}

const initialState = {
  restaurant: null,
  selectedBranch: null,
  reservationDate: getTodayInputValue(),
  startTime: "",
  durationMinutes: 120,
  guestCount: 2,
  availableTables: [] as PublicRestaurantTable[],
  selectedTable: null,
  customerInformation: null,
  currentStep: 1,
  success: null,
};

function clearTableSelection() {
  return {
    selectedTable: null,
    availableTables: [],
  };
}

export const useReservationStore = create<ReservationState>((set) => ({
  ...initialState,
  setRestaurant: (restaurant) =>
    set({
      restaurant,
      selectedBranch:
        restaurant.branches.length === 1
          ? restaurant.branches[0]
          : null,
      ...clearTableSelection(),
    }),
  setBranch: (selectedBranch) =>
    set({
      selectedBranch,
      startTime: "",
      ...clearTableSelection(),
    }),
  setReservationDate: (reservationDate) =>
    set({
      reservationDate,
      startTime: "",
      ...clearTableSelection(),
    }),
  setStartTime: (startTime) =>
    set({
      startTime,
      ...clearTableSelection(),
    }),
  setDurationMinutes: (durationMinutes) =>
    set({
      durationMinutes,
      startTime: "",
      ...clearTableSelection(),
    }),
  setGuestCount: (guestCount) =>
    set({
      guestCount,
      ...clearTableSelection(),
    }),
  setAvailableTables: (availableTables) => set({ availableTables }),
  selectTable: (selectedTable) => set({ selectedTable }),
  setCustomerInformation: (customerInformation) =>
    set({ customerInformation }),
  setCurrentStep: (currentStep) => set({ currentStep }),
  setSuccess: (success) => set({ success }),
  reset: () => set(initialState),
}));
