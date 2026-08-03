import { describe, expect, it } from "vitest";
import type { WorkingHour } from "@/api/contracts";
import {
  normalizeWorkingHours,
  setWorkingDayOpen,
  validateWorkingHours,
} from "@/features/settings/lib/workingHours";

describe("working hours helpers", () => {
  it("normalizes API hours into Monday-to-Sunday display order", () => {
    const hours: WorkingHour[] = [
      { dayOfWeek: 0, opensAt: null, closesAt: null, isClosed: true },
      { dayOfWeek: 1, opensAt: "08:30:00", closesAt: "22:15:00", isClosed: false },
    ];

    const result = normalizeWorkingHours(hours);

    expect(result.map((hour) => hour.dayOfWeek)).toEqual([1, 2, 3, 4, 5, 6, 0]);
    expect(result[0]).toMatchObject({ opensAt: "08:30", closesAt: "22:15" });
    expect(result[6]).toMatchObject({ isClosed: true, opensAt: "09:00", closesAt: "23:00" });
  });

  it("adds editable defaults when a closed day is opened", () => {
    const result = setWorkingDayOpen(
      { dayOfWeek: 2, opensAt: null, closesAt: null, isClosed: true },
      true,
    );

    expect(result).toEqual({
      dayOfWeek: 2,
      opensAt: "09:00",
      closesAt: "23:00",
      isClosed: false,
    });
  });

  it("rejects an open day whose closing time is not later", () => {
    expect(validateWorkingHours([
      { dayOfWeek: 1, opensAt: "18:00", closesAt: "09:00", isClosed: false },
    ])).toContain("bağlanış saatı");
  });
});
