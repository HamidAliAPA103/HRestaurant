import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Edit3, Plus, RefreshCw, Search, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { branchApi } from "@/api/branchApi";
import { customerApi, customerKeys } from "@/api/customerApi";
import type { CustomerDto, CustomerInput } from "@/api/contracts";
import { useAuthStore } from "@/features/auth/store/auth-store";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { Modal } from "@/shared/components/Modal";
import { PageHeader } from "@/shared/components/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { formatCurrency, formatDate, getErrorMessage } from "@/shared/lib/utils";

const schema = z.object({
  branchId: z.string().optional(),
  fullName: z.string().trim().min(2, "Ad ən azı 2 simvol olmalıdır.").max(100),
  phone: z.string().trim().min(7, "Telefon nömrəsi düzgün deyil.").max(20),
  email: z.string().trim().email("Email düzgün deyil.").or(z.literal("")),
  birthday: z.string().optional(),
  notes: z.string().max(1000).optional(),
});
type FormValues = z.infer<typeof schema>;
const defaults: FormValues = { branchId: "", fullName: "", phone: "", email: "", birthday: "", notes: "" };

export function CustomerPage() {
  const restaurantId = useAuthStore((state) => state.user?.restaurantId ?? "");
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<CustomerDto | null>(null);
  const queryClient = useQueryClient();
  const query = useQuery({ queryKey: [...customerKeys.all, page, search], queryFn: ({ signal }) => customerApi.list({ pageNumber: page, pageSize: 20, search: search || undefined, signal }) });
  const branches = useQuery({ queryKey: ["branches", "customers"], queryFn: ({ signal }) => branchApi.list({ pageSize: 100, signal }) });
  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: defaults });

  useEffect(() => {
    if (!open) return;
    reset(editing ? {
      branchId: editing.branchId ?? "", fullName: editing.fullName,
      phone: editing.phone, email: editing.email ?? "", birthday: editing.birthday ?? "",
      notes: editing.notes ?? "",
    } : defaults);
  }, [editing, open, reset]);

  const save = useMutation({
    mutationFn: (values: FormValues) => {
      const input: CustomerInput = { ...values, branchId: values.branchId || null, email: values.email || null, birthday: values.birthday || null, notes: values.notes || null };
      return editing ? customerApi.update(editing.id, input) : customerApi.create({ ...input, restaurantId });
    },
    onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: customerKeys.all }); setOpen(false); setEditing(null); reset(defaults); },
  });
  const remove = useMutation({ mutationFn: customerApi.remove, onSuccess: () => queryClient.invalidateQueries({ queryKey: customerKeys.all }) });

  function askDelete(customer: CustomerDto) {
    if (window.confirm(`${customer.fullName} adlı müştərini silmək istədiyinizə əminsiniz?`)) remove.mutate(customer.id);
  }

  return <div className="page-enter space-y-6">
    <PageHeader eyebrow="CRM" title="Müştərilər" description="Müştəri profilləri və real satış göstəriciləri." actions={<Button onClick={() => { setEditing(null); setOpen(true); }}><Plus className="h-4 w-4" />Yeni müştəri</Button>} />
    <div className="card flex flex-col gap-3 p-4 sm:flex-row">
      <label className="relative flex-1"><Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-[#968d85]" /><input value={search} onChange={(e) => { setSearch(e.target.value); setPage(1); }} placeholder="Ad, telefon və ya email..." className="h-11 w-full rounded-xl border bg-[#faf8f5] pl-10 pr-4 text-sm outline-none" /></label>
      <Button variant="secondary" loading={query.isFetching} onClick={() => query.refetch()}><RefreshCw className="h-4 w-4" />Yenilə</Button>
    </div>
    {query.isLoading ? <LoadingState label="Müştərilər yüklənir" /> : query.isError ? <ErrorState message={getErrorMessage(query.error)} onRetry={() => query.refetch()} /> : !query.data?.data?.length ? <EmptyState title="Müştəri tapılmadı" /> : <div className="table-shell overflow-x-auto"><table className="data-table min-w-[850px]"><thead><tr><th>Müştəri</th><th>Telefon</th><th>Sifariş</th><th>Xərcləmə</th><th>Son ziyarət</th><th>Əməliyyat</th></tr></thead><tbody>
      {query.data.data.map((customer) => <tr key={customer.id}><td><p className="font-bold">{customer.fullName}</p><p className="text-xs text-[#8a8078]">{customer.email || "Email yoxdur"}</p></td><td>{customer.phone}</td><td>{customer.totalOrders}</td><td className="font-bold">{formatCurrency(customer.totalSpent)}</td><td>{customer.lastVisitDate ? formatDate(customer.lastVisitDate, true) : "—"}</td><td><div className="flex gap-1"><button type="button" className="rounded-lg p-2 hover:bg-[#f0ece6]" aria-label="Redaktə et" onClick={() => { setEditing(customer); setOpen(true); }}><Edit3 className="h-4 w-4" /></button><button type="button" className="rounded-lg p-2 text-red-600 hover:bg-red-50" aria-label="Sil" disabled={remove.isPending} onClick={() => askDelete(customer)}><Trash2 className="h-4 w-4" /></button></div></td></tr>)}
    </tbody></table></div>}
    {query.data && query.data.totalPages > 1 && <div className="flex items-center justify-end gap-2"><Button variant="secondary" size="sm" disabled={!query.data.hasPreviousPage} onClick={() => setPage((x) => x - 1)}>Əvvəlki</Button><span className="text-xs">{page} / {query.data.totalPages}</span><Button variant="secondary" size="sm" disabled={!query.data.hasNextPage} onClick={() => setPage((x) => x + 1)}>Növbəti</Button></div>}

    <Modal open={open} onClose={() => { if (!save.isPending) setOpen(false); }} title={editing ? "Müştərini redaktə et" : "Yeni müştəri"} description="Məlumatlar restoran daxilində unikal yoxlanılır.">
      <form className="space-y-4" onSubmit={handleSubmit((values) => save.mutate(values))}>
        <FormField label="Tam ad" error={errors.fullName?.message} {...register("fullName")} />
        <div className="grid gap-4 sm:grid-cols-2"><FormField label="Telefon" error={errors.phone?.message} {...register("phone")} /><FormField label="Email" type="email" error={errors.email?.message} {...register("email")} /></div>
        <div className="grid gap-4 sm:grid-cols-2"><FormField label="Doğum tarixi" type="date" error={errors.birthday?.message} {...register("birthday")} /><label className="block"><span className="mb-2 block text-sm font-semibold">Filial</span><select className="h-12 w-full rounded-xl border bg-white px-4 text-sm" {...register("branchId")}><option value="">Filialsız</option>{branches.data?.data?.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label></div>
        <label className="block"><span className="mb-2 block text-sm font-semibold">Qeyd</span><textarea className="min-h-24 w-full rounded-xl border p-3 text-sm" {...register("notes")} />{errors.notes && <span className="text-xs text-red-600">{errors.notes.message}</span>}</label>
        {save.isError && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{getErrorMessage(save.error)}</p>}
        <div className="flex justify-end gap-2"><Button type="button" variant="secondary" disabled={save.isPending} onClick={() => setOpen(false)}>Ləğv et</Button><Button type="submit" loading={save.isPending}>{editing ? "Yadda saxla" : "Yarat"}</Button></div>
      </form>
    </Modal>
  </div>;
}
