import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Link, Navigate, useNavigate } from "react-router-dom";
import { z } from "zod";
import { register as registerAccount } from "@/features/auth/api/auth-api";
import { useAuthStore } from "@/features/auth/store/auth-store";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { getErrorMessage } from "@/shared/lib/utils";

const schema = z.object({
  fullName: z.string().trim().min(2).max(150),
  email: z.string().email(),
  restaurantId: z.string().uuid("Düzgün restoran ID-si daxil edin."),
  password: z.string().min(8).regex(/[A-Z]/).regex(/[a-z]/).regex(/[0-9]/).regex(/[^a-zA-Z0-9]/),
  confirmPassword: z.string(),
}).refine((value) => value.password === value.confirmPassword, { path: ["confirmPassword"], message: "Şifrələr uyğun deyil." });
type Values = z.infer<typeof schema>;

export function RegisterPage() {
  const accessToken = useAuthStore((state) => state.accessToken); const setSession = useAuthStore((state) => state.setSession); const navigate = useNavigate();
  const form = useForm<Values>({ resolver: zodResolver(schema), defaultValues: { fullName: "", email: "", restaurantId: "", password: "", confirmPassword: "" } });
  const mutation = useMutation({ mutationFn: registerAccount, onSuccess: (response) => { if (!response.success || !response.data) throw new Error(response.message); setSession(response.data); navigate("/dashboard", { replace: true }); } });
  if (accessToken) return <Navigate to="/dashboard" replace />;
  return <main className="grid min-h-screen place-items-center bg-[#f6f2eb] p-4"><section className="w-full max-w-lg rounded-3xl bg-white p-7 shadow-xl"><Link to="/login" className="text-sm font-bold text-[#d64f34]">← Girişə qayıt</Link><h1 className="mt-6 text-3xl font-bold">İlk owner hesabı</h1><p className="mt-2 text-sm leading-6 text-[#7d736b]">Bu forma yalnız hələ heç bir hesabla əlaqələndirilməmiş restoranın ilk owner hesabı üçündür. Mövcud restoran əməkdaşlarını owner panelindən əlavə edin.</p><form className="mt-7 space-y-4" onSubmit={form.handleSubmit((value) => mutation.mutate(value))}><FormField label="Ad və soyad" autoComplete="name" error={form.formState.errors.fullName?.message} {...form.register("fullName")} /><FormField label="Email" type="email" autoComplete="email" error={form.formState.errors.email?.message} {...form.register("email")} /><FormField label="Restoran ID" error={form.formState.errors.restaurantId?.message} {...form.register("restaurantId")} /><div className="grid gap-4 sm:grid-cols-2"><FormField label="Şifrə" type="password" autoComplete="new-password" error={form.formState.errors.password?.message} {...form.register("password")} /><FormField label="Şifrəni təkrarla" type="password" autoComplete="new-password" error={form.formState.errors.confirmPassword?.message} {...form.register("confirmPassword")} /></div>{mutation.isError && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{getErrorMessage(mutation.error)}</p>}<Button type="submit" className="w-full" loading={mutation.isPending}>Hesabı yarat</Button></form></section></main>;
}
