import type { WorkingHour } from "@/api/contracts";

function toApiTime(value: string | null) {
  if (!value) return null;

  const [hours, minutes, seconds = "00"] = value.split(":");
  return `${hours}:${minutes}:${seconds}`;
}

export function serializeWorkingHours(hours: WorkingHour[]): WorkingHour[] {
  return hours.map((hour) => ({
    ...hour,
    opensAt: hour.isClosed ? null : toApiTime(hour.opensAt),
    closesAt: hour.isClosed ? null : toApiTime(hour.closesAt),
  }));
}
