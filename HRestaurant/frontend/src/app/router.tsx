import { lazy, Suspense, type ReactNode } from "react";
import { Navigate, createBrowserRouter } from "react-router-dom";
import {
  ProtectedRoute,
  RoleProtectedRoute,
} from "@/features/auth/components/ProtectedRoute";
import { LoginPage } from "@/features/auth/pages/LoginPage";
import { UnauthorizedPage } from "@/features/auth/pages/UnauthorizedPage";
import { LoadingState } from "@/shared/components/StatePanel";
import { NotFoundPage } from "@/shared/pages/NotFoundPage";
import { DashboardLayout } from "@/widgets/dashboard-layout/DashboardLayout";
import { managementRoles } from "@/widgets/dashboard-layout/navigation";
import { publicRoutes } from "@/routes/public-routes";

const DashboardPage = lazy(() =>
  import("@/features/dashboard/pages/DashboardPage").then((module) => ({
    default: module.DashboardPage,
  })),
);
const RestaurantPage = lazy(() =>
  import("@/features/restaurants/pages/RestaurantPage").then((module) => ({
    default: module.RestaurantPage,
  })),
);
const EmployeePage = lazy(() =>
  import("@/features/employees/pages/EmployeePage").then((module) => ({
    default: module.EmployeePage,
  })),
);
const MenuPage = lazy(() =>
  import("@/features/menu/pages/MenuPage").then((module) => ({
    default: module.MenuPage,
  })),
);
const InventoryPage = lazy(() =>
  import("@/features/inventory/pages/InventoryPage").then((module) => ({
    default: module.InventoryPage,
  })),
);
const ReportsPage = lazy(() =>
  import("@/features/reports/pages/ReportsPage").then((module) => ({
    default: module.ReportsPage,
  })),
);
const PosOrderPage = lazy(() =>
  import("@/features/pos/pages/PosOrderPage").then((module) => ({
    default: module.PosOrderPage,
  })),
);
const ReservationPage = lazy(() =>
  import("@/features/reservations/pages/ReservationPage").then((module) => ({
    default: module.ReservationPage,
  })),
);
const KitchenDashboardPage = lazy(() =>
  import("@/features/kitchen/pages/KitchenDashboardPage").then((module) => ({
    default: module.KitchenDashboardPage,
  })),
);
const TableLayoutPage = lazy(() =>
  import("@/features/tables/pages/TableLayoutPage").then((module) => ({
    default: module.TableLayoutPage,
  })),
);
const CustomerPage = lazy(() =>
  import("@/features/customers/pages/CustomerPage").then((module) => ({
    default: module.CustomerPage,
  })),
);

function lazyPage(page: ReactNode) {
  return (
    <Suspense fallback={<LoadingState label="Səhifə hazırlanır" />}>
      {page}
    </Suspense>
  );
}

export const router = createBrowserRouter([
  ...publicRoutes,
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <DashboardLayout />,
        children: [
          {
            index: true,
            element: <Navigate to="/dashboard" replace />,
          },
          {
            path: "dashboard",
            element: lazyPage(<DashboardPage />),
          },
          {
            path: "unauthorized",
            element: <UnauthorizedPage />,
          },
          {
            element: <RoleProtectedRoute roles={managementRoles} />,
            children: [
              {
                path: "restaurants",
                element: lazyPage(<RestaurantPage />),
              },
              {
                path: "employees",
                element: lazyPage(<EmployeePage />),
              },
              {
                path: "menu",
                element: lazyPage(<MenuPage />),
              },
              {
                path: "inventory",
                element: lazyPage(<InventoryPage />),
              },
              {
                path: "reports",
                element: lazyPage(<ReportsPage />),
              },
            ],
          },
          {
            element: (
              <RoleProtectedRoute
                roles={["SuperAdmin", "Manager", "Cashier", "Waiter"]}
              />
            ),
            children: [
              {
                path: "pos",
                element: lazyPage(<PosOrderPage />),
              },
            ],
          },
          {
            element: (
              <RoleProtectedRoute
                roles={[
                  "SuperAdmin",
                  "RestaurantOwner",
                  "Manager",
                  "Waiter",
                ]}
              />
            ),
            children: [
              {
                path: "reservations",
                element: lazyPage(<ReservationPage />),
              },
            ],
          },
          {
            element: (
              <RoleProtectedRoute
                roles={["SuperAdmin", "Manager", "Chef"]}
              />
            ),
            children: [
              {
                path: "kitchen",
                element: lazyPage(<KitchenDashboardPage />),
              },
            ],
          },
          {
            element: (
              <RoleProtectedRoute
                roles={[
                  "SuperAdmin",
                  "RestaurantOwner",
                  "Manager",
                  "Waiter",
                ]}
              />
            ),
            children: [
              {
                path: "tables",
                element: lazyPage(<TableLayoutPage />),
              },
            ],
          },
          {
            element: (
              <RoleProtectedRoute
                roles={[
                  "SuperAdmin",
                  "RestaurantOwner",
                  "Manager",
                  "Waiter",
                  "Cashier",
                ]}
              />
            ),
            children: [
              {
                path: "customers",
                element: lazyPage(<CustomerPage />),
              },
            ],
          },
        ],
      },
    ],
  },
  {
    path: "*",
    element: <NotFoundPage />,
  },
]);
