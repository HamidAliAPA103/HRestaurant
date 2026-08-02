import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Edit3, Plus, Power, RefreshCw, Search, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { branchApi, branchKeys } from "@/api/branchApi";
import type { BranchDto } from "@/api/contracts";
import { useAuthStore } from "@/features/auth/store/auth-store";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { Modal } from "@/shared/components/Modal";
import { PageHeader } from "@/shared/components/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { getErrorMessage } from "@/shared/lib/utils";

const schema = z.object({ name: z.string().trim().min(2).max(100), slug: z.string().trim().regex(/^[A-Za-z0-9-]*$/), address: z.string().trim().min(3).max(250), phone: z.string().max(20), email: z.string().email().or(z.literal("")), timeZoneId: z.string().min(1) });
type Values = z.infer<typeof schema>;
const defaults: Values = { name: "", slug: "", address: "", phone: "", email: "", timeZoneId: "Asia/Baku" };

export function BranchPage() {
  const restaurantId = useAuthStore((x) => x.user?.restaurantId ?? ""); const [search, setSearch] = useState(""); const [open, setOpen] = useState(false); const [editing, setEditing] = useState<BranchDto | null>(null);
  const queryClient = useQueryClient(); const query = useQuery({ queryKey: [...branchKeys.all, search], queryFn: ({ signal }) => branchApi.list({ pageSize: 100, search: search || undefined, signal }) });
  const { register, handleSubmit, reset, formState: { errors } } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: defaults });
  useEffect(() => { if (open) reset(editing ? { name: editing.name, slug: editing.slug, address: editing.address, phone: editing.phone ?? "", email: editing.email ?? "", timeZoneId: editing.timeZoneId } : defaults); }, [editing, open, reset]);
  const save = useMutation({ mutationFn: (values: Values) => editing ? branchApi.update(editing.id, { ...values, phone: values.phone || undefined, email: values.email || undefined }) : branchApi.create({ ...values, phone: values.phone || undefined, email: values.email || undefined, restaurantId }), onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: branchKeys.all }); setOpen(false); setEditing(null); } });
  const action = useMutation({ mutationFn: ({ id, kind, active }: { id: string; kind: "delete" | "active"; active?: boolean }) => kind === "delete" ? branchApi.remove(id) : branchApi.setActive(id, Boolean(active)), onSuccess: () => queryClient.invalidateQueries({ queryKey: branchKeys.all }) });
  return <div className="page-enter space-y-6"><PageHeader eyebrow="Struktur" title="Filiallar" description="Filial məlumatları, status və iş saatları." actions={<Button onClick={() => { setEditing(null); setOpen(true); }}><Plus className="h-4 w-4" />Yeni filial</Button>} />
    <div className="card flex gap-3 p-4"><label className="relative flex-1"><Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2" /><input value={search} onChange={(e) => setSearch(e.target.value)} className="h-11 w-full rounded-xl border pl-10 pr-3" placeholder="Filial axtar..." /></label><Button variant="secondary" loading={query.isFetching} onClick={() => query.refetch()}><RefreshCw className="h-4 w-4" />Yenilə</Button></div>
    {query.isLoading ? <LoadingState label="Filiallar yüklənir" /> : query.isError ? <ErrorState message={getErrorMessage(query.error)} onRetry={() => query.refetch()} /> : !query.data?.data?.length ? <EmptyState title="Filial yoxdur" /> : <div className="table-shell overflow-x-auto"><table className="data-table min-w-[800px]"><thead><tr><th>Filial</th><th>Ünvan</th><th>Əlaqə</th><th>Menecer</th><th>Status</th><th>Əməliyyat</th></tr></thead><tbody>{query.data.data.map((branch) => <tr key={branch.id}><td><p className="font-bold">{branch.name}</p><p className="text-xs text-[#8a8078]">/{branch.slug}</p></td><td>{branch.address}</td><td>{branch.phone || branch.email || "—"}</td><td>{branch.managerName ?? "Təyin edilməyib"}</td><td><Badge tone={branch.isActive ? "success" : "danger"}>{branch.isActive ? "Aktiv" : "Deaktiv"}</Badge></td><td><div className="flex gap-1"><button type="button" aria-label="Redaktə et" className="rounded-lg p-2 hover:bg-[#f0ece6]" onClick={() => { setEditing(branch); setOpen(true); }}><Edit3 className="h-4 w-4" /></button><button type="button" aria-label={branch.isActive ? "Deaktiv et" : "Aktiv et"} className="rounded-lg p-2 hover:bg-[#f0ece6]" onClick={() => action.mutate({ id: branch.id, kind: "active", active: !branch.isActive })}><Power className="h-4 w-4" /></button><button type="button" aria-label="Sil" className="rounded-lg p-2 text-red-600 hover:bg-red-50" onClick={() => { if (window.confirm(`${branch.name} filialı silinsin?`)) action.mutate({ id: branch.id, kind: "delete" }); }}><Trash2 className="h-4 w-4" /></button></div></td></tr>)}</tbody></table></div>}
    <Modal open={open} onClose={() => setOpen(false)} title={editing ? "Filialı redaktə et" : "Yeni filial"} description="Filial məlumatlarını daxil edin."><form className="space-y-4" onSubmit={handleSubmit((values) => save.mutate(values))}><div className="grid gap-4 sm:grid-cols-2"><FormField label="Ad" error={errors.name?.message} {...register("name")} /><FormField label="Slug" error={errors.slug?.message} {...register("slug")} /></div><FormField label="Ünvan" error={errors.address?.message} {...register("address")} /><div className="grid gap-4 sm:grid-cols-2"><FormField label="Telefon" error={errors.phone?.message} {...register("phone")} /><FormField label="Email" type="email" error={errors.email?.message} {...register("email")} /></div><FormField label="Saat qurşağı" error={errors.timeZoneId?.message} {...register("timeZoneId")} />{save.isError && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{getErrorMessage(save.error)}</p>}<div className="flex justify-end gap-2"><Button type="button" variant="secondary" onClick={() => setOpen(false)}>Ləğv et</Button><Button type="submit" loading={save.isPending}>Yadda saxla</Button></div></form></Modal>
  </div>;
}
