import { lazy, Suspense } from "react";
import { Navigate } from "react-router-dom";
import type { RouteObject } from "react-router-dom";
import { PublicLayout } from "@/layouts/PublicLayout";
import { LoadingState } from "@/shared/components/StatePanel";

const PublicRestaurantPage = lazy(() =>
  import(
    "@/features/public-restaurant/pages/PublicRestaurantPage"
  ).then((module) => ({
    default: module.PublicRestaurantPage,
  })),
);
const PublicHomePage = lazy(() =>
  import("@/features/public-restaurant/pages/PublicHomePage").then((module) => ({ default: module.PublicHomePage })),
);
const PublicMenuPage = lazy(() =>
  import("@/features/public-restaurant/pages/PublicMenuPage").then((module) => ({ default: module.PublicMenuPage })),
);
const FoodDetailPage = lazy(() =>
  import("@/features/food-3d/pages/FoodDetailPage").then((module) => ({
    default: module.FoodDetailPage,
  })),
);
const RestaurantExperiencePage = lazy(() =>
  import("@/features/restaurant-experience/pages/RestaurantExperiencePage").then(
    (module) => ({ default: module.RestaurantExperiencePage }),
  ),
);
const PublicBranchesPage = lazy(() => import("@/features/public-restaurant/pages/PublicBranchesPage").then((module) => ({ default: module.PublicBranchesPage })));
const BranchDetailPage = lazy(() => import("@/features/public-restaurant/pages/BranchDetailPage").then((module) => ({ default: module.BranchDetailPage })));

const ReservationTrackingPage = lazy(() =>
  import("@/features/reservations/pages/ReservationTrackingPage").then(
    (module) => ({
      default: module.ReservationTrackingPage,
    }),
  ),
);

export const publicRoutes: RouteObject[] = [
  {
    element: <PublicLayout />,
    children: [
      {
        index: true,
        element: <Suspense fallback={<LoadingState label="Restoranlar yüklənir" />}><PublicHomePage /></Suspense>,
      },
      {
        path: "/restaurants/:restaurantSlug",
        element: (
          <Suspense fallback={<LoadingState label="Restoran yüklənir" />}>
            <PublicRestaurantPage />
          </Suspense>
        ),
      },
      {
        path: "/restaurants/:restaurantSlug/menu",
        element: <Suspense fallback={<LoadingState label="Menyu yüklənir" />}><PublicMenuPage /></Suspense>,
      },
      {
        path: "/restaurants/:restaurantSlug/menu/:menuItemId",
        element: (
          <Suspense fallback={<LoadingState label="3D yemək təcrübəsi yüklənir" />}>
            <FoodDetailPage />
          </Suspense>
        ),
      },
      { path: "/restaurants/:restaurantSlug/branches", element: <Suspense fallback={<LoadingState label="Filiallar yüklənir" />}><PublicBranchesPage /></Suspense> },
      { path: "/restaurants/:restaurantSlug/branches/:branchId", element: <Suspense fallback={<LoadingState label="Filial yüklənir" />}><BranchDetailPage /></Suspense> },
      {
        path: "/restaurants/:restaurantSlug/experience",
        element: (
          <Suspense fallback={<LoadingState label="3D virtual tur yüklənir" />}>
            <RestaurantExperiencePage />
          </Suspense>
        ),
      },
      {
        path: "/restaurants/:restaurantSlug/reservation",
        element: <Suspense fallback={<LoadingState label="Rezervasiya açılır" />}><PublicRestaurantPage /></Suspense>,
      },
      {
        path: "/reservation/track",
        element: (
          <Suspense fallback={<LoadingState label="Rezervasiya açılır" />}>
            <ReservationTrackingPage />
          </Suspense>
        ),
      },
      {
        path: "/reservation/success",
        element: <Navigate to="/reservation/track" replace />,
      },
    ],
  },
];
