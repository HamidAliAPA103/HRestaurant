import { useQuery } from "@tanstack/react-query";
import {
  ArrowDownRight,
  ArrowUpRight,
  CalendarRange,
  Download,
  ReceiptText,
  TrendingUp,
  UsersRound,
} from "lucide-react";
import { listResource } from "@/shared/api/resources";
import { Button } from "@/shared/components/Button";
import { PageHeader } from "@/shared/components/PageHeader";
import {
  ErrorState,
  LoadingState,
} from "@/shared/components/StatePanel";
import { formatCurrency, getErrorMessage } from "@/shared/lib/utils";
import type {
  Order,
  Reservation,
  User,
} from "@/shared/types/domain";
import { OrderStatus } from "@/shared/types/domain";

const weekly = [
  { day: "B.e", value: 62 },
  { day: "Ç.a", value: 74 },
  { day: "Ç", value: 58 },
  { day: "C.a", value: 81 },
  { day: "C", value: 95 },
  { day: "Ş", value: 100 },
  { day: "B", value: 88 },
];

export function ReportsPage() {
  const ordersQuery = useQuery({
    queryKey: ["orders", "reports"],
    queryFn: () => listResource<Order>("/Order"),
  });
  const reservationsQuery = useQuery({
    queryKey: ["reservations", "reports"],
    queryFn: () => listResource<Reservation>("/Reservation"),
  });
  const usersQuery = useQuery({
    queryKey: ["users", "reports"],
    queryFn: () => listResource<User>("/User"),
  });

  if (
    ordersQuery.isLoading ||
    reservationsQuery.isLoading ||
    usersQuery.isLoading
  ) {
    return <LoadingState label="Hesabat hazırlanır" />;
  }

  if (ordersQuery.isError) {
    return (
      <ErrorState
        message={getErrorMessage(ordersQuery.error)}
        onRetry={() => ordersQuery.refetch()}
      />
    );
  }

  const orders = ordersQuery.data?.data ?? [];
  const revenue = orders
    .filter((order) => order.status !== OrderStatus.Cancelled)
    .reduce((sum, order) => sum + order.totalAmount, 0);
  const average = orders.length ? revenue / orders.length : 0;
  const customers = (usersQuery.data?.data ?? []).filter(
    (user) => user.role.toLowerCase() === "customer",
  ).length;

  return (
    <div className="page-enter space-y-6">
      <PageHeader
        eyebrow="Biznes analitikası"
        title="Hesabatlar"
        description="Satış, sifariş və müştəri performansını müqayisəli şəkildə izləyin."
        actions={
          <>
            <Button variant="secondary">
              <CalendarRange className="h-4 w-4" />
              22–28 İyul
            </Button>
            <Button>
              <Download className="h-4 w-4" />
              Hesabatı yüklə
            </Button>
          </>
        }
      />

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {[
          {
            label: "Xalis dövriyyə",
            value: formatCurrency(revenue),
            change: "+14.2%",
            positive: true,
            icon: TrendingUp,
          },
          {
            label: "Sifariş sayı",
            value: orders.length.toString(),
            change: "+8.7%",
            positive: true,
            icon: ReceiptText,
          },
          {
            label: "Orta çek",
            value: formatCurrency(average),
            change: "+3.1%",
            positive: true,
            icon: TrendingUp,
          },
          {
            label: "Müştəri sayı",
            value: customers.toString(),
            change: "-1.4%",
            positive: false,
            icon: UsersRound,
          },
        ].map((stat) => (
          <div key={stat.label} className="card p-5">
            <div className="flex items-center justify-between">
              <div className="grid h-10 w-10 place-items-center rounded-xl bg-[#f2ede6] text-[#e85d3f]">
                <stat.icon className="h-4 w-4" />
              </div>
              <span
                className={`inline-flex items-center gap-1 text-xs font-bold ${
                  stat.positive ? "text-[#4c8b60]" : "text-[#c6533d]"
                }`}
              >
                {stat.positive ? (
                  <ArrowUpRight className="h-3.5 w-3.5" />
                ) : (
                  <ArrowDownRight className="h-3.5 w-3.5" />
                )}
                {stat.change}
              </span>
            </div>
            <div className="mt-5 text-2xl font-bold tracking-tight">
              {stat.value}
            </div>
            <div className="mt-1 text-xs text-[#877d75]">{stat.label}</div>
          </div>
        ))}
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.5fr_1fr]">
        <section className="card p-5 sm:p-6">
          <div className="flex items-start justify-between">
            <div>
              <h2 className="font-bold">Həftəlik satış</h2>
              <p className="mt-1 text-xs text-[#8a8078]">
                Günlər üzrə dövriyyə performansı
              </p>
            </div>
            <span className="text-xl font-bold">{formatCurrency(revenue)}</span>
          </div>
          <div className="mt-8 flex h-64 items-end gap-3 sm:gap-5">
            {weekly.map((item, index) => (
              <div
                key={item.day}
                className="flex h-full flex-1 flex-col justify-end gap-2"
              >
                <div
                  className={`w-full rounded-t-lg ${
                    index === 5 ? "bg-[#e85d3f]" : "bg-[#e9e3db]"
                  }`}
                  style={{ height: `${item.value}%` }}
                />
                <span className="text-center text-[10px] font-semibold text-[#8d837b]">
                  {item.day}
                </span>
              </div>
            ))}
          </div>
        </section>

        <section className="card p-5 sm:p-6">
          <h2 className="font-bold">Sifariş statusları</h2>
          <p className="mt-1 text-xs text-[#8a8078]">
            Cari dövrün bölgüsü
          </p>
          <div className="mt-6 space-y-5">
            {[
              {
                label: "Çatdırılıb",
                status: OrderStatus.Served,
                color: "#58a36c",
              },
              {
                label: "Aktiv",
                status: OrderStatus.Preparing,
                color: "#e85d3f",
              },
              {
                label: "Gözləyir",
                status: OrderStatus.Pending,
                color: "#d99b3f",
              },
              {
                label: "Ləğv",
                status: OrderStatus.Cancelled,
                color: "#9c9188",
              },
            ].map((item) => {
              const count = orders.filter(
                (order) => order.status === item.status,
              ).length;
              const percent = orders.length
                ? Math.round((count / orders.length) * 100)
                : 0;
              return (
                <div key={item.label}>
                  <div className="mb-2 flex items-center justify-between text-xs">
                    <span className="flex items-center gap-2 font-semibold text-[#625a53]">
                      <span
                        className="h-2 w-2 rounded-full"
                        style={{ backgroundColor: item.color }}
                      />
                      {item.label}
                    </span>
                    <span className="font-bold">
                      {count} · {percent}%
                    </span>
                  </div>
                  <div className="h-2 overflow-hidden rounded-full bg-[#eee9e3]">
                    <div
                      className="h-full rounded-full"
                      style={{
                        width: `${Math.max(percent, 2)}%`,
                        backgroundColor: item.color,
                      }}
                    />
                  </div>
                </div>
              );
            })}
          </div>
        </section>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <div className="card p-5">
          <div className="text-xs font-bold uppercase tracking-[0.13em] text-[#8b8179]">
            Rezervasiyalar
          </div>
          <div className="mt-3 text-3xl font-bold">
            {reservationsQuery.data?.totalCount ?? 0}
          </div>
          <p className="mt-2 text-xs text-[#8a8078]">Cari dövrdə ümumi</p>
        </div>
        <div className="card p-5">
          <div className="text-xs font-bold uppercase tracking-[0.13em] text-[#8b8179]">
            Tamamlanma
          </div>
          <div className="mt-3 text-3xl font-bold">
            {orders.length
              ? Math.round(
                  (orders.filter(
                    (order) => order.status === OrderStatus.Served,
                  ).length /
                    orders.length) *
                    100,
                )
              : 0}
            %
          </div>
          <p className="mt-2 text-xs text-[#8a8078]">Sifarişlərin uğur faizi</p>
        </div>
        <div className="card p-5">
          <div className="text-xs font-bold uppercase tracking-[0.13em] text-[#8b8179]">
            Pik saat
          </div>
          <div className="mt-3 text-3xl font-bold">19:00</div>
          <p className="mt-2 text-xs text-[#8a8078]">Ən yüksək sifariş axını</p>
        </div>
      </div>
    </div>
  );
}
