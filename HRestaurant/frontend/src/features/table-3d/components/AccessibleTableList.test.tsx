import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import type { PublicRestaurantTable } from "@/types/public";
import { AccessibleTableList } from "./AccessibleTableList";

const availableTable: PublicRestaurantTable = {
  id: "available",
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

const reservedTable: PublicRestaurantTable = {
  ...availableTable,
  id: "reserved",
  tableNumber: "T-2",
  status: "Reserved",
  isAvailable: false,
  unavailableReason: "Reserved",
};

describe("AccessibleTableList", () => {
  it("allows only available tables to be selected", () => {
    const onSelect = vi.fn();
    render(
      <AccessibleTableList
        tables={[availableTable, reservedTable]}
        selectedTable={null}
        onSelect={onSelect}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: /Masa T-1/i }));
    fireEvent.click(screen.getByRole("button", { name: /Masa T-2/i }));

    expect(onSelect).toHaveBeenCalledTimes(1);
    expect(onSelect).toHaveBeenCalledWith(availableTable);
    expect(
      screen.getByRole("button", { name: /Masa T-2/i }),
    ).toBeDisabled();
  });
});
