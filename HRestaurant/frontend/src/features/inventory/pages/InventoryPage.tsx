import {
  AlertTriangle,
  Boxes,
  CircleCheck,
  Download,
  Filter,
  PackagePlus,
  Search,
} from "lucide-react";
import { useMemo, useState } from "react";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { EmptyState } from "@/shared/components/StatePanel";
import { PageHeader } from "@/shared/components/PageHeader";
import type { InventoryItem } from "@/shared/types/domain";

const initialInventory: InventoryItem[] = [
  {
    id: "inv-1",
    name: "Pomidor",
    category: "Tərəvəz",
    amount: 4.2,
    unit: "kq",
    minimum: 8,
    supplier: "Green Farm",
    updatedAt: "Bu gün, 09:20",
  },
  {
    id: "inv-2",
    name: "Dana əti",
    category: "Ət məhsulları",
    amount: 18,
    unit: "kq",
    minimum: 10,
    supplier: "Caspian Meat",
    updatedAt: "Bu gün, 08:15",
  },
  {
    id: "inv-3",
    name: "Mozzarella",
    category: "Süd məhsulları",
    amount: 6,
    unit: "kq",
    minimum: 5,
    supplier: "Dairy House",
    updatedAt: "Dünən, 18:40",
  },
  {
    id: "inv-4",
    name: "Zeytun yağı",
    category: "Yağlar",
    amount: 3,
    unit: "litr",
    minimum: 6,
    supplier: "Mediterraneo",
    updatedAt: "Dünən, 15:10",
  },
  {
    id: "inv-5",
    name: "Basmati düyü",
    category: "Quru ərzaq",
    amount: 24,
    unit: "kq",
    minimum: 12,
    supplier: "Baku Foods",
    updatedAt: "27 İyul, 12:30",
  },
];

export function InventoryPage() {
  const [search, setSearch] = useState("");
  const items = useMemo(
    () =>
      initialInventory.filter((item) =>
        `${item.name} ${item.category} ${item.supplier}`
          .toLowerCase()
          .includes(search.toLowerCase()),
      ),
    [search],
  );
  const lowStock = initialInventory.filter(
    (item) => item.amount < item.minimum,
  );
  const healthy = initialInventory.length - lowStock.length;

  return (
    <div className="page-enter space-y-6">
      <PageHeader
        eyebrow="Təchizat"
        title="Anbar"
        description="Stok səviyyələrini, kritik məhsulları və təchizatçı yenilənmələrini izləyin."
        actions={
          <>
            <Button variant="secondary">
              <Download className="h-4 w-4" />
              Export
            </Button>
            <Button>
              <PackagePlus className="h-4 w-4" />
              Stok əlavə et
            </Button>
          </>
        }
      />

      <div className="grid gap-4 sm:grid-cols-3">
        <div className="card flex items-center gap-4 p-5">
          <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#eee9e2] text-[#60574f]">
            <Boxes className="h-5 w-5" />
          </div>
          <div>
            <div className="text-2xl font-bold">{initialInventory.length}</div>
            <div className="text-xs text-[#877d75]">Məhsul çeşidi</div>
          </div>
        </div>
        <div className="card flex items-center gap-4 p-5">
          <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#fff0dc] text-[#bd761b]">
            <AlertTriangle className="h-5 w-5" />
          </div>
          <div>
            <div className="text-2xl font-bold">{lowStock.length}</div>
            <div className="text-xs text-[#877d75]">Kritik stok</div>
          </div>
        </div>
        <div className="card flex items-center gap-4 p-5">
          <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#e6f4ea] text-[#438059]">
            <CircleCheck className="h-5 w-5" />
          </div>
          <div>
            <div className="text-2xl font-bold">{healthy}</div>
            <div className="text-xs text-[#877d75]">Normal səviyyə</div>
          </div>
        </div>
      </div>

      {lowStock.length > 0 && (
        <div className="flex items-start gap-3 rounded-2xl border border-[#f1d3b1] bg-[#fff8ea] p-4 text-sm">
          <AlertTriangle className="mt-0.5 h-5 w-5 shrink-0 text-[#c98224]" />
          <div>
            <div className="font-bold text-[#60431f]">Stok xəbərdarlığı</div>
            <div className="mt-1 text-[#82633d]">
              {lowStock.map((item) => item.name).join(", ")} minimum səviyyədən
              aşağıdır. Təchizat sifarişi yaradın.
            </div>
          </div>
        </div>
      )}

      <div className="card flex flex-col gap-3 p-4 sm:flex-row sm:items-center">
        <label className="relative block w-full max-w-md">
          <Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-[#968d85]" />
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Məhsul və ya təchizatçı axtar..."
            className="h-11 w-full rounded-xl border border-[#ded8d0] bg-[#faf8f5] pl-10 pr-4 text-sm outline-none focus:border-[#e85d3f]"
          />
        </label>
        <Button variant="secondary">
          <Filter className="h-4 w-4" />
          Filtrlər
        </Button>
      </div>

      {items.length === 0 ? (
        <EmptyState title="Uyğun stok tapılmadı" />
      ) : (
        <div className="table-shell overflow-x-auto">
          <table className="data-table min-w-[820px]">
            <thead>
              <tr>
                <th>Məhsul</th>
                <th>Stok</th>
                <th>Minimum</th>
                <th>Status</th>
                <th>Təchizatçı</th>
                <th>Yenilənib</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => {
                const low = item.amount < item.minimum;
                const percent = Math.min(
                  100,
                  (item.amount / (item.minimum * 2)) * 100,
                );
                return (
                  <tr key={item.id}>
                    <td>
                      <div className="font-bold text-[#302a26]">
                        {item.name}
                      </div>
                      <div className="mt-0.5 text-xs text-[#958b83]">
                        {item.category}
                      </div>
                    </td>
                    <td>
                      <div className="font-bold text-[#302a26]">
                        {item.amount} {item.unit}
                      </div>
                      <div className="mt-2 h-1.5 w-24 overflow-hidden rounded-full bg-[#eee9e3]">
                        <div
                          className={`h-full rounded-full ${
                            low ? "bg-[#e85d3f]" : "bg-[#58a36c]"
                          }`}
                          style={{ width: `${percent}%` }}
                        />
                      </div>
                    </td>
                    <td>
                      {item.minimum} {item.unit}
                    </td>
                    <td>
                      <Badge tone={low ? "danger" : "success"} dot>
                        {low ? "Kritik" : "Normal"}
                      </Badge>
                    </td>
                    <td>{item.supplier}</td>
                    <td className="text-[#887e76]">{item.updatedAt}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      <p className="text-xs text-[#91877f]">
        Qeyd: Backend-də inventory endpoint-i əlavə edilənədək bu bölmə
        frontend demo datası ilə işləyir.
      </p>
    </div>
  );
}
