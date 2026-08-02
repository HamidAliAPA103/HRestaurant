import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Save } from "lucide-react";
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

const schema = z.object({ currency: z.string().trim().length(3), taxRate: z.number().min(0).max(100) }); type Values = z.infer<typeof schema>;
const dayNames = ["Bazar", "Bazar ertəsi", "Çərşənbə axşamı", "Çərşənbə", "Cümə axşamı", "Cümə", "Şənbə"];
const defaultHours = (): WorkingHour[] => dayNames.map((_, dayOfWeek) => ({ dayOfWeek, opensAt: "09:00", closesAt: "23:00", isClosed: false }));
export function SettingsPage() {
  const queryClient = useQueryClient(); const query = useQuery({ queryKey: [...restaurantKeys.all, "current"], queryFn: ({ signal }) => restaurantApi.current(signal) }); const [hours, setHours] = useState<WorkingHour[]>(defaultHours()); const { register, handleSubmit, reset, formState: { errors } } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: { currency: "AZN", taxRate: 0 } });
  useEffect(() => { if (query.data) { reset({ currency: query.data.currency, taxRate: query.data.taxRate }); setHours(query.data.workingHours.length === 7 ? query.data.workingHours : defaultHours()); } }, [query.data, reset]);
  const settings = useMutation({ mutationFn: (values: Values) => restaurantApi.updateSettings(query.data!.id, values.currency.toUpperCase(), values.taxRate), onSuccess: () => queryClient.invalidateQueries({ queryKey: restaurantKeys.all }) }); const working = useMutation({ mutationFn: () => restaurantApi.updateWorkingHours(query.data!.id, hours.map((x) => ({ ...x, opensAt: x.isClosed ? null : x.opensAt, closesAt: x.isClosed ? null : x.closesAt }))), onSuccess: () => queryClient.invalidateQueries({ queryKey: restaurantKeys.all }) });
  if (query.isLoading) return <LoadingState label="Ayarlar yüklənir" />; if (query.isError) return <ErrorState message={getErrorMessage(query.error)} onRetry={() => query.refetch()} />;
  return <div className="page-enter space-y-6"><PageHeader eyebrow="Konfiqurasiya" title="Restoran ayarları" description={`${query.data!.name} üçün valyuta, vergi və iş saatları.`} /><section className="card p-5"><h2 className="font-bold">Maliyyə ayarları</h2><form className="mt-5 grid gap-4 sm:grid-cols-[1fr_1fr_auto] sm:items-end" onSubmit={handleSubmit((values) => settings.mutate(values))}><FormField label="Valyuta" maxLength={3} error={errors.currency?.message} {...register("currency")} /><FormField label="Vergi faizi" type="number" min={0} max={100} step="0.01" error={errors.taxRate?.message} {...register("taxRate", { valueAsNumber: true })} /><Button type="submit" loading={settings.isPending}><Save className="h-4 w-4" />Yadda saxla</Button></form>{settings.isError && <p className="mt-3 text-sm text-red-600">{getErrorMessage(settings.error)}</p>}</section><section className="card p-5"><div className="flex items-center justify-between"><div><h2 className="font-bold">İş saatları</h2><p className="mt-1 text-xs text-[#82776f]">Bütün yeddi gün üçün.</p></div><Button loading={working.isPending} onClick={() => working.mutate()}><Save className="h-4 w-4" />Saatları saxla</Button></div><div className="mt-5 space-y-3">{hours.map((hour, index) => <div key={hour.dayOfWeek} className="grid items-center gap-3 rounded-xl border p-3 sm:grid-cols-[1fr_auto_1fr_1fr]"><span className="font-semibold">{dayNames[hour.dayOfWeek]}</span><label className="flex items-center gap-2 text-xs"><input type="checkbox" checked={hour.isClosed} onChange={(e) => setHours((all) => all.map((x,i) => i === index ? { ...x, isClosed: e.target.checked } : x))} />Bağlıdır</label><input aria-label="Açılış" type="time" disabled={hour.isClosed} value={hour.opensAt ?? ""} onChange={(e) => setHours((all) => all.map((x,i) => i === index ? { ...x, opensAt: e.target.value } : x))} className="h-10 rounded-xl border px-3" /><input aria-label="Bağlanış" type="time" disabled={hour.isClosed} value={hour.closesAt ?? ""} onChange={(e) => setHours((all) => all.map((x,i) => i === index ? { ...x, closesAt: e.target.value } : x))} className="h-10 rounded-xl border px-3" /></div>)}</div>{working.isError && <p className="mt-3 text-sm text-red-600">{getErrorMessage(working.error)}</p>}</section></div>;
}
