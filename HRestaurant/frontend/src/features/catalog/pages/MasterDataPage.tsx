import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Edit3, Plus, Power, RefreshCw, Search, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { categoryApi } from "@/api/categoryApi";
import { IngredientUnit } from "@/api/contracts";
import { ingredientApi } from "@/api/ingredientApi";
import { supplierApi } from "@/api/supplierApi";
import { useAuthStore } from "@/features/auth/store/auth-store";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { Modal } from "@/shared/components/Modal";
import { PageHeader } from "@/shared/components/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { getErrorMessage } from "@/shared/lib/utils";

export type MasterDataMode = "categories" | "ingredients" | "suppliers";
interface Row { id: string; name: string; isActive: boolean; description?: string | null; imageUrl?: string | null; displayOrder?: number; unit?: number; contactPerson?: string; phone?: string; email?: string; address?: string }
const schema = z.object({ name: z.string().trim().min(2).max(100), description: z.string().max(500).optional(), imageUrl: z.string().url().or(z.literal("")).optional(), displayOrder: z.number().int().min(0), unit: z.number().int().min(0).max(4), contactPerson: z.string().max(100), phone: z.string().max(20), email: z.string().email().or(z.literal("")), address: z.string().max(250) });
type Values = z.infer<typeof schema>;
const defaults: Values = { name: "", description: "", imageUrl: "", displayOrder: 0, unit: 0, contactPerson: "", phone: "", email: "", address: "" };
const meta = { categories: { eyebrow: "Menyu strukturu", title: "Kateqoriyalar", description: "Menyu kateqoriyalarını və sıralamanı idarə edin." }, ingredients: { eyebrow: "Resept bazası", title: "İnqrediyentlər", description: "Resept və anbar üçün vahid inqrediyent kataloqu." }, suppliers: { eyebrow: "Təchizat", title: "Təchizatçılar", description: "Təchizatçı əlaqələri və aktiv statusları." } };
const unitLabels = ["Qram", "Kiloqram", "Millilitr", "Litr", "Ədəd"];

export function MasterDataPage({ mode }: { mode: MasterDataMode }) {
  const restaurantId = useAuthStore((x) => x.user?.restaurantId ?? ""); const [search, setSearch] = useState(""); const [open, setOpen] = useState(false); const [editing, setEditing] = useState<Row | null>(null); const queryClient = useQueryClient();
  const key = [mode];
  const query = useQuery({ queryKey: [...key, search], queryFn: async ({ signal }) => {
    if (mode === "categories") { const page = await categoryApi.list({ pageSize: 100, restaurantId, signal }); return { ...page, data: (page.data ?? []).map((x) => ({ ...x } as Row)) }; }
    if (mode === "ingredients") { const page = await ingredientApi.list({ pageSize: 100, restaurantId, search: search || undefined, signal }); return { ...page, data: (page.data ?? []).map((x) => ({ ...x } as Row)) }; }
    const page = await supplierApi.list({ pageSize: 100, restaurantId, search: search || undefined, signal }); return { ...page, data: (page.data ?? []).map((x) => ({ ...x } as Row)) };
  } });
  const { register, handleSubmit, reset, formState: { errors } } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: defaults });
  useEffect(() => { if (open) reset(editing ? { name: editing.name, description: editing.description ?? "", imageUrl: editing.imageUrl ?? "", displayOrder: editing.displayOrder ?? 0, unit: editing.unit ?? 0, contactPerson: editing.contactPerson ?? "", phone: editing.phone ?? "", email: editing.email ?? "", address: editing.address ?? "" } : defaults); }, [editing, open, reset]);
  const save = useMutation({ mutationFn: (values: Values) => {
    if (mode === "categories") { const input = { name: values.name, description: values.description || undefined, imageUrl: values.imageUrl || undefined, displayOrder: values.displayOrder }; return editing ? categoryApi.update(editing.id, input) : categoryApi.create({ ...input, resdaranId: restaurantId }); }
    if (mode === "ingredients") return editing ? ingredientApi.update(editing.id, { name: values.name, unit: values.unit as IngredientUnit, isActive: editing.isActive }) : ingredientApi.create({ restaurantId, name: values.name, unit: values.unit as IngredientUnit });
    const input = { name: values.name, contactPerson: values.contactPerson, phone: values.phone, email: values.email, address: values.address }; return editing ? supplierApi.update(editing.id, input) : supplierApi.create({ ...input, restaurantId });
  }, onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: key }); setOpen(false); setEditing(null); } });
  const action = useMutation({ mutationFn: ({ row, kind }: { row: Row; kind: "delete" | "toggle" }) => {
    if (kind === "delete") return mode === "categories" ? categoryApi.remove(row.id) : mode === "ingredients" ? ingredientApi.remove(row.id) : supplierApi.remove(row.id);
    if (mode === "categories") return categoryApi.setActive(row.id, !row.isActive); if (mode === "suppliers") return supplierApi.setActive(row.id, !row.isActive); return ingredientApi.update(row.id, { name: row.name, unit: (row.unit ?? 0) as IngredientUnit, isActive: !row.isActive });
  }, onSuccess: () => queryClient.invalidateQueries({ queryKey: key }) });
  const rows = (query.data?.data ?? []).filter((row) => row.name.toLocaleLowerCase("az").includes(search.toLocaleLowerCase("az")));
  return <div className="page-enter space-y-6"><PageHeader {...meta[mode]} actions={<Button onClick={() => { setEditing(null); setOpen(true); }}><Plus className="h-4 w-4" />Yeni əlavə et</Button>} /><div className="card flex gap-3 p-4"><label className="relative flex-1"><Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2" /><input value={search} onChange={(e) => setSearch(e.target.value)} className="h-11 w-full rounded-xl border pl-10 pr-3" placeholder="Axtar..." /></label><Button variant="secondary" loading={query.isFetching} onClick={() => query.refetch()}><RefreshCw className="h-4 w-4" />Yenilə</Button></div>
    {query.isLoading ? <LoadingState label="Məlumatlar yüklənir" /> : query.isError ? <ErrorState message={getErrorMessage(query.error)} onRetry={() => query.refetch()} /> : rows.length === 0 ? <EmptyState title="Məlumat tapılmadı" /> : <div className="table-shell overflow-x-auto"><table className="data-table min-w-[700px]"><thead><tr><th>Ad</th><th>Məlumat</th><th>Status</th><th>Əməliyyat</th></tr></thead><tbody>{rows.map((row) => <tr key={row.id}><td className="font-bold">{row.name}</td><td>{mode === "categories" ? `${row.description || "—"} · sıra ${row.displayOrder}` : mode === "ingredients" ? unitLabels[row.unit ?? 0] : `${row.contactPerson} · ${row.phone}`}</td><td><Badge tone={row.isActive ? "success" : "danger"}>{row.isActive ? "Aktiv" : "Deaktiv"}</Badge></td><td><div className="flex gap-1"><button type="button" aria-label="Redaktə et" className="rounded-lg p-2 hover:bg-[#f0ece6]" onClick={() => { setEditing(row); setOpen(true); }}><Edit3 className="h-4 w-4" /></button><button type="button" aria-label="Statusu dəyiş" className="rounded-lg p-2 hover:bg-[#f0ece6]" onClick={() => action.mutate({ row, kind: "toggle" })}><Power className="h-4 w-4" /></button><button type="button" aria-label="Sil" className="rounded-lg p-2 text-red-600 hover:bg-red-50" onClick={() => { if (window.confirm(`${row.name} silinsin?`)) action.mutate({ row, kind: "delete" }); }}><Trash2 className="h-4 w-4" /></button></div></td></tr>)}</tbody></table></div>}
    <Modal open={open} onClose={() => setOpen(false)} title={editing ? "Redaktə et" : "Yeni qeyd"} description={meta[mode].description}><form className="space-y-4" onSubmit={handleSubmit((values) => save.mutate(values))}><FormField label="Ad" error={errors.name?.message} {...register("name")} />{mode === "categories" && <><FormField label="Təsvir" error={errors.description?.message} {...register("description")} /><div className="grid gap-4 sm:grid-cols-2"><FormField label="Şəkil URL" error={errors.imageUrl?.message} {...register("imageUrl")} /><FormField label="Sıra" type="number" error={errors.displayOrder?.message} {...register("displayOrder", { valueAsNumber: true })} /></div></>}{mode === "ingredients" && <label className="block text-sm font-semibold">Vahid<select className="mt-2 h-12 w-full rounded-xl border px-3" {...register("unit", { valueAsNumber: true })}>{unitLabels.map((label, index) => <option key={label} value={index}>{label}</option>)}</select></label>}{mode === "suppliers" && <><FormField label="Əlaqəli şəxs" error={errors.contactPerson?.message} {...register("contactPerson")} /><div className="grid gap-4 sm:grid-cols-2"><FormField label="Telefon" error={errors.phone?.message} {...register("phone")} /><FormField label="Email" type="email" error={errors.email?.message} {...register("email")} /></div><FormField label="Ünvan" error={errors.address?.message} {...register("address")} /></>}{save.isError && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{getErrorMessage(save.error)}</p>}<div className="flex justify-end gap-2"><Button type="button" variant="secondary" onClick={() => setOpen(false)}>Ləğv et</Button><Button type="submit" loading={save.isPending}>Yadda saxla</Button></div></form></Modal>
  </div>;
}
