export type PublicTableStatus =
  | "Available"
  | "Selected"
  | "Reserved"
  | "Occupied"
  | "Cleaning"
  | "Disabled"
  | "CapacityNotSuitable";

export interface PublicWorkingHour {
  dayOfWeek: number;
  dayName: string;
  opensAt: string | null;
  closesAt: string | null;
  isClosed: boolean;
}

export interface PublicBranch {
  id: string;
  name: string;
  slug: string;
  address: string;
  phone: string | null;
  email: string | null;
  timeZoneId: string;
  isOpenNow: boolean;
  workingHours: PublicWorkingHour[];
}

export interface PublicRestaurant {
  id: string;
  slug: string;
  name: string;
  logoUrl: string | null;
  coverImageUrl: string | null;
  description: string | null;
  phone: string;
  email: string | null;
  address: string;
  isOpenNow: boolean;
  workingHours: PublicWorkingHour[];
  branches: PublicBranch[];
}

export interface PublicMenuItem {
  id: string;
  categoryId: string;
  name: string;
  description: string;
  nutrition: string;
  imageUrl: string | null;
  price: number;
  discountPercentage: number;
  finalPrice: number;
  preparationTimeMinutes: number;
  isAvailable: boolean;
  isPopular: boolean;
}

export interface PublicMenuCategory {
  id: string;
  name: string;
  description: string | null;
  displayOrder: number;
  items: PublicMenuItem[];
}

export interface PublicRestaurantTable {
  id: string;
  tableNumber: string;
  capacity: number;
  shape: "Round" | "Square" | "Rectangle";
  positionX: number;
  positionY: number;
  positionZ: number;
  rotationX: number;
  rotationY: number;
  rotationZ: number;
  width: number;
  length: number;
  height: number;
  status: PublicTableStatus;
  isAvailable: boolean;
  unavailableReason: PublicTableStatus | null;
}

export interface PublicTableLayout {
  id: string;
  tableNumber: string;
  capacity: number;
  shape: "Round" | "Square" | "Rectangle";
  position: { x: number; y: number; z: number };
  rotation: { x: number; y: number; z: number };
  dimensions: { width: number; length: number; height: number };
  publicStatus: string;
}

export interface TableAvailabilityRequest {
  reservationDate: string;
  startTime: string;
  guestCount: number;
  durationMinutes: number;
}

export interface CustomerInformation {
  fullName: string;
  phone: string;
  email?: string;
  specialNotes?: string;
  termsAccepted: boolean;
}

export interface PublicCreateReservationRequest
  extends TableAvailabilityRequest,
    CustomerInformation {
  branchId: string;
  tableId: string;
  captchaToken?: string;
}

export interface PublicReservationCreated {
  reservationId: string;
  confirmationCode: string;
  trackingToken: string;
  status: string;
  restaurantName: string;
  branchName: string;
  tableNumber: string;
  reservationDate: string;
  startTime: string;
  endTime: string;
  emailDeliveryQueued: boolean;
}

export interface PublicReservationLookupRequest {
  confirmationCode?: string;
  phone?: string;
  trackingToken?: string;
  captchaToken?: string;
}

export interface PublicReservationDetails {
  confirmationCode: string;
  status: string;
  restaurantName: string;
  branchName: string;
  branchAddress: string;
  reservationDate: string;
  startTime: string;
  endTime: string;
  guestCount: number;
  tableNumber: string;
  fullName: string;
  maskedPhone: string;
  maskedEmail: string | null;
  specialNotes: string | null;
  canCancel: boolean;
  cancelledAt: string | null;
}

export interface PublicCancelReservationRequest {
  phone?: string;
  trackingToken?: string;
  reason?: string;
  captchaToken?: string;
}
