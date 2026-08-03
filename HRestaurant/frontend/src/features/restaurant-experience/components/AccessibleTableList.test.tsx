import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import type { PublicRestaurantTable } from "@/types/public";
import { AccessibleTableList } from "./AccessibleTableList";

const table = (
  id: string,
  tableNumber: string,
  status: PublicRestaurantTable["status"],
): PublicRestaurantTable => ({
  id,
  tableNumber,
  capacity: 4,
  shape: "Round",
  status,
  positionX: 0,
  positionY: 0,
  positionZ: 0,
  rotationX: 0,
  rotationY: 0,
  rotationZ: 0,
  width: 1.8,
  length: 1.8,
  height: 0.75,
  isAvailable: status === "Available",
  unavailableReason: status === "Available" ? null : status,
});

describe("Restaurant experience accessible table list", () => {
  it("prevents unavailable selection and supports arrow-key navigation", () => {
    const onSelect = vi.fn();
    render(
      <AccessibleTableList
        tables={[
          table("available", "A1", "Available"),
          table("reserved", "R1", "Reserved"),
          table("available-2", "A2", "Available"),
        ]}
        selectedTableId={null}
        onSelect={onSelect}
        onFocus={vi.fn()}
      />,
    );

    const available = screen.getByRole("button", { name: /Masa A1/i });
    const reserved = screen.getByRole("button", { name: /Masa R1/i });
    expect(reserved).toBeDisabled();
    expect(screen.getByText("Rezerv edilib")).toBeInTheDocument();

    available.focus();
    fireEvent.keyDown(available, { key: "ArrowLeft" });
    expect(screen.getByRole("button", { name: /Masa A2/i })).toHaveFocus();
    fireEvent.click(available);
    expect(onSelect).toHaveBeenCalledWith(expect.objectContaining({ id: "available" }));
  });
});
