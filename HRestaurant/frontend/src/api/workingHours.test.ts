import { describe, expect, it } from "vitest";
import { serializeWorkingHours } from "@/api/workingHours";

describe("serializeWorkingHours", () => {
  it("serializes editable HH:mm values to the TimeOnly API format", () => {
    expect(serializeWorkingHours([
      { dayOfWeek: 1, opensAt: "08:30", closesAt: "22:15", isClosed: false },
    ])).toEqual([
      { dayOfWeek: 1, opensAt: "08:30:00", closesAt: "22:15:00", isClosed: false },
    ]);
  });

  it("keeps already serialized values and clears hours for closed days", () => {
    expect(serializeWorkingHours([
      { dayOfWeek: 2, opensAt: "09:00:00", closesAt: "23:00:00", isClosed: false },
      { dayOfWeek: 3, opensAt: "09:00", closesAt: "23:00", isClosed: true },
    ])).toEqual([
      { dayOfWeek: 2, opensAt: "09:00:00", closesAt: "23:00:00", isClosed: false },
      { dayOfWeek: 3, opensAt: null, closesAt: null, isClosed: true },
    ]);
  });
});
