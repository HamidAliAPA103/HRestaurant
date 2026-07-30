import { Check, Clock3, MapPin } from "lucide-react";
import type { PublicBranch } from "@/types/public";
import { formatTime } from "@/utils/reservation-date";

interface BranchSelectorProps {
  branches: PublicBranch[];
  selectedBranch: PublicBranch | null;
  onSelect: (branch: PublicBranch) => void;
}

export function BranchSelector({
  branches,
  selectedBranch,
  onSelect,
}: BranchSelectorProps) {
  if (branches.length === 0) {
    return (
      <div className="rounded-3xl border border-amber-200 bg-amber-50 p-6 text-amber-900">
        Hazırda rezervasiya qəbul edən aktiv filial yoxdur.
      </div>
    );
  }

  return (
    <div className="grid gap-4 md:grid-cols-2">
      {branches.map((branch) => {
        const isSelected = selectedBranch?.id === branch.id;
        const today = branch.workingHours.find(
          (entry) => entry.dayOfWeek === new Date().getDay(),
        );

        return (
          <button
            key={branch.id}
            type="button"
            aria-pressed={isSelected}
            onClick={() => onSelect(branch)}
            className={`relative rounded-3xl border p-5 text-left transition focus:outline-none focus-visible:ring-2 focus-visible:ring-[#b5422d] ${
              isSelected
                ? "border-[#b5422d] bg-[#fff8f3] shadow-lg shadow-[#7d2d1d]/10"
                : "border-[#e1d8cd] bg-white hover:-translate-y-0.5 hover:border-[#c8b9aa]"
            }`}
          >
            {isSelected && (
              <span className="absolute right-4 top-4 grid h-7 w-7 place-items-center rounded-full bg-[#b5422d] text-white">
                <Check className="h-4 w-4" />
              </span>
            )}
            <div className="pr-10">
              <p className="font-serif text-xl font-semibold">{branch.name}</p>
              <p className="mt-3 flex gap-2 text-sm text-[#6d6259]">
                <MapPin className="mt-0.5 h-4 w-4 shrink-0" />
                {branch.address}
              </p>
              <p className="mt-2 flex items-center gap-2 text-sm text-[#6d6259]">
                <Clock3 className="h-4 w-4 shrink-0" />
                {today?.isClosed || !today?.opensAt || !today.closesAt
                  ? "Bu gün bağlıdır"
                  : `${formatTime(today.opensAt)}–${formatTime(today.closesAt)}`}
              </p>
            </div>
            <span
              className={`mt-4 inline-flex rounded-full px-2.5 py-1 text-xs font-bold ${
                branch.isOpenNow
                  ? "bg-emerald-100 text-emerald-800"
                  : "bg-stone-100 text-stone-600"
              }`}
            >
              {branch.isOpenNow ? "Açıq" : "Bağlı"}
            </span>
          </button>
        );
      })}
    </div>
  );
}
