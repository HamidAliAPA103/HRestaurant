import { useQuery } from "@tanstack/react-query";
import {
  Mail,
  Search,
  Star,
  UserPlus,
  UsersRound,
} from "lucide-react";
import { useMemo, useState } from "react";
import { listResource } from "@/shared/api/resources";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { PageHeader } from "@/shared/components/PageHeader";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "@/shared/components/StatePanel";
import {
  formatCurrency,
  getErrorMessage,
  initials,
} from "@/shared/lib/utils";
import type { Order, User } from "@/shared/types/domain";
import { OrderStatus } from "@/shared/types/domain";

export function CustomerPage() {
  const [search, setSearch] = useState("");
  const usersQuery = useQuery({
    queryKey: ["users", "customers"],
    queryFn: () => listResource<User>("/User"),
  });
  const ordersQuery = useQuery({
    queryKey: ["orders", "customers"],
    queryFn: () => listResource<Order>("/Order"),
  });
  const customers = useMemo(
    () =>
      (usersQuery.data?.data ?? []).filter(
        (user) =>
          user.role.toLowerCase() === "customer" &&
          `${user.name} ${user.email}`
            .toLowerCase()
            .includes(search.toLowerCase()),
      ),
    [search, usersQuery.data?.data],
  );
  const orders = ordersQuery.data?.data ?? [];

  return (
    <div className="page-enter space-y-6">
      <PageHeader
        eyebrow="CRM"
        title="Müştərilər"
        description="Qonaq profillərini, sifariş tarixçəsini və loyallıq göstəricilərini izləyin."
        actions={
          <Button>
            <UserPlus className="h-4 w-4" />
            Müştəri əlavə et
          </Button>
        }
      />

      <div className="grid gap-4 sm:grid-cols-3">
        <div className="card flex items-center gap-4 p-5">
          <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#f2ede6] text-[#e85d3f]">
            <UsersRound className="h-5 w-5" />
          </div>
          <div>
            <div className="text-2xl font-bold">{customers.length}</div>
            <div className="text-xs text-[#877d75]">Ümumi müştəri</div>
          </div>
        </div>
        <div className="card flex items-center gap-4 p-5">
          <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#fff3dd] text-[#bb7924]">
            <Star className="h-5 w-5" />
          </div>
          <div>
            <div className="text-2xl font-bold">
              {
                customers.filter(
                  (customer) =>
                    orders.filter(
                      (order) => order.customerID === customer.id,
                    ).length >= 3,
                ).length
              }
            </div>
            <div className="text-xs text-[#877d75]">Loyal müştəri</div>
          </div>
        </div>
        <div className="card flex items-center gap-4 p-5">
          <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#e7f4eb] text-[#47815a]">
            <Star className="h-5 w-5" />
          </div>
          <div>
            <div className="text-2xl font-bold">
              {formatCurrency(
                orders
                  .filter(
                    (order) => order.status !== OrderStatus.Cancelled,
                  )
                  .reduce((sum, order) => sum + order.totalPrices, 0),
              )}
            </div>
            <div className="text-xs text-[#877d75]">Müştəri dövriyyəsi</div>
          </div>
        </div>
      </div>

      <div className="card p-4">
        <label className="relative block max-w-md">
          <Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-[#968d85]" />
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Müştəri adı və ya email..."
            className="h-11 w-full rounded-xl border border-[#ded8d0] bg-[#faf8f5] pl-10 pr-4 text-sm outline-none focus:border-[#e85d3f]"
          />
        </label>
      </div>

      {usersQuery.isLoading ? (
        <LoadingState label="Müştərilər yüklənir" />
      ) : usersQuery.isError ? (
        <ErrorState
          message={getErrorMessage(usersQuery.error)}
          onRetry={() => usersQuery.refetch()}
        />
      ) : customers.length === 0 ? (
        <EmptyState title="Müştəri tapılmadı" />
      ) : (
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {customers.map((customer) => {
            const customerOrders = orders.filter(
              (order) => order.customerID === customer.id,
            );
            const spend = customerOrders.reduce(
              (sum, order) => sum + order.totalPrices,
              0,
            );
            return (
              <article key={customer.id} className="card p-5">
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-3">
                    <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#27211d] text-xs font-bold text-white">
                      {initials(customer.name)}
                    </div>
                    <div>
                      <h2 className="font-bold text-[#302a26]">
                        {customer.name}
                      </h2>
                      <div className="mt-1 flex items-center gap-1 text-xs text-[#8b8179]">
                        <Mail className="h-3 w-3" />
                        {customer.email}
                      </div>
                    </div>
                  </div>
                  <Badge
                    tone={customerOrders.length >= 3 ? "warning" : "neutral"}
                  >
                    {customerOrders.length >= 3 ? "VIP" : "Standart"}
                  </Badge>
                </div>
                <div className="mt-5 grid grid-cols-2 gap-3">
                  <div className="rounded-xl bg-[#f7f4ef] p-3">
                    <div className="text-lg font-bold">
                      {customerOrders.length}
                    </div>
                    <div className="text-[10px] text-[#8a8078]">Sifariş</div>
                  </div>
                  <div className="rounded-xl bg-[#f7f4ef] p-3">
                    <div className="text-lg font-bold">
                      {formatCurrency(spend)}
                    </div>
                    <div className="text-[10px] text-[#8a8078]">Dövriyyə</div>
                  </div>
                </div>
              </article>
            );
          })}
        </div>
      )}
    </div>
  );
}
