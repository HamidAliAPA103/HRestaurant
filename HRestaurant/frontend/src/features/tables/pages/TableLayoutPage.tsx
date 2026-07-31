import { useQuery } from "@tanstack/react-query";
import {
  Armchair,
  Grid2X2,
  ListFilter,
  Plus,
  Users,
} from "lucide-react";
import { useState } from "react";
import { listResource } from "@/shared/api/resources";
import { Button } from "@/shared/components/Button";
import { PageHeader } from "@/shared/components/PageHeader";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "@/shared/components/StatePanel";
import { getErrorMessage } from "@/shared/lib/utils";
import {
  TableStatus,
  type DiningTable,
} from "@/shared/types/domain";

const statusMeta = {
  [TableStatus.Available]: {
    label: "Boş",
    card: "border-[#cae5d2] bg-[#f2fbf5]",
    icon: "bg-[#dff2e5] text-[#3b8153]",
    dot: "bg-[#54a56c]",
  },
  [TableStatus.Occupied]: {
    label: "Dolu",
    card: "border-[#f0c7bd] bg-[#fff7f5]",
    icon: "bg-[#ffe4de] text-[#c64f37]",
    dot: "bg-[#e85d3f]",
  },
  [TableStatus.Reserved]: {
    label: "Rezerv",
    card: "border-[#efd9aa] bg-[#fffbf0]",
    icon: "bg-[#fff0c9] text-[#a97019]",
    dot: "bg-[#dfa03e]",
  },
  [TableStatus.Disabled]: {
    label: "Deaktiv",
    card: "border-[#d6d0ca] bg-[#f5f3f1]",
    icon: "bg-[#e8e4df] text-[#6b625b]",
    dot: "bg-[#8b8178]",
  },
  [TableStatus.Cleaning]: {
    label: "Təmizlənir",
    card: "border-[#bad9e8] bg-[#f2f9fc]",
    icon: "bg-[#dceef6] text-[#37728f]",
    dot: "bg-[#4d9abd]",
  },
};

export function TableLayoutPage() {
  const [filter, setFilter] = useState<TableStatus | "all">("all");
  const query = useQuery({
    queryKey: ["tables"],
    queryFn: () => listResource<DiningTable>("/Table"),
  });
  const tables = (query.data?.data ?? []).filter(
    (table) => filter === "all" || table.status === filter,
  );

  return (
    <div className="page-enter space-y-6">
      <PageHeader
        eyebrow="Zal görünüşü"
        title="Masa planı"
        description="Masa doluluğunu izləyin, rezerv olunmuş və boş masaları bir baxışda görün."
        actions={
          <Button>
            <Plus className="h-4 w-4" />
            Yeni masa
          </Button>
        }
      />

      <div className="card flex flex-col gap-4 p-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-2">
          <Grid2X2 className="h-4 w-4 text-[#e85d3f]" />
          <span className="text-sm font-bold text-[#403933]">Əsas zal</span>
          <span className="rounded-full bg-[#eee9e3] px-2 py-0.5 text-[10px] font-bold text-[#777067]">
            {query.data?.totalCount ?? tables.length} masa
          </span>
        </div>
        <div className="flex gap-2 overflow-x-auto">
          <button
            onClick={() => setFilter("all")}
            className={`rounded-xl px-3 py-2 text-xs font-semibold ${
              filter === "all"
                ? "bg-[#26201c] text-white"
                : "bg-[#f1ede7] text-[#6b625b]"
            }`}
          >
            Hamısı
          </button>
          {Object.entries(statusMeta).map(([key, meta]) => (
            <button
              key={key}
              onClick={() => setFilter(Number(key) as TableStatus)}
              className={`inline-flex items-center gap-2 rounded-xl px-3 py-2 text-xs font-semibold ${
                filter === Number(key)
                  ? "bg-[#26201c] text-white"
                  : "bg-[#f1ede7] text-[#6b625b]"
              }`}
            >
              <span className={`h-2 w-2 rounded-full ${meta.dot}`} />
              {meta.label}
            </button>
          ))}
          <Button variant="secondary" size="sm">
            <ListFilter className="h-4 w-4" />
            Zona
          </Button>
        </div>
      </div>

      {query.isLoading ? (
        <LoadingState label="Masa planı yüklənir" />
      ) : query.isError ? (
        <ErrorState
          message={getErrorMessage(query.error)}
          onRetry={() => query.refetch()}
        />
      ) : tables.length === 0 ? (
        <EmptyState title="Bu filtrə uyğun masa yoxdur" />
      ) : (
        <section className="card relative overflow-hidden p-5 sm:p-8">
          <div
            className="pointer-events-none absolute inset-0 opacity-35"
            style={{
              backgroundImage:
                "linear-gradient(#e8e1d9 1px, transparent 1px), linear-gradient(90deg, #e8e1d9 1px, transparent 1px)",
              backgroundSize: "32px 32px",
            }}
          />
          <div className="relative grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6">
            {tables.map((table, index) => {
              const meta = statusMeta[table.status];
              return (
                <button
                  key={table.id}
                  className={`group min-h-40 rounded-3xl border-2 p-4 text-left transition hover:-translate-y-1 hover:shadow-lg ${meta.card}`}
                >
                  <div className="flex items-start justify-between">
                    <div
                      className={`grid h-10 w-10 place-items-center rounded-2xl ${meta.icon}`}
                    >
                      <Armchair className="h-5 w-5" />
                    </div>
                    <span className={`h-2.5 w-2.5 rounded-full ${meta.dot}`} />
                  </div>
                  <div className="mt-5 text-xl font-bold text-[#322b26]">
                    Masa {String(index + 1).padStart(2, "0")}
                  </div>
                  <div className="mt-2 flex items-center justify-between text-xs">
                    <span className="flex items-center gap-1 text-[#776e66]">
                      <Users className="h-3.5 w-3.5" />
                      {table.capacity} nəfər
                    </span>
                    <span className="font-bold text-[#5d554e]">{meta.label}</span>
                  </div>
                </button>
              );
            })}
          </div>
        </section>
      )}
    </div>
  );
}
