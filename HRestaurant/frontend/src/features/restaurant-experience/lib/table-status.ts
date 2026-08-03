import {
  Armchair,
  Ban,
  CheckCircle2,
  Clock3,
  Sparkles,
  UserRoundX,
  type LucideIcon,
} from "lucide-react";
import type { PublicTableStatus } from "@/types/public";

export const tableStatusColors: Record<PublicTableStatus, string> = {
  Available: "#3f8a68",
  Selected: "#ef6542",
  Reserved: "#c58b34",
  Occupied: "#a94343",
  Cleaning: "#397f9a",
  Disabled: "#77716c",
  CapacityNotSuitable: "#80699a",
};

export const tableStatusLabels: Record<PublicTableStatus, string> = {
  Available: "Boşdur",
  Selected: "Seçilib",
  Reserved: "Rezerv edilib",
  Occupied: "Məşğuldur",
  Cleaning: "Təmizlənir",
  Disabled: "Aktiv deyil",
  CapacityNotSuitable: "Tutumu uyğun deyil",
};

export const tableStatusIcons: Record<PublicTableStatus, LucideIcon> = {
  Available: CheckCircle2,
  Selected: Armchair,
  Reserved: Clock3,
  Occupied: UserRoundX,
  Cleaning: Sparkles,
  Disabled: Ban,
  CapacityNotSuitable: UserRoundX,
};

export const isSelectableStatus = (status: PublicTableStatus) =>
  status === "Available" || status === "Selected";
