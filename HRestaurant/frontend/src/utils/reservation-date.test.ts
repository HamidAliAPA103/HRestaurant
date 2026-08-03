import { describe, expect, it } from "vitest";
import type { PublicWorkingHour } from "@/types/public";
import { generateTimeSlots, toApiTime } from "./reservation-date";

const mondaySchedule: PublicWorkingHour[] = [
  {
    dayOfWeek: 1,
    dayName: "Monday",
    opensAt: "09:00:00",
    closesAt: "23:00:00",
    isClosed: false,
  },
];

describe("generateTimeSlots", () => {
  it("excludes elapsed slots in the branch timezone", () => {
    const slots = generateTimeSlots(
      mondaySchedule,
      "2026-08-03",
      120,
      {
        timeZoneId: "Asia/Baku",
        now: new Date("2026-08-03T16:07:00Z"),
      },
    );

    expect(slots).toEqual(["20:30", "21:00"]);
  });

  it("returns no slots when the remaining service window is too short", () => {
    const slots = generateTimeSlots(
      mondaySchedule,
      "2026-08-03",
      120,
      {
        timeZoneId: "Asia/Baku",
        now: new Date("2026-08-03T17:07:00Z"),
      },
    );

    expect(slots).toEqual([]);
  });

  it("keeps the full schedule for a future date", () => {
    const slots = generateTimeSlots(
      mondaySchedule,
      "2026-08-03",
      120,
      {
        timeZoneId: "Asia/Baku",
        now: new Date("2026-08-02T17:07:00Z"),
      },
    );

    expect(slots[0]).toBe("09:00");
    expect(slots.at(-1)).toBe("21:00");
    expect(slots).toHaveLength(25);
  });
});

describe("toApiTime", () => {
  it("adds seconds required by the API TimeOnly JSON converter", () => {
    expect(toApiTime("14:00")).toBe("14:00:00");
  });

  it("does not alter a value that already includes seconds", () => {
    expect(toApiTime("14:00:00")).toBe("14:00:00");
  });
});
