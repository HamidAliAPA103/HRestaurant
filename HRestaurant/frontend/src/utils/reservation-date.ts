import type { PublicWorkingHour } from "@/types/public";

export function getTodayInputValue() {
  const today = new Date();
  const offset = today.getTimezoneOffset();
  return new Date(today.getTime() - offset * 60_000)
    .toISOString()
    .slice(0, 10);
}

export function getWorkingHour(
  workingHours: PublicWorkingHour[],
  reservationDate: string,
) {
  if (!reservationDate) {
    return undefined;
  }

  const day = new Date(`${reservationDate}T12:00:00`).getDay();
  return workingHours.find((entry) => entry.dayOfWeek === day);
}

export function generateTimeSlots(
  workingHours: PublicWorkingHour[],
  reservationDate: string,
  durationMinutes: number,
  intervalMinutes = 30,
) {
  const schedule = getWorkingHour(workingHours, reservationDate);

  if (
    !schedule ||
    schedule.isClosed ||
    !schedule.opensAt ||
    !schedule.closesAt
  ) {
    return [];
  }

  const openingMinutes = toMinutes(schedule.opensAt);
  let closingMinutes = toMinutes(schedule.closesAt);

  if (closingMinutes <= openingMinutes) {
    closingMinutes += 24 * 60;
  }

  const slots: string[] = [];

  for (
    let minutes = openingMinutes;
    minutes + durationMinutes <= closingMinutes;
    minutes += intervalMinutes
  ) {
    slots.push(fromMinutes(minutes));
  }

  return slots;
}

export function formatTime(value: string) {
  return value.slice(0, 5);
}

export function formatDate(value: string) {
  return new Intl.DateTimeFormat("az-AZ", {
    day: "2-digit",
    month: "long",
    year: "numeric",
  }).format(new Date(`${value}T12:00:00`));
}

function toMinutes(value: string) {
  const [hours, minutes] = value.split(":").map(Number);
  return hours * 60 + minutes;
}

function fromMinutes(totalMinutes: number) {
  const normalized = totalMinutes % (24 * 60);
  const hours = Math.floor(normalized / 60)
    .toString()
    .padStart(2, "0");
  const minutes = (normalized % 60).toString().padStart(2, "0");
  return `${hours}:${minutes}`;
}
