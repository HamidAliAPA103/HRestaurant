import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  CheckCircle2,
  ChefHat,
  Clock3,
  Flame,
  RefreshCw,
  Timer,
} from "lucide-react";
import { listResource, updateResource } from "@/shared/api/resources";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "@/shared/components/StatePanel";
import {
  formatCurrency,
  formatDate,
  getErrorMessage,
  shortId,
} from "@/shared/lib/utils";
import {
  OrderStatus,
  type Order,
} from "@/shared/types/domain";

const columns = [
  {
    status: OrderStatus.Confirmed,
    title: "Yeni sifarişlər",
    subtitle: "Hazırlanma növbəsi",
    icon: Flame,
    tone: "text-[#c98025] bg-[#fff1d9]",
    next: OrderStatus.Preparing,
    action: "Hazırlamağa başla",
  },
  {
    status: OrderStatus.Preparing,
    title: "Hazırlanır",
    subtitle: "Mətbəxdə aktiv",
    icon: ChefHat,
    tone: "text-[#d14f37] bg-[#ffe8e2]",
    next: OrderStatus.Ready,
    action: "Hazırdır",
  },
  {
    status: OrderStatus.Ready,
    title: "Servisə hazır",
    subtitle: "Ofisiant gözləyir",
    icon: CheckCircle2,
    tone: "text-[#3f8156] bg-[#e5f4e9]",
    next: OrderStatus.Delivered,
    action: "Təhvil verildi",
  },
] as const;

export function KitchenDashboardPage() {
  const queryClient = useQueryClient();
  const query = useQuery({
    queryKey: ["orders", "kitchen"],
    queryFn: () => listResource<Order>("/Order"),
    refetchInterval: 20_000,
  });
  const mutation = useMutation({
    mutationFn: ({
      order,
      status,
    }: {
      order: Order;
      status: OrderStatus;
    }) =>
      updateResource("/Order", order.id, {
        tableID: order.tableID,
        status,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["orders"] });
    },
  });

  if (query.isLoading) return <LoadingState label="Mətbəx paneli yüklənir" />;
  if (query.isError) {
    return (
      <ErrorState
        message={getErrorMessage(query.error)}
        onRetry={() => query.refetch()}
      />
    );
  }

  const orders = query.data?.data ?? [];

  return (
    <div className="page-enter space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-[0.18em] text-[#e85d3f]">
            <span className="h-2 w-2 animate-pulse rounded-full bg-[#e85d3f]" />
            Canlı mətbəx
          </div>
          <h1 className="mt-2 text-3xl font-bold tracking-[-0.04em] text-[#241f1b]">
            Mətbəx dashboard
          </h1>
          <p className="mt-2 text-sm text-[#7c726a]">
            Sifariş axınını izləyin və hazırlıq statuslarını yeniləyin.
          </p>
        </div>
        <Button variant="secondary" onClick={() => query.refetch()}>
          <RefreshCw className="h-4 w-4" />
          Yenilə
        </Button>
      </div>

      <div className="grid gap-4 sm:grid-cols-3">
        <div className="card flex items-center gap-4 p-5">
          <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#fff1dc] text-[#c77b21]">
            <Clock3 className="h-5 w-5" />
          </div>
          <div>
            <div className="text-2xl font-bold">
              {
                orders.filter(
                  (order) => order.status === OrderStatus.Confirmed,
                ).length
              }
            </div>
            <div className="text-xs text-[#877d75]">Növbədə</div>
          </div>
        </div>
        <div className="card flex items-center gap-4 p-5">
          <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#ffe9e3] text-[#d14f37]">
            <ChefHat className="h-5 w-5" />
          </div>
          <div>
            <div className="text-2xl font-bold">
              {
                orders.filter(
                  (order) => order.status === OrderStatus.Preparing,
                ).length
              }
            </div>
            <div className="text-xs text-[#877d75]">Hazırlanır</div>
          </div>
        </div>
        <div className="card flex items-center gap-4 p-5">
          <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#e7f4eb] text-[#46815a]">
            <Timer className="h-5 w-5" />
          </div>
          <div>
            <div className="text-2xl font-bold">18 dəq.</div>
            <div className="text-xs text-[#877d75]">Orta hazırlıq</div>
          </div>
        </div>
      </div>

      <div className="grid gap-5 xl:grid-cols-3">
        {columns.map((column) => {
          const Icon = column.icon;
          const columnOrders = orders.filter(
            (order) => order.status === column.status,
          );
          return (
            <section key={column.title} className="min-w-0">
              <div className="mb-3 flex items-center justify-between px-1">
                <div className="flex items-center gap-3">
                  <div
                    className={`grid h-9 w-9 place-items-center rounded-xl ${column.tone}`}
                  >
                    <Icon className="h-4 w-4" />
                  </div>
                  <div>
                    <h2 className="text-sm font-bold text-[#332c27]">
                      {column.title}
                    </h2>
                    <p className="text-[10px] text-[#91877f]">
                      {column.subtitle}
                    </p>
                  </div>
                </div>
                <span className="grid h-7 min-w-7 place-items-center rounded-full bg-[#eae5de] px-2 text-xs font-bold">
                  {columnOrders.length}
                </span>
              </div>

              <div className="min-h-80 space-y-3 rounded-2xl bg-[#efece6]/70 p-3">
                {columnOrders.length === 0 ? (
                  <EmptyState
                    title="Sifariş yoxdur"
                    description="Bu mərhələdə aktiv sifariş yoxdur."
                  />
                ) : (
                  columnOrders.map((order, index) => (
                    <article
                      key={order.id}
                      className="rounded-2xl border border-[#e2dcd4] bg-white p-4 shadow-sm"
                    >
                      <div className="flex items-start justify-between">
                        <div>
                          <div className="text-sm font-bold">
                            {shortId(order.id)}
                          </div>
                          <div className="mt-1 text-xs text-[#8e847c]">
                            {order.tableID
                              ? `Masa ${index + 1}`
                              : "Takeaway"}
                          </div>
                        </div>
                        <Badge
                          tone={
                            column.status === OrderStatus.Ready
                              ? "success"
                              : "warning"
                          }
                        >
                          {formatDate(order.creatAt, true)}
                        </Badge>
                      </div>
                      <div className="mt-4 rounded-xl bg-[#faf8f5] p-3 text-xs text-[#756c64]">
                        Məhsul detalları backend order response-na əlavə
                        olunduqda burada görünəcək.
                      </div>
                      <div className="mt-4 flex items-center justify-between">
                        <span className="font-bold">
                          {formatCurrency(order.totalPrices)}
                        </span>
                        <Button
                          size="sm"
                          loading={
                            mutation.isPending &&
                            mutation.variables?.order.id === order.id
                          }
                          onClick={() =>
                            mutation.mutate({
                              order,
                              status: column.next,
                            })
                          }
                        >
                          {column.action}
                        </Button>
                      </div>
                    </article>
                  ))
                )}
              </div>
            </section>
          );
        })}
      </div>
    </div>
  );
}
