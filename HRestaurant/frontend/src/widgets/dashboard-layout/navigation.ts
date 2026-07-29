import {
  BarChart3,
  BookOpen,
  Boxes,
  CalendarDays,
  ChefHat,
  ContactRound,
  CreditCard,
  LayoutDashboard,
  Store,
  TableProperties,
  UsersRound,
} from "lucide-react";
import type { AppRole } from "@/shared/types/auth";

export interface NavigationItem {
  label: string;
  path: string;
  icon: typeof LayoutDashboard;
  roles?: AppRole[];
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
      {
        label: "İcmal",
        path: "/dashboard",
        icon: LayoutDashboard,
      },
      {
        label: "POS sifariş",
        path: "/pos",
        icon: CreditCard,
        roles: ["SuperAdmin", "Manager", "Cashier", "Waiter"],
      },
      {
        label: "Rezervasiyalar",
        path: "/reservations",
        icon: CalendarDays,
        roles: [
          "SuperAdmin",
          "RestaurantOwner",
          "Manager",
          "Waiter",
        ],
      },
      {
        label: "Mətbəx",
        path: "/kitchen",
        icon: ChefHat,
        roles: ["SuperAdmin", "Manager", "Chef"],
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
      {
        label: "Restoranlar",
        path: "/restaurants",
        icon: Store,
        roles: managementRoles,
      },
      {
        label: "Əməkdaşlar",
        path: "/employees",
        icon: UsersRound,
        roles: managementRoles,
      },
      {
        label: "Menyu",
        path: "/menu",
        icon: BookOpen,
        roles: managementRoles,
      },
      {
        label: "Anbar",
        path: "/inventory",
        icon: Boxes,
        roles: managementRoles,
      },
      {
        label: "Müştərilər",
        path: "/customers",
        icon: ContactRound,
        roles: [
          "SuperAdmin",
          "RestaurantOwner",
          "Manager",
          "Waiter",
          "Cashier",
        ],
      },
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
      },
    ],
  },
];

export const pageTitles = Object.fromEntries(
  navigationGroups.flatMap((group) =>
    group.items.map((item) => [item.path, item.label]),
  ),
);
