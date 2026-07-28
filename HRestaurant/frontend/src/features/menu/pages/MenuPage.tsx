import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ImagePlus,
  Leaf,
  MoreHorizontal,
  Plus,
  Search,
  UtensilsCrossed,
} from "lucide-react";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { apiClient } from "@/shared/api/client";
import { listResource } from "@/shared/api/resources";
import type { ApiResponse } from "@/shared/types/api";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { Modal } from "@/shared/components/Modal";
import { PageHeader } from "@/shared/components/PageHeader";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "@/shared/components/StatePanel";
import { formatCurrency, getErrorMessage } from "@/shared/lib/utils";
import type {
  MenuCategory,
  MenuItem,
} from "@/shared/types/domain";

const schema = z.object({
  desc: z.string().min(3, "Məhsul adını və təsvirini daxil edin."),
  nutrition: z.string().min(2, "Tərkib məlumatını daxil edin."),
  price: z.number().positive("Qiymət sıfırdan böyük olmalıdır."),
  categoryId: z.string().min(1, "Kateqoriya seçin."),
});

type MenuForm = z.infer<typeof schema>;

export function MenuPage() {
  const [modalOpen, setModalOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [image, setImage] = useState<File | null>(null);
  const queryClient = useQueryClient();
  const menuQuery = useQuery({
    queryKey: ["menu"],
    queryFn: () => listResource<MenuItem>("/Menu"),
  });
  const categoryQuery = useQuery({
    queryKey: ["menu-categories"],
    queryFn: () => listResource<MenuCategory>("/MenuCategory"),
  });
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<MenuForm>({
    resolver: zodResolver(schema),
    defaultValues: {
      desc: "",
      nutrition: "",
      price: 0,
      categoryId: "",
    },
  });
  const categories = categoryQuery.data?.data ?? [];
  const categoryMap = Object.fromEntries(
    categories.map((category) => [category.id, category.name]),
  );
  const menuItems = useMemo(
    () =>
      (menuQuery.data?.data ?? []).filter((item) =>
        `${item.desc} ${item.nutrition}`
          .toLowerCase()
          .includes(search.toLowerCase()),
      ),
    [menuQuery.data?.data, search],
  );

  const mutation = useMutation({
    mutationFn: async (values: MenuForm) => {
      if (!image) throw new Error("Məhsul şəklini seçin.");
      const formData = new FormData();
      formData.append("Image", image);
      formData.append("Price", values.price.toString());
      formData.append("Desc", values.desc);
      formData.append("CategoryId", values.categoryId);
      formData.append("Nutrition", values.nutrition);
      const { data } = await apiClient.post<ApiResponse<string>>(
        "/Menu",
        formData,
      );
      if (!data.success) throw new Error(data.message);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["menu"] });
      reset();
      setImage(null);
      setModalOpen(false);
    },
  });

  return (
    <div className="page-enter space-y-6">
      <PageHeader
        eyebrow="Menyu mühərriki"
        title="Menyu"
        description="Məhsulları, kateqoriyaları, qiymətləri və əlçatanlığı idarə edin."
        actions={
          <Button onClick={() => setModalOpen(true)}>
            <Plus className="h-4 w-4" />
            Yeni məhsul
          </Button>
        }
      />

      <div className="card flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between">
        <label className="relative block w-full max-w-md">
          <Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-[#968d85]" />
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Menyu üzrə axtar..."
            className="h-11 w-full rounded-xl border border-[#ded8d0] bg-[#faf8f5] pl-10 pr-4 text-sm outline-none focus:border-[#e85d3f]"
          />
        </label>
        <div className="flex gap-2 overflow-x-auto">
          {["Hamısı", ...categories.slice(0, 4).map((item) => item.name)].map(
            (category, index) => (
              <button
                key={category}
                className={`whitespace-nowrap rounded-xl px-3.5 py-2 text-xs font-semibold ${
                  index === 0
                    ? "bg-[#26201c] text-white"
                    : "bg-[#f1ede7] text-[#6b625b] hover:bg-[#e8e2db]"
                }`}
              >
                {category}
              </button>
            ),
          )}
        </div>
      </div>

      {menuQuery.isLoading ? (
        <LoadingState label="Menyu yüklənir" />
      ) : menuQuery.isError ? (
        <ErrorState
          message={getErrorMessage(menuQuery.error)}
          onRetry={() => menuQuery.refetch()}
        />
      ) : menuItems.length === 0 ? (
        <EmptyState
          title={search ? "Uyğun məhsul tapılmadı" : "Menyu boşdur"}
          description={
            search
              ? "Axtarış sözünü dəyişin."
              : "İlk menyu məhsulunu əlavə edin."
          }
        />
      ) : (
        <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">
          {menuItems.map((item) => (
            <article
              key={item.id}
              className="card group overflow-hidden transition hover:-translate-y-1 hover:shadow-[0_16px_38px_rgba(52,42,34,.09)]"
            >
              <div className="relative h-44 overflow-hidden bg-[#ece6de]">
                {item.imageURL ? (
                  <img
                    src={item.imageURL}
                    alt={item.desc}
                    className="h-full w-full object-cover transition duration-500 group-hover:scale-105"
                  />
                ) : (
                  <div className="grid h-full place-items-center text-[#a0978f]">
                    <UtensilsCrossed className="h-8 w-8" />
                  </div>
                )}
                <div className="absolute left-3 top-3">
                  <Badge tone="success" dot>
                    Satışda
                  </Badge>
                </div>
                <button className="absolute right-3 top-3 grid h-8 w-8 place-items-center rounded-full bg-white/90 text-[#5d554e] shadow">
                  <MoreHorizontal className="h-4 w-4" />
                </button>
              </div>
              <div className="p-5">
                <div className="text-[10px] font-bold uppercase tracking-[0.13em] text-[#e85d3f]">
                  {categoryMap[item.categoryId] ?? "Menyu"}
                </div>
                <h2 className="mt-2 line-clamp-2 min-h-12 font-bold leading-6 text-[#2b2521]">
                  {item.desc}
                </h2>
                <div className="mt-3 flex items-center gap-1.5 text-xs text-[#82786f]">
                  <Leaf className="h-3.5 w-3.5 text-[#5c9b6d]" />
                  <span className="line-clamp-1">{item.nutrition}</span>
                </div>
                <div className="mt-5 flex items-center justify-between border-t border-[#eee9e3] pt-4">
                  <span className="text-lg font-bold text-[#29231f]">
                    {formatCurrency(item.price)}
                  </span>
                  <span className="text-xs text-[#8d837a]">Mövcuddur</span>
                </div>
              </div>
            </article>
          ))}
        </div>
      )}

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title="Menyuya məhsul əlavə et"
        description="Məhsul məlumatlarını və şəklini daxil edin."
      >
        <form
          className="space-y-4"
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
        >
          <label className="block">
            <span className="mb-2 block text-sm font-semibold text-[#3c3530]">
              Məhsul şəkli
            </span>
            <span className="flex min-h-24 cursor-pointer items-center justify-center gap-3 rounded-xl border border-dashed border-[#d6cec4] bg-[#faf8f5] p-4 text-sm text-[#766c64] hover:border-[#e85d3f]">
              <ImagePlus className="h-5 w-5 text-[#e85d3f]" />
              {image ? image.name : "Şəkil seçin"}
              <input
                type="file"
                accept="image/*"
                className="hidden"
                onChange={(event) =>
                  setImage(event.target.files?.[0] ?? null)
                }
              />
            </span>
          </label>
          <FormField
            label="Məhsul adı və təsviri"
            placeholder="Məsələn, Trüf souslu tagliatelle"
            error={errors.desc?.message}
            {...register("desc")}
          />
          <div className="grid gap-4 sm:grid-cols-2">
            <FormField
              label="Qiymət (₼)"
              type="number"
              step="0.01"
              error={errors.price?.message}
              {...register("price", { valueAsNumber: true })}
            />
            <label className="block">
              <span className="mb-2 block text-sm font-semibold text-[#3c3530]">
                Kateqoriya
              </span>
              <select
                className="h-12 w-full rounded-xl border border-[#dcd5cc] bg-white px-4 text-sm outline-none focus:border-[#e85d3f]"
                {...register("categoryId")}
              >
                <option value="">Seçin</option>
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
              {errors.categoryId && (
                <span className="mt-1.5 block text-xs text-[#c94a33]">
                  {errors.categoryId.message}
                </span>
              )}
            </label>
          </div>
          <FormField
            label="Tərkib və qida məlumatı"
            placeholder="Əsas inqrediyentlər və allergenlər"
            error={errors.nutrition?.message}
            {...register("nutrition")}
          />
          {mutation.isError && (
            <p className="rounded-xl bg-[#fff0ed] p-3 text-sm text-[#b5442f]">
              {getErrorMessage(mutation.error)}
            </p>
          )}
          <div className="flex justify-end gap-2 pt-2">
            <Button
              type="button"
              variant="secondary"
              onClick={() => setModalOpen(false)}
            >
              Ləğv et
            </Button>
            <Button type="submit" loading={mutation.isPending}>
              Məhsulu əlavə et
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
