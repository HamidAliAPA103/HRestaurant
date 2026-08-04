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
  latitude: number | null;
  longitude: number | null;
  frontImageUrl: string | null;
  coverImageUrl: string | null;
  shortDescription: string | null;
  googleMapsUrl: string | null;
  virtualTourUrl: string | null;
  parkingInfo: string | null;
  landmark: string | null;
  isActive: boolean;
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

export interface PublicRestaurantExperience {
  restaurant: PublicRestaurant;
  defaultBranchId: string | null;
}

export interface PublicSceneHotspot {
  key: string;
  name: string;
  description: string;
  positionX: number;
  positionY: number;
  positionZ: number;
  cameraX: number;
  cameraY: number;
  cameraZ: number;
  tableIds: string[];
  availableTableCount: number;
}

export interface PublicSceneTable {
  id: string;
  tableNumber: string;
  capacity: number;
  shape: "Round" | "Square" | "Rectangle";
  status: PublicTableStatus;
  positionX: number;
  positionY: number;
  positionZ: number;
  rotationX: number;
  rotationY: number;
  rotationZ: number;
  width: number;
  length: number;
  height: number;
}

export interface PublicBranchScene {
  branchId: string;
  branchName: string;
  floorWidth: number;
  floorDepth: number;
  wallHeight: number;
  centerX: number;
  centerZ: number;
  tables: PublicSceneTable[];
  hotspots: PublicSceneHotspot[];
}

export interface PublicRestaurantScene {
  restaurantId: string;
  restaurantSlug: string;
  restaurantName: string;
  branches: PublicBranchScene[];
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
  is3DEnabled: boolean;
  has3DModel: boolean;
  modelPosterUrl: string | null;
  videoUrl: string | null;
  videoPosterUrl: string | null;
  videoDurationSeconds: number | null;
  isVideoEnabled: boolean;
  videoDisplayOrder: number;
  ingredients: string[];
}

export interface PublicFood3D {
  id: string;
  restaurantSlug: string;
  restaurantName: string;
  categoryName: string;
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
  model3DUrl: string | null;
  modelPosterUrl: string | null;
  modelScale: number;
  modelRotationX: number;
  modelRotationY: number;
  modelRotationZ: number;
  is3DEnabled: boolean;
  usesProceduralFallback: boolean;
}

export type IngredientFallbackKind =
  | "tomato"
  | "cucumber"
  | "cheese"
  | "sauce"
  | "herb"
  | "generic";

export interface PublicIngredient3D {
  id: string;
  name: string;
  unit: string;
  requiredQuantity: number;
  model3DUrl: string | null;
  imageUrl: string | null;
  description: string | null;
  calories: number | null;
  protein: number | null;
  carbohydrates: number | null;
  fat: number | null;
  origin: string | null;
  allergenInformation: string | null;
  explodedPositionX: number;
  explodedPositionY: number;
  explodedPositionZ: number;
  explodedRotationX: number;
  explodedRotationY: number;
  explodedRotationZ: number;
  displayOrder: number;
  isVisibleIn3D: boolean;
  fallbackKind: IngredientFallbackKind;
  usesProceduralFallback: boolean;
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
