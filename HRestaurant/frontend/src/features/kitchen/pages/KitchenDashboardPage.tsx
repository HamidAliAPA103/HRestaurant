import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCircle2, ChefHat, Clock3, Flame, RefreshCw, Timer } from "lucide-react";
import { useEffect, useState } from "react";
import { createKitchenConnection, type KitchenOrderEvent } from "@/features/kitchen/api/kitchen-realtime";
import { apiClient } from "@/shared/api/client";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { getErrorMessage } from "@/shared/lib/utils";
import type { ApiResponse } from "@/shared/types/api";
import { OrderStatus, type KitchenDashboard, type KitchenOrder } from "@/shared/types/domain";

const columns = [
  { status: OrderStatus.Pending, title: "Gözləyir", icon: Clock3, next: OrderStatus.Confirmed, action: "Təsdiqlə" },
  { status: OrderStatus.Confirmed, title: "Təsdiqlənib", icon: Flame, next: OrderStatus.Preparing, action: "Hazırlamağa başla" },
  { status: OrderStatus.Preparing, title: "Hazırlanır", icon: ChefHat, next: OrderStatus.Ready, action: "Hazırdır" },
  { status: OrderStatus.Ready, title: "Servisə hazır", icon: CheckCircle2, action: "Ofisiant gözləyir" },
] as const;

async function getKitchenDashboard() {
  const { data } = await apiClient.get<ApiResponse<KitchenDashboard>>("/orders/kitchen");
  if (!data.success || !data.data) throw new Error(data.message);
  return data.data;
}

export function KitchenDashboardPage() {
  const queryClient = useQueryClient();
  const [connectionState, setConnectionState] = useState("Qoşulur");
  const query = useQuery({ queryKey: ["orders", "kitchen"], queryFn: getKitchenDashboard, refetchInterval: 60_000 });
  const mutation = useMutation({
    mutationFn: ({ order, status }: { order: KitchenOrder; status: OrderStatus }) =>
      apiClient.patch(`/orders/${order.id}/kitchen-status`, { status, rowVersion: order.rowVersion }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["orders", "kitchen"] }),
  });

  useEffect(() => {
    const onEvent = (event: KitchenOrderEvent) => {
      void queryClient.invalidateQueries({ queryKey: ["orders", "kitchen"] });
      if (event.audioCue) window.dispatchEvent(new CustomEvent("kitchen-audio-cue", { detail: event.audioCue }));
    };
    const connection = createKitchenConnection(onEvent);
    connection.onreconnecting(() => setConnectionState("Yenidən qoşulur"));
    connection.onreconnected(() => setConnectionState("Canlı"));
    connection.onclose(() => setConnectionState("Bağlantı kəsilib"));
    void connection.start().then(() => setConnectionState("Canlı")).catch(() => setConnectionState("Bağlantı xətası"));
    return () => { void connection.stop(); };
  }, [queryClient]);

  if (query.isLoading) return <LoadingState label="Mətbəx paneli yüklənir" />;
  if (query.isError) return <ErrorState message={getErrorMessage(query.error)} onRetry={() => query.refetch()} />;
  const dashboard = query.data!;

  return <div className="page-enter space-y-6">
    <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
      <div><div className="flex items-center gap-2 text-xs font-bold uppercase tracking-[0.18em] text-[#e85d3f]"><span className="h-2 w-2 animate-pulse rounded-full bg-[#e85d3f]" />{connectionState}</div>
        <h1 className="mt-2 text-3xl font-bold tracking-[-0.04em]">Mətbəx dashboard</h1>
        <p className="mt-2 text-sm text-[#7c726a]">Sifarişlər SignalR ilə real vaxtda yenilənir.</p></div>
      <Button variant="secondary" onClick={() => query.refetch()}><RefreshCw className="h-4 w-4" />Yenilə</Button>
    </div>
    <div className="grid gap-4 sm:grid-cols-4">
      <Metric icon={Clock3} value={dashboard.pendingCount} label="Növbədə" />
      <Metric icon={ChefHat} value={dashboard.preparingCount} label="Hazırlanır" />
      <Metric icon={CheckCircle2} value={dashboard.readyCount} label="Hazır" />
      <Metric icon={Timer} value={`${dashboard.averagePreparationMinutes} dəq.`} label="Orta hazırlıq" />
    </div>
    {mutation.isError && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-800">{getErrorMessage(mutation.error)}</p>}
    <div className="grid gap-5 2xl:grid-cols-4 xl:grid-cols-2">
      {columns.map((column) => {
        const Icon = column.icon;
        const orders = dashboard.orders.filter((order) => order.status === column.status);
        return <section key={column.status} className="min-w-0">
          <div className="mb-3 flex items-center justify-between px-1"><div className="flex items-center gap-2"><Icon className="h-4 w-4" /><h2 className="text-sm font-bold">{column.title}</h2></div><Badge>{orders.length}</Badge></div>
          <div className="min-h-80 space-y-3 rounded-2xl bg-[#efece6]/70 p-3">
            {orders.length === 0 ? <EmptyState title="Sifariş yoxdur" description="Bu mərhələdə aktiv sifariş yoxdur." />
              : orders.map((order) => <article key={order.id} className={`rounded-2xl border bg-white p-4 shadow-sm ${order.isDelayed ? "border-red-400" : "border-[#e2dcd4]"}`}>
                <div className="flex items-start justify-between"><div><p className="font-bold">{order.orderNumber}</p><p className="mt-1 text-xs text-[#8e847c]">{order.tableNumber ? `Masa ${order.tableNumber}` : "Takeaway"} · {order.waiterName ?? "Ofisiantsız"}</p></div>
                  <div className="flex gap-1">{order.isPriority && <Badge tone="danger">Prioritet</Badge>}{order.isDelayed && <Badge tone="danger">Gecikir</Badge>}</div></div>
                <div className="mt-3 space-y-2 rounded-xl bg-[#faf8f5] p-3">
                  {order.items.map((item) => <div key={item.id} className="flex justify-between text-sm"><span><strong>{item.quantity}×</strong> {item.menuItemName}</span>{item.kitchenNote && <span className="ml-2 text-xs text-[#b74731]">{item.kitchenNote}</span>}</div>)}
                  {order.kitchenNotes.length > 0 && <p className="border-t pt-2 text-xs text-[#756c64]">{order.kitchenNotes.join(" · ")}</p>}
                </div>
                <div className="mt-4 flex items-center justify-between"><span className="text-xs font-semibold"><Timer className="mr-1 inline h-3.5 w-3.5" />{Math.round(order.preparationDurationMinutes)} dəq.</span>
                  {"next" in column ? <Button size="sm" loading={mutation.isPending && mutation.variables?.order.id === order.id} onClick={() => mutation.mutate({ order, status: column.next })}>{column.action}</Button>
                    : <Badge tone="success">{column.action}</Badge>}</div>
              </article>)}
          </div>
        </section>;
      })}
    </div>
  </div>;
}

function Metric({ icon: Icon, value, label }: { icon: typeof Clock3; value: number | string; label: string }) {
  return <div className="card flex items-center gap-4 p-5"><div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#fff1dc] text-[#c77b21]"><Icon className="h-5 w-5" /></div><div><div className="text-2xl font-bold">{value}</div><div className="text-xs text-[#877d75]">{label}</div></div></div>;
}
