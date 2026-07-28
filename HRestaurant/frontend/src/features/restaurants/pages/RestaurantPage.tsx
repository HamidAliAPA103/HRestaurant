import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { MapPin, Phone, Plus, Store, Users } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import {
  createResource,
  listResource,
} from "@/shared/api/resources";
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
import { getErrorMessage } from "@/shared/lib/utils";
import type {
  Restaurant,
  RestaurantInput,
} from "@/shared/types/domain";

const schema = z.object({
  name: z.string().min(2, "Restoran adı ən azı 2 simvol olmalıdır."),
  adres: z.string().min(5, "Ünvanı tam daxil edin."),
  number: z.string().min(7, "Telefon nömrəsini daxil edin."),
});

export function RestaurantPage() {
  const [modalOpen, setModalOpen] = useState(false);
  const queryClient = useQueryClient();
  const query = useQuery({
    queryKey: ["restaurants"],
    queryFn: () => listResource<Restaurant>("/Restaurant"),
  });
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RestaurantInput>({
    resolver: zodResolver(schema),
    defaultValues: { name: "", adres: "", number: "" },
  });

  const mutation = useMutation({
    mutationFn: async (input: RestaurantInput) => {
      const response = await createResource<RestaurantInput>(
        "/Restaurant",
        input,
      );
      if (!response.success) throw new Error(response.message);
      return response;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["restaurants"] });
      reset();
      setModalOpen(false);
    },
  });

  return (
    <div className="page-enter space-y-6">
      <PageHeader
        eyebrow="Şəbəkə idarəetməsi"
        title="Restoranlar"
        description="Filiallarınızı, əlaqə məlumatlarını və əməliyyat statusunu bir yerdən idarə edin."
        actions={
          <Button onClick={() => setModalOpen(true)}>
            <Plus className="h-4 w-4" />
            Yeni restoran
          </Button>
        }
      />

      {query.isLoading ? (
        <LoadingState label="Restoranlar yüklənir" />
      ) : query.isError ? (
        <ErrorState
          message={getErrorMessage(query.error)}
          onRetry={() => query.refetch()}
        />
      ) : !query.data?.data?.length ? (
        <EmptyState
          title="Restoran əlavə edilməyib"
          description="İlk filialınızı yaradaraq idarəetməyə başlayın."
          action={
            <Button size="sm" onClick={() => setModalOpen(true)}>
              <Plus className="h-4 w-4" />
              Restoran yarat
            </Button>
          }
        />
      ) : (
        <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
          {query.data.data.map((restaurant, index) => (
            <article
              key={restaurant.id}
              className="card group overflow-hidden transition hover:-translate-y-1 hover:shadow-[0_16px_40px_rgba(54,43,34,.08)]"
            >
              <div
                className={`h-2 ${
                  index % 3 === 0
                    ? "bg-[#e85d3f]"
                    : index % 3 === 1
                      ? "bg-[#e0a045]"
                      : "bg-[#5b8d68]"
                }`}
              />
              <div className="p-6">
                <div className="flex items-start justify-between">
                  <div className="grid h-12 w-12 place-items-center rounded-2xl bg-[#f2ede6] text-[#4f4640]">
                    <Store className="h-5 w-5" />
                  </div>
                  <Badge tone="success" dot>
                    Aktiv
                  </Badge>
                </div>
                <h2 className="mt-5 text-xl font-bold tracking-tight text-[#29231f]">
                  {restaurant.name}
                </h2>
                <div className="mt-4 space-y-2.5 text-sm text-[#716860]">
                  <div className="flex items-start gap-2">
                    <MapPin className="mt-0.5 h-4 w-4 shrink-0 text-[#e85d3f]" />
                    {restaurant.adres}
                  </div>
                  <div className="flex items-center gap-2">
                    <Phone className="h-4 w-4 text-[#e85d3f]" />
                    {restaurant.number}
                  </div>
                </div>
                <div className="mt-6 flex items-center justify-between border-t border-[#eee9e3] pt-4 text-xs">
                  <span className="flex items-center gap-1.5 text-[#8b8179]">
                    <Users className="h-3.5 w-3.5" />
                    Əməliyyat filialı
                  </span>
                  <button className="font-bold text-[#e85d3f]">İdarə et →</button>
                </div>
              </div>
            </article>
          ))}
        </div>
      )}

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title="Yeni restoran"
        description="Yeni filialın əsas məlumatlarını daxil edin."
      >
        <form
          className="space-y-4"
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
        >
          <FormField
            label="Restoran adı"
            placeholder="Məsələn, HRestaurant Mərkəz"
            error={errors.name?.message}
            {...register("name")}
          />
          <FormField
            label="Ünvan"
            placeholder="Şəhər, küçə və bina"
            error={errors.adres?.message}
            {...register("adres")}
          />
          <FormField
            label="Telefon"
            placeholder="+994 50 000 00 00"
            error={errors.number?.message}
            {...register("number")}
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
              Restoranı yarat
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
