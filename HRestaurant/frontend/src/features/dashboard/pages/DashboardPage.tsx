import { useQuery } from "@tanstack/react-query";
import {
  ArrowUpRight,
  CalendarCheck,
  ChefHat,
  CircleDollarSign,
  Clock3,
  MoreHorizontal,
  ReceiptText,
  UsersRound,
} from "lucide-react";
import { Link } from "react-router-dom";
import { listResource } from "@/shared/api/resources";
import { Badge } from "@/shared/components/Badge";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "@/shared/components/StatePanel";
import {
  formatCurrency,
  formatDate,
  shortId,
} from "@/shared/lib/utils";
import type {
  DiningTable,
  Order,
  Reservation,
} from "@/shared/types/domain";
import { OrderStatus, TableStatus } from "@/shared/types/domain";

const orderStatus = {
  [OrderStatus.Pending]: { label: "Gözləyir", tone: "warning" },
  [OrderStatus.Confirmed]: { label: "Təsdiqlənib", tone: "info" },
  [OrderStatus.Preparing]: { label: "Hazırlanır", tone: "warning" },
  [OrderStatus.Ready]: { label: "Hazırdır", tone: "success" },
  [OrderStatus.Delivered]: { label: "Çatdırılıb", tone: "success" },
  [OrderStatus.Cancelled]: { label: "Ləğv edilib", tone: "danger" },
} as const;

const salesBars = [42, 58, 48, 72, 67, 88, 76, 91, 84, 100, 86, 94];

export function DashboardPage() {
  const ordersQuery = useQuery({
    queryKey: ["orders", "dashboard"],
    queryFn: () => listResource<Order>("/Order", { pageSize: 50 }),
  });
  const reservationsQuery = useQuery({
    queryKey: ["reservations", "dashboard"],
    queryFn: () =>
      listResource<Reservation>("/Reservation", { pageSize: 50 }),
  });
  const tablesQuery = useQuery({
    queryKey: ["tables", "dashboard"],
    queryFn: () => listResource<DiningTable>("/Table", { pageSize: 100 }),
  });
  const queries = [
    ordersQuery,
    reservationsQuery,
    tablesQuery,
  ];

  if (queries.some((query) => query.isLoading)) {
    return <LoadingState label="Dashboard hazırlanır" />;
  }

  if (queries.every((query) => query.isError)) {
    return (
      <ErrorState
        message="Dashboard məlumatları backend API-dən alına bilmədi."
        onRetry={() => queries.forEach((query) => query.refetch())}
      />
    );
  }

  const orders = ordersQuery.data?.data ?? [];
  const reservations = reservationsQuery.data?.data ?? [];
  const tables = tablesQuery.data?.data ?? [];
  const revenue = orders
    .filter((order) => order.status !== OrderStatus.Cancelled)
    .reduce((sum, order) => sum + order.totalPrices, 0);
  const activeOrders = orders.filter((order) =>
    [
      OrderStatus.Pending,
      OrderStatus.Confirmed,
      OrderStatus.Preparing,
      OrderStatus.Ready,
    ].includes(order.status),
  ).length;
  const occupiedTables = tables.filter(
    (table) => table.status === TableStatus.Occupied,
  ).length;

  const stats = [
    {
      label: "Ümumi dövriyyə",
      value: formatCurrency(revenue),
      change: "+12.8%",
      icon: CircleDollarSign,
      tone: "coral",
    },
    {
      label: "Aktiv sifarişlər",
      value: activeOrders.toString(),
      change: `${orders.length} ümumi`,
      icon: ReceiptText,
      tone: "amber",
    },
    {
      label: "Bu gün rezervasiya",
      value: reservations.length.toString(),
      change: "Növbəti 18:30",
      icon: CalendarCheck,
      tone: "blue",
    },
    {
      label: "Masa doluluğu",
      value: tables.length
        ? `${Math.round((occupiedTables / tables.length) * 100)}%`
        : "0%",
      change: `${occupiedTables}/${tables.length} masa`,
      icon: UsersRound,
      tone: "green",
    },
  ];

  return (
    <div className="page-enter space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-sm text-[#8a8078]">Bazar ertəsi, 28 İyul</p>
          <h1 className="mt-1 text-3xl font-bold tracking-[-0.04em] text-[#241f1b]">
            Sabahınız xeyir 👋
          </h1>
          <p className="mt-2 text-sm text-[#7d736b]">
            Restoranınızdakı bu günün əsas göstəriciləri.
          </p>
        </div>
        <div className="flex items-center gap-2 rounded-xl border border-[#e2dcd4] bg-white px-3.5 py-2 text-xs font-semibold text-[#5d554f]">
          <span className="h-2 w-2 rounded-full bg-[#58a870] ring-4 ring-[#58a870]/10" />
          Real vaxtda yenilənir
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <div key={stat.label} className="card p-5">
              <div className="flex items-start justify-between">
                <div
                  className={`grid h-10 w-10 place-items-center rounded-xl ${
                    stat.tone === "coral"
                      ? "bg-[#fff0ec] text-[#e85d3f]"
                      : stat.tone === "amber"
                        ? "bg-[#fff5df] text-[#be7b20]"
                        : stat.tone === "blue"
                          ? "bg-[#ebf2ff] text-[#4d78b1]"
                          : "bg-[#e9f6ed] text-[#438058]"
                  }`}
                >
                  <Icon className="h-[18px] w-[18px]" />
                </div>
                <button className="text-[#b0a69e]" aria-label="Ətraflı">
                  <MoreHorizontal className="h-5 w-5" />
                </button>
              </div>
              <div className="mt-5 text-2xl font-bold tracking-tight text-[#29231f]">
                {stat.value}
              </div>
              <div className="mt-1 flex items-center justify-between">
                <span className="text-xs text-[#867c74]">{stat.label}</span>
                <span className="text-[11px] font-semibold text-[#5b8065]">
                  {stat.change}
                </span>
              </div>
            </div>
          );
        })}
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.6fr_1fr]">
        <section className="card p-5 sm:p-6">
          <div className="flex items-start justify-between">
            <div>
              <h2 className="font-bold text-[#29231f]">Satış dinamikası</h2>
              <p className="mt-1 text-xs text-[#8b8179]">
                Gün ərzində sifariş dövriyyəsi
              </p>
            </div>
            <select className="rounded-lg border border-[#e1dbd3] bg-white px-3 py-2 text-xs text-[#625a53] outline-none">
              <option>Bu gün</option>
              <option>Bu həftə</option>
            </select>
          </div>
          <div className="mt-8 flex h-52 items-end gap-2 sm:gap-3">
            {salesBars.map((value, index) => (
              <div
                key={`${value}-${index}`}
                className="group flex h-full flex-1 items-end"
              >
                <div
                  className="w-full rounded-t-md bg-[#eee8e0] transition-colors group-hover:bg-[#e85d3f]"
                  style={{ height: `${value}%` }}
                  title={`${value}%`}
                />
              </div>
            ))}
          </div>
          <div className="mt-3 flex justify-between text-[10px] text-[#9b9189]">
            <span>10:00</span>
            <span>12:00</span>
            <span>14:00</span>
            <span>16:00</span>
            <span>18:00</span>
            <span>20:00</span>
          </div>
        </section>

        <section className="card p-5 sm:p-6">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="font-bold text-[#29231f]">Mətbəx ritmi</h2>
              <p className="mt-1 text-xs text-[#8b8179]">
                Aktiv sifarişlərin statusu
              </p>
            </div>
            <div className="grid h-9 w-9 place-items-center rounded-xl bg-[#fff1ed] text-[#e85d3f]">
              <ChefHat className="h-4 w-4" />
            </div>
          </div>
          <div className="mt-6 space-y-4">
            {[
              {
                label: "Gözləyən",
                status: OrderStatus.Pending,
                color: "#e6a442",
              },
              {
                label: "Hazırlanan",
                status: OrderStatus.Preparing,
                color: "#e85d3f",
              },
              {
                label: "Hazır",
                status: OrderStatus.Ready,
                color: "#57a66d",
              },
            ].map((item) => {
              const count = orders.filter(
                (order) => order.status === item.status,
              ).length;
              const percent = activeOrders
                ? Math.max(8, (count / activeOrders) * 100)
                : 8;
              return (
                <div key={item.label}>
                  <div className="mb-2 flex justify-between text-xs">
                    <span className="font-medium text-[#615952]">
                      {item.label}
                    </span>
                    <span className="font-bold text-[#302a26]">{count}</span>
                  </div>
                  <div className="h-2 overflow-hidden rounded-full bg-[#eee9e2]">
                    <div
                      className="h-full rounded-full"
                      style={{
                        width: `${percent}%`,
                        backgroundColor: item.color,
                      }}
                    />
                  </div>
                </div>
              );
            })}
          </div>
          <Link
            to="/kitchen"
            className="mt-7 flex items-center justify-center gap-2 rounded-xl bg-[#f4f0ea] py-3 text-xs font-bold text-[#524a44] hover:bg-[#ece6df]"
          >
            Mətbəx panelini aç
            <ArrowUpRight className="h-3.5 w-3.5" />
          </Link>
        </section>
      </div>

      <section className="card overflow-hidden">
        <div className="flex items-center justify-between px-5 py-5 sm:px-6">
          <div>
            <h2 className="font-bold text-[#29231f]">Son sifarişlər</h2>
            <p className="mt-1 text-xs text-[#8b8179]">
              Ən son əməliyyatlar və statusları
            </p>
          </div>
          <Link
            to="/pos"
            className="text-xs font-bold text-[#e85d3f] hover:text-[#c94b31]"
          >
            Hamısına bax
          </Link>
        </div>
        {orders.length === 0 ? (
          <div className="p-5">
            <EmptyState title="Sifariş yoxdur" />
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="data-table min-w-[720px]">
              <thead>
                <tr>
                  <th>Sifariş</th>
                  <th>Masa</th>
                  <th>Vaxt</th>
                  <th>Status</th>
                  <th>Məbləğ</th>
                </tr>
              </thead>
              <tbody>
                {orders.slice(0, 6).map((order) => {
                  const status = orderStatus[order.status];
                  return (
                    <tr key={order.id}>
                      <td className="font-bold text-[#302a26]">
                        {shortId(order.id)}
                      </td>
                      <td>{order.tableID ? shortId(order.tableID) : "Takeaway"}</td>
                      <td>
                        <span className="inline-flex items-center gap-1.5">
                          <Clock3 className="h-3.5 w-3.5 text-[#a0978f]" />
                          {formatDate(order.creatAt, true)}
                        </span>
                      </td>
                      <td>
                        <Badge tone={status.tone} dot>
                          {status.label}
                        </Badge>
                      </td>
                      <td className="font-bold text-[#302a26]">
                        {formatCurrency(order.totalPrices)}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}
