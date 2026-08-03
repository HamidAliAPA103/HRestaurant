import type { IngredientFallbackKind } from "@/types/public";

const legend: Array<{
  kind: IngredientFallbackKind;
  label: string;
  color: string;
}> = [
  { kind: "tomato", label: "Pomidor", color: "#d74632" },
  { kind: "cucumber", label: "Xiyar", color: "#4f8f48" },
  { kind: "cheese", label: "Pendir", color: "#f5c84c" },
  { kind: "sauce", label: "Sous", color: "#a92720" },
  { kind: "herb", label: "Göyərti", color: "#3f8b42" },
  { kind: "generic", label: "Digər", color: "#cf895b" },
];

export function IngredientLegend() {
  return (
    <div className="rounded-2xl bg-[#f7f2ec] p-4" aria-label="Stilizə edilmiş ingredient əfsanəsi">
      <p className="text-xs font-bold uppercase tracking-[0.16em] text-[#796b61]">
        Procedural rəng əfsanəsi
      </p>
      <ul className="mt-3 flex flex-wrap gap-x-4 gap-y-2 text-xs text-[#564b43]">
        {legend.map((item) => (
          <li key={item.kind} className="flex items-center gap-1.5">
            <span
              className="h-2.5 w-2.5 rounded-full"
              style={{ backgroundColor: item.color }}
              aria-hidden
            />
            {item.label}
          </li>
        ))}
      </ul>
    </div>
  );
}
