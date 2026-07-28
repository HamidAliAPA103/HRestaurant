import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Check,
  Minus,
  Plus,
  Search,
  ShoppingBag,
  Trash2,
  UtensilsCrossed,
} from "lucide-react";
import { useMemo, useState } from "react";
import {
  createResource,
  listResource,
} from "@/shared/api/resources";
import { Button } from "@/shared/components/Button";
import {
  ErrorState,
  LoadingState,
} from "@/shared/components/StatePanel";
import { formatCurrency, getErrorMessage } from "@/shared/lib/utils";
import type {
  DiningTable,
  MenuItem,
  OrderInput,
  User,
} from "@/shared/types/domain";
import { TableStatus } from "@/shared/types/domain";

interface CartItem {
  item: MenuItem;
  quantity: number;
}

export function PosOrderPage() {
  const [search, setSearch] = useState("");
  const [cart, setCart] = useState<CartItem[]>([]);
  const [tableId, setTableId] = useState("");
  const [customerId, setCustomerId] = useState("");
  const queryClient = useQueryClient();
  const menuQuery = useQuery({
    queryKey: ["menu", "pos"],
    queryFn: () => listResource<MenuItem>("/Menu"),
  });
  const tablesQuery = useQuery({
    queryKey: ["tables", "pos"],
    queryFn: () => listResource<DiningTable>("/Table"),
  });
  const customersQuery = useQuery({
    queryKey: ["users", "customers", "pos"],
    queryFn: () => listResource<User>("/User"),
  });
  const menu = useMemo(
    () =>
      (menuQuery.data?.data ?? []).filter((item) =>
        `${item.desc} ${item.nutrition}`
          .toLowerCase()
          .includes(search.toLowerCase()),
      ),
    [menuQuery.data?.data, search],
  );
  const customers = (customersQuery.data?.data ?? []).filter(
    (user) => user.role.toLowerCase() === "customer",
  );
  const total = cart.reduce(
    (sum, cartItem) => sum + cartItem.item.price * cartItem.quantity,
    0,
  );

  const mutation = useMutation({
    mutationFn: async () => {
      const input: OrderInput = {
        customerID: customerId,
        tableID: tableId || null,
        items: cart.map(({ item, quantity }) => ({
          orderId: "00000000-0000-0000-0000-000000000000",
          menuId: item.id,
          say: quantity,
          prices: item.price,
        })),
      };
      const response = await createResource<OrderInput>("/Order", input);
      if (!response.success) throw new Error(response.message);
      return response;
    },
    onSuccess: () => {
      setCart([]);
      setTableId("");
      queryClient.invalidateQueries({ queryKey: ["orders"] });
      queryClient.invalidateQueries({ queryKey: ["tables"] });
    },
  });

  function changeQuantity(item: MenuItem, delta: number) {
    setCart((current) => {
      const found = current.find((entry) => entry.item.id === item.id);
      if (!found && delta > 0) {
        return [...current, { item, quantity: 1 }];
      }
      return current
        .map((entry) =>
          entry.item.id === item.id
            ? { ...entry, quantity: entry.quantity + delta }
            : entry,
        )
        .filter((entry) => entry.quantity > 0);
    });
  }

  if (
    menuQuery.isLoading ||
    tablesQuery.isLoading ||
    customersQuery.isLoading
  ) {
    return <LoadingState label="POS terminalı hazırlanır" />;
  }

  if (menuQuery.isError) {
    return (
      <ErrorState
        message={getErrorMessage(menuQuery.error)}
        onRetry={() => menuQuery.refetch()}
      />
    );
  }

  return (
    <div className="page-enter grid gap-5 xl:grid-cols-[1fr_390px]">
      <section className="min-w-0 space-y-5">
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.18em] text-[#e85d3f]">
            Satış terminalı
          </p>
          <h1 className="mt-2 text-3xl font-bold tracking-[-0.04em] text-[#241f1b]">
            Yeni sifariş
          </h1>
          <p className="mt-2 text-sm text-[#7c726a]">
            Məhsulları seçin və sifarişi birbaşa mətbəxə göndərin.
          </p>
        </div>

        <div className="card p-4">
          <label className="relative block">
            <Search className="absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-[#978d85]" />
            <input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Menyuda axtar..."
              className="h-12 w-full rounded-xl border border-[#ded8d0] bg-[#faf8f5] pl-11 pr-4 text-sm outline-none focus:border-[#e85d3f]"
            />
          </label>
        </div>

        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 2xl:grid-cols-4">
          {menu.map((item) => {
            const cartItem = cart.find(
              (entry) => entry.item.id === item.id,
            );
            return (
              <button
                key={item.id}
                onClick={() => changeQuantity(item, 1)}
                className="card group overflow-hidden text-left transition hover:-translate-y-1 hover:border-[#e85d3f]/40 hover:shadow-lg"
              >
                <div className="relative h-32 overflow-hidden bg-[#eee9e2]">
                  {item.imageURL ? (
                    <img
                      src={item.imageURL}
                      alt={item.desc}
                      className="h-full w-full object-cover transition group-hover:scale-105"
                    />
                  ) : (
                    <div className="grid h-full place-items-center text-[#a29a91]">
                      <UtensilsCrossed className="h-7 w-7" />
                    </div>
                  )}
                  {cartItem && (
                    <span className="absolute right-2 top-2 grid h-7 w-7 place-items-center rounded-full bg-[#e85d3f] text-xs font-bold text-white shadow">
                      {cartItem.quantity}
                    </span>
                  )}
                </div>
                <div className="p-4">
                  <div className="line-clamp-2 min-h-10 text-sm font-bold leading-5 text-[#302a26]">
                    {item.desc}
                  </div>
                  <div className="mt-3 flex items-center justify-between">
                    <span className="font-bold text-[#e85d3f]">
                      {formatCurrency(item.price)}
                    </span>
                    <span className="grid h-7 w-7 place-items-center rounded-lg bg-[#f1ede7] text-[#5c544d]">
                      <Plus className="h-4 w-4" />
                    </span>
                  </div>
                </div>
              </button>
            );
          })}
        </div>
      </section>

      <aside className="xl:sticky xl:top-25 xl:h-[calc(100vh-7rem)]">
        <div className="card flex h-full flex-col overflow-hidden">
          <div className="border-b border-[#ebe5de] p-5">
            <div className="flex items-center justify-between">
              <div>
                <h2 className="font-bold text-[#29231f]">Cari sifariş</h2>
                <p className="mt-1 text-xs text-[#8a8078]">
                  {cart.reduce((sum, item) => sum + item.quantity, 0)} məhsul
                </p>
              </div>
              <div className="grid h-10 w-10 place-items-center rounded-xl bg-[#fff0ed] text-[#e85d3f]">
                <ShoppingBag className="h-5 w-5" />
              </div>
            </div>
            <div className="mt-4 grid grid-cols-2 gap-2">
              <select
                value={tableId}
                onChange={(event) => setTableId(event.target.value)}
                className="h-10 rounded-xl border border-[#ded8d0] bg-white px-3 text-xs outline-none focus:border-[#e85d3f]"
              >
                <option value="">Takeaway</option>
                {(tablesQuery.data?.data ?? [])
                  .filter((table) => table.status === TableStatus.Empty)
                  .map((table, index) => (
                    <option key={table.id} value={table.id}>
                      Masa {index + 1} · {table.tutum} nəfər
                    </option>
                  ))}
              </select>
              <select
                value={customerId}
                onChange={(event) => setCustomerId(event.target.value)}
                className="h-10 rounded-xl border border-[#ded8d0] bg-white px-3 text-xs outline-none focus:border-[#e85d3f]"
              >
                <option value="">Müştəri seçin</option>
                {customers.map((customer) => (
                  <option key={customer.id} value={customer.id}>
                    {customer.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="flex-1 space-y-3 overflow-y-auto p-4">
            {cart.length === 0 ? (
              <div className="grid h-full min-h-52 place-items-center text-center">
                <div>
                  <ShoppingBag className="mx-auto h-8 w-8 text-[#c1b8af]" />
                  <p className="mt-3 text-sm font-bold text-[#655d56]">
                    Səbət boşdur
                  </p>
                  <p className="mt-1 text-xs text-[#978d85]">
                    Menyudan məhsul seçin
                  </p>
                </div>
              </div>
            ) : (
              cart.map(({ item, quantity }) => (
                <div
                  key={item.id}
                  className="rounded-2xl border border-[#e9e3dc] p-3"
                >
                  <div className="flex gap-3">
                    <div className="h-12 w-12 shrink-0 overflow-hidden rounded-xl bg-[#eee9e3]">
                      {item.imageURL && (
                        <img
                          src={item.imageURL}
                          alt=""
                          className="h-full w-full object-cover"
                        />
                      )}
                    </div>
                    <div className="min-w-0 flex-1">
                      <div className="line-clamp-1 text-sm font-bold">
                        {item.desc}
                      </div>
                      <div className="mt-1 text-xs font-semibold text-[#e85d3f]">
                        {formatCurrency(item.price * quantity)}
                      </div>
                    </div>
                    <button
                      onClick={() =>
                        setCart((current) =>
                          current.filter(
                            (entry) => entry.item.id !== item.id,
                          ),
                        )
                      }
                      className="text-[#a49a92] hover:text-[#d34d35]"
                      aria-label="Məhsulu sil"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                  <div className="mt-3 flex items-center justify-end gap-2">
                    <button
                      onClick={() => changeQuantity(item, -1)}
                      className="grid h-7 w-7 place-items-center rounded-lg bg-[#f0ece6]"
                    >
                      <Minus className="h-3.5 w-3.5" />
                    </button>
                    <span className="w-5 text-center text-xs font-bold">
                      {quantity}
                    </span>
                    <button
                      onClick={() => changeQuantity(item, 1)}
                      className="grid h-7 w-7 place-items-center rounded-lg bg-[#26201c] text-white"
                    >
                      <Plus className="h-3.5 w-3.5" />
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>

          <div className="border-t border-[#e9e3dc] bg-[#faf8f5] p-5">
            <div className="flex items-center justify-between text-sm text-[#776e66]">
              <span>Ara cəm</span>
              <span>{formatCurrency(total)}</span>
            </div>
            <div className="mt-3 flex items-end justify-between">
              <span className="text-sm font-bold">Yekun</span>
              <span className="text-2xl font-bold tracking-tight">
                {formatCurrency(total)}
              </span>
            </div>
            {mutation.isError && (
              <p className="mt-3 rounded-lg bg-[#fff0ed] p-2 text-xs text-[#b5442f]">
                {getErrorMessage(mutation.error)}
              </p>
            )}
            <Button
              className="mt-4 w-full"
              size="lg"
              loading={mutation.isPending}
              disabled={!cart.length || !customerId}
              onClick={() => mutation.mutate()}
            >
              <Check className="h-4 w-4" />
              Sifarişi mətbəxə göndər
            </Button>
          </div>
        </div>
      </aside>
    </div>
  );
}
