import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Edit3, MapPin, Phone, Plus, Power, RefreshCw, Search, Store, Trash2 } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { restaurantApi, restaurantKeys } from "@/api/restaurantApi";
import { uploadApi } from "@/api/uploadApi";
import type { RestaurantDto } from "@/api/contracts";
import { useAuthStore } from "@/features/auth/store/auth-store";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { Modal } from "@/shared/components/Modal";
import { PageHeader } from "@/shared/components/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { getErrorMessage } from "@/shared/lib/utils";

const schema = z.object({
  name: z.string().trim().min(2, "Restoran adı ən azı 2 simvol olmalıdır.").max(150),
  slug: z.string().trim().regex(/^[A-Za-z0-9-]*$/, "Slug yalnız hərf, rəqəm və tire ola bilər."),
  adres: z.string().trim().min(5, "Ünvanı tam daxil edin.").max(250),
  number: z.string().trim().min(7, "Telefon nömrəsini daxil edin.").max(25),
  email: z.string().trim().email("Düzgün email daxil edin.").or(z.literal("")),
  description: z.string().trim().max(2000),
  logoUrl: z.string().trim().url("Düzgün logo URL-i daxil edin.").or(z.literal("")),
  coverImageUrl: z.string().trim().url("Düzgün cover URL-i daxil edin.").or(z.literal("")),
  currency: z.string().trim().length(3, "3 hərfli ISO valyuta kodu daxil edin."),
  taxRate: z.number().min(0).max(100),
});
type Values = z.infer<typeof schema>;
const defaults: Values = { name: "", slug: "", adres: "", number: "", email: "", description: "", logoUrl: "", coverImageUrl: "", currency: "AZN", taxRate: 18 };

export function RestaurantPage() {
  const isSuperAdmin = useAuthStore((state) => state.hasRole(["SuperAdmin"]));
  const [search, setSearch] = useState("");
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<RestaurantDto | null>(null);
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [coverFile, setCoverFile] = useState<File | null>(null);
  const [uploadProgress, setUploadProgress] = useState<number | null>(null);
  const queryClient = useQueryClient();
  const query = useQuery({
    queryKey: [...restaurantKeys.all, search],
    queryFn: ({ signal }) => restaurantApi.list({ pageSize: 100, search: search || undefined, signal }),
  });
  const { register, handleSubmit, reset, watch, formState: { errors } } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: defaults });
  const logoUrl = watch("logoUrl");
  const coverImageUrl = watch("coverImageUrl");
  const logoPreview = useMemo(() => logoFile ? URL.createObjectURL(logoFile) : logoUrl || null, [logoFile, logoUrl]);
  const coverPreview = useMemo(() => coverFile ? URL.createObjectURL(coverFile) : coverImageUrl || null, [coverFile, coverImageUrl]);
  useEffect(() => () => {
    if (logoPreview?.startsWith("blob:")) URL.revokeObjectURL(logoPreview);
  }, [logoPreview]);
  useEffect(() => () => {
    if (coverPreview?.startsWith("blob:")) URL.revokeObjectURL(coverPreview);
  }, [coverPreview]);
  useEffect(() => {
    if (!open) return;
    reset(editing ? {
      name: editing.name, slug: editing.slug, adres: editing.adres, number: editing.number,
      email: editing.email ?? "", description: editing.description ?? "", logoUrl: editing.logoUrl ?? "",
      coverImageUrl: editing.coverImageUrl ?? "", currency: editing.currency, taxRate: editing.taxRate,
    } : defaults); setLogoFile(null); setCoverFile(null);
  }, [editing, open, reset]);
  const save = useMutation({
    mutationFn: async (values: Values) => {
      const normalized = { ...values, slug: values.slug || undefined, email: values.email || undefined, description: values.description || undefined, logoUrl: values.logoUrl || undefined, coverImageUrl: values.coverImageUrl || undefined };
      if (!editing) {
        const created = await restaurantApi.create(normalized);
        const id = created.data;
        if (id && (logoFile || coverFile)) {
          const logo = logoFile ? await uploadApi.image(logoFile, "restaurant-logo", id, undefined, setUploadProgress) : null;
          const cover = coverFile ? await uploadApi.image(coverFile, "restaurant-cover", id, undefined, setUploadProgress) : null;
          await restaurantApi.update(id, { logoUrl: logo?.data?.url, coverImageUrl: cover?.data?.url });
        }
        return created;
      }
      const uploadedLogo = logoFile ? await uploadApi.image(logoFile, "restaurant-logo", editing.id, undefined, setUploadProgress) : null;
      const uploadedCover = coverFile ? await uploadApi.image(coverFile, "restaurant-cover", editing.id, undefined, setUploadProgress) : null;
      const response = await restaurantApi.update(editing.id, {
        name: values.name, adres: values.adres, number: values.number, email: values.email || undefined,
        description: values.description || undefined, logoUrl: (uploadedLogo?.data?.url ?? values.logoUrl) || undefined, coverImageUrl: (uploadedCover?.data?.url ?? values.coverImageUrl) || undefined,
      });
      if (values.currency !== editing.currency || values.taxRate !== editing.taxRate) await restaurantApi.updateSettings(editing.id, values.currency, values.taxRate);
      if (uploadedLogo?.data?.url && editing.logoUrl) await uploadApi.remove(editing.logoUrl, editing.id);
      if (uploadedCover?.data?.url && editing.coverImageUrl) await uploadApi.remove(editing.coverImageUrl, editing.id);
      return response;
    },
    onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: restaurantKeys.all }); setOpen(false); setEditing(null); },
    onSettled: () => setUploadProgress(null),
  });
  const action = useMutation({
    mutationFn: ({ restaurant, kind }: { restaurant: RestaurantDto; kind: "delete" | "status" }) => kind === "delete" ? restaurantApi.remove(restaurant.id) : restaurantApi.setActive(restaurant.id, !restaurant.isActive),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: restaurantKeys.all }),
  });
  const items = query.data?.data ?? [];
  return <div className="page-enter space-y-6">
    <PageHeader eyebrow="Şəbəkə idarəetməsi" title="Restoranlar" description="Restoran profili, əlaqə məlumatları və əməliyyat statusu." actions={isSuperAdmin ? <Button onClick={() => { setEditing(null); setOpen(true); }}><Plus className="h-4 w-4" />Yeni restoran</Button> : undefined} />
    <div className="card flex flex-col gap-3 p-4 sm:flex-row"><label className="relative flex-1"><Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2" /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Restoran axtar..." className="h-11 w-full rounded-xl border pl-10 pr-3" /></label><Button variant="secondary" loading={query.isFetching} onClick={() => query.refetch()}><RefreshCw className="h-4 w-4" />Yenilə</Button></div>
    {query.isLoading ? <LoadingState label="Restoranlar yüklənir" /> : query.isError ? <ErrorState message={getErrorMessage(query.error)} onRetry={() => query.refetch()} /> : items.length === 0 ? <EmptyState title="Restoran tapılmadı" /> :
      <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">{items.map((restaurant) => <article key={restaurant.id} className="card overflow-hidden"><div className={`h-2 ${restaurant.isActive ? "bg-[#5b8d68]" : "bg-[#b7afa8]"}`} />{restaurant.coverImageUrl && <img src={restaurant.coverImageUrl} alt="" className="h-32 w-full object-cover" />}<div className="p-6"><div className="flex items-start justify-between"><div className="grid h-12 w-12 place-items-center overflow-hidden rounded-2xl bg-[#f2ede6]">{restaurant.logoUrl ? <img src={restaurant.logoUrl} alt={`${restaurant.name} loqosu`} className="h-full w-full object-cover" /> : <Store className="h-5 w-5" />}</div><Badge tone={restaurant.isActive ? "success" : "danger"}>{restaurant.isActive ? "Aktiv" : "Deaktiv"}</Badge></div><h2 className="mt-5 text-xl font-bold">{restaurant.name}</h2><p className="mt-1 text-xs text-[#8b8179]">/{restaurant.slug}</p><div className="mt-4 space-y-2 text-sm text-[#716860]"><p className="flex gap-2"><MapPin className="h-4 w-4 shrink-0 text-[#e85d3f]" />{restaurant.adres}</p><p className="flex gap-2"><Phone className="h-4 w-4 text-[#e85d3f]" />{restaurant.number}</p></div><div className="mt-5 flex items-center justify-between border-t pt-4"><span className="text-xs text-[#8b8179]">{restaurant.currency} · {restaurant.taxRate}% vergi</span><div className="flex gap-1"><button type="button" aria-label="Redaktə et" className="rounded-lg p-2 hover:bg-[#f0ece6]" onClick={() => { setEditing(restaurant); setOpen(true); }}><Edit3 className="h-4 w-4" /></button><button type="button" aria-label={restaurant.isActive ? "Deaktiv et" : "Aktiv et"} disabled={action.isPending} className="rounded-lg p-2 hover:bg-[#f0ece6]" onClick={() => action.mutate({ restaurant, kind: "status" })}><Power className="h-4 w-4" /></button>{isSuperAdmin && <button type="button" aria-label="Sil" disabled={action.isPending} className="rounded-lg p-2 text-red-600 hover:bg-red-50" onClick={() => { if (window.confirm(`${restaurant.name} silinsin?`)) action.mutate({ restaurant, kind: "delete" }); }}><Trash2 className="h-4 w-4" /></button>}</div></div></div></article>)}</div>}
    <Modal open={open} onClose={() => { setOpen(false); setEditing(null); }} title={editing ? "Restoranı redaktə et" : "Yeni restoran"} description="Profil və əməliyyat məlumatlarını daxil edin."><form className="space-y-4" onSubmit={handleSubmit((values) => save.mutate(values))}><div className="grid gap-4 sm:grid-cols-2"><FormField label="Restoran adı" error={errors.name?.message} {...register("name")} /><FormField label="Slug" disabled={Boolean(editing)} error={errors.slug?.message} {...register("slug")} /></div><FormField label="Ünvan" error={errors.adres?.message} {...register("adres")} /><div className="grid gap-4 sm:grid-cols-2"><FormField label="Telefon" error={errors.number?.message} {...register("number")} /><FormField label="Email" type="email" error={errors.email?.message} {...register("email")} /></div><label className="block"><span className="mb-2 block text-sm font-semibold">Təsvir</span><textarea rows={3} className="w-full rounded-xl border p-3" {...register("description")} />{errors.description && <span className="text-xs text-red-600">{errors.description.message}</span>}</label><div className="grid gap-4 sm:grid-cols-2"><div>{logoPreview && <img src={logoPreview} alt="Logo önizləməsi" className="mb-2 h-24 w-full rounded-xl border object-contain" />}<FormField label="Logo URL" error={errors.logoUrl?.message} {...register("logoUrl")} /><input aria-label="Logo faylı" type="file" accept="image/jpeg,image/png,image/webp" className="mt-2 text-xs" onChange={(event) => setLogoFile(event.target.files?.[0] ?? null)} /></div><div>{coverPreview && <img src={coverPreview} alt="Cover önizləməsi" className="mb-2 h-24 w-full rounded-xl border object-cover" />}<FormField label="Cover URL" error={errors.coverImageUrl?.message} {...register("coverImageUrl")} /><input aria-label="Cover faylı" type="file" accept="image/jpeg,image/png,image/webp" className="mt-2 text-xs" onChange={(event) => setCoverFile(event.target.files?.[0] ?? null)} /></div></div>{uploadProgress !== null && <div role="progressbar" aria-label="Şəkil yüklənməsi" aria-valuemin={0} aria-valuemax={100} aria-valuenow={uploadProgress} className="space-y-1"><div className="h-2 overflow-hidden rounded-full bg-[#eee9e3]"><div className="h-full bg-[#e85d3f] transition-[width]" style={{ width: `${uploadProgress}%` }} /></div><p className="text-right text-xs text-[#786f68]">Yüklənir: {uploadProgress}%</p></div>}<div className="grid gap-4 sm:grid-cols-2"><FormField label="Valyuta" error={errors.currency?.message} {...register("currency")} /><FormField label="Vergi faizi" type="number" step="0.01" error={errors.taxRate?.message} {...register("taxRate", { valueAsNumber: true })} /></div>{save.isError && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{getErrorMessage(save.error)}</p>}<div className="flex justify-end gap-2"><Button type="button" variant="secondary" onClick={() => setOpen(false)}>Ləğv et</Button><Button type="submit" loading={save.isPending}>Yadda saxla</Button></div></form></Modal>
  </div>;
}
