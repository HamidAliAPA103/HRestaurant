import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Clock3, Save } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { restaurantApi, restaurantKeys } from "@/api/restaurantApi";
import type { WorkingHour } from "@/api/contracts";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { PageHeader } from "@/shared/components/PageHeader";
import { ErrorState, LoadingState } from "@/shared/components/StatePanel";
import { getErrorMessage } from "@/shared/lib/utils";
import {
  createDefaultWorkingHours,
  dayNames,
  normalizeWorkingHours,
  setWorkingDayOpen,
  validateWorkingHours,
} from "@/features/settings/lib/workingHours";

const schema = z.object({
  currency: z.string().trim().length(3),
  taxRate: z.number().min(0).max(100),
});

type Values = z.infer<typeof schema>;

export function SettingsPage() {
  const queryClient = useQueryClient();
  const query = useQuery({
    queryKey: [...restaurantKeys.all, "current"],
    queryFn: ({ signal }) => restaurantApi.current(signal),
  });
  const [hours, setHours] = useState<WorkingHour[]>(createDefaultWorkingHours());
  const [hoursError, setHoursError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: { currency: "AZN", taxRate: 0 },
  });

  useEffect(() => {
    if (!query.data) return;

    reset({ currency: query.data.currency, taxRate: query.data.taxRate });
    setHours(normalizeWorkingHours(query.data.workingHours));
    setHoursError(null);
  }, [query.data, reset]);

  const settings = useMutation({
    mutationFn: (values: Values) => restaurantApi.updateSettings(
      query.data!.id,
      values.currency.toUpperCase(),
      values.taxRate,
    ),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: restaurantKeys.all }),
  });

  const working = useMutation({
    mutationFn: () => restaurantApi.updateWorkingHours(
      query.data!.id,
      hours.map((hour) => ({
        ...hour,
        opensAt: hour.isClosed ? null : hour.opensAt,
        closesAt: hour.isClosed ? null : hour.closesAt,
      })),
    ),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: restaurantKeys.all }),
  });

  function updateHour(dayOfWeek: number, patch: Partial<WorkingHour>) {
    setHours((current) => current.map((hour) => (
      hour.dayOfWeek === dayOfWeek ? { ...hour, ...patch } : hour
    )));
    setHoursError(null);
    working.reset();
  }

  function changeDayStatus(hour: WorkingHour, isOpen: boolean) {
    updateHour(hour.dayOfWeek, setWorkingDayOpen(hour, isOpen));
  }

  function saveWorkingHours() {
    const validationError = validateWorkingHours(hours);
    if (validationError) {
      setHoursError(validationError);
      return;
    }

    setHoursError(null);
    working.mutate();
  }

  if (query.isLoading) return <LoadingState label="Ayarlar yüklənir" />;
  if (query.isError) {
    return <ErrorState message={getErrorMessage(query.error)} onRetry={() => query.refetch()} />;
  }

  return (
    <div className="page-enter space-y-6">
      <PageHeader
        eyebrow="Konfiqurasiya"
        title="Restoran ayarları"
        description={`${query.data!.name} üçün valyuta, vergi və iş saatları.`}
      />

      <section className="card p-5">
        <h2 className="font-bold">Maliyyə ayarları</h2>
        <form
          className="mt-5 grid gap-4 sm:grid-cols-[1fr_1fr_auto] sm:items-end"
          onSubmit={handleSubmit((values) => settings.mutate(values))}
        >
          <FormField
            label="Valyuta"
            maxLength={3}
            error={errors.currency?.message}
            {...register("currency")}
          />
          <FormField
            label="Vergi faizi"
            type="number"
            min={0}
            max={100}
            step="0.01"
            error={errors.taxRate?.message}
            {...register("taxRate", { valueAsNumber: true })}
          />
          <Button type="submit" loading={settings.isPending}>
            <Save className="h-4 w-4" />Yadda saxla
          </Button>
        </form>
        {settings.isError && (
          <p className="mt-3 text-sm text-red-600">{getErrorMessage(settings.error)}</p>
        )}
      </section>

      <section className="card p-5">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="font-bold">İş saatları</h2>
            <p className="mt-1 text-xs text-[#82776f]">
              Hər günü açıq və ya bağlı seçin, sonra saatları özünüz təyin edin.
            </p>
          </div>
          <Button loading={working.isPending} onClick={saveWorkingHours}>
            <Save className="h-4 w-4" />Saatları saxla
          </Button>
        </div>

        <div className="mt-5 space-y-3">
          {hours.map((hour) => {
            const name = dayNames[hour.dayOfWeek];
            return (
              <div
                key={hour.dayOfWeek}
                role="group"
                aria-labelledby={`working-day-${hour.dayOfWeek}`}
                className="grid items-end gap-3 rounded-xl border p-4 lg:grid-cols-[1.1fr_180px_1fr_1fr]"
              >
                <div className="flex h-11 items-center gap-3">
                  <span className={`grid h-9 w-9 place-items-center rounded-xl ${hour.isClosed ? "bg-[#f2efeb] text-[#8b8179]" : "bg-[#fff0ed] text-[#e85d3f]"}`}>
                    <Clock3 className="h-4 w-4" />
                  </span>
                  <span id={`working-day-${hour.dayOfWeek}`} className="font-semibold">
                    {name}
                  </span>
                </div>

                <label className="block">
                  <span className="mb-1.5 block text-xs font-semibold text-[#756b63]">Status</span>
                  <select
                    aria-label={`${name} statusu`}
                    value={hour.isClosed ? "closed" : "open"}
                    onChange={(event) => changeDayStatus(hour, event.target.value === "open")}
                    className="h-11 w-full rounded-xl border bg-white px-3"
                  >
                    <option value="open">Açıqdır</option>
                    <option value="closed">Bağlıdır</option>
                  </select>
                </label>

                <label className="block">
                  <span className="mb-1.5 block text-xs font-semibold text-[#756b63]">Açılış</span>
                  <input
                    aria-label={`${name} açılış saatı`}
                    type="time"
                    step="60"
                    disabled={hour.isClosed}
                    value={hour.opensAt ?? ""}
                    onChange={(event) => updateHour(hour.dayOfWeek, { opensAt: event.target.value })}
                    className="h-11 w-full rounded-xl border px-3 disabled:cursor-not-allowed disabled:bg-[#f3f0ec] disabled:text-[#9b928b]"
                  />
                </label>

                <label className="block">
                  <span className="mb-1.5 block text-xs font-semibold text-[#756b63]">Bağlanış</span>
                  <input
                    aria-label={`${name} bağlanış saatı`}
                    type="time"
                    step="60"
                    disabled={hour.isClosed}
                    value={hour.closesAt ?? ""}
                    onChange={(event) => updateHour(hour.dayOfWeek, { closesAt: event.target.value })}
                    className="h-11 w-full rounded-xl border px-3 disabled:cursor-not-allowed disabled:bg-[#f3f0ec] disabled:text-[#9b928b]"
                  />
                </label>
              </div>
            );
          })}
        </div>

        {(hoursError || working.isError) && (
          <p role="alert" className="mt-4 rounded-xl bg-red-50 p-3 text-sm text-red-700">
            {hoursError ?? getErrorMessage(working.error)}
          </p>
        )}
        {working.isSuccess && !hoursError && (
          <p role="status" className="mt-4 rounded-xl bg-green-50 p-3 text-sm text-green-700">
            İş saatları uğurla yadda saxlanıldı.
          </p>
        )}
      </section>
    </div>
  );
}
