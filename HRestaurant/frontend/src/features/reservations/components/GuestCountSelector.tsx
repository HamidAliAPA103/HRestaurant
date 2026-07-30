import { Minus, Plus, Users } from "lucide-react";

interface GuestCountSelectorProps {
  value: number;
  min?: number;
  max?: number;
  onChange: (value: number) => void;
}

export function GuestCountSelector({
  value,
  min = 1,
  max = 20,
  onChange,
}: GuestCountSelectorProps) {
  return (
    <div>
      <span className="mb-2 flex items-center gap-2 text-sm font-bold text-[#4d443d]">
        <Users className="h-4 w-4 text-[#b5422d]" />
        Qonaq sayı
      </span>
      <div className="flex w-full items-center justify-between rounded-2xl border border-[#d9d0c6] bg-white p-1.5">
        <button
          type="button"
          aria-label="Qonaq sayını azalt"
          disabled={value <= min}
          onClick={() => onChange(Math.max(min, value - 1))}
          className="grid h-10 w-10 place-items-center rounded-xl bg-stone-100 transition hover:bg-stone-200 disabled:cursor-not-allowed disabled:opacity-40"
        >
          <Minus className="h-4 w-4" />
        </button>
        <output
          aria-live="polite"
          className="font-serif text-2xl font-semibold"
        >
          {value}
        </output>
        <button
          type="button"
          aria-label="Qonaq sayını artır"
          disabled={value >= max}
          onClick={() => onChange(Math.min(max, value + 1))}
          className="grid h-10 w-10 place-items-center rounded-xl bg-stone-100 transition hover:bg-stone-200 disabled:cursor-not-allowed disabled:opacity-40"
        >
          <Plus className="h-4 w-4" />
        </button>
      </div>
    </div>
  );
}
