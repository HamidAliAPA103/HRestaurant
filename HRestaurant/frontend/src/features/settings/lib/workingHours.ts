import type { WorkingHour } from "@/api/contracts";

export const dayNames: Record<number, string> = {
  0: "Bazar",
  1: "Bazar ertəsi",
  2: "Çərşənbə axşamı",
  3: "Çərşənbə",
  4: "Cümə axşamı",
  5: "Cümə",
  6: "Şənbə",
};

export const displayDayOrder = [1, 2, 3, 4, 5, 6, 0] as const;

const defaultOpensAt = "09:00";
const defaultClosesAt = "23:00";

function toTimeInputValue(value: string | null, fallback: string) {
  return value ? value.slice(0, 5) : fallback;
}

export function createDefaultWorkingHours(): WorkingHour[] {
  return displayDayOrder.map((dayOfWeek) => ({
    dayOfWeek,
    opensAt: defaultOpensAt,
    closesAt: defaultClosesAt,
    isClosed: false,
  }));
}

export function normalizeWorkingHours(hours: WorkingHour[]): WorkingHour[] {
  return displayDayOrder.map((dayOfWeek) => {
    const current = hours.find((hour) => hour.dayOfWeek === dayOfWeek);

    return {
      dayOfWeek,
      opensAt: toTimeInputValue(current?.opensAt ?? null, defaultOpensAt),
      closesAt: toTimeInputValue(current?.closesAt ?? null, defaultClosesAt),
      isClosed: current?.isClosed ?? false,
    };
  });
}

export function setWorkingDayOpen(hour: WorkingHour, isOpen: boolean): WorkingHour {
  return {
    ...hour,
    isClosed: !isOpen,
    opensAt: toTimeInputValue(hour.opensAt, defaultOpensAt),
    closesAt: toTimeInputValue(hour.closesAt, defaultClosesAt),
  };
}

export function validateWorkingHours(hours: WorkingHour[]): string | null {
  for (const hour of hours) {
    if (hour.isClosed) continue;

    const name = dayNames[hour.dayOfWeek];
    if (!hour.opensAt || !hour.closesAt) {
      return `${name} üçün açılış və bağlanış saatlarını seçin.`;
    }

    if (hour.opensAt >= hour.closesAt) {
      return `${name} üçün bağlanış saatı açılış saatından gec olmalıdır.`;
    }
  }

  return null;
}
