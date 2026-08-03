import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { SettingsPage } from "@/features/settings/pages/SettingsPage";

const apiMocks = vi.hoisted(() => ({
  current: vi.fn(),
  updateSettings: vi.fn(),
  updateWorkingHours: vi.fn(),
}));

vi.mock("@/api/restaurantApi", () => ({
  restaurantKeys: { all: ["restaurants"] },
  restaurantApi: apiMocks,
}));

const closedHours = Array.from({ length: 7 }, (_, dayOfWeek) => ({
  dayOfWeek,
  opensAt: null,
  closesAt: null,
  isClosed: true,
}));

describe("SettingsPage working hours", () => {
  beforeEach(() => {
    apiMocks.current.mockReset().mockResolvedValue({
      id: "restaurant-id",
      name: "Test Restaurant",
      currency: "AZN",
      taxRate: 10,
      workingHours: closedHours,
    });
    apiMocks.updateSettings.mockReset().mockResolvedValue({ success: true });
    apiMocks.updateWorkingHours.mockReset().mockResolvedValue({ success: true });
  });

  it("submits the status and custom times selected by the user", async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsPage />
      </QueryClientProvider>,
    );

    const status = await screen.findByRole("combobox", {
      name: "Bazar ertəsi statusu",
    });
    fireEvent.change(status, { target: { value: "open" } });
    fireEvent.change(screen.getByLabelText("Bazar ertəsi açılış saatı"), {
      target: { value: "08:30" },
    });
    fireEvent.change(screen.getByLabelText("Bazar ertəsi bağlanış saatı"), {
      target: { value: "22:15" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Saatları saxla" }));

    await waitFor(() => expect(apiMocks.updateWorkingHours).toHaveBeenCalledTimes(1));
    expect(apiMocks.updateWorkingHours).toHaveBeenCalledWith(
      "restaurant-id",
      expect.arrayContaining([
        expect.objectContaining({
          dayOfWeek: 1,
          opensAt: "08:30",
          closesAt: "22:15",
          isClosed: false,
        }),
      ]),
    );
  });
});
