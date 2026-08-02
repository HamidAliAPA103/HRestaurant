import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Edit3, Mail, Plus, Power, RefreshCw, Search, Trash2, UsersRound } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { branchApi, branchKeys } from "@/api/branchApi";
import { employeeApi, employeeKeys } from "@/api/employeeApi";
import { uploadApi } from "@/api/uploadApi";
import { useAuthStore } from "@/features/auth/store/auth-store";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { Modal } from "@/shared/components/Modal";
import { PageHeader } from "@/shared/components/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { getErrorMessage, initials } from "@/shared/lib/utils";
import type { User } from "@/shared/types/domain";

const schema = z.object({
  name: z.string().trim().min(2, "Ad ən azı 2 simvol olmalıdır.").max(150),
  email: z.string().trim().email("Düzgün email daxil edin."),
  phone: z.string().trim().min(7, "Telefon nömrəsini daxil edin.").max(25),
  role: z.enum(["Manager", "Chef", "Waiter", "Cashier"]),
  branchId: z.string().uuid("Filial seçin."),
  salary: z.number().min(0, "Maaş mənfi ola bilməz."),
  hireDate: z.string().min(1, "İşə qəbul tarixini seçin."),
  emergencyContact: z.string().trim().min(7, "Təcili əlaqə nömrəsini daxil edin.").max(100),
  password: z.string(),
}).superRefine((value, context) => {
  if (value.password && value.password.length < 8) {
    context.addIssue({ code: "custom", path: ["password"], message: "Şifrə ən azı 8 simvol olmalıdır." });
  }
});
type Values = z.infer<typeof schema>;
const defaults: Values = {
  name: "", email: "", phone: "", role: "Waiter", branchId: "",
  salary: 0, hireDate: new Date().toISOString().slice(0, 10), emergencyContact: "", password: "",
};
const roleTone: Record<string, "success" | "warning" | "danger" | "info"> = {
  Manager: "danger", Chef: "warning", Waiter: "info", Cashier: "success",
};

export function EmployeePage() {
  const restaurantId = useAuthStore((state) => state.user?.restaurantId ?? "");
  const [search, setSearch] = useState("");
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<User | null>(null);
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const queryClient = useQueryClient();
  const employees = useQuery({
    queryKey: [...employeeKeys.all, search],
    queryFn: ({ signal }) => employeeApi.list({ pageSize: 100, search: search || undefined, signal }),
  });
  const branches = useQuery({
    queryKey: branchKeys.all,
    queryFn: ({ signal }) => branchApi.list({ pageSize: 100, signal }),
  });
  const { register, handleSubmit, reset, formState: { errors } } = useForm<Values>({
    resolver: zodResolver(schema), defaultValues: defaults,
  });
  useEffect(() => {
    if (!open) return;
    reset(editing ? {
      name: editing.name, email: editing.email, phone: editing.phone ?? "",
      role: editing.role as Values["role"], branchId: editing.branchId,
      salary: editing.salary, hireDate: editing.hireDate?.slice(0, 10) ?? defaults.hireDate,
      emergencyContact: editing.emergencyContact ?? "", password: "",
    } : defaults); setAvatarFile(null);
  }, [editing, open, reset]);

  const save = useMutation({
    mutationFn: async (values: Values) => {
      if (!editing && values.password.length < 8) {
        throw new Error("Yeni əməkdaş üçün ən azı 8 simvolluq şifrə daxil edin.");
      }
      const uploadedAvatar = avatarFile
        ? await uploadApi.image(avatarFile, "employee-avatar", restaurantId)
        : null;
      try {
        if (editing) {
          const response = await employeeApi.update(editing.id, {
            name: values.name, email: values.email, phone: values.phone, role: values.role,
            salary: values.salary, hireDate: values.hireDate, emergencyContact: values.emergencyContact,
            avatarUrl: uploadedAvatar?.data?.url ?? editing.avatarUrl ?? undefined,
          });
          if (editing.branchId !== values.branchId) await employeeApi.assignBranch(editing.id, values.branchId);
          if (uploadedAvatar?.data?.url && editing.avatarUrl) await uploadApi.remove(editing.avatarUrl, restaurantId);
          return response;
        }
        return await employeeApi.create({ ...values, restaurantId, avatarUrl: uploadedAvatar?.data?.url ?? undefined });
      } catch (error) {
        if (uploadedAvatar?.data?.url) {
          await uploadApi.remove(uploadedAvatar.data.url, restaurantId).catch(() => undefined);
        }
        throw error;
      }
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: employeeKeys.all });
      setOpen(false); setEditing(null); reset(defaults);
    },
  });
  const action = useMutation({
    mutationFn: ({ employee, kind }: { employee: User; kind: "delete" | "status" }) =>
      kind === "delete" ? employeeApi.remove(employee.id) : employeeApi.setActive(employee.id, !employee.isActive),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: employeeKeys.all }),
  });
  const items = employees.data?.data ?? [];

  return <div className="page-enter space-y-6">
    <PageHeader eyebrow="Komanda" title="Əməkdaşlar" description="Əməkdaş profilləri, filialları, rolları və giriş statusları." actions={
      <Button onClick={() => { setEditing(null); setOpen(true); }}><Plus className="h-4 w-4" />Əməkdaş əlavə et</Button>
    } />
    <div className="grid gap-4 sm:grid-cols-3">
      {[["Ümumi əməkdaş", employees.data?.totalCount ?? 0], ["Aktiv əməkdaş", items.filter((x) => x.isActive).length], ["Aktiv rollar", new Set(items.filter((x) => x.isActive).map((x) => x.role)).size]].map(([label, value]) =>
        <div key={String(label)} className="card flex items-center gap-4 p-5"><div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#f2ede6] text-[#e85d3f]"><UsersRound className="h-5 w-5" /></div><div><div className="text-2xl font-bold">{value}</div><div className="text-xs text-[#877d75]">{label}</div></div></div>)}
    </div>
    <div className="card flex flex-col gap-3 p-4 sm:flex-row">
      <label className="relative flex-1"><Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2" /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Ad, email və ya rol üzrə axtar..." className="h-11 w-full rounded-xl border bg-[#faf8f5] pl-10 pr-4 text-sm" /></label>
      <Button variant="secondary" loading={employees.isFetching} onClick={() => employees.refetch()}><RefreshCw className="h-4 w-4" />Yenilə</Button>
    </div>
    {employees.isLoading ? <LoadingState label="Əməkdaşlar yüklənir" /> : employees.isError ? <ErrorState message={getErrorMessage(employees.error)} onRetry={() => employees.refetch()} /> : items.length === 0 ? <EmptyState title="Əməkdaş tapılmadı" /> :
      <div className="table-shell overflow-x-auto"><table className="data-table min-w-[900px]"><thead><tr><th>Əməkdaş</th><th>Filial</th><th>Rol</th><th>Maaş</th><th>Status</th><th>Əməliyyat</th></tr></thead><tbody>{items.map((employee) => <tr key={employee.id}>
        <td><div className="flex items-center gap-3"><div className="grid h-10 w-10 place-items-center rounded-xl bg-[#efeae3] text-xs font-bold">{initials(employee.name)}</div><div><p className="font-bold">{employee.name}</p><p className="flex items-center gap-1 text-xs text-[#8b8179]"><Mail className="h-3 w-3" />{employee.email}</p></div></div></td>
        <td>{employee.branchName}</td><td><Badge tone={roleTone[employee.role] ?? "neutral"}>{employee.role}</Badge></td><td>{employee.salary.toFixed(2)} ₼</td><td><Badge tone={employee.isActive ? "success" : "danger"}>{employee.isActive ? "Aktiv" : "Deaktiv"}</Badge></td>
        <td><div className="flex gap-1"><button type="button" aria-label="Redaktə et" className="rounded-lg p-2 hover:bg-[#f0ece6]" onClick={() => { setEditing(employee); setOpen(true); }}><Edit3 className="h-4 w-4" /></button><button type="button" aria-label={employee.isActive ? "Deaktiv et" : "Aktiv et"} disabled={action.isPending} className="rounded-lg p-2 hover:bg-[#f0ece6]" onClick={() => action.mutate({ employee, kind: "status" })}><Power className="h-4 w-4" /></button><button type="button" aria-label="Sil" disabled={action.isPending} className="rounded-lg p-2 text-red-600 hover:bg-red-50" onClick={() => { if (window.confirm(`${employee.name} silinsin?`)) action.mutate({ employee, kind: "delete" }); }}><Trash2 className="h-4 w-4" /></button></div></td>
      </tr>)}</tbody></table></div>}
    <Modal open={open} onClose={() => { setOpen(false); setEditing(null); }} title={editing ? "Əməkdaşı redaktə et" : "Yeni əməkdaş"} description="Profil, filial və sistem rolunu təyin edin.">
      <form className="space-y-4" onSubmit={handleSubmit((values) => save.mutate(values))}>
        <div className="grid gap-4 sm:grid-cols-2"><FormField label="Ad və soyad" error={errors.name?.message} {...register("name")} /><FormField label="Email" type="email" error={errors.email?.message} {...register("email")} /></div>
        <div className="grid gap-4 sm:grid-cols-2"><FormField label="Telefon" error={errors.phone?.message} {...register("phone")} /><FormField label="Təcili əlaqə" error={errors.emergencyContact?.message} {...register("emergencyContact")} /></div>
        <div className="grid gap-4 sm:grid-cols-2"><label className="block"><span className="mb-2 block text-sm font-semibold">Filial</span><select className="h-12 w-full rounded-xl border px-4" {...register("branchId")}><option value="">Seçin</option>{(branches.data?.data ?? []).map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select>{errors.branchId && <span className="mt-1 block text-xs text-red-600">{errors.branchId.message}</span>}</label><label className="block"><span className="mb-2 block text-sm font-semibold">Rol</span><select className="h-12 w-full rounded-xl border px-4" {...register("role")}><option>Manager</option><option>Chef</option><option>Waiter</option><option>Cashier</option></select></label></div>
        <div className="grid gap-4 sm:grid-cols-2"><FormField label="Maaş" type="number" step="0.01" error={errors.salary?.message} {...register("salary", { valueAsNumber: true })} /><FormField label="İşə qəbul tarixi" type="date" error={errors.hireDate?.message} {...register("hireDate")} /></div>
        {!editing && <FormField label="Müvəqqəti şifrə" type="password" error={errors.password?.message} {...register("password")} />}
        <label className="block"><span className="mb-2 block text-sm font-semibold">Avatar (JPEG, PNG və ya WebP)</span>{editing?.avatarUrl && !avatarFile && <img src={editing.avatarUrl} alt={`${editing.name} avatarı`} className="mb-2 h-20 w-20 rounded-xl object-cover" />}<input type="file" accept="image/jpeg,image/png,image/webp" onChange={(event) => setAvatarFile(event.target.files?.[0] ?? null)} className="w-full text-sm" /></label>
        {save.isError && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{getErrorMessage(save.error)}</p>}
        <div className="flex justify-end gap-2"><Button type="button" variant="secondary" onClick={() => setOpen(false)}>Ləğv et</Button><Button type="submit" loading={save.isPending}>Yadda saxla</Button></div>
      </form>
    </Modal>
  </div>;
}
