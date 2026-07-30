import { Users } from "lucide-react";
import type { PublicRestaurantTable } from "@/types/public";
import { statusLabels } from "./RestaurantTable3D";

interface AccessibleTableListProps {
  tables: PublicRestaurantTable[];
  selectedTable: PublicRestaurantTable | null;
  onSelect: (table: PublicRestaurantTable) => void;
}

export function AccessibleTableList({
  tables,
  selectedTable,
  onSelect,
}: AccessibleTableListProps) {
  return (
    <section aria-labelledby="accessible-table-list-title">
      <div className="mb-3 flex items-end justify-between gap-3">
        <div>
          <h3
            id="accessible-table-list-title"
            className="font-serif text-xl font-semibold"
          >
            Masaların siyahısı
          </h3>
          <p className="text-sm text-[#766b62]">
            3D görünüşə alternativ klaviatura seçimi
          </p>
        </div>
        <span className="text-sm font-semibold text-[#766b62]">
          {tables.filter((table) => table.isAvailable).length} boş masa
        </span>
      </div>
      <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
        {tables.map((table) => {
          const selected = selectedTable?.id === table.id;
          const status = selected ? "Selected" : table.status;

          return (
            <button
              key={table.id}
              type="button"
              disabled={!table.isAvailable}
              aria-pressed={selected}
              aria-label={`Masa ${table.tableNumber}, ${table.capacity} nəfər, ${statusLabels[status]}`}
              onClick={() => onSelect(table)}
              className={`flex items-center justify-between rounded-2xl border px-4 py-3 text-left transition focus:outline-none focus-visible:ring-2 focus-visible:ring-[#b5422d] ${
                selected
                  ? "border-[#b5422d] bg-[#fff3ed]"
                  : table.isAvailable
                    ? "border-[#ded5cb] bg-white hover:border-[#8fae9c]"
                    : "cursor-not-allowed border-[#e7e1da] bg-stone-100 opacity-65"
              }`}
            >
              <span>
                <span className="block font-bold">
                  Masa {table.tableNumber}
                </span>
                <span className="mt-1 flex items-center gap-1 text-xs text-[#766b62]">
                  <Users className="h-3.5 w-3.5" />
                  {table.capacity} nəfər
                </span>
              </span>
              <span className="rounded-full bg-black/5 px-2 py-1 text-[10px] font-bold uppercase tracking-wide">
                {statusLabels[status]}
              </span>
            </button>
          );
        })}
      </div>
    </section>
  );
}
