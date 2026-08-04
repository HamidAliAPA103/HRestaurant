import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Edit3, ImagePlus, Plus, Power, RefreshCw, Search, Star, Trash2, UtensilsCrossed } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { categoryApi, categoryKeys } from "@/api/categoryApi";
import { ingredientApi, ingredientKeys } from "@/api/ingredientApi";
import { menuItemApi, menuItemKeys } from "@/api/menuItemApi";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { Modal } from "@/shared/components/Modal";
import { PageHeader } from "@/shared/components/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { formatCurrency, getErrorMessage } from "@/shared/lib/utils";
import type { MenuItem } from "@/shared/types/domain";

const optionalUrl = z.string().trim().url("Düzgün URL daxil edin.").or(z.literal(""));
const schema = z.object({
  name: z.string().trim().min(2).max(100),
  desc: z.string().trim().min(3).max(1000),
  nutrition: z.string().trim().max(1000),
  price: z.number().positive(),
  discountPercentage: z.number().min(0).max(100),
  preparationTimeMinutes: z.number().int().min(1).max(1440),
  categoryId: z.string().uuid("Kateqoriya seçin."),
  imageUrl: optionalUrl,
  model3DUrl: optionalUrl,
  modelPosterUrl: optionalUrl,
  modelScale: z.number().min(0.01).max(100),
  modelRotationX: z.number().min(-360).max(360),
  modelRotationY: z.number().min(-360).max(360),
  modelRotationZ: z.number().min(-360).max(360),
  is3DEnabled: z.boolean(),
  videoUrl: optionalUrl,
  videoPosterUrl: optionalUrl,
  videoDurationSeconds: z.number().int().min(0),
  isVideoEnabled: z.boolean(),
  videoDisplayOrder: z.number().int().min(0),
}).superRefine((values, context) => {
  if (values.is3DEnabled && !values.model3DUrl) {
    context.addIssue({
      code: "custom",
      path: ["model3DUrl"],
      message: "3D görünüş aktivdirsə model URL-i tələb olunur.",
    });
  }
  if (values.isVideoEnabled && !values.videoUrl) context.addIssue({ code: "custom", path: ["videoUrl"], message: "Video aktivdirsə video URL-i tələb olunur." });
});
type Values = z.infer<typeof schema>;
const defaults: Values = {
  name: "", desc: "", nutrition: "", price: 0, discountPercentage: 0,
  preparationTimeMinutes: 20, categoryId: "", imageUrl: "", model3DUrl: "",
  modelPosterUrl: "", modelScale: 1, modelRotationX: 0, modelRotationY: 0,
  modelRotationZ: 0, is3DEnabled: false,
  videoUrl: "", videoPosterUrl: "", videoDurationSeconds: 0, isVideoEnabled: false, videoDisplayOrder: 0,
};
const allowedImages = ["image/jpeg", "image/png", "image/webp"];

export function MenuPage() {
  const [search, setSearch] = useState(""); const [categoryId, setCategoryId] = useState(""); const [available, setAvailable] = useState(""); const [page, setPage] = useState(1); const [open, setOpen] = useState(false); const [editing, setEditing] = useState<MenuItem | null>(null); const [image, setImage] = useState<File | null>(null); const [imageError, setImageError] = useState(""); const [ingredientAmounts, setIngredientAmounts] = useState<Record<string, number>>({});
  const queryClient = useQueryClient();
  const menu = useQuery({ queryKey: [...menuItemKeys.all, page, search, categoryId, available], queryFn: ({ signal }) => menuItemApi.list({ pageNumber: page, pageSize: 20, search: search || undefined, categoryId: categoryId || undefined, isAvailable: available === "" ? undefined : available === "true", signal }) });
  const categories = useQuery({ queryKey: categoryKeys.all, queryFn: ({ signal }) => categoryApi.list({ pageSize: 100, isActive: true, signal }) });
  const ingredients = useQuery({ queryKey: ingredientKeys.all, queryFn: ({ signal }) => ingredientApi.list({ pageSize: 100, isActive: true, signal }) });
  const { register, handleSubmit, reset, formState: { errors } } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: defaults });
  useEffect(() => { if (!open) return; reset(editing ? {
    name: editing.name, desc: editing.desc, nutrition: editing.nutrition,
    price: editing.price, discountPercentage: editing.discountPercentage,
    preparationTimeMinutes: editing.preparationTimeMinutes,
    categoryId: editing.categoryId, imageUrl: editing.imageURL ?? "",
    model3DUrl: editing.model3DUrl ?? "",
    modelPosterUrl: editing.modelPosterUrl ?? "",
    modelScale: editing.modelScale || 1,
    modelRotationX: editing.modelRotationX || 0,
    modelRotationY: editing.modelRotationY || 0,
    modelRotationZ: editing.modelRotationZ || 0,
    is3DEnabled: editing.is3DEnabled,
    videoUrl: editing.videoUrl ?? "", videoPosterUrl: editing.videoPosterUrl ?? "",
    videoDurationSeconds: editing.videoDurationSeconds ?? 0,
    isVideoEnabled: editing.isVideoEnabled, videoDisplayOrder: editing.videoDisplayOrder,
  } : defaults); setImage(null); setImageError(""); setIngredientAmounts(Object.fromEntries((editing?.ingredients ?? []).map((item) => [item.ingredientId, item.requiredQuantity]))); }, [editing, open, reset]);
  const save = useMutation({ mutationFn: (values: Values) => { if (!editing && !image && !values.imageUrl) throw new Error("Şəkil faylı və ya URL daxil edin."); const selectedIngredients = Object.entries(ingredientAmounts).filter(([, amount]) => amount > 0).map(([ingredientId, requiredQuantity]) => ({ ingredientId, requiredQuantity })); const input = { ...values, imageUrl: values.imageUrl || editing?.imageURL || undefined, model3DUrl: values.model3DUrl || undefined, modelPosterUrl: values.modelPosterUrl || undefined, videoUrl: values.videoUrl || undefined, videoPosterUrl: values.videoPosterUrl || undefined, image: image ?? undefined, ingredients: selectedIngredients }; return editing ? menuItemApi.update(editing.id, input) : menuItemApi.create(input); }, onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: menuItemKeys.all }); setOpen(false); setEditing(null); } });
  const action = useMutation({ mutationFn: ({ item, kind }: { item: MenuItem; kind: "delete" | "available" | "popular" }) => kind === "delete" ? menuItemApi.remove(item.id) : kind === "available" ? menuItemApi.setAvailability(item.id, !item.isAvailable) : menuItemApi.setPopular(item.id, !item.isPopular), onSuccess: () => queryClient.invalidateQueries({ queryKey: menuItemKeys.all }) });
  const preview = useMemo(() => image ? URL.createObjectURL(image) : editing?.imageURL, [editing?.imageURL, image]);
  useEffect(() => () => { if (preview?.startsWith("blob:")) URL.revokeObjectURL(preview); }, [preview]);
  const selectImage = (file: File | null) => { setImageError(""); if (!file) { setImage(null); return; } if (!allowedImages.includes(file.type)) { setImageError("Yalnız JPEG, PNG və WebP şəkilləri qəbul edilir."); return; } if (file.size > 5 * 1024 * 1024) { setImageError("Şəkil 5 MB-dan böyük ola bilməz."); return; } setImage(file); };
  const items = menu.data?.data ?? [];
  return <div className="page-enter space-y-6"><PageHeader eyebrow="Menyu mühərriki" title="Menyu" description="Məhsullar, qiymətlər, resept inqrediyentləri və əlçatanlıq." actions={<Button onClick={() => { setEditing(null); setOpen(true); }}><Plus className="h-4 w-4" />Yeni məhsul</Button>} />
    <div className="card grid gap-3 p-4 sm:grid-cols-[1fr_auto_auto_auto]"><label className="relative"><Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2" /><input value={search} onChange={(event) => { setSearch(event.target.value); setPage(1); }} placeholder="Məhsul axtar..." className="h-11 w-full rounded-xl border pl-10 pr-3" /></label><select aria-label="Kateqoriya filtri" value={categoryId} onChange={(event) => { setCategoryId(event.target.value); setPage(1); }} className="h-11 rounded-xl border px-3"><option value="">Bütün kateqoriyalar</option>{(categories.data?.data ?? []).map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select><select aria-label="Mövcudluq filtri" value={available} onChange={(event) => setAvailable(event.target.value)} className="h-11 rounded-xl border px-3"><option value="">Bütün statuslar</option><option value="true">Mövcud</option><option value="false">Mövcud deyil</option></select><Button variant="secondary" loading={menu.isFetching} onClick={() => menu.refetch()}><RefreshCw className="h-4 w-4" />Yenilə</Button></div>
    {menu.isLoading ? <LoadingState label="Menyu yüklənir" /> : menu.isError ? <ErrorState message={getErrorMessage(menu.error)} onRetry={() => menu.refetch()} /> : !items.length ? <EmptyState title="Menyu məhsulu tapılmadı" /> : <><div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">{items.map((item) => <article key={item.id} className="card overflow-hidden"><div className="relative h-44 bg-[#ece6de]">{item.imageURL ? <img src={item.imageURL} alt={item.name} className="h-full w-full object-cover" /> : <div className="grid h-full place-items-center"><UtensilsCrossed className="h-8 w-8" /></div>}<div className="absolute left-3 top-3"><Badge tone={item.isAvailable ? "success" : "danger"}>{item.isAvailable ? "Satışda" : "Dayandırılıb"}</Badge></div>{item.isPopular && <Star aria-label="Populyar" className="absolute right-3 top-3 h-7 w-7 fill-amber-400 text-amber-400" />}</div><div className="p-5"><p className="text-xs font-bold uppercase text-[#e85d3f]">{item.categoryName}</p><h2 className="mt-2 text-lg font-bold">{item.name}</h2><p className="mt-2 line-clamp-2 min-h-10 text-sm text-[#82786f]">{item.desc}</p><div className="mt-4 flex items-center justify-between border-t pt-4"><div><span className="font-bold">{formatCurrency(item.finalPrice)}</span>{item.discountPercentage > 0 && <span className="ml-2 text-xs line-through">{formatCurrency(item.price)}</span>}</div><div className="flex gap-1"><button type="button" aria-label="Redaktə et" className="rounded-lg p-2" onClick={() => { setEditing(item); setOpen(true); }}><Edit3 className="h-4 w-4" /></button><button type="button" aria-label={item.isAvailable ? "Satışı dayandır" : "Satışa aç"} className="rounded-lg p-2" onClick={() => action.mutate({ item, kind: "available" })}><Power className="h-4 w-4" /></button><button type="button" aria-label={item.isPopular ? "Populyardan çıxar" : "Populyar et"} className="rounded-lg p-2" onClick={() => action.mutate({ item, kind: "popular" })}><Star className="h-4 w-4" /></button><button type="button" aria-label="Sil" className="rounded-lg p-2 text-red-600" onClick={() => { if (window.confirm(`${item.name} silinsin?`)) action.mutate({ item, kind: "delete" }); }}><Trash2 className="h-4 w-4" /></button></div></div></div></article>)}</div><div className="flex justify-end gap-2"><Button variant="secondary" disabled={!menu.data?.hasPreviousPage} onClick={() => setPage((value) => value - 1)}>Əvvəlki</Button><span className="grid h-11 place-items-center px-3 text-sm">{page} / {menu.data?.totalPages || 1}</span><Button variant="secondary" disabled={!menu.data?.hasNextPage} onClick={() => setPage((value) => value + 1)}>Növbəti</Button></div></>}
    <Modal open={open} onClose={() => { setOpen(false); setEditing(null); }} title={editing ? "Məhsulu redaktə et" : "Yeni məhsul"}>
      <form className="space-y-4" onSubmit={handleSubmit((values) => save.mutate(values))}>
        <label className="block"><span className="mb-2 block text-sm font-semibold">Şəkil</span>{preview && <img src={preview} alt="Şəkil önizləməsi" className="mb-3 h-32 w-full rounded-xl object-cover" />}<span className="flex cursor-pointer items-center justify-center gap-2 rounded-xl border border-dashed p-4"><ImagePlus className="h-5 w-5" />{image?.name ?? "Şəkil seçin"}<input type="file" accept="image/jpeg,image/png,image/webp" className="hidden" onChange={(event) => selectImage(event.target.files?.[0] ?? null)} /></span>{imageError && <span className="mt-1 block text-xs text-red-600">{imageError}</span>}</label>
        <FormField label="Şəkil URL-i" error={errors.imageUrl?.message} {...register("imageUrl")} />
        <div className="grid gap-4 sm:grid-cols-2"><FormField label="Ad" error={errors.name?.message} {...register("name")} /><label><span className="mb-2 block text-sm font-semibold">Kateqoriya</span><select className="h-12 w-full rounded-xl border px-3" {...register("categoryId")}><option value="">Seçin</option>{(categories.data?.data ?? []).map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select></label></div>
        <label><span className="mb-2 block text-sm font-semibold">Təsvir</span><textarea rows={3} className="w-full rounded-xl border p-3" {...register("desc")} /></label>
        <FormField label="Qida/allergen məlumatı" error={errors.nutrition?.message} {...register("nutrition")} />
        <div className="grid grid-cols-3 gap-3"><FormField label="Qiymət" type="number" step="0.01" error={errors.price?.message} {...register("price", { valueAsNumber: true })} /><FormField label="Endirim %" type="number" step="0.01" error={errors.discountPercentage?.message} {...register("discountPercentage", { valueAsNumber: true })} /><FormField label="Hazırlanma dəq." type="number" error={errors.preparationTimeMinutes?.message} {...register("preparationTimeMinutes", { valueAsNumber: true })} /></div>
        <fieldset className="space-y-3 rounded-2xl border p-4">
          <legend className="px-2 text-sm font-bold">3D model</legend>
          <label className="flex items-center gap-2 text-sm font-semibold"><input type="checkbox" {...register("is3DEnabled")} />3D görünüşü aktiv et</label>
          <div className="grid gap-3 sm:grid-cols-2"><FormField label="GLB/GLTF model URL-i" error={errors.model3DUrl?.message} {...register("model3DUrl")} /><FormField label="Model poster URL-i" error={errors.modelPosterUrl?.message} {...register("modelPosterUrl")} /></div>
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-4"><FormField label="Miqyas" type="number" min="0.01" max="100" step="0.01" error={errors.modelScale?.message} {...register("modelScale", { valueAsNumber: true })} /><FormField label="Rotasiya X" type="number" min="-360" max="360" step="0.1" error={errors.modelRotationX?.message} {...register("modelRotationX", { valueAsNumber: true })} /><FormField label="Rotasiya Y" type="number" min="-360" max="360" step="0.1" error={errors.modelRotationY?.message} {...register("modelRotationY", { valueAsNumber: true })} /><FormField label="Rotasiya Z" type="number" min="-360" max="360" step="0.1" error={errors.modelRotationZ?.message} {...register("modelRotationZ", { valueAsNumber: true })} /></div>
        </fieldset>
        <fieldset className="space-y-3 rounded-2xl border p-4"><legend className="px-2 text-sm font-bold">Qısa video</legend><label className="flex items-center gap-2 text-sm font-semibold"><input type="checkbox" {...register("isVideoEnabled")} />Video təqdimatını aktiv et</label><div className="grid gap-3 sm:grid-cols-2"><FormField label="MP4/WebM video URL-i" error={errors.videoUrl?.message} {...register("videoUrl")} /><FormField label="Video poster URL-i" error={errors.videoPosterUrl?.message} {...register("videoPosterUrl")} /></div><div className="grid gap-3 sm:grid-cols-2"><FormField label="Müddət (saniyə)" type="number" min="0" error={errors.videoDurationSeconds?.message} {...register("videoDurationSeconds", { valueAsNumber: true })} /><FormField label="Göstərilmə sırası" type="number" min="0" error={errors.videoDisplayOrder?.message} {...register("videoDisplayOrder", { valueAsNumber: true })} /></div></fieldset>
        <div><p className="mb-2 text-sm font-semibold">Resept inqrediyentləri</p><div className="max-h-44 space-y-2 overflow-y-auto rounded-xl border p-3">{(ingredients.data?.data ?? []).map((ingredient) => <label key={ingredient.id} className="flex items-center gap-3 text-sm"><input type="checkbox" checked={(ingredientAmounts[ingredient.id] ?? 0) > 0} onChange={(event) => setIngredientAmounts((current) => ({ ...current, [ingredient.id]: event.target.checked ? 1 : 0 }))} /><span className="flex-1">{ingredient.name}</span><input aria-label={`${ingredient.name} miqdarı`} type="number" min="0.001" step="0.001" disabled={!ingredientAmounts[ingredient.id]} value={ingredientAmounts[ingredient.id] || ""} onChange={(event) => setIngredientAmounts((current) => ({ ...current, [ingredient.id]: Number(event.target.value) }))} className="h-9 w-24 rounded-lg border px-2" /></label>)}</div></div>
        {save.isError && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{getErrorMessage(save.error)}</p>}
        <div className="flex justify-end gap-2"><Button type="button" variant="secondary" onClick={() => setOpen(false)}>Ləğv et</Button><Button type="submit" loading={save.isPending}>Yadda saxla</Button></div>
      </form>
    </Modal>
  </div>;
}
