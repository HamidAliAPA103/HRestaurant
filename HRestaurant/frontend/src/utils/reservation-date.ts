import type { PublicWorkingHour } from "@/types/public";

export function getTodayInputValue(today = new Date()) {
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
  optionsOrInterval: number | TimeSlotOptions = {},
) {
  const options = typeof optionsOrInterval === "number"
    ? { intervalMinutes: optionsOrInterval }
    : optionsOrInterval;
  const {
    intervalMinutes = 30,
    timeZoneId,
    now = new Date(),
  } = options;
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

  const current = getDateTimeInZone(now, timeZoneId);

  if (reservationDate < current.date) {
    return [];
  }

  const slots: string[] = [];

  for (
    let minutes = openingMinutes;
    minutes + durationMinutes <= closingMinutes;
    minutes += intervalMinutes
  ) {
    if (
      reservationDate === current.date &&
      minutes <= current.minutes
    ) {
      continue;
    }

    slots.push(fromMinutes(minutes));
  }

  return slots;
}

interface TimeSlotOptions {
  intervalMinutes?: number;
  timeZoneId?: string;
  now?: Date;
}

export function formatTime(value: string) {
  return value.slice(0, 5);
}

export function toApiTime(value: string) {
  return /^\d{2}:\d{2}$/.test(value) ? `${value}:00` : value;
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

function getDateTimeInZone(value: Date, timeZoneId?: string) {
  if (timeZoneId) {
    try {
      const parts = new Intl.DateTimeFormat("en-CA", {
        timeZone: timeZoneId,
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
        hourCycle: "h23",
      }).formatToParts(value);
      const part = (type: Intl.DateTimeFormatPartTypes) =>
        parts.find((entry) => entry.type === type)?.value ?? "";
      const hour = Number(part("hour")) % 24;

      return {
        date: `${part("year")}-${part("month")}-${part("day")}`,
        minutes: hour * 60 + Number(part("minute")),
      };
    } catch {
      // Fall back to the browser's timezone for an unsupported zone id.
    }
  }

  return {
    date: getTodayInputValue(value),
    minutes: value.getHours() * 60 + value.getMinutes(),
  };
}
