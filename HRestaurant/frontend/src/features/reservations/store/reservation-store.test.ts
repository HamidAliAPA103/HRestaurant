import { beforeEach, describe, expect, it } from "vitest";
import type { PublicRestaurantTable } from "@/types/public";
import { useReservationStore } from "./reservation-store";

const table: PublicRestaurantTable = {
  id: "table-1",
  tableNumber: "T-1",
  capacity: 4,
  shape: "Round",
  positionX: 0,
  positionY: 0,
  positionZ: 0,
  rotationX: 0,
  rotationY: 0,
  rotationZ: 0,
  width: 1.8,
  length: 1.8,
  height: 0.75,
  status: "Available",
  isAvailable: true,
  unavailableReason: null,
};

describe("reservation store", () => {
  beforeEach(() => {
    useReservationStore.getState().reset();
  });

  it("clears a selected table when guest count changes", () => {
    useReservationStore.getState().setAvailableTables([table]);
    useReservationStore.getState().selectTable(table);

    useReservationStore.getState().setGuestCount(5);

    expect(useReservationStore.getState().selectedTable).toBeNull();
    expect(useReservationStore.getState().availableTables).toEqual([]);
  });

  it("moves to the success step only after a reservation is created", () => {
    useReservationStore.getState().setSuccess({
      reservationId: "reservation-1",
      confirmationCode: "RSV-8F3K2M",
      trackingToken: "plain-token",
      status: "Pending",
      restaurantName: "Demo Restaurant",
      branchName: "Mərkəz",
      tableNumber: "T-1",
      reservationDate: "2026-08-10",
      startTime: "19:00",
      endTime: "21:00",
      emailDeliveryQueued: false,
    });

    expect(useReservationStore.getState().currentStep).toBe(7);
    expect(useReservationStore.getState().success?.confirmationCode)
      .toBe("RSV-8F3K2M");
  });
});
