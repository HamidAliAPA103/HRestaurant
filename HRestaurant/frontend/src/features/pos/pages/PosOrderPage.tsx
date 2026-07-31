import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Check, Minus, Plus, Search, ShoppingBag, Trash2, UtensilsCrossed } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useAuthStore } from "@/features/auth/store/auth-store";
import { createResource, listResource } from "@/shared/api/resources";
import { Button } from "@/shared/components/Button";
import { ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { formatCurrency, getErrorMessage } from "@/shared/lib/utils";
import {
  OrderType,
  TableStatus,
  type BranchSummary,
  type DiningTable,
  type MenuItem,
  type OrderCreateInput,
  type User,
} from "@/shared/types/domain";

interface CartItem { item: MenuItem; quantity: number; kitchenNote: string }

export function PosOrderPage() {
  const restaurantId = useAuthStore((state) => state.user?.restaurantId ?? "");
  const [search, setSearch] = useState("");
  const [category, setCategory] = useState("all");
  const [cart, setCart] = useState<CartItem[]>([]);
  const [branchId, setBranchId] = useState("");
  const [tableId, setTableId] = useState("");
  const [customerId, setCustomerId] = useState("");
  const [orderType, setOrderType] = useState(OrderType.DineIn);
  const [discount, setDiscount] = useState(0);
  const [notes, setNotes] = useState("");
  const [success, setSuccess] = useState("");
  const queryClient = useQueryClient();

  const menuQuery = useQuery({ queryKey: ["menu", "pos"], queryFn: () => listResource<MenuItem>("/Menu") });
  const branchesQuery = useQuery({
    queryKey: ["branches", "pos", restaurantId],
    queryFn: () => listResource<BranchSummary>("/Branch", { pageSize: 100 }),
    enabled: Boolean(restaurantId),
  });
  const tablesQuery = useQuery({ queryKey: ["tables", "pos"], queryFn: () => listResource<DiningTable>("/tables") });
  const customersQuery = useQuery({ queryKey: ["users", "customers", "pos"], queryFn: () => listResource<User>("/User") });

  const branches = (branchesQuery.data?.data ?? []).filter((branch) =>
    branch.restaurantId === restaurantId && branch.isActive);
  useEffect(() => {
    if (!branchId && branches.length === 1) setBranchId(branches[0].id);
  }, [branchId, branches]);

  const categories = useMemo(() => Array.from(new Set(
    (menuQuery.data?.data ?? []).map((item) => item.categoryName).filter(Boolean))), [menuQuery.data?.data]);
  const menu = useMemo(() => (menuQuery.data?.data ?? []).filter((item) =>
    item.isAvailable
    && (category === "all" || item.categoryName === category)
    && `${item.name} ${item.desc}`.toLocaleLowerCase("az").includes(search.toLocaleLowerCase("az"))),
  [menuQuery.data?.data, category, search]);
  const customers = (customersQuery.data?.data ?? []).filter((user) =>
    user.role.toLowerCase() === "customer" && (!user.restaurantId || user.restaurantId === restaurantId));
  const tables = (tablesQuery.data?.data ?? []).filter((table) =>
    table.branchId === branchId && table.isActive && table.status === TableStatus.Available);
  const subtotal = cart.reduce((sum, entry) =>
    sum + (entry.item.finalPrice || entry.item.price) * entry.quantity, 0);
  const estimatedTotal = subtotal * (1 - discount / 100);

  const mutation = useMutation({
    mutationFn: () => {
      const input: OrderCreateInput = {
        restaurantId,
        branchId,
        tableId: orderType === OrderType.DineIn ? tableId : null,
        customerId: customerId || null,
        orderType,
        notes: notes || undefined,
        discountPercentage: discount,
        isPriority: false,
        items: cart.map(({ item, quantity, kitchenNote }) => ({
          menuItemId: item.id,
          quantity,
          kitchenNote: kitchenNote || undefined,
        })),
      };
      return createResource<OrderCreateInput>("/orders", input);
    },
    onSuccess: (response) => {
      setSuccess(`Sifariş yaradıldı: ${response.data ?? ""}`);
      setCart([]); setTableId(""); setCustomerId(""); setDiscount(0); setNotes("");
      queryClient.invalidateQueries({ queryKey: ["orders"] });
      queryClient.invalidateQueries({ queryKey: ["tables"] });
    },
  });

  function changeQuantity(item: MenuItem, delta: number) {
    setCart((current) => {
      const found = current.find((entry) => entry.item.id === item.id);
      if (!found && delta > 0) return [...current, { item, quantity: 1, kitchenNote: "" }];
      return current.map((entry) => entry.item.id === item.id
        ? { ...entry, quantity: entry.quantity + delta } : entry)
        .filter((entry) => entry.quantity > 0);
    });
  }

  if (menuQuery.isLoading || branchesQuery.isLoading || tablesQuery.isLoading || customersQuery.isLoading)
    return <LoadingState label="POS terminalı hazırlanır" />;
  const failedQuery = [menuQuery, branchesQuery, tablesQuery, customersQuery].find((query) => query.isError);
  if (failedQuery?.isError) return <ErrorState message={getErrorMessage(failedQuery.error)} onRetry={() => failedQuery.refetch()} />;

  return (
    <div className="page-enter grid gap-5 xl:grid-cols-[1fr_410px]">
      <section className="min-w-0 space-y-5">
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.18em] text-[#e85d3f]">Satış terminalı</p>
          <h1 className="mt-2 text-3xl font-bold tracking-[-0.04em]">Yeni sifariş</h1>
          <p className="mt-2 text-sm text-[#7c726a]">Qiymətlər serverdən alınır və bütün məbləğlər backend-də hesablanır.</p>
        </div>
        <div className="card space-y-3 p-4">
          <label className="relative block">
            <Search className="absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-[#978d85]" />
            <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Menyuda axtar..."
              className="h-12 w-full rounded-xl border border-[#ded8d0] bg-[#faf8f5] pl-11 pr-4 text-sm outline-none" />
          </label>
          <div className="flex gap-2 overflow-x-auto">
            {["all", ...categories].map((name) => <button key={name} onClick={() => setCategory(name)}
              className={`whitespace-nowrap rounded-xl px-3 py-2 text-xs font-semibold ${category === name ? "bg-[#26201c] text-white" : "bg-[#f1ede7]"}`}>
              {name === "all" ? "Hamısı" : name}
            </button>)}
          </div>
        </div>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 2xl:grid-cols-4">
          {menu.map((item) => {
            const selected = cart.find((entry) => entry.item.id === item.id);
            return <button key={item.id} onClick={() => changeQuantity(item, 1)} className="card group overflow-hidden text-left transition hover:-translate-y-1">
              <div className="relative h-32 overflow-hidden bg-[#eee9e2]">
                {item.imageURL ? <img src={item.imageURL} alt={item.name} className="h-full w-full object-cover" />
                  : <div className="grid h-full place-items-center"><UtensilsCrossed className="h-7 w-7" /></div>}
                {selected && <span className="absolute right-2 top-2 grid h-7 w-7 place-items-center rounded-full bg-[#e85d3f] text-xs font-bold text-white">{selected.quantity}</span>}
              </div>
              <div className="p-4"><div className="line-clamp-2 min-h-10 text-sm font-bold">{item.name}</div>
                <div className="mt-3 flex items-center justify-between"><span className="font-bold text-[#e85d3f]">{formatCurrency(item.finalPrice || item.price)}</span><Plus className="h-4 w-4" /></div>
              </div>
            </button>;
          })}
        </div>
      </section>

      <aside className="xl:sticky xl:top-25 xl:h-[calc(100vh-7rem)]">
        <div className="card flex h-full flex-col overflow-hidden">
          <div className="space-y-3 border-b border-[#ebe5de] p-5">
            <div className="flex items-center justify-between"><div><h2 className="font-bold">Cari sifariş</h2><p className="text-xs text-[#8a8078]">{cart.reduce((sum, item) => sum + item.quantity, 0)} məhsul</p></div><ShoppingBag className="h-5 w-5" /></div>
            <div className="grid grid-cols-2 gap-2">
              <select value={branchId} onChange={(event) => { setBranchId(event.target.value); setTableId(""); }} className="h-10 rounded-xl border px-3 text-xs">
                <option value="">Filial seçin</option>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}
              </select>
              <select value={orderType} onChange={(event) => { setOrderType(Number(event.target.value)); setTableId(""); }} className="h-10 rounded-xl border px-3 text-xs">
                <option value={OrderType.DineIn}>Zalda</option><option value={OrderType.Takeaway}>Takeaway</option><option value={OrderType.Delivery}>Çatdırılma</option>
              </select>
              {orderType === OrderType.DineIn && <select value={tableId} onChange={(event) => setTableId(event.target.value)} className="h-10 rounded-xl border px-3 text-xs">
                <option value="">Masa seçin</option>{tables.map((table) => <option key={table.id} value={table.id}>{table.tableNumber} · {table.capacity} nəfər</option>)}
              </select>}
              <select value={customerId} onChange={(event) => setCustomerId(event.target.value)} className="h-10 rounded-xl border px-3 text-xs">
                <option value="">Müştəri (istəyə bağlı)</option>{customers.map((customer) => <option key={customer.id} value={customer.id}>{customer.name}</option>)}
              </select>
            </div>
            <div className="grid grid-cols-2 gap-2">
              <input type="number" min={0} max={100} value={discount} onChange={(event) => setDiscount(Math.min(100, Math.max(0, Number(event.target.value))))} placeholder="Endirim %" className="h-10 rounded-xl border px-3 text-xs" />
              <input value={notes} onChange={(event) => setNotes(event.target.value)} placeholder="Sifariş qeydi" className="h-10 rounded-xl border px-3 text-xs" />
            </div>
          </div>
          <div className="flex-1 space-y-3 overflow-y-auto p-4">
            {cart.length === 0 ? <div className="grid h-full min-h-52 place-items-center text-center"><div><ShoppingBag className="mx-auto h-8 w-8" /><p className="mt-3 text-sm font-bold">Səbət boşdur</p></div></div>
              : cart.map(({ item, quantity, kitchenNote }) => <div key={item.id} className="rounded-2xl border p-3">
                <div className="flex items-center gap-3"><div className="min-w-0 flex-1"><p className="truncate text-sm font-bold">{item.name}</p><p className="text-xs text-[#e85d3f]">{formatCurrency((item.finalPrice || item.price) * quantity)}</p></div>
                  <button onClick={() => setCart((current) => current.filter((entry) => entry.item.id !== item.id))} aria-label="Məhsulu sil"><Trash2 className="h-4 w-4" /></button></div>
                <div className="mt-3 flex items-center gap-2"><button onClick={() => changeQuantity(item, -1)}><Minus className="h-4 w-4" /></button><span className="w-6 text-center text-xs font-bold">{quantity}</span><button onClick={() => changeQuantity(item, 1)}><Plus className="h-4 w-4" /></button>
                  <input value={kitchenNote} onChange={(event) => setCart((current) => current.map((entry) => entry.item.id === item.id ? { ...entry, kitchenNote: event.target.value } : entry))} placeholder="Mətbəx qeydi" className="ml-auto h-8 min-w-0 flex-1 rounded-lg border px-2 text-xs" />
                </div>
              </div>)}
          </div>
          <div className="border-t bg-[#faf8f5] p-5">
            <div className="flex justify-between text-sm"><span>Ara cəm</span><span>{formatCurrency(subtotal)}</span></div>
            <div className="mt-2 flex justify-between text-sm"><span>Endirim</span><span>{discount}%</span></div>
            <div className="mt-3 flex justify-between font-bold"><span>Təxmini yekun</span><span>{formatCurrency(estimatedTotal)}</span></div>
            {success && <p className="mt-3 rounded-lg bg-green-50 p-2 text-xs text-green-800">{success}</p>}
            {mutation.isError && <p className="mt-3 rounded-lg bg-red-50 p-2 text-xs text-red-800">{getErrorMessage(mutation.error)}</p>}
            <Button className="mt-4 w-full" size="lg" loading={mutation.isPending}
              disabled={!cart.length || !branchId || (orderType === OrderType.DineIn && !tableId)} onClick={() => mutation.mutate()}>
              <Check className="h-4 w-4" />Sifarişi yarat
            </Button>
          </div>
        </div>
      </aside>
    </div>
  );
}
