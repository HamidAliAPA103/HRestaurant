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
  email: string;
  name: string;
  role: string;
}

export interface UserInput {
  email: string;
  name: string;
  role: string;
}

export interface MenuCategory extends BaseEntity {
  resdaranId: string;
  name: string;
}

export interface MenuItem extends BaseEntity {
  categoryId: string;
  image: string;
  imageURL: string;
  price: number;
  desc: string;
  nutrition: string;
}

export enum TableStatus {
  Empty = 0,
  Occupied = 1,
  Reserved = 2,
}

export interface DiningTable extends BaseEntity {
  restaurantID: string;
  tutum: number;
  status: TableStatus;
}

export enum OrderStatus {
  Pending = 0,
  Confirmed = 1,
  Preparing = 2,
  Ready = 3,
  Delivered = 4,
  Cancelled = 5,
}

export interface Order extends BaseEntity {
  customerID: string;
  tableID?: string | null;
  status: OrderStatus;
  totalPrices: number;
}

export interface OrderItemInput {
  orderId: string;
  menuId: string;
  say: number;
  prices: number;
}

export interface OrderInput {
  customerID: string;
  tableID?: string | null;
  items: OrderItemInput[];
}

export enum ReservationStatus {
  Pending = 0,
  Confirmed = 1,
  Cancelled = 2,
  Completed = 3,
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
