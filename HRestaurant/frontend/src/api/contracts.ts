import type {
  DiningTable,
  KitchenDashboard,
  MenuCategory,
  MenuItem,
  Order,
  OrderCreateInput,
  OrderStatus,
  ReservationStatus,
  TableStatus,
  User,
} from "@/shared/types/domain";

export interface ListParams {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  signal?: AbortSignal;
}

export interface WorkingHour {
  dayOfWeek: number;
  opensAt: string | null;
  closesAt: string | null;
  isClosed: boolean;
}

export interface RestaurantDto {
  id: string;
  creatAt: string;
  updateAt: string | null;
  deletedAt: string | null;
  name: string;
  slug: string;
  adres: string;
  number: string;
  email: string | null;
  description: string | null;
  logoUrl: string | null;
  coverImageUrl: string | null;
  isActive: boolean;
  currency: string;
  taxRate: number;
  isDeleted: boolean;
  workingHours: WorkingHour[];
}

export interface RestaurantInput {
  name: string;
  slug?: string;
  adres: string;
  number: string;
  email?: string;
  description?: string;
  logoUrl?: string;
  coverImageUrl?: string;
  currency: string;
  taxRate: number;
  workingHours?: WorkingHour[];
}

export interface BranchDto {
  id: string;
  restaurantId: string;
  restaurantName: string;
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
  isPubliclyVisible: boolean;
  managerId: string | null;
  managerName: string | null;
  managerEmail: string | null;
  timeZoneId: string;
  isActive: boolean;
  creatAt: string;
  updateAt: string | null;
  workingHours: WorkingHour[];
}

export interface BranchInput {
  restaurantId?: string;
  name: string;
  slug?: string;
  address: string;
  phone?: string;
  email?: string;
  latitude?: number | null;
  longitude?: number | null;
  frontImageUrl?: string;
  coverImageUrl?: string;
  shortDescription?: string;
  googleMapsUrl?: string;
  virtualTourUrl?: string;
  parkingInfo?: string;
  landmark?: string;
  isPubliclyVisible: boolean;
  timeZoneId: string;
  workingHours?: WorkingHour[];
}

export interface CustomerDto {
  id: string;
  restaurantId: string;
  branchId: string | null;
  fullName: string;
  phone: string;
  email: string | null;
  birthday: string | null;
  notes: string | null;
  totalOrders: number;
  totalSpent: number;
  lastVisitDate: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CustomerInput {
  restaurantId?: string;
  branchId?: string | null;
  fullName: string;
  phone: string;
  email?: string | null;
  birthday?: string | null;
  notes?: string | null;
}

export interface ReservationDto {
  id: string;
  creatAt: string;
  updateAt: string | null;
  customerId: string | null;
  branchId: string;
  tableId: string;
  reservationTime: string;
  endTime: string;
  durationMinutes: number;
  guestCount: number;
  fullName: string;
  confirmationCode: string;
  cancelledAt: string | null;
  cancellationReason: string | null;
  status: ReservationStatus;
}

export interface ReservationInput {
  customerId: string;
  branchId: string;
  tableId: string;
  reservationTime: string;
  durationMinutes: number;
  guestCount: number;
  status: ReservationStatus;
}

export enum IngredientUnit {
  Gram,
  Kilogram,
  Milliliter,
  Liter,
  Piece,
}

export interface InventoryDto {
  id: string;
  restaurantId: string;
  branchId: string;
  branchName: string;
  ingredientId: string;
  ingredientName: string;
  supplierId: string | null;
  supplierName: string | null;
  currentQuantity: number;
  minimumQuantity: number;
  unit: IngredientUnit;
  purchasePrice: number;
  expirationDate: string | null;
  batchNumber: string | null;
  isActive: boolean;
  rowVersion: string;
  creatAt: string;
  updateAt: string | null;
}

export interface InventoryInput {
  restaurantId: string;
  branchId: string;
  ingredientId: string;
  supplierId?: string | null;
  currentQuantity: number;
  minimumQuantity: number;
  unit: IngredientUnit;
  purchasePrice: number;
  expirationDate?: string | null;
  batchNumber?: string | null;
}

export interface DashboardSummary {
  revenue: number;
  orderCount: number;
  averageOrderValue: number;
  reservationCount: number;
  customerCount: number;
  lowStockCount: number;
  refundedAmount: number;
  recentOrders: RecentOrder[];
  topItems: NamedValue[];
  sales: TimeSeriesPoint[];
}

export interface RecentOrder {
  id: string;
  orderNumber: string;
  branchName: string;
  total: number;
  status: string;
  createdAt: string;
}

export interface NamedValue { name: string; value: number; count: number }
export interface TimeSeriesPoint {
  period: string;
  revenue: number;
  orderCount: number;
  averageOrderValue: number;
}

export interface PaymentSummary {
  orderId: string;
  orderNumber: string;
  totalAmount: number;
  paidAmount: number;
  refundedAmount: number;
  remainingAmount: number;
  isFullyPaid: boolean;
  paymentStatus: number;
  orderRowVersion: string;
  payments: PaymentDto[];
}

export interface PaymentDto {
  id: string;
  paymentMethod: number;
  paymentStatus: number;
  amount: number;
  refundedAmount: number;
  refundableAmount: number;
  transactionReference: string | null;
  failureReason: string | null;
  paidAt: string | null;
  createdByName: string;
  createdAt: string;
  rowVersion: string;
}

export interface ReceiptDto {
  restaurantName: string;
  branchName: string;
  address: string;
  orderNumber: string;
  tableNumber: string | null;
  items: ReceiptItemDto[];
  subtotal: number;
  discount: number;
  tax: number;
  total: number;
  payments: ReceiptPaymentDto[];
  paidAt: string | null;
  cashierName: string;
}

export interface ReceiptItemDto {
  name: string;
  unitPrice: number;
  quantity: number;
  discount: number;
  total: number;
}

export interface ReceiptPaymentDto {
  method: number;
  amount: number;
  transactionReference: string | null;
  paidAt: string;
}

export interface NotificationDto {
  id: string;
  branchId: string;
  inventoryItemId: string | null;
  relatedEntityId: string | null;
  ingredientName: string | null;
  type: number;
  title: string;
  message: string;
  targetUrl: string | null;
  isRead: boolean;
  isResolved: boolean;
  creatAt: string;
}

export type { DiningTable, KitchenDashboard, MenuCategory, MenuItem, Order,
  OrderCreateInput, OrderStatus, TableStatus, User };
