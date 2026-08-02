import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Eye, Merge, Percent, RefreshCw, Search, Split, Table2, XCircle } from "lucide-react";
import { useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { orderApi, orderKeys } from "@/api/orderApi";
import { tableApi } from "@/api/tableApi";
import { OrderStatus, TableStatus, type Order } from "@/shared/types/domain";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { Modal } from "@/shared/components/Modal";
import { PageHeader } from "@/shared/components/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { formatCurrency, formatDate, getErrorMessage } from "@/shared/lib/utils";

const labels = ["Gözləyir", "Təsdiqlənib", "Hazırlanır", "Hazır", "Servis edilib", "Tamamlanıb", "Ləğv edilib"];
const nextStatus: Partial<Record<OrderStatus, OrderStatus>> = {
  [OrderStatus.Pending]: OrderStatus.Confirmed,
  [OrderStatus.Ready]: OrderStatus.Served,
  [OrderStatus.Served]: OrderStatus.Completed,
};
type Operation = "table" | "discount" | "merge" | "split";

export function OrderPage() {
  const [searchParams] = useSearchParams();
  const [search, setSearch] = useState(() => searchParams.get("search") ?? "");
  const [status, setStatus] = useState("");
  const [selected, setSelected] = useState<Order | null>(null);
  const [operation, setOperation] = useState<Operation | null>(null);
  const [tableId, setTableId] = useState("");
  const [discount, setDiscount] = useState("0");
  const [sourceOrderId, setSourceOrderId] = useState("");
  const [splitItemId, setSplitItemId] = useState("");
  const [splitQuantity, setSplitQuantity] = useState("1");
  const queryClient = useQueryClient();

  const query = useQuery({
    queryKey: [...orderKeys.all, search, status],
    queryFn: ({ signal }) => orderApi.list({
      pageSize: 100,
      search: search || undefined,
      status: status === "" ? undefined : Number(status) as OrderStatus,
      signal,
    }),
  });
  const tables = useQuery({
    queryKey: ["tables", "order-operation", selected?.branchId],
    queryFn: ({ signal }) => tableApi.list({ branchId: selected!.branchId, isActive: true, pageSize: 100, signal }),
    enabled: Boolean(selected?.branchId),
  });
  const transition = useMutation({
    mutationFn: ({ order, next }: { order: Order; next: OrderStatus }) => orderApi.setStatus(order.id, next, order.rowVersion),
    onSuccess: async () => { setSelected(null); await queryClient.invalidateQueries({ queryKey: orderKeys.all }); },
  });
  const cancel = useMutation({
    mutationFn: (order: Order) => orderApi.cancel(
      order.id,
      window.prompt("Ləğv səbəbini daxil edin:")?.trim() || "Operator tərəfindən ləğv edildi.",
      order.isPaid,
      order.rowVersion,
    ),
    onSuccess: async () => { setSelected(null); await queryClient.invalidateQueries({ queryKey: orderKeys.all }); },
  });
  const manage = useMutation({
    mutationFn: async () => {
      if (!selected || !operation) throw new Error("Sifariş əməliyyatı seçilməyib.");
      if (operation === "table") {
        if (!tableId) throw new Error("Yeni masanı seçin.");
        await orderApi.changeTable(selected.id, tableId, selected.rowVersion);
      } else if (operation === "discount") {
        const value = Number(discount);
        if (!Number.isFinite(value) || value < 0 || value > 100) throw new Error("Endirim 0–100 aralığında olmalıdır.");
        await orderApi.applyDiscount(selected.id, value, selected.rowVersion);
      } else if (operation === "merge") {
        if (!sourceOrderId) throw new Error("Birləşdiriləcək sifarişi seçin.");
        await orderApi.merge(selected.id, [sourceOrderId], selected.rowVersion);
      } else {
        const item = selected.items.find((candidate) => candidate.id === splitItemId);
        const quantity = Number(splitQuantity);
        if (!item) throw new Error("Bölünəcək məhsulu seçin.");
        if (!Number.isInteger(quantity) || quantity < 1 || quantity > item.quantity) throw new Error(`Miqdar 1–${item.quantity} aralığında olmalıdır.`);
        await orderApi.split(selected.id, [{ orderItemId: item.id, quantity }], tableId || null, selected.rowVersion);
      }
    },
    onSuccess: async () => {
      setOperation(null); setSelected(null);
      await queryClient.invalidateQueries({ queryKey: orderKeys.all });
      await queryClient.invalidateQueries({ queryKey: ["tables"] });
    },
  });

  const openOperation = (value: Operation) => {
    setOperation(value);
    setTableId(selected?.tableId ?? "");
    setDiscount("0");
    setSourceOrderId("");
    setSplitItemId(selected?.items[0]?.id ?? "");
    setSplitQuantity("1");
    manage.reset();
  };
  const orders = query.data?.data ?? [];
  const mergeCandidates = orders.filter((order) => order.id !== selected?.id
    && order.branchId === selected?.branchId
    && order.status !== OrderStatus.Completed
    && order.status !== OrderStatus.Cancelled
    && !order.isPaid);
  const availableTables = (tables.data?.data ?? []).filter((table) =>
    table.status === TableStatus.Available || table.id === selected?.tableId);

  return <div className="page-enter space-y-6">
    <PageHeader eyebrow="Satış axını" title="Sifarişlər" description="Sifariş detalları, status keçidləri, masa və ödəniş əməliyyatları." actions={<Link to="/pos"><Button>Yeni sifariş</Button></Link>} />
    <div className="card flex flex-col gap-3 p-4 sm:flex-row">
      <label className="relative flex-1"><Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2" /><input value={search} onChange={(event) => setSearch(event.target.value)} className="h-11 w-full rounded-xl border pl-10" placeholder="Sifariş nömrəsi..." /></label>
      <select value={status} onChange={(event) => setStatus(event.target.value)} className="h-11 rounded-xl border px-3"><option value="">Bütün statuslar</option>{labels.map((label, index) => <option key={label} value={index}>{label}</option>)}</select>
      <Button variant="secondary" loading={query.isFetching} onClick={() => query.refetch()}><RefreshCw className="h-4 w-4" />Yenilə</Button>
    </div>
    {query.isLoading ? <LoadingState label="Sifarişlər yüklənir" /> : query.isError ? <ErrorState message={getErrorMessage(query.error)} onRetry={() => query.refetch()} /> : orders.length === 0 ? <EmptyState title="Sifariş yoxdur" /> :
      <div className="table-shell overflow-x-auto"><table className="data-table min-w-[900px]"><thead><tr><th>Sifariş</th><th>Filial / Masa</th><th>Müştəri</th><th>Status</th><th>Yekun</th><th>Ödəniş</th><th>Tarix</th><th>Əməliyyat</th></tr></thead><tbody>{orders.map((order) => <tr key={order.id}>
        <td className="font-bold">{order.orderNumber}</td><td>{order.branchName}<span className="block text-xs text-[#8a8078]">{order.tableNumber ? `Masa ${order.tableNumber}` : "Takeaway"}</span></td><td>{order.customerName ?? "Qonaq"}</td><td><Badge tone={order.status === OrderStatus.Cancelled ? "danger" : order.status === OrderStatus.Completed ? "success" : "warning"}>{labels[order.status]}</Badge></td><td className="font-bold">{formatCurrency(order.totalAmount)}</td><td><Badge tone={order.isPaid ? "success" : "danger"}>{order.isPaid ? "Ödənib" : "Qalıq var"}</Badge></td><td>{formatDate(order.creatAt, true)}</td>
        <td><div className="flex gap-1"><button type="button" aria-label="Detallara bax" className="rounded-lg p-2" onClick={() => { setSelected(order); setOperation(null); }}><Eye className="h-4 w-4" /></button>{order.status !== OrderStatus.Completed && order.status !== OrderStatus.Cancelled && <button type="button" aria-label="Ləğv et" className="rounded-lg p-2 text-red-600" onClick={() => cancel.mutate(order)}><XCircle className="h-4 w-4" /></button>}</div></td>
      </tr>)}</tbody></table></div>}

    <Modal open={Boolean(selected)} onClose={() => { setSelected(null); setOperation(null); }} title={selected?.orderNumber ?? "Sifariş"} description={selected ? `${selected.branchName} · ${formatDate(selected.creatAt, true)}` : ""}>
      {selected && <div className="space-y-4">
        <div className="space-y-2 rounded-xl bg-[#faf8f5] p-4">{selected.items.map((item) => <div key={item.id} className="flex justify-between text-sm"><span>{item.quantity}× {item.menuItemName}</span><span>{formatCurrency(item.totalPrice)}</span></div>)}</div>
        <div className="space-y-2 text-sm"><div className="flex justify-between"><span>Ara cəm</span><span>{formatCurrency(selected.subtotal)}</span></div><div className="flex justify-between"><span>Endirim</span><span>{formatCurrency(selected.discountAmount)}</span></div><div className="flex justify-between"><span>Vergi</span><span>{formatCurrency(selected.taxAmount)}</span></div><div className="flex justify-between border-t pt-2 font-bold"><span>Yekun</span><span>{formatCurrency(selected.totalAmount)}</span></div></div>
        {!selected.isPaid && selected.status !== OrderStatus.Completed && selected.status !== OrderStatus.Cancelled && <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
          <Button variant="secondary" onClick={() => openOperation("table")}><Table2 className="h-4 w-4" />Masa</Button>
          <Button variant="secondary" onClick={() => openOperation("discount")}><Percent className="h-4 w-4" />Endirim</Button>
          <Button variant="secondary" disabled={mergeCandidates.length === 0} onClick={() => openOperation("merge")}><Merge className="h-4 w-4" />Birləşdir</Button>
          <Button variant="secondary" disabled={selected.items.length === 0} onClick={() => openOperation("split")}><Split className="h-4 w-4" />Böl</Button>
        </div>}
        {operation && <form className="space-y-3 rounded-xl border p-4" onSubmit={(event) => { event.preventDefault(); manage.mutate(); }}>
          <h3 className="font-bold">{operation === "table" ? "Masanı dəyiş" : operation === "discount" ? "Endirim tətbiq et" : operation === "merge" ? "Sifarişləri birləşdir" : "Sifarişi böl"}</h3>
          {(operation === "table" || operation === "split") && <label className="block"><span className="mb-1 block text-sm font-semibold">{operation === "split" ? "Yeni sifariş üçün masa (istəyə bağlı)" : "Yeni masa"}</span><select value={tableId} onChange={(event) => setTableId(event.target.value)} className="h-11 w-full rounded-xl border px-3"><option value="">{operation === "split" ? "Masa təyin etmə" : "Masa seçin"}</option>{availableTables.map((table) => <option key={table.id} value={table.id}>Masa {table.tableNumber} · {table.capacity} nəfər</option>)}</select></label>}
          {operation === "discount" && <label className="block"><span className="mb-1 block text-sm font-semibold">Endirim faizi</span><input type="number" min="0" max="100" step="0.01" value={discount} onChange={(event) => setDiscount(event.target.value)} className="h-11 w-full rounded-xl border px-3" /></label>}
          {operation === "merge" && <label className="block"><span className="mb-1 block text-sm font-semibold">Mənbə sifariş</span><select value={sourceOrderId} onChange={(event) => setSourceOrderId(event.target.value)} className="h-11 w-full rounded-xl border px-3"><option value="">Sifariş seçin</option>{mergeCandidates.map((order) => <option key={order.id} value={order.id}>{order.orderNumber} · {formatCurrency(order.totalAmount)}</option>)}</select></label>}
          {operation === "split" && <div className="grid gap-3 sm:grid-cols-2"><label className="block"><span className="mb-1 block text-sm font-semibold">Məhsul</span><select value={splitItemId} onChange={(event) => setSplitItemId(event.target.value)} className="h-11 w-full rounded-xl border px-3">{selected.items.map((item) => <option key={item.id} value={item.id}>{item.menuItemName} · {item.quantity} əd.</option>)}</select></label><label className="block"><span className="mb-1 block text-sm font-semibold">Miqdar</span><input type="number" min="1" value={splitQuantity} onChange={(event) => setSplitQuantity(event.target.value)} className="h-11 w-full rounded-xl border px-3" /></label></div>}
          {manage.isError && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{getErrorMessage(manage.error)}</p>}
          <div className="flex justify-end gap-2"><Button type="button" variant="secondary" onClick={() => setOperation(null)}>İmtina</Button><Button type="submit" loading={manage.isPending}>Təsdiqlə</Button></div>
        </form>}
        {(transition.isError || cancel.isError) && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{getErrorMessage(transition.error ?? cancel.error)}</p>}
        <div className="flex flex-wrap justify-end gap-2"><Button variant="secondary" onClick={() => setSelected(null)}>Bağla</Button>{!selected.isPaid && <Link to={`/payments?orderId=${selected.id}`}><Button variant="secondary">Ödənişə keç</Button></Link>}{nextStatus[selected.status] !== undefined && <Button loading={transition.isPending} onClick={() => transition.mutate({ order: selected, next: nextStatus[selected.status]! })}>{labels[nextStatus[selected.status]!]}</Button>}</div>
      </div>}
    </Modal>
  </div>;
}
