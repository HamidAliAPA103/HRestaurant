import { useQuery } from "@tanstack/react-query";
import { Download, Printer, RefreshCw } from "lucide-react";
import { useMemo, useState } from "react";
import { branchApi, branchKeys } from "@/api/branchApi";
import { reportApi } from "@/api/reportApi";
import { Button } from "@/shared/components/Button";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { formatCurrency, formatDate, getErrorMessage } from "@/shared/lib/utils";

function isoDate(date: Date) { return date.toISOString().slice(0, 10); }

export function ReportsPage() {
  const today = useMemo(() => new Date(), []);
  const monthAgo = useMemo(() => new Date(today.getTime() - 29 * 86_400_000), [today]);
  const [from, setFrom] = useState(isoDate(monthAgo));
  const [to, setTo] = useState(isoDate(today));
  const [branchId, setBranchId] = useState("");
  const branchQuery = useQuery({
    queryKey: [...branchKeys.all, "report-filter"],
    queryFn: ({ signal }) => branchApi.list({ pageNumber: 1, pageSize: 100, signal }),
  });
  const params = useMemo(
    () => ({ from, to, branchId: branchId || undefined }),
    [branchId, from, to],
  );
  const query = useQuery({
    queryKey: ["reports", "complete", params],
    queryFn: async ({ signal }) => {
      const scoped = { ...params, signal };
      const [dashboard, sales, categories, methods, branches, items] = await Promise.all([
        reportApi.dashboard(scoped), reportApi.sales({ ...scoped, period: "day" }),
        reportApi.categories(scoped), reportApi.paymentMethods(scoped),
        reportApi.branches(scoped), reportApi.menuItems({ ...scoped, pageSize: 10 }),
      ]);
      return { dashboard, sales, categories, methods, branches, items };
    },
  });

  function exportCsv() {
    if (!query.data) return;
    const rows = [
      ["Tarix", "Dövriyyə", "Sifariş sayı", "Orta çek"],
      ...query.data.sales.map((x) => [x.period, x.revenue, x.orderCount, x.averageOrderValue]),
    ];
    const csv = rows.map((row) => row.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(",")).join("\r\n");
    const url = URL.createObjectURL(new Blob(["\uFEFF", csv], { type: "text/csv;charset=utf-8" }));
    const link = document.createElement("a"); link.href = url; link.download = `hrestaurant-report-${from}-${to}.csv`; link.click();
    URL.revokeObjectURL(url);
  }

  if (query.isLoading) return <LoadingState label="Hesabatlar hesablanır" />;
  if (query.isError) return <ErrorState message={getErrorMessage(query.error)} onRetry={() => query.refetch()} />;
  const data = query.data!;
  const maxSales = Math.max(1, ...data.sales.map((x) => x.revenue));

  return <div className="page-enter space-y-6 print:bg-white">
    <header className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
      <div><p className="text-xs font-bold uppercase tracking-[0.18em] text-[#e85d3f]">Analitika</p><h1 className="mt-2 text-3xl font-bold tracking-[-0.04em]">Hesabatlar</h1><p className="mt-2 text-sm text-[#7c726a]">Yalnız backend-də təsdiqlənmiş əməliyyatlar əsasında.</p></div>
      <div className="flex flex-wrap items-end gap-2 print:hidden">
        <label className="text-xs font-semibold">Filial
          <select
            value={branchId}
            onChange={(event) => setBranchId(event.target.value)}
            disabled={branchQuery.isLoading}
            className="mt-1 block h-10 min-w-44 rounded-xl border bg-white px-3"
          >
            <option value="">Bütün filiallar</option>
            {branchQuery.data?.data?.map((branch) => (
              <option key={branch.id} value={branch.id}>{branch.name}</option>
            ))}
          </select>
        </label>
        <label className="text-xs font-semibold">Başlanğıc<input type="date" value={from} max={to} onChange={(e) => setFrom(e.target.value)} className="mt-1 block h-10 rounded-xl border px-3" /></label>
        <label className="text-xs font-semibold">Son<input type="date" value={to} min={from} max={isoDate(today)} onChange={(e) => setTo(e.target.value)} className="mt-1 block h-10 rounded-xl border px-3" /></label>
        <Button variant="secondary" loading={query.isFetching} onClick={() => query.refetch()}><RefreshCw className="h-4 w-4" />Yenilə</Button>
        <Button variant="secondary" onClick={() => window.print()}><Printer className="h-4 w-4" />Çap</Button>
        <Button onClick={exportCsv}><Download className="h-4 w-4" />CSV</Button>
      </div>
    </header>

    <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      {[ ["Dövriyyə", formatCurrency(data.dashboard.revenue)], ["Sifariş", data.dashboard.orderCount], ["Orta çek", formatCurrency(data.dashboard.averageOrderValue)], ["Refund", formatCurrency(data.dashboard.refundedAmount)] ].map(([label, value]) =>
        <article key={label} className="card p-5"><p className="text-xs font-semibold text-[#82776f]">{label}</p><p className="mt-3 text-2xl font-bold">{value}</p></article>)}
    </section>

    <section className="card p-6">
      <div className="flex justify-between"><div><h2 className="font-bold">Dövriyyə qrafiki</h2><p className="mt-1 text-xs text-[#887e76]">{formatDate(from)} — {formatDate(to)}</p></div></div>
      {data.sales.length === 0 ? <EmptyState title="Seçilən dövrdə satış yoxdur" /> : <div className="mt-6 flex h-64 items-end gap-2">
        {data.sales.map((point) => <div key={point.period} className="group flex min-w-0 flex-1 flex-col items-center justify-end gap-2"><span className="hidden text-[9px] group-hover:block">{formatCurrency(point.revenue)}</span><div className="w-full rounded-t-lg bg-[#e85d3f]" style={{ height: `${Math.max(4, point.revenue / maxSales * 100)}%` }} title={formatCurrency(point.revenue)} /><span className="truncate text-[9px] text-[#8a8078]">{new Date(point.period).toLocaleDateString("az-AZ", { day: "2-digit", month: "2-digit" })}</span></div>)}
      </div>}
    </section>

    <section className="grid gap-5 xl:grid-cols-3">
      <Breakdown title="Kateqoriyalar" rows={data.categories} />
      <Breakdown title="Ödəniş üsulları" rows={data.methods} />
      <Breakdown title="Filiallar" rows={data.branches} />
    </section>

    <section className="card overflow-hidden"><div className="border-b p-5"><h2 className="font-bold">Məhsul performansı</h2></div>
      {data.items.data?.length ? <div className="overflow-x-auto"><table className="data-table min-w-[600px]"><thead><tr><th>Məhsul</th><th>Say</th><th>Dövriyyə</th></tr></thead><tbody>{data.items.data.map((item) => <tr key={item.name}><td className="font-bold">{item.name}</td><td>{item.count}</td><td>{formatCurrency(item.value)}</td></tr>)}</tbody></table></div> : <div className="p-5"><EmptyState title="Məhsul satışı yoxdur" /></div>}
    </section>
  </div>;
}

function Breakdown({ title, rows }: { title: string; rows: Array<{ name: string; value: number; count: number }> }) {
  const max = Math.max(1, ...rows.map((row) => row.value));
  return <article className="card p-5"><h2 className="font-bold">{title}</h2><div className="mt-5 space-y-4">{rows.length === 0 ? <EmptyState title="Məlumat yoxdur" /> : rows.map((row) => <div key={row.name}><div className="mb-1.5 flex justify-between text-xs"><span className="font-semibold">{row.name}</span><span>{formatCurrency(row.value)} · {row.count}</span></div><div className="h-2 overflow-hidden rounded-full bg-[#eee9e3]"><div className="h-full rounded-full bg-[#e85d3f]" style={{ width: `${row.value / max * 100}%` }} /></div></div>)}</div></article>;
}
