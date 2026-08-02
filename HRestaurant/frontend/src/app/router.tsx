import { lazy, Suspense, type ReactNode } from "react";
import { Navigate, createBrowserRouter, useParams } from "react-router-dom";
import {
  ProtectedRoute,
  RoleProtectedRoute,
} from "@/features/auth/components/ProtectedRoute";
import { LoginPage } from "@/features/auth/pages/LoginPage";
import { UnauthorizedPage } from "@/features/auth/pages/UnauthorizedPage";
import { RegisterPage } from "@/features/auth/pages/RegisterPage";
import { ForgotPasswordPage, ResetPasswordPage, VerifyEmailPage } from "@/features/auth/pages/AccountRecoveryPages";
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
const BranchPage = lazy(() => import("@/features/branches/pages/BranchPage").then((module) => ({ default: module.BranchPage })));
const ShiftPage = lazy(() => import("@/features/shifts/pages/ShiftPage").then((module) => ({ default: module.ShiftPage })));
const MasterDataPage = lazy(() => import("@/features/catalog/pages/MasterDataPage").then((module) => ({ default: module.MasterDataPage })));
const OrderPage = lazy(() => import("@/features/orders/pages/OrderPage").then((module) => ({ default: module.OrderPage })));
const PaymentPage = lazy(() => import("@/features/payments/pages/PaymentPage").then((module) => ({ default: module.PaymentPage })));
const NotificationPage = lazy(() => import("@/features/notifications/pages/NotificationPage").then((module) => ({ default: module.NotificationPage })));
const SettingsPage = lazy(() => import("@/features/settings/pages/SettingsPage").then((module) => ({ default: module.SettingsPage })));

function AdminAliasRedirect() {
  const { "*": target = "dashboard" } = useParams();
  const aliases: Record<string, string> = { "menu-items": "menu" };
  return <Navigate to={`/${aliases[target] ?? target}`} replace />;
}

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
  { path: "/register", element: <RegisterPage /> },
  { path: "/forgot-password", element: <ForgotPasswordPage /> },
  { path: "/reset-password", element: <ResetPasswordPage /> },
  { path: "/verify-email", element: <VerifyEmailPage /> },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: "admin/*",
        element: <AdminAliasRedirect />,
      },
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
                path: "branches",
                element: lazyPage(<BranchPage />),
              },
              {
                path: "shifts",
                element: lazyPage(<ShiftPage />),
              },
              {
                path: "categories",
                element: lazyPage(<MasterDataPage mode="categories" />),
              },
              {
                path: "ingredients",
                element: lazyPage(<MasterDataPage mode="ingredients" />),
              },
              {
                path: "suppliers",
                element: lazyPage(<MasterDataPage mode="suppliers" />),
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
              {
                path: "settings",
                element: lazyPage(<SettingsPage />),
              },
            ],
          },
          {
            element: (
              <RoleProtectedRoute
                roles={["SuperAdmin", "RestaurantOwner", "Manager", "Cashier", "Waiter"]}
              />
            ),
            children: [
              {
                path: "pos",
                element: lazyPage(<PosOrderPage />),
              },
              {
                path: "orders",
                element: lazyPage(<OrderPage />),
              },
              {
                path: "payments",
                element: lazyPage(<PaymentPage />),
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
                roles={["SuperAdmin", "RestaurantOwner", "Manager", "Chef"]}
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
          {
            element: (
              <RoleProtectedRoute
                roles={["SuperAdmin", "RestaurantOwner", "Manager", "Chef"]}
              />
            ),
            children: [
              {
                path: "notifications",
                element: lazyPage(<NotificationPage />),
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
