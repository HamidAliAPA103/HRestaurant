import { useQuery } from "@tanstack/react-query";
import {
  CalendarCheck, CircleDollarSign, PackageSearch, ReceiptText,
  RefreshCw, RotateCcw, UsersRound,
} from "lucide-react";
import { Link } from "react-router-dom";
import { reportApi, reportKeys } from "@/api/reportApi";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { formatCurrency, formatDate, getErrorMessage } from "@/shared/lib/utils";

export function DashboardPage() {
  const query = useQuery({
    queryKey: reportKeys.dashboard({ range: "last30days" }),
    queryFn: ({ signal }) => reportApi.dashboard({ signal }),
  });

  if (query.isLoading) return <LoadingState label="Dashboard hazırlanır" />;
  if (query.isError) return <ErrorState message={getErrorMessage(query.error)} onRetry={() => query.refetch()} />;

  const dashboard = query.data!;
  const maxRevenue = Math.max(1, ...dashboard.sales.map((point) => point.revenue));
  const stats = [
    { label: "Dövriyyə", value: formatCurrency(dashboard.revenue), icon: CircleDollarSign },
    { label: "Sifariş", value: dashboard.orderCount, icon: ReceiptText },
    { label: "Rezervasiya", value: dashboard.reservationCount, icon: CalendarCheck },
    { label: "Müştəri", value: dashboard.customerCount, icon: UsersRound },
    { label: "Aşağı stok", value: dashboard.lowStockCount, icon: PackageSearch },
    { label: "Refund", value: formatCurrency(dashboard.refundedAmount), icon: RotateCcw },
  ];

  return (
    <div className="page-enter space-y-6">
      <header className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.18em] text-[#e85d3f]">Real vaxt icmalı</p>
          <h1 className="mt-2 text-3xl font-bold tracking-[-0.04em]">Dashboard</h1>
          <p className="mt-2 text-sm text-[#7c726a]">Son 30 günün server tərəfindən hesablanmış göstəriciləri.</p>
        </div>
        <Button variant="secondary" loading={query.isFetching} onClick={() => query.refetch()}>
          <RefreshCw className="h-4 w-4" /> Yenilə
        </Button>
      </header>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-6">
        {stats.map(({ label, value, icon: Icon }) => (
          <article key={label} className="card p-5">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold text-[#82776f]">{label}</span>
              <span className="grid h-9 w-9 place-items-center rounded-xl bg-[#fff0ec] text-[#d85337]"><Icon className="h-4 w-4" /></span>
            </div>
            <p className="mt-5 text-2xl font-bold tracking-tight">{value}</p>
          </article>
        ))}
      </section>

      <section className="grid gap-5 xl:grid-cols-[1.5fr_1fr]">
        <article className="card p-5 sm:p-6">
          <div className="flex items-end justify-between gap-4">
            <div><h2 className="font-bold">Satış dinamikası</h2><p className="mt-1 text-xs text-[#8a8078]">Günlük ödənmiş sifarişlər</p></div>
            <div className="text-right"><p className="text-xl font-bold">{formatCurrency(dashboard.revenue)}</p><p className="text-xs text-[#8a8078]">Orta {formatCurrency(dashboard.averageOrderValue)}</p></div>
          </div>
          {dashboard.sales.length === 0 ? <div className="mt-6"><EmptyState title="Satış məlumatı yoxdur" /></div> : (
            <div className="mt-7 flex h-64 items-end gap-2" aria-label="Günlük satış qrafiki">
              {dashboard.sales.map((point) => (
                <div key={point.period} className="group flex min-w-0 flex-1 flex-col items-center justify-end gap-2">
                  <span className="hidden text-[10px] font-semibold group-hover:block">{formatCurrency(point.revenue)}</span>
                  <div className="w-full rounded-t-lg bg-[#ed684a] transition hover:bg-[#cf4c31]" style={{ height: `${Math.max(5, point.revenue / maxRevenue * 100)}%` }} title={`${formatDate(point.period)}: ${formatCurrency(point.revenue)}`} />
                  <span className="max-w-full truncate text-[9px] text-[#8a8078]">{new Date(point.period).toLocaleDateString("az-AZ", { day: "2-digit", month: "2-digit" })}</span>
                </div>
              ))}
            </div>
          )}
        </article>

        <article className="card p-5 sm:p-6">
          <div className="flex items-center justify-between"><div><h2 className="font-bold">Populyar məhsullar</h2><p className="mt-1 text-xs text-[#8a8078]">Satış dəyərinə görə</p></div><Link className="text-xs font-bold text-[#d85337]" to="/menu">Menyuya bax</Link></div>
          <div className="mt-5 space-y-3">
            {dashboard.topItems.length === 0 ? <EmptyState title="Satış yoxdur" /> : dashboard.topItems.map((item, index) => (
              <div key={item.name} className="flex items-center gap-3 rounded-xl bg-[#faf8f5] p-3">
                <span className="grid h-8 w-8 place-items-center rounded-lg bg-white text-xs font-bold">{index + 1}</span>
                <div className="min-w-0 flex-1"><p className="truncate text-sm font-bold">{item.name}</p><p className="text-xs text-[#8a8078]">{item.count} ədəd</p></div>
                <span className="text-sm font-bold">{formatCurrency(item.value)}</span>
              </div>
            ))}
          </div>
        </article>
      </section>

      <article className="card overflow-hidden">
        <div className="flex items-center justify-between border-b border-[#ece6df] p-5"><div><h2 className="font-bold">Son sifarişlər</h2><p className="mt-1 text-xs text-[#8a8078]">İcazə dairənizdə olan əməliyyatlar</p></div><Link to="/orders" className="text-xs font-bold text-[#d85337]">Hamısı</Link></div>
        {dashboard.recentOrders.length === 0 ? <div className="p-5"><EmptyState title="Sifariş yoxdur" /></div> : (
          <div className="overflow-x-auto"><table className="data-table min-w-[650px]"><thead><tr><th>Sifariş</th><th>Filial</th><th>Status</th><th>Məbləğ</th><th>Tarix</th></tr></thead><tbody>
            {dashboard.recentOrders.map((order) => <tr key={order.id}><td className="font-bold">{order.orderNumber}</td><td>{order.branchName}</td><td><Badge>{order.status}</Badge></td><td>{formatCurrency(order.total)}</td><td>{formatDate(order.createdAt, true)}</td></tr>)}
          </tbody></table></div>
        )}
      </article>
    </div>
  );
}
