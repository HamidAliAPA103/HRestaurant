export interface BaseEntity {
  id: string;
  creatAt: string;
  updateAt?: string | null;
  deletedAt?: string | null;
  isDeleted: boolean;
}

export interface Restaurant extends BaseEntity {
  name: string;
  adres: string;
  number: string;
}

export interface RestaurantInput {
  name: string;
  adres: string;
  number: string;
}

export interface User extends BaseEntity {
  restaurantId: string;
  branchId: string;
  branchName: string;
  appUserId?: string | null;
  email: string;
  name: string;
  phone?: string | null;
  role: string;
  salary: number;
  hireDate?: string | null;
  avatarUrl?: string | null;
  emergencyContact?: string | null;
  isActive: boolean;
}

export interface UserInput {
  email: string;
  name: string;
  role: string;
}

export interface BranchSummary {
  id: string;
  restaurantId: string;
  name: string;
  isActive: boolean;
}

export interface MenuCategory extends BaseEntity {
  resdaranId: string;
  name: string;
}

export interface MenuItem extends BaseEntity {
  restaurantId: string;
  categoryId: string;
  categoryName: string;
  name: string;
  image: string;
  imageURL: string;
  price: number;
  finalPrice: number;
  discountPercentage: number;
  preparationTimeMinutes: number;
  isAvailable: boolean;
  isPopular: boolean;
  desc: string;
  nutrition: string;
  model3DUrl: string | null;
  modelPosterUrl: string | null;
  modelScale: number;
  modelRotationX: number;
  modelRotationY: number;
  modelRotationZ: number;
    is3DEnabled: boolean;
    enableIngredientAnimation: boolean;
    videoUrl: string | null;
    videoPosterUrl: string | null;
    videoDurationSeconds: number | null;
    isVideoEnabled: boolean;
    videoDisplayOrder: number;
  ingredients?: Array<{
    ingredientId: string;
    name: string;
    unit: number;
    requiredQuantity: number;
  }>;
}

export enum TableStatus {
  Available = 0,
  Occupied = 1,
  Reserved = 2,
  Disabled = 3,
  Cleaning = 4,
}

export interface DiningTable extends BaseEntity {
  restaurantId: string;
  branchId: string;
  branchName: string;
  tableNumber: string;
  capacity: number;
  status: TableStatus;
  isActive: boolean;
  shape: number;
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

export enum OrderType {
  DineIn = 0,
  Takeaway = 1,
  Delivery = 2,
}

export enum OrderStatus {
  Pending = 0,
  Confirmed = 1,
  Preparing = 2,
  Ready = 3,
  Served = 4,
  Completed = 5,
  Cancelled = 6,
}

export interface Order extends BaseEntity {
  restaurantId: string;
  branchId: string;
  branchName: string;
  tableId?: string | null;
  tableNumber?: string | null;
  waiterId?: string | null;
  waiterName?: string | null;
  customerId?: string | null;
  customerName?: string | null;
  orderNumber: string;
  orderType: OrderType;
  status: OrderStatus;
  subtotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  notes?: string | null;
  isPriority: boolean;
  isPaid: boolean;
  rowVersion: string;
  items: OrderItem[];
}

export interface OrderItem {
  id: string;
  menuItemId: string;
  menuItemName: string;
  unitPrice: number;
  quantity: number;
  discountAmount: number;
  totalPrice: number;
  kitchenNote?: string | null;
  status: number;
}

export interface OrderCreateInput {
  restaurantId: string;
  branchId: string;
  tableId?: string | null;
  customerId?: string | null;
  orderType: OrderType;
  notes?: string;
  discountPercentage: number;
  isPriority: boolean;
  items: Array<{
    menuItemId: string;
    quantity: number;
    kitchenNote?: string;
  }>;
}

export interface KitchenOrder {
  id: string;
  restaurantId: string;
  branchId: string;
  orderNumber: string;
  status: OrderStatus;
  tableNumber?: string | null;
  waiterName?: string | null;
  kitchenNotes: string[];
  items: OrderItem[];
  preparationDurationMinutes: number;
  isDelayed: boolean;
  isPriority: boolean;
  createdAt: string;
  rowVersion: string;
}

export interface KitchenDashboard {
  pendingCount: number;
  preparingCount: number;
  readyCount: number;
  averagePreparationMinutes: number;
  orders: KitchenOrder[];
}

export enum ReservationStatus {
  Pending = 0,
  Confirmed = 1,
  Cancelled = 2,
  Completed = 3,
  Seated = 4,
  NoShow = 5,
}

export interface Reservation extends BaseEntity {
  customerId: string;
  tableId: string;
  reservationTime: string;
  guestCount: number;
  status: ReservationStatus;
}

export interface ReservationInput {
  customerId: string;
  tableId: string;
  reservationTime: string;
  guestCount: number;
  status: ReservationStatus;
}

export interface InventoryItem {
  id: string;
  name: string;
  category: string;
  amount: number;
  unit: string;
  minimum: number;
  supplier: string;
  updatedAt: string;
}
