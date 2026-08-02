import {
  BarChart3,
  Bell,
  BookOpen,
  Boxes,
  Building2,
  CalendarDays,
  ChefHat,
  ContactRound,
  CreditCard,
  FlaskConical,
  LayoutDashboard,
  Settings,
  ShoppingCart,
  Store,
  TableProperties,
  Tags,
  Timer,
  Truck,
  UsersRound,
} from "lucide-react";
import type { AppRole } from "@/shared/types/auth";

export interface NavigationItem {
  label: string;
  path: string;
  icon: typeof LayoutDashboard;
  roles?: AppRole[];
  permission?: string;
  badge?: string;
}

export const managementRoles: AppRole[] = [
  "SuperAdmin",
  "RestaurantOwner",
  "Manager",
];

export const navigationGroups: Array<{
  label: string;
  items: NavigationItem[];
}> = [
  {
    label: "Əməliyyatlar",
    items: [
      { label: "İcmal", path: "/dashboard", icon: LayoutDashboard },
      {
        label: "POS sifariş",
        path: "/pos",
        icon: CreditCard,
        roles: ["SuperAdmin", "RestaurantOwner", "Manager", "Cashier", "Waiter"],
      },
      {
        label: "Sifarişlər",
        path: "/orders",
        icon: ShoppingCart,
        roles: ["SuperAdmin", "RestaurantOwner", "Manager", "Cashier", "Waiter"],
      },
      {
        label: "Ödənişlər",
        path: "/payments",
        icon: CreditCard,
        roles: ["SuperAdmin", "RestaurantOwner", "Manager", "Cashier", "Waiter"],
        permission: "Payments.Read",
      },
      {
        label: "Rezervasiyalar",
        path: "/reservations",
        icon: CalendarDays,
        roles: ["SuperAdmin", "RestaurantOwner", "Manager", "Waiter"],
      },
      {
        label: "Mətbəx",
        path: "/kitchen",
        icon: ChefHat,
        roles: ["SuperAdmin", "RestaurantOwner", "Manager", "Chef"],
        badge: "Canlı",
      },
      {
        label: "Masa planı",
        path: "/tables",
        icon: TableProperties,
        roles: ["SuperAdmin", "RestaurantOwner", "Manager", "Waiter"],
      },
    ],
  },
  {
    label: "İdarəetmə",
    items: [
      { label: "Restoranlar", path: "/restaurants", icon: Store, roles: managementRoles },
      { label: "Filiallar", path: "/branches", icon: Building2, roles: managementRoles },
      { label: "Əməkdaşlar", path: "/employees", icon: UsersRound, roles: managementRoles },
      { label: "Növbələr", path: "/shifts", icon: Timer, roles: managementRoles },
      { label: "Menyu", path: "/menu", icon: BookOpen, roles: managementRoles },
      { label: "Kateqoriyalar", path: "/categories", icon: Tags, roles: managementRoles },
      { label: "İnqrediyentlər", path: "/ingredients", icon: FlaskConical, roles: managementRoles },
      { label: "Təchizatçılar", path: "/suppliers", icon: Truck, roles: managementRoles },
      { label: "Anbar", path: "/inventory", icon: Boxes, roles: managementRoles },
      {
        label: "Müştərilər",
        path: "/customers",
        icon: ContactRound,
        roles: ["SuperAdmin", "RestaurantOwner", "Manager", "Waiter", "Cashier"],
        permission: "Customers.Read",
      },
      {
        label: "Bildirişlər",
        path: "/notifications",
        icon: Bell,
        roles: ["SuperAdmin", "RestaurantOwner", "Manager", "Chef"],
        permission: "Notifications.Read",
      },
      { label: "Tənzimləmələr", path: "/settings", icon: Settings, roles: managementRoles },
    ],
  },
  {
    label: "Analitika",
    items: [
      {
        label: "Hesabatlar",
        path: "/reports",
        icon: BarChart3,
        roles: managementRoles,
        permission: "Reports.Read",
      },
    ],
  },
];

export const pageTitles = Object.fromEntries(
  navigationGroups.flatMap((group) =>
    group.items.map((item) => [item.path, item.label]),
  ),
);
