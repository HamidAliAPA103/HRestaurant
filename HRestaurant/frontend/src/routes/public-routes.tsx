import { lazy, Suspense } from "react";
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
        path: "/restaurants/:restaurantSlug",
        element: (
          <Suspense fallback={<LoadingState label="Restoran yüklənir" />}>
            <PublicRestaurantPage />
          </Suspense>
        ),
      },
      {
        path: "/reservation/track",
        element: (
          <Suspense fallback={<LoadingState label="Rezervasiya açılır" />}>
            <ReservationTrackingPage />
          </Suspense>
        ),
      },
    ],
  },
];
