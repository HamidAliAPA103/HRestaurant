import { createElement, useRef } from "react";
import {
  tableStatusIcons,
  tableStatusLabels,
} from "@/features/restaurant-experience/lib/table-status";
import type { PublicRestaurantTable, PublicTableStatus } from "@/types/public";

interface AccessibleTableListProps {
  tables: PublicRestaurantTable[];
  selectedTableId: string | null;
  onSelect: (table: PublicRestaurantTable) => void;
  onFocus: (table: PublicRestaurantTable) => void;
}

export function AccessibleTableList({
  tables,
  selectedTableId,
  onSelect,
  onFocus,
}: AccessibleTableListProps) {
  const refs = useRef<Array<HTMLButtonElement | null>>([]);
  const focusAt = (index: number) => {
    const normalized = (index + tables.length) % tables.length;
    refs.current[normalized]?.focus();
  };

  return (
    <section aria-labelledby="experience-table-list-title">
      <div className="flex items-end justify-between gap-4">
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.2em] text-[#b5422d]">
            3D görünüşün alternativi
          </p>
          <h2 id="experience-table-list-title" className="mt-1 font-serif text-3xl">
            Masalar
          </h2>
        </div>
        <span className="text-sm font-semibold text-[#766b62]">
          {tables.filter((table) => table.isAvailable).length} boş masa
        </span>
      </div>
      <div className="mt-4 grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
        {tables.map((table, index) => {
          const selected = selectedTableId === table.id;
          const status: PublicTableStatus = selected ? "Selected" : table.status;
          const Icon = tableStatusIcons[status];
          return (
            <button
              key={table.id}
              ref={(element) => {
                refs.current[index] = element;
              }}
              type="button"
              disabled={!table.isAvailable}
              aria-pressed={selected}
              onFocus={() => onFocus(table)}
              onClick={() => onSelect(table)}
              onKeyDown={(event) => {
                if (event.key === "ArrowRight" || event.key === "ArrowDown") {
                  event.preventDefault();
                  focusAt(index + 1);
                } else if (event.key === "ArrowLeft" || event.key === "ArrowUp") {
                  event.preventDefault();
                  focusAt(index - 1);
                }
              }}
              className="flex items-center justify-between rounded-2xl border border-[#ded5cb] bg-white px-4 py-3 text-left transition hover:border-[#b5422d] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[#b5422d] disabled:cursor-not-allowed disabled:bg-stone-100 disabled:opacity-65 aria-pressed:border-[#b5422d] aria-pressed:bg-[#fff3ed]"
            >
              <span>
                <strong className="block">Masa {table.tableNumber}</strong>
                <span className="mt-1 block text-xs text-[#766b62]">
                  {table.capacity} nəfər · {table.shape}
                </span>
              </span>
              <span className="flex items-center gap-1 text-[10px] font-bold uppercase text-[#675b52]">
                {createElement(Icon, { size: 13, "aria-hidden": true })}
                {tableStatusLabels[status]}
              </span>
            </button>
          );
        })}
      </div>
    </section>
  );
}
