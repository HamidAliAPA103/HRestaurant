import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Edit3, PackagePlus, Plus, RefreshCw, Search, Trash2 } from "lucide-react";
import { useEffect, useState, type FormEvent } from "react";
import { branchApi } from "@/api/branchApi";
import { IngredientUnit, type InventoryDto, type InventoryInput } from "@/api/contracts";
import { ingredientApi } from "@/api/ingredientApi";
import { inventoryApi, inventoryKeys } from "@/api/inventoryApi";
import { supplierApi } from "@/api/supplierApi";
import { useAuthStore } from "@/features/auth/store/auth-store";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { Modal } from "@/shared/components/Modal";
import { PageHeader } from "@/shared/components/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { formatCurrency, formatDate, getErrorMessage } from "@/shared/lib/utils";

const unitLabels = ["qram", "kq", "ml", "litr", "ədəd"];
const emptyForm = { branchId: "", ingredientId: "", supplierId: "", currentQuantity: 0, minimumQuantity: 0, unit: IngredientUnit.Gram, purchasePrice: 0, expirationDate: "", batchNumber: "" };

export function InventoryPage() {
  const restaurantId = useAuthStore((state) => state.user?.restaurantId ?? "");
  const [search, setSearch] = useState(""); const [page, setPage] = useState(1);
  const [open, setOpen] = useState(false); const [stockOpen, setStockOpen] = useState(false);
  const [editing, setEditing] = useState<InventoryDto | null>(null); const [selected, setSelected] = useState<InventoryDto | null>(null);
  const [form, setForm] = useState(emptyForm); const [stock, setStock] = useState({ mode: "in", quantity: 0, reason: "", referenceNumber: "" });
  const queryClient = useQueryClient();
  const query = useQuery({ queryKey: [...inventoryKeys.all, page, search], queryFn: ({ signal }) => inventoryApi.list({ pageNumber: page, pageSize: 20, search: search || undefined, signal }) });
  const branches = useQuery({ queryKey: ["branches", "inventory"], queryFn: ({ signal }) => branchApi.list({ pageSize: 100, signal }) });
  const ingredients = useQuery({ queryKey: ["ingredients", "inventory"], queryFn: ({ signal }) => ingredientApi.list({ pageSize: 100, isActive: true, signal }) });
  const suppliers = useQuery({ queryKey: ["suppliers", "inventory"], queryFn: ({ signal }) => supplierApi.list({ pageSize: 100, isActive: true, signal }) });

  useEffect(() => { if (!open) return; setForm(editing ? { branchId: editing.branchId, ingredientId: editing.ingredientId, supplierId: editing.supplierId ?? "", currentQuantity: editing.currentQuantity, minimumQuantity: editing.minimumQuantity, unit: editing.unit, purchasePrice: editing.purchasePrice, expirationDate: editing.expirationDate ?? "", batchNumber: editing.batchNumber ?? "" } : { ...emptyForm, branchId: branches.data?.data?.[0]?.id ?? "" }); }, [editing, open, branches.data?.data]);

  const save = useMutation({ mutationFn: () => {
    if (editing) return inventoryApi.update(editing.id, { supplierId: form.supplierId || null, minimumQuantity: form.minimumQuantity, unit: form.unit, purchasePrice: form.purchasePrice, expirationDate: form.expirationDate || null, batchNumber: form.batchNumber || null, isActive: editing.isActive, rowVersion: editing.rowVersion });
    const input: InventoryInput = { restaurantId, branchId: form.branchId, ingredientId: form.ingredientId, supplierId: form.supplierId || null, currentQuantity: form.currentQuantity, minimumQuantity: form.minimumQuantity, unit: form.unit, purchasePrice: form.purchasePrice, expirationDate: form.expirationDate || null, batchNumber: form.batchNumber || null };
    return inventoryApi.create(input);
  }, onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: inventoryKeys.all }); setOpen(false); setEditing(null); } });
  const movement = useMutation({ mutationFn: () => {
    if (!selected) throw new Error("Stok seçilməyib.");
    if (stock.mode === "adjust") return inventoryApi.adjust(selected.id, { newQuantity: stock.quantity, reason: stock.reason, referenceNumber: stock.referenceNumber || null, rowVersion: selected.rowVersion });
    const body = { quantity: stock.quantity, transactionType: stock.mode === "in" ? 0 : 1, reason: stock.reason, referenceNumber: stock.referenceNumber || null, rowVersion: selected.rowVersion };
    return stock.mode === "in" ? inventoryApi.stockIn(selected.id, body) : inventoryApi.stockOut(selected.id, body);
  }, onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: inventoryKeys.all }); setStockOpen(false); setSelected(null); } });
  const remove = useMutation({ mutationFn: inventoryApi.remove, onSuccess: () => queryClient.invalidateQueries({ queryKey: inventoryKeys.all }) });

  function submit(event: FormEvent) { event.preventDefault(); if (!form.branchId || !form.ingredientId || form.minimumQuantity < 0 || form.purchasePrice < 0) return; save.mutate(); }
  function askDelete(item: InventoryDto) { if (window.confirm(`${item.ingredientName} stok qeydini silmək istəyirsiniz?`)) remove.mutate(item.id); }

  const dependenciesLoading = branches.isLoading || ingredients.isLoading || suppliers.isLoading;
  return <div className="page-enter space-y-6">
    <PageHeader eyebrow="Təchizat zənciri" title="Anbar" description="Stok məlumatları, hərəkətlər və minimum həddlər real API ilə idarə olunur." actions={<Button onClick={() => { setEditing(null); setOpen(true); }}><Plus className="h-4 w-4" />Yeni stok</Button>} />
    <div className="card flex gap-3 p-4"><label className="relative flex-1"><Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2" /><input className="h-11 w-full rounded-xl border bg-[#faf8f5] pl-10 pr-4 text-sm" placeholder="İnqrediyent və ya təchizatçı..." value={search} onChange={(e) => { setSearch(e.target.value); setPage(1); }} /></label><Button variant="secondary" loading={query.isFetching} onClick={() => query.refetch()}><RefreshCw className="h-4 w-4" />Yenilə</Button></div>
    {query.isLoading ? <LoadingState label="Stok yüklənir" /> : query.isError ? <ErrorState message={getErrorMessage(query.error)} onRetry={() => query.refetch()} /> : !query.data?.data?.length ? <EmptyState title="Stok tapılmadı" /> : <div className="table-shell overflow-x-auto"><table className="data-table min-w-[950px]"><thead><tr><th>Məhsul</th><th>Filial</th><th>Miqdar</th><th>Minimum</th><th>Status</th><th>Qiymət</th><th>Son istifadə</th><th>Əməliyyat</th></tr></thead><tbody>{query.data.data.map((item) => {
      const low = item.currentQuantity <= item.minimumQuantity; return <tr key={item.id}><td><p className="font-bold">{item.ingredientName}</p><p className="text-xs text-[#8a8078]">{item.supplierName ?? "Təchizatçısız"}</p></td><td>{item.branchName}</td><td>{item.currentQuantity} {unitLabels[item.unit]}</td><td>{item.minimumQuantity} {unitLabels[item.unit]}</td><td><Badge tone={low ? "danger" : "success"} dot>{low ? "Kritik" : "Normal"}</Badge></td><td>{formatCurrency(item.purchasePrice)}</td><td>{item.expirationDate ? formatDate(item.expirationDate) : "—"}</td><td><div className="flex gap-1"><button type="button" aria-label="Stok hərəkəti" className="rounded-lg p-2 hover:bg-[#f0ece6]" onClick={() => { setSelected(item); setStock({ mode: "in", quantity: 0, reason: "", referenceNumber: "" }); setStockOpen(true); }}><PackagePlus className="h-4 w-4" /></button><button type="button" aria-label="Redaktə et" className="rounded-lg p-2 hover:bg-[#f0ece6]" onClick={() => { setEditing(item); setOpen(true); }}><Edit3 className="h-4 w-4" /></button><button type="button" aria-label="Sil" className="rounded-lg p-2 text-red-600 hover:bg-red-50" onClick={() => askDelete(item)}><Trash2 className="h-4 w-4" /></button></div></td></tr>;
    })}</tbody></table></div>}
    {query.data && query.data.totalPages > 1 && <div className="flex justify-end gap-2"><Button size="sm" variant="secondary" disabled={!query.data.hasPreviousPage} onClick={() => setPage((x) => x - 1)}>Əvvəlki</Button><Button size="sm" variant="secondary" disabled={!query.data.hasNextPage} onClick={() => setPage((x) => x + 1)}>Növbəti</Button></div>}

    <Modal open={open} onClose={() => setOpen(false)} title={editing ? "Stoku redaktə et" : "Yeni stok"} description="Miqdar dəyişiklikləri ayrıca stok hərəkəti kimi saxlanılır."><form className="space-y-4" onSubmit={submit}>
      <div className="grid gap-4 sm:grid-cols-2"><Select label="Filial" value={form.branchId} disabled={Boolean(editing)} onChange={(value) => setForm((x) => ({ ...x, branchId: value }))} options={branches.data?.data?.map((x) => [x.id, x.name]) ?? []} /><Select label="İnqrediyent" value={form.ingredientId} disabled={Boolean(editing)} onChange={(value) => setForm((x) => ({ ...x, ingredientId: value }))} options={ingredients.data?.data?.map((x) => [x.id, x.name]) ?? []} /></div>
      <div className="grid gap-4 sm:grid-cols-2"><Select label="Təchizatçı" value={form.supplierId} onChange={(value) => setForm((x) => ({ ...x, supplierId: value }))} options={[["", "Seçilməyib"], ...(suppliers.data?.data?.map((x) => [x.id, x.name] as [string,string]) ?? [])]} /><Select label="Vahid" value={String(form.unit)} onChange={(value) => setForm((x) => ({ ...x, unit: Number(value) as IngredientUnit }))} options={unitLabels.map((x, i) => [String(i), x])} /></div>
      <div className="grid gap-4 sm:grid-cols-3">{!editing && <NumberField label="Başlanğıc miqdar" value={form.currentQuantity} onChange={(value) => setForm((x) => ({ ...x, currentQuantity: value }))} />}<NumberField label="Minimum" value={form.minimumQuantity} onChange={(value) => setForm((x) => ({ ...x, minimumQuantity: value }))} /><NumberField label="Alış qiyməti" value={form.purchasePrice} onChange={(value) => setForm((x) => ({ ...x, purchasePrice: value }))} /></div>
      <div className="grid gap-4 sm:grid-cols-2"><label className="text-sm font-semibold">Son istifadə<input type="date" value={form.expirationDate} onChange={(e) => setForm((x) => ({ ...x, expirationDate: e.target.value }))} className="mt-2 h-11 w-full rounded-xl border px-3" /></label><label className="text-sm font-semibold">Partiya №<input value={form.batchNumber} onChange={(e) => setForm((x) => ({ ...x, batchNumber: e.target.value }))} className="mt-2 h-11 w-full rounded-xl border px-3" /></label></div>
      {(save.isError || dependenciesLoading) && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{dependenciesLoading ? "Seçimlər yüklənir..." : getErrorMessage(save.error)}</p>}<div className="flex justify-end gap-2"><Button type="button" variant="secondary" onClick={() => setOpen(false)}>Ləğv et</Button><Button type="submit" loading={save.isPending} disabled={dependenciesLoading || !form.branchId || !form.ingredientId}>Yadda saxla</Button></div>
    </form></Modal>

    <Modal open={stockOpen} onClose={() => setStockOpen(false)} title="Stok hərəkəti" description={selected?.ingredientName}><form className="space-y-4" onSubmit={(e) => { e.preventDefault(); movement.mutate(); }}><Select label="Əməliyyat" value={stock.mode} onChange={(value) => setStock((x) => ({ ...x, mode: value }))} options={[["in", "Stok girişi"], ["out", "Stok çıxışı"], ["adjust", "Düzəliş"]]} /><NumberField label={stock.mode === "adjust" ? "Yeni miqdar" : "Miqdar"} value={stock.quantity} onChange={(quantity) => setStock((x) => ({ ...x, quantity }))} /><label className="text-sm font-semibold">Səbəb<input required value={stock.reason} onChange={(e) => setStock((x) => ({ ...x, reason: e.target.value }))} className="mt-2 h-11 w-full rounded-xl border px-3" /></label><label className="text-sm font-semibold">İstinad nömrəsi<input value={stock.referenceNumber} onChange={(e) => setStock((x) => ({ ...x, referenceNumber: e.target.value }))} className="mt-2 h-11 w-full rounded-xl border px-3" /></label>{movement.isError && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{getErrorMessage(movement.error)}</p>}<div className="flex justify-end gap-2"><Button type="button" variant="secondary" onClick={() => setStockOpen(false)}>Ləğv et</Button><Button type="submit" loading={movement.isPending} disabled={stock.quantity < 0 || !stock.reason.trim()}>Tətbiq et</Button></div></form></Modal>
  </div>;
}

function Select({ label, value, options, disabled, onChange }: { label: string; value: string; options: string[][]; disabled?: boolean; onChange: (value: string) => void }) { return <label className="text-sm font-semibold">{label}<select disabled={disabled} value={value} onChange={(e) => onChange(e.target.value)} className="mt-2 h-11 w-full rounded-xl border bg-white px-3"><option value="">Seçin</option>{options.map(([id,name]) => <option key={`${id}-${name}`} value={id}>{name}</option>)}</select></label>; }
function NumberField({ label, value, onChange }: { label: string; value: number; onChange: (value: number) => void }) { return <label className="text-sm font-semibold">{label}<input type="number" min={0} step="0.01" value={value} onChange={(e) => onChange(Number(e.target.value))} className="mt-2 h-11 w-full rounded-xl border px-3" /></label>; }
